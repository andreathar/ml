using UnityEngine;

/// <summary>
/// Changes the object's color when a player gets near.
/// Attach to any GameObject with a Renderer component.
/// </summary>
public class ProximityColorChanger : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    [Tooltip("Distance at which color change triggers")]
    public float detectionRadius = 3f;

    [Header("Color Settings")]
    [Tooltip("Default color when player is far")]
    public Color normalColor = Color.white;

    [Tooltip("Color when player is near")]
    public Color nearColor = Color.red;

    [Tooltip("Speed of color transition")]
    public float colorTransitionSpeed = 5f;

    private Renderer objectRenderer;
    private Material material;
    private Transform playerTransform;
    private Color targetColor;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError($"ProximityColorChanger: No Renderer found on {gameObject.name}");
            enabled = false;
            return;
        }

        // Create instance of material to avoid modifying shared material
        material = objectRenderer.material;
        material.color = normalColor;
        targetColor = normalColor;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Try to find player if not found yet
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                return;
            }
        }

        // Calculate distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Set target color based on proximity
        targetColor = distance <= detectionRadius ? nearColor : normalColor;

        // Smoothly transition to target color
        material.color = Color.Lerp(material.color, targetColor, Time.deltaTime * colorTransitionSpeed);
    }

    void OnDestroy()
    {
        // Clean up instanced material
        if (material != null)
        {
            Destroy(material);
        }
    }

    // Visualize detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
