using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Manages synchronized game state across all clients.
    /// Features:
    /// - State machine (Lobby → Countdown → Playing → Paused → GameOver)
    /// - Ready-up system
    /// - Countdown synchronization
    /// - Pause/Resume coordination
    /// - Custom state data synchronization
    ///
    /// Execution Order: Should run after NetworkSpawnManager (-40)
    /// </summary>
    [DefaultExecutionOrder(-40)]
    [AddComponentMenu("Game Creator/Network/Network Game State Manager")]
    public class NetworkGameStateManager : NetworkBehaviour
    {
        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkGameStateManager s_Instance;
        public static NetworkGameStateManager Instance => s_Instance;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("State Settings")]
        [SerializeField]
        [Tooltip("Initial game state when network starts")]
        private GameState m_InitialState = GameState.Lobby;

        [SerializeField]
        [Tooltip("Countdown duration before game starts (seconds)")]
        private float m_CountdownDuration = 3f;

        [Header("Ready System")]
        [SerializeField]
        [Tooltip("Require all players to be ready before starting")]
        private bool m_RequireAllPlayersReady = true;

        [SerializeField]
        [Tooltip("Minimum players required to start")]
        private int m_MinPlayersToStart = 1;

        [SerializeField]
        [Tooltip("Auto-start countdown when all players ready")]
        private bool m_AutoStartCountdown = true;

        // NETWORK VARIABLES: ---------------------------------------------------------------------

        private NetworkVariable<GameState> m_CurrentState = new(
            GameState.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<float> m_CountdownTimer = new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<float> m_GameTimer = new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<bool> m_IsPaused = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // MEMBERS: -------------------------------------------------------------------------------

        private readonly HashSet<ulong> m_ReadyPlayers = new();
        private readonly Dictionary<string, object> m_CustomStateData = new();
        private bool m_CountdownActive;
        private GameState m_PreviousState;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>Fired when game state changes. Passes (oldState, newState).</summary>
        public static event Action<GameState, GameState> EventStateChanged;

        /// <summary>Fired when countdown starts.</summary>
        public static event Action<float> EventCountdownStarted;

        /// <summary>Fired every countdown tick. Passes remaining time.</summary>
        public static event Action<float> EventCountdownTick;

        /// <summary>Fired when countdown completes.</summary>
        public static event Action EventCountdownComplete;

        /// <summary>Fired when a player's ready state changes. Passes (clientId, isReady).</summary>
        public static event Action<ulong, bool> EventPlayerReadyChanged;

        /// <summary>Fired when all players are ready.</summary>
        public static event Action EventAllPlayersReady;

        /// <summary>Fired when game is paused.</summary>
        public static event Action EventGamePaused;

        /// <summary>Fired when game is resumed.</summary>
        public static event Action EventGameResumed;

        /// <summary>Fired when game timer updates (every second).</summary>
        public static event Action<float> EventGameTimerUpdate;

        // ENUMS: ---------------------------------------------------------------------------------

        public enum GameState
        {
            None = 0, // Not initialized
            Lobby = 1, // Waiting for players
            Countdown = 2, // Starting countdown
            Loading = 3, // Loading game resources
            Playing = 4, // Game in progress
            Paused = 5, // Game paused
            GameOver = 6, // Game ended
            Transitioning = 7, // Between states
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>Current game state.</summary>
        public GameState CurrentState => m_CurrentState.Value;

        /// <summary>Previous game state.</summary>
        public GameState PreviousState => m_PreviousState;

        /// <summary>Countdown timer remaining (seconds).</summary>
        public float CountdownRemaining => m_CountdownTimer.Value;

        /// <summary>Game timer (seconds since game started).</summary>
        public float GameTime => m_GameTimer.Value;

        /// <summary>Is the game currently paused?</summary>
        public bool IsPaused => m_IsPaused.Value;

        /// <summary>Number of ready players.</summary>
        public int ReadyPlayerCount => m_ReadyPlayers.Count;

        /// <summary>Are all connected players ready?</summary>
        public bool AreAllPlayersReady
        {
            get
            {
                if (NetworkManager.Singleton == null)
                    return false;
                int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
                return connectedCount >= m_MinPlayersToStart
                    && m_ReadyPlayers.Count >= connectedCount;
            }
        }

        /// <summary>Is the game in a playable state?</summary>
        public bool IsPlayable => CurrentState == GameState.Playing && !IsPaused;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning(
                    "[NetworkGameStateManager] Duplicate instance detected. Destroying."
                );
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
        }

        public override void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to state changes
            m_CurrentState.OnValueChanged += OnStateChanged;
            m_IsPaused.OnValueChanged += OnPausedChanged;

            if (IsServer)
            {
                // Set initial state
                m_CurrentState.Value = m_InitialState;

                // Subscribe to client events
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

                Debug.Log($"[NetworkGameStateManager] Initialized with state: {m_InitialState}");
            }
        }

        public override void OnNetworkDespawn()
        {
            m_CurrentState.OnValueChanged -= OnStateChanged;
            m_IsPaused.OnValueChanged -= OnPausedChanged;

            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            base.OnNetworkDespawn();
        }

        // UPDATE: --------------------------------------------------------------------------------

        private void Update()
        {
            if (!IsServer)
                return;

            // Update countdown
            if (m_CountdownActive && CurrentState == GameState.Countdown)
            {
                m_CountdownTimer.Value -= Time.deltaTime;

                if (m_CountdownTimer.Value <= 0)
                {
                    m_CountdownActive = false;
                    m_CountdownTimer.Value = 0;
                    OnCountdownComplete();
                }
            }

            // Update game timer
            if (CurrentState == GameState.Playing && !IsPaused)
            {
                m_GameTimer.Value += Time.deltaTime;
            }
        }

        // STATE CHANGE CALLBACKS: ----------------------------------------------------------------

        private void OnStateChanged(GameState oldState, GameState newState)
        {
            m_PreviousState = oldState;

            Debug.Log($"[NetworkGameStateManager] State changed: {oldState} -> {newState}");
            EventStateChanged?.Invoke(oldState, newState);
        }

        private void OnPausedChanged(bool wasPaused, bool isPaused)
        {
            if (isPaused)
            {
                Debug.Log("[NetworkGameStateManager] Game paused");
                EventGamePaused?.Invoke();
            }
            else
            {
                Debug.Log("[NetworkGameStateManager] Game resumed");
                EventGameResumed?.Invoke();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            // Remove from ready list
            if (m_ReadyPlayers.Remove(clientId))
            {
                NotifyReadyStateChangedClientRpc(clientId, false);
            }
        }

        // PUBLIC STATE METHODS: ------------------------------------------------------------------

        /// <summary>
        /// Set the game state. Server-only.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetworkGameStateManager] Only server can set state");
                return;
            }

            if (m_CurrentState.Value == newState)
                return;

            m_CurrentState.Value = newState;
        }

        /// <summary>
        /// Start the countdown. Transitions to Countdown state.
        /// Server-only.
        /// </summary>
        public void StartCountdown(float? customDuration = null)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetworkGameStateManager] Only server can start countdown");
                return;
            }

            float duration = customDuration ?? m_CountdownDuration;
            m_CountdownTimer.Value = duration;
            m_CountdownActive = true;
            SetState(GameState.Countdown);

            EventCountdownStarted?.Invoke(duration);
            NotifyCountdownStartedClientRpc(duration);

            Debug.Log($"[NetworkGameStateManager] Countdown started: {duration}s");
        }

        /// <summary>
        /// Skip countdown and start game immediately.
        /// Server-only.
        /// </summary>
        public void StartGameImmediate()
        {
            if (!IsServer)
                return;

            m_CountdownActive = false;
            m_CountdownTimer.Value = 0;
            m_GameTimer.Value = 0;
            SetState(GameState.Playing);

            Debug.Log("[NetworkGameStateManager] Game started immediately");
        }

        /// <summary>
        /// Pause the game. Server-only.
        /// </summary>
        public void Pause()
        {
            if (!IsServer)
                return;
            if (CurrentState != GameState.Playing)
                return;

            m_IsPaused.Value = true;
        }

        /// <summary>
        /// Resume the game. Server-only.
        /// </summary>
        public void Resume()
        {
            if (!IsServer)
                return;

            m_IsPaused.Value = false;
        }

        /// <summary>
        /// Toggle pause state. Server-only.
        /// </summary>
        public void TogglePause()
        {
            if (!IsServer)
                return;

            m_IsPaused.Value = !m_IsPaused.Value;
        }

        /// <summary>
        /// End the game. Server-only.
        /// </summary>
        public void EndGame()
        {
            if (!IsServer)
                return;

            m_IsPaused.Value = false;
            m_CountdownActive = false;
            SetState(GameState.GameOver);

            Debug.Log($"[NetworkGameStateManager] Game ended. Duration: {m_GameTimer.Value:F1}s");
        }

        /// <summary>
        /// Return to lobby. Server-only.
        /// </summary>
        public void ReturnToLobby()
        {
            if (!IsServer)
                return;

            m_ReadyPlayers.Clear();
            m_GameTimer.Value = 0;
            m_CountdownTimer.Value = 0;
            m_IsPaused.Value = false;
            m_CountdownActive = false;
            SetState(GameState.Lobby);

            Debug.Log("[NetworkGameStateManager] Returned to lobby");
        }

        // READY SYSTEM: --------------------------------------------------------------------------

        /// <summary>
        /// Set local player's ready state. Sends RPC to server.
        /// </summary>
        public void SetReady(bool isReady)
        {
            SetReadyRpc(isReady);
        }

        /// <summary>
        /// Check if a specific client is ready.
        /// </summary>
        public bool IsClientReady(ulong clientId)
        {
            return m_ReadyPlayers.Contains(clientId);
        }

        /// <summary>
        /// Get all ready client IDs.
        /// </summary>
        public IEnumerable<ulong> GetReadyClients()
        {
            return m_ReadyPlayers;
        }

        // CUSTOM STATE DATA: ---------------------------------------------------------------------

        /// <summary>
        /// Set custom state data (local only, not synced).
        /// Use RPCs to sync custom data.
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            m_CustomStateData[key] = value;
        }

        /// <summary>
        /// Get custom state data.
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (m_CustomStateData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnCountdownComplete()
        {
            m_GameTimer.Value = 0;
            SetState(GameState.Playing);

            EventCountdownComplete?.Invoke();
            NotifyCountdownCompleteClientRpc();

            Debug.Log("[NetworkGameStateManager] Countdown complete, game started");
        }

        // RPCs: ----------------------------------------------------------------------------------

        [Rpc(SendTo.Server)]
        private void SetReadyRpc(bool isReady, RpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            bool wasReady = m_ReadyPlayers.Contains(clientId);
            if (isReady == wasReady)
                return;

            if (isReady)
            {
                m_ReadyPlayers.Add(clientId);
            }
            else
            {
                m_ReadyPlayers.Remove(clientId);
            }

            Debug.Log($"[NetworkGameStateManager] Client {clientId} ready: {isReady}");

            // Notify all clients
            NotifyReadyStateChangedClientRpc(clientId, isReady);
            EventPlayerReadyChanged?.Invoke(clientId, isReady);

            // Check if all players ready
            if (AreAllPlayersReady)
            {
                EventAllPlayersReady?.Invoke();
                NotifyAllPlayersReadyClientRpc();

                if (m_AutoStartCountdown && CurrentState == GameState.Lobby)
                {
                    StartCountdown();
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyReadyStateChangedClientRpc(ulong clientId, bool isReady)
        {
            if (!IsServer) // Server already invoked the event
            {
                EventPlayerReadyChanged?.Invoke(clientId, isReady);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyAllPlayersReadyClientRpc()
        {
            if (!IsServer)
            {
                EventAllPlayersReady?.Invoke();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyCountdownStartedClientRpc(float duration)
        {
            if (!IsServer)
            {
                EventCountdownStarted?.Invoke(duration);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyCountdownCompleteClientRpc()
        {
            if (!IsServer)
            {
                EventCountdownComplete?.Invoke();
            }
        }

        // STATIC RESET: --------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_Instance = null;
        }
    }
}
