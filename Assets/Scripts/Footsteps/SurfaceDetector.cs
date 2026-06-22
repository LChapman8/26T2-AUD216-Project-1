using UnityEngine;

public class SurfaceDetector : MonoBehaviour
{
    [Tooltip("Layers to include in surface detection")]
    public LayerMask layerMask = -1;
    public bool DebugFootstepSurface;

    [SerializeField] private float sphereRadius = 0.3f;
    [SerializeField] private float rayStartHeight = 0.1f;  // Lowered to just above feet
    [SerializeField] private float rayDistance = 1.0f;     // Added explicit ray distance

    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public string GetSurfaceType(Vector3 position)
    {
        // Start from the feet position
        Vector3 rayStart = position + Vector3.up * rayStartHeight;
        RaycastHit hit;

        // Try SphereCast first
        if (Physics.SphereCast(rayStart, sphereRadius, Vector3.down, out hit, rayDistance, layerMask))
        {
            if (DebugFootstepSurface)
            {
                //Debug.Log($"Surface Detection - Hit: {hit.collider.name}, Tag: {hit.collider.tag}, Distance: {hit.distance}, Point: {hit.point}");
                Debug.DrawLine(rayStart, hit.point, Color.green, 0.1f);
            }

            // Check for tagged objects first (highest priority)
            switch (hit.collider.tag)
            {
                case "Wood": return "wood";
                case "Rock": return "rock";
                case "Water": return "water";
                case "Swamp": return "swamp";
            }

            // Check for terrain (secondary priority)
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                TerrainTextureDetector detector = terrain.GetComponent<TerrainTextureDetector>();
                if (detector != null)
                {
                    string textureType = detector.MapTextureToCategory(
                        detector.GetTextureAtPoint(hit.point));
                    
                    if (DebugFootstepSurface)
                    {
                   //     Debug.Log($"Terrain Surface Detected: {textureType}");
                    }
                    return textureType;
                }
            }
        }
        else
        {
            // Fallback to regular Raycast if SphereCast fails
            if (Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance, layerMask))
            {
                if (DebugFootstepSurface)
                {
                   // Debug.Log($"Raycast Detection - Hit: {hit.collider.name}, Tag: {hit.collider.tag}");
                }
                
                // Same tag checks as above
                switch (hit.collider.tag)
                {
                    case "Wood": return "wood";
                    case "Rock": return "rock";
                    case "Water": return "water";
                    case "Swamp": return "swamp";
                }

                // Check terrain as before
                Terrain terrain = hit.collider.GetComponent<Terrain>();
                if (terrain != null)
                {
                    TerrainTextureDetector detector = terrain.GetComponent<TerrainTextureDetector>();
                    if (detector != null)
                    {
                        return detector.MapTextureToCategory(
                            detector.GetTextureAtPoint(hit.point));
                    }
                }
            }
            else if (DebugFootstepSurface)
            {
                Debug.DrawRay(rayStart, Vector3.down * rayDistance, Color.red, 0.1f);
               // Debug.Log($"No surface detected at all from position {rayStart}");
            }
        }

        return "default";
    }

    private void OnDrawGizmos()
    {
        if (DebugFootstepSurface)
        {
            Vector3 rayStart = transform.position + Vector3.up * rayStartHeight;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rayStart, sphereRadius);
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * rayDistance);
        }
    }
}
