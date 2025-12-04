using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Controls script execution order and initialization phases for network objects.
    /// Features:
    /// - Initialization phases (PreNetwork, NetworkReady, PostNetwork)
    /// - Dependency resolution
    /// - Deferred initialization queue
    /// - Ensures proper initialization order across all network components
    ///
    /// Execution Order: First to run (-100)
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("Game Creator/Network/Network Initialization Manager")]
    public class NetworkInitializationManager : MonoBehaviour
    {
        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkInitializationManager s_Instance;
        public static NetworkInitializationManager Instance => s_Instance;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Initialization Settings")]
        [SerializeField]
        [Tooltip("Log initialization phases for debugging")]
        private bool m_DebugLogging = true;

        [SerializeField]
        [Tooltip("Wait frames between phases to ensure Unity processes")]
        private int m_FramesBetweenPhases = 1;

        // MEMBERS: -------------------------------------------------------------------------------

        private InitializationPhase m_CurrentPhase = InitializationPhase.None;
        private readonly Queue<Action> m_PreNetworkQueue = new();
        private readonly Queue<Action> m_NetworkReadyQueue = new();
        private readonly Queue<Action> m_PostNetworkQueue = new();
        private readonly Queue<Action> m_DeferredQueue = new();
        private readonly List<INetworkInitializable> m_Initializables = new();
        private bool m_NetworkManagerReady;
        private bool m_NetworkSpawned;
        private int m_FrameCounter;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>Fired when an initialization phase starts.</summary>
        public static event Action<InitializationPhase> EventPhaseStarted;

        /// <summary>Fired when an initialization phase completes.</summary>
        public static event Action<InitializationPhase> EventPhaseCompleted;

        /// <summary>Fired when all initialization is complete.</summary>
        public static event Action EventInitializationComplete;

        /// <summary>Fired when network becomes ready (NetworkManager started).</summary>
        public static event Action EventNetworkReady;

        /// <summary>Fired when network spawning begins.</summary>
        public static event Action EventNetworkSpawning;

        // ENUMS: ---------------------------------------------------------------------------------

        public enum InitializationPhase
        {
            None = 0,
            PreNetwork = 1, // Before NetworkManager starts
            WaitingForNetwork = 2, // Waiting for NetworkManager
            NetworkReady = 3, // NetworkManager is ready
            WaitingForSpawn = 4, // Waiting for network spawn
            PostNetwork = 5, // After network spawn
            Complete = 6, // All initialization done
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>Current initialization phase.</summary>
        public InitializationPhase CurrentPhase => m_CurrentPhase;

        /// <summary>Is initialization complete?</summary>
        public bool IsInitialized => m_CurrentPhase == InitializationPhase.Complete;

        /// <summary>Is the NetworkManager ready?</summary>
        public bool IsNetworkReady => m_NetworkManagerReady;

        /// <summary>Has network spawning occurred?</summary>
        public bool IsNetworkSpawned => m_NetworkSpawned;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning(
                    "[NetworkInitializationManager] Duplicate instance detected. Destroying."
                );
                Destroy(gameObject);
                return;
            }

            s_Instance = this;

            // Start PreNetwork phase immediately
            StartPhase(InitializationPhase.PreNetwork);
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }

            // Unsubscribe from events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= OnNetworkStarted;
                NetworkManager.Singleton.OnClientStarted -= OnNetworkStarted;
            }
        }

        private void Start()
        {
            // Complete PreNetwork phase
            CompletePhase(InitializationPhase.PreNetwork);

            // Subscribe to NetworkManager events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += OnNetworkStarted;
                NetworkManager.Singleton.OnClientStarted += OnNetworkStarted;

                // Check if already started
                if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
                {
                    OnNetworkStarted();
                }
                else
                {
                    StartPhase(InitializationPhase.WaitingForNetwork);
                }
            }
            else
            {
                // No NetworkManager, skip to complete
                if (m_DebugLogging)
                {
                    Debug.Log(
                        "[NetworkInitializationManager] No NetworkManager found, completing initialization"
                    );
                }
                StartPhase(InitializationPhase.Complete);
            }
        }

        // UPDATE: --------------------------------------------------------------------------------

        private void Update()
        {
            // Process deferred queue
            ProcessDeferredQueue();

            // Frame counting for phase transitions
            if (m_FrameCounter > 0)
            {
                m_FrameCounter--;
                if (m_FrameCounter == 0)
                {
                    ProcessPhaseTransition();
                }
            }
        }

        // NETWORK CALLBACKS: ---------------------------------------------------------------------

        private void OnNetworkStarted()
        {
            if (m_NetworkManagerReady)
                return;

            m_NetworkManagerReady = true;

            if (m_DebugLogging)
            {
                Debug.Log("[NetworkInitializationManager] Network started");
            }

            EventNetworkReady?.Invoke();

            // Transition to NetworkReady phase
            CompletePhase(InitializationPhase.WaitingForNetwork);
            StartPhase(InitializationPhase.NetworkReady);

            // Subscribe to spawn events
            NetworkSessionEvents.EventLocalPlayerSpawned += OnLocalPlayerSpawned;
        }

        private void OnLocalPlayerSpawned(NetworkCharacter player)
        {
            NetworkSessionEvents.EventLocalPlayerSpawned -= OnLocalPlayerSpawned;

            m_NetworkSpawned = true;

            if (m_DebugLogging)
            {
                Debug.Log("[NetworkInitializationManager] Local player spawned");
            }

            EventNetworkSpawning?.Invoke();

            // Transition to PostNetwork phase
            CompletePhase(InitializationPhase.WaitingForSpawn);
            StartPhase(InitializationPhase.PostNetwork);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Register an action to run during PreNetwork phase.
        /// </summary>
        public void RegisterPreNetwork(Action action)
        {
            if (m_CurrentPhase > InitializationPhase.PreNetwork)
            {
                // Already past this phase, execute immediately
                action?.Invoke();
            }
            else
            {
                m_PreNetworkQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Register an action to run during NetworkReady phase.
        /// </summary>
        public void RegisterNetworkReady(Action action)
        {
            if (m_CurrentPhase > InitializationPhase.NetworkReady)
            {
                action?.Invoke();
            }
            else
            {
                m_NetworkReadyQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Register an action to run during PostNetwork phase.
        /// </summary>
        public void RegisterPostNetwork(Action action)
        {
            if (m_CurrentPhase > InitializationPhase.PostNetwork)
            {
                action?.Invoke();
            }
            else
            {
                m_PostNetworkQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Register an action to run next frame (deferred execution).
        /// </summary>
        public void RegisterDeferred(Action action)
        {
            m_DeferredQueue.Enqueue(action);
        }

        /// <summary>
        /// Register an INetworkInitializable component for managed initialization.
        /// </summary>
        public void RegisterInitializable(INetworkInitializable initializable)
        {
            if (!m_Initializables.Contains(initializable))
            {
                m_Initializables.Add(initializable);
            }
        }

        /// <summary>
        /// Unregister an INetworkInitializable component.
        /// </summary>
        public void UnregisterInitializable(INetworkInitializable initializable)
        {
            m_Initializables.Remove(initializable);
        }

        /// <summary>
        /// Force complete initialization (for testing/debugging).
        /// </summary>
        public void ForceComplete()
        {
            StartPhase(InitializationPhase.Complete);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartPhase(InitializationPhase phase)
        {
            m_CurrentPhase = phase;

            if (m_DebugLogging)
            {
                Debug.Log($"[NetworkInitializationManager] Starting phase: {phase}");
            }

            EventPhaseStarted?.Invoke(phase);

            // Process phase-specific queue
            switch (phase)
            {
                case InitializationPhase.PreNetwork:
                    ProcessQueue(m_PreNetworkQueue);
                    InitializePhase(phase);
                    break;

                case InitializationPhase.NetworkReady:
                    ProcessQueue(m_NetworkReadyQueue);
                    InitializePhase(phase);
                    SchedulePhaseTransition();
                    break;

                case InitializationPhase.PostNetwork:
                    ProcessQueue(m_PostNetworkQueue);
                    InitializePhase(phase);
                    SchedulePhaseTransition();
                    break;

                case InitializationPhase.Complete:
                    CompleteInitialization();
                    break;
            }
        }

        private void CompletePhase(InitializationPhase phase)
        {
            if (m_DebugLogging)
            {
                Debug.Log($"[NetworkInitializationManager] Completed phase: {phase}");
            }

            EventPhaseCompleted?.Invoke(phase);
        }

        private void InitializePhase(InitializationPhase phase)
        {
            foreach (var initializable in m_Initializables)
            {
                try
                {
                    initializable.OnInitializationPhase(phase);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[NetworkInitializationManager] Error in {initializable}: {e.Message}"
                    );
                }
            }
        }

        private void ProcessQueue(Queue<Action> queue)
        {
            while (queue.Count > 0)
            {
                var action = queue.Dequeue();
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[NetworkInitializationManager] Error processing queue: {e.Message}"
                    );
                }
            }
        }

        private void ProcessDeferredQueue()
        {
            int count = m_DeferredQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var action = m_DeferredQueue.Dequeue();
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[NetworkInitializationManager] Error in deferred action: {e.Message}"
                    );
                }
            }
        }

        private void SchedulePhaseTransition()
        {
            m_FrameCounter = m_FramesBetweenPhases;
        }

        private void ProcessPhaseTransition()
        {
            switch (m_CurrentPhase)
            {
                case InitializationPhase.NetworkReady:
                    CompletePhase(InitializationPhase.NetworkReady);
                    StartPhase(InitializationPhase.WaitingForSpawn);
                    break;

                case InitializationPhase.PostNetwork:
                    CompletePhase(InitializationPhase.PostNetwork);
                    StartPhase(InitializationPhase.Complete);
                    break;
            }
        }

        private void CompleteInitialization()
        {
            if (m_DebugLogging)
            {
                Debug.Log("[NetworkInitializationManager] Initialization complete");
            }

            EventInitializationComplete?.Invoke();
        }

        // STATIC RESET: --------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_Instance = null;
        }
    }

    /// <summary>
    /// Interface for components that need managed network initialization.
    /// </summary>
    public interface INetworkInitializable
    {
        /// <summary>
        /// Called during each initialization phase.
        /// Implement to handle phase-specific initialization.
        /// </summary>
        void OnInitializationPhase(NetworkInitializationManager.InitializationPhase phase);
    }
}
