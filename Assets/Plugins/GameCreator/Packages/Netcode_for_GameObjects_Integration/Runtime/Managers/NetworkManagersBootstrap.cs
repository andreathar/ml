using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Bootstrap component that ensures all network managers are present and properly configured.
    /// Add this to your NetworkManager GameObject or create a dedicated Network Managers object.
    ///
    /// Execution Order: Very first (-200)
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [AddComponentMenu("Game Creator/Network/Network Managers Bootstrap")]
    public class NetworkManagersBootstrap : MonoBehaviour
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Manager Components")]
        [SerializeField]
        [Tooltip("Auto-add NetworkInitializationManager if missing")]
        private bool m_EnsureInitializationManager = true;

        [SerializeField]
        [Tooltip("Auto-add NetworkSessionEvents if missing")]
        private bool m_EnsureSessionEvents = true;

        [SerializeField]
        [Tooltip("Auto-add NetworkSpawnManager if missing")]
        private bool m_EnsureSpawnManager = true;

        [SerializeField]
        [Tooltip("Auto-add NetworkGameStateManager if missing")]
        private bool m_EnsureGameStateManager = true;

        [SerializeField]
        [Tooltip("Auto-add NetworkRPCManager if missing")]
        private bool m_EnsureRPCManager = true;

        [SerializeField]
        [Tooltip("Auto-add NetworkSceneCoordinator if missing")]
        private bool m_EnsureSceneCoordinator = true;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("Don't destroy this object on scene load")]
        private bool m_DontDestroyOnLoad = true;

        [SerializeField]
        [Tooltip("Log manager setup information")]
        private bool m_DebugLogging = true;

        // MEMBERS: -------------------------------------------------------------------------------

        private static bool s_Bootstrapped;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Bootstrapped)
            {
                // Already bootstrapped, destroy duplicate
                Destroy(gameObject);
                return;
            }

            s_Bootstrapped = true;

            if (m_DontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureManagers();

            if (m_DebugLogging)
            {
                LogManagerStatus();
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnsureManagers()
        {
            // Check if NetworkManager exists
            if (NetworkManager.Singleton == null)
            {
                var nm = FindAnyObjectByType<NetworkManager>();
                if (nm == null)
                {
                    Debug.LogWarning("[NetworkManagersBootstrap] No NetworkManager found in scene!");
                }
            }

            // Ensure each manager
            if (m_EnsureInitializationManager)
            {
                EnsureComponent<NetworkInitializationManager>();
            }

            if (m_EnsureSessionEvents)
            {
                EnsureComponent<NetworkSessionEvents>();
            }

            // NetworkBehaviour managers need to be on a NetworkObject
            // Check if this GameObject has NetworkObject for the NetworkBehaviour managers
            NetworkObject networkObject = GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                // Try to find existing managers or create them on NetworkManager object
                var networkManagerObj = NetworkManager.Singleton?.gameObject;

                if (networkManagerObj != null)
                {
                    if (m_EnsureSpawnManager)
                    {
                        EnsureComponentOn<NetworkSpawnManager>(networkManagerObj);
                    }

                    if (m_EnsureGameStateManager)
                    {
                        EnsureComponentOn<NetworkGameStateManager>(networkManagerObj);
                    }

                    if (m_EnsureRPCManager)
                    {
                        EnsureComponentOn<NetworkRPCManager>(networkManagerObj);
                    }

                    if (m_EnsureSceneCoordinator)
                    {
                        EnsureComponentOn<NetworkSceneCoordinator>(networkManagerObj);
                    }
                }
                else
                {
                    Debug.LogWarning("[NetworkManagersBootstrap] NetworkManager not found. NetworkBehaviour managers will not be added.");
                }
            }
            else
            {
                // This GameObject has NetworkObject, add managers here
                if (m_EnsureSpawnManager)
                {
                    EnsureComponent<NetworkSpawnManager>();
                }

                if (m_EnsureGameStateManager)
                {
                    EnsureComponent<NetworkGameStateManager>();
                }

                if (m_EnsureRPCManager)
                {
                    EnsureComponent<NetworkRPCManager>();
                }

                if (m_EnsureSceneCoordinator)
                {
                    EnsureComponent<NetworkSceneCoordinator>();
                }
            }
        }

        private T EnsureComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = FindAnyObjectByType<T>();
            }
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                if (m_DebugLogging)
                {
                    Debug.Log($"[NetworkManagersBootstrap] Added {typeof(T).Name}");
                }
            }
            return component;
        }

        private T EnsureComponentOn<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                component = FindAnyObjectByType<T>();
            }
            if (component == null)
            {
                component = target.AddComponent<T>();
                if (m_DebugLogging)
                {
                    Debug.Log($"[NetworkManagersBootstrap] Added {typeof(T).Name} to {target.name}");
                }
            }
            return component;
        }

        private void LogManagerStatus()
        {
            Debug.Log("=== Network Managers Status ===");
            Debug.Log($"  NetworkManager: {(NetworkManager.Singleton != null ? "OK" : "MISSING")}");
            Debug.Log($"  NetworkInitializationManager: {(NetworkInitializationManager.Instance != null ? "OK" : "MISSING")}");
            Debug.Log($"  NetworkSessionEvents: {(NetworkSessionEvents.Instance != null ? "OK" : "MISSING")}");
            Debug.Log($"  NetworkSpawnManager: {(NetworkSpawnManager.Instance != null ? "OK" : "MISSING")}");
            Debug.Log($"  NetworkGameStateManager: {(NetworkGameStateManager.Instance != null ? "OK" : "MISSING")}");
            Debug.Log($"  NetworkRPCManager: {(NetworkRPCManager.Instance != null ? "OK" : "MISSING")}");
            Debug.Log($"  NetworkSceneCoordinator: {(NetworkSceneCoordinator.Instance != null ? "OK" : "MISSING")}");
            Debug.Log("===============================");
        }

        // STATIC RESET: --------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_Bootstrapped = false;
        }
    }
}
