using UnityEngine;

namespace MLCreator.Runtime.Gameplay.Effects
{
    /// <summary>
    /// Changes the object's color when a player gets near.
    /// Attach to any GameObject with a Renderer component.
    /// </summary>
    public class ProximityColorChanger : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Tag used to identify the player")]
        [SerializeField] private string m_PlayerTag = "Player";

        [Tooltip("Distance at which color change triggers")]
        [SerializeField] private float m_DetectionRadius = 3f;

        [Header("Color Settings")]
        [Tooltip("Default color when player is far")]
        [SerializeField] private Color m_NormalColor = Color.white;

        [Tooltip("Color when player is near")]
        [SerializeField] private Color m_NearColor = Color.red;

        [Tooltip("Speed of color transition")]
        [SerializeField] private float m_ColorTransitionSpeed = 5f;

        private Renderer m_ObjectRenderer;
        private Material m_Material;
        private Transform m_PlayerTransform;
        private Color m_TargetColor;

        private void Start()
        {
            m_ObjectRenderer = GetComponent<Renderer>();
            if (m_ObjectRenderer == null)
            {
                Debug.LogError($"ProximityColorChanger: No Renderer found on {gameObject.name}");
                enabled = false;
                return;
            }

            // Create instance of material to avoid modifying shared material
            m_Material = m_ObjectRenderer.material;
            m_Material.color = m_NormalColor;
            m_TargetColor = m_NormalColor;

            // Find player
            GameObject player = GameObject.FindGameObjectWithTag(m_PlayerTag);
            if (player != null)
            {
                m_PlayerTransform = player.transform;
            }
        }

        private void Update()
        {
            // Try to find player if not found yet
            if (m_PlayerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(m_PlayerTag);
                if (player != null)
                {
                    m_PlayerTransform = player.transform;
                }
                else
                {
                    return;
                }
            }

            // Calculate distance to player
            float distance = Vector3.Distance(transform.position, m_PlayerTransform.position);

            // Set target color based on proximity
            m_TargetColor = distance <= m_DetectionRadius ? m_NearColor : m_NormalColor;

            // Smoothly transition to target color
            m_Material.color = Color.Lerp(m_Material.color, m_TargetColor, Time.deltaTime * m_ColorTransitionSpeed);
        }

        private void OnDestroy()
        {
            // Clean up instanced material
            if (m_Material != null)
            {
                Destroy(m_Material);
            }
        }

        // Visualize detection radius in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);
        }
    }
}
