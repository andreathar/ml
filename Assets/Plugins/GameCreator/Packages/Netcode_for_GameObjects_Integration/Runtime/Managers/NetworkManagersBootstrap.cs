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
        }

        private void Start()
        {
            // Log status in Start() after all Awake() methods have completed
            // This ensures singletons are properly initialized
            if (m_DebugLogging)
            {
                LogManagerStatus();
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnsureManagers()
        {
            // Find NetworkManager using FindAnyObjectByType since Singleton may not be set yet during Awake
            var networkManager = FindAnyObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogWarning("[NetworkManagersBootstrap] No NetworkManager found in scene!");
            }

            // Ensure each manager (MonoBehaviours - don't need NetworkObject)
            if (m_EnsureInitializationManager)
            {
                EnsureComponent<NetworkInitializationManager>();
            }

            if (m_EnsureSessionEvents)
            {
                EnsureComponent<NetworkSessionEvents>();
            }

            // NetworkBehaviour managers are already placed in the scene on their own GameObjects
            // with NetworkObject components. We just need to verify they exist.
            // Don't try to auto-add them as they need proper NetworkObject setup.

            // The managers should already exist in scene on separate GameObjects:
            // - NetworkSpawnManager (on its own GO with NetworkObject)
            // - NetworkGameStateManager (on its own GO with NetworkObject)
            // - NetworkRPCManager (on its own GO with NetworkObject)
            // - NetworkSceneCoordinator (on its own GO with NetworkObject)

            // Just verify they exist - don't auto-create as that causes issues
            if (m_DebugLogging)
            {
                VerifyNetworkBehaviourManagers();
            }
        }

        private void VerifyNetworkBehaviourManagers()
        {
            // Just check that the managers exist in the scene
            // They should be on their own GameObjects with NetworkObject components

            if (m_EnsureSpawnManager && FindAnyObjectByType<NetworkSpawnManager>() == null)
            {
                Debug.LogWarning(
                    "[NetworkManagersBootstrap] NetworkSpawnManager not found in scene. Add it to a GameObject with NetworkObject."
                );
            }

            if (m_EnsureGameStateManager && FindAnyObjectByType<NetworkGameStateManager>() == null)
            {
                Debug.LogWarning(
                    "[NetworkManagersBootstrap] NetworkGameStateManager not found in scene. Add it to a GameObject with NetworkObject."
                );
            }

            if (m_EnsureRPCManager && FindAnyObjectByType<NetworkRPCManager>() == null)
            {
                Debug.LogWarning(
                    "[NetworkManagersBootstrap] NetworkRPCManager not found in scene. Add it to a GameObject with NetworkObject."
                );
            }

            if (m_EnsureSceneCoordinator && FindAnyObjectByType<NetworkSceneCoordinator>() == null)
            {
                Debug.LogWarning(
                    "[NetworkManagersBootstrap] NetworkSceneCoordinator not found in scene. Add it to a GameObject with NetworkObject."
                );
            }
        }

        private T EnsureComponent<T>()
            where T : Component
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

        private T EnsureComponentOn<T>(GameObject target)
            where T : Component
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
                    Debug.Log(
                        $"[NetworkManagersBootstrap] Added {typeof(T).Name} to {target.name}"
                    );
                }
            }
            return component;
        }

        private void LogManagerStatus()
        {
            // Use FindAnyObjectByType for reliability - singletons may not be set depending on execution order
            Debug.Log("=== Network Managers Status ===");
            Debug.Log(
                $"  NetworkManager: {(FindAnyObjectByType<NetworkManager>() != null ? "OK" : "MISSING")}"
            );
            Debug.Log(
                $"  NetworkInitializationManager: {(FindAnyObjectByType<NetworkInitializationManager>() != null ? "OK" : "MISSING")}"
            );
            Debug.Log(
                $"  NetworkSessionEvents: {(FindAnyObjectByType<NetworkSessionEvents>() != null ? "OK" : "MISSING")}"
            );
            Debug.Log(
                $"  NetworkSpawnManager: {(FindAnyObjectByType<NetworkSpawnManager>() != null ? "OK" : "MISSING")}"
            );
            Debug.Log(
                $"  NetworkGameStateManager: {(FindAnyObjectByType<NetworkGameStateManager>() != null ? "OK" : "MISSING")}"
            );
            Debug.Log(
                $"  NetworkRPCManager: {(FindAnyObjectByType<NetworkRPCManager>() != null ? "OK" : "MISSING")}"
            );
            Debug.Log(
                $"  NetworkSceneCoordinator: {(FindAnyObjectByType<NetworkSceneCoordinator>() != null ? "OK" : "MISSING")}"
            );
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
