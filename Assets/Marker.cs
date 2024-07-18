using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public GameObject brushPrefab;
    public float _distance = 5f;
    public Material lineMaterial;  // Assign this in the Unity Inspector
    public GameObject whiteBoard;  // Assign the WhiteBoard object in the Inspector
    private LineRenderer lineRenderer;
    private List<Vector3> linePositions = new List<Vector3>();

    void Start()
    {
        // Ensure the brushPrefab has a LineRenderer component
        lineRenderer = brushPrefab.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = brushPrefab.AddComponent<LineRenderer>();
        }

        // Set initial properties of the LineRenderer
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 0;
        
        // Assign the material to the LineRenderer
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            Debug.LogWarning("Line Material is not assigned. Please assign a material in the inspector.");
        }

        // Ensure the whiteboard's mesh is readable
        MakeMeshReadable(whiteBoard);
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(brushPrefab.transform.position, brushPrefab.transform.forward, out hit, _distance))
        {
            if (hit.collider.gameObject.CompareTag("WhiteBoard"))
            {
                // Get the UV coordinates of the hit point
                Vector2 uv = hit.textureCoord;

                // Convert UV coordinates to world space positions
                Vector3 worldPosition = UVToWorldPosition(uv, whiteBoard);

                // Add the world position to the list of line positions
                linePositions.Add(worldPosition);

                // Update the LineRenderer with the new positions
                lineRenderer.positionCount = linePositions.Count;
                lineRenderer.SetPositions(linePositions.ToArray());
            }
        }
    }

    void MakeMeshReadable(GameObject board)
    {
        MeshFilter meshFilter = board.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Mesh originalMesh = meshFilter.sharedMesh;
            Mesh readableMesh = new Mesh();
            readableMesh.vertices = originalMesh.vertices;
            readableMesh.uv = originalMesh.uv;
            readableMesh.triangles = originalMesh.triangles;
            readableMesh.RecalculateBounds();
            readableMesh.RecalculateNormals();

            MeshCollider meshCollider = board.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = readableMesh;
            }
            meshFilter.sharedMesh = readableMesh;
        }
    }

    Vector3 UVToWorldPosition(Vector2 uv, GameObject board)
    {
        Renderer renderer = board.GetComponent<Renderer>();
        MeshCollider meshCollider = board.GetComponent<MeshCollider>();

        if (renderer == null || meshCollider == null)
        {
            Debug.LogError("WhiteBoard does not have a Renderer or MeshCollider.");
            return Vector3.zero;
        }

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        // Find the triangle that contains the UV coordinate
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 uv1 = uvs[triangles[i]];
            Vector2 uv2 = uvs[triangles[i + 1]];
            Vector2 uv3 = uvs[triangles[i + 2]];

            if (IsPointInTriangle(uv, uv1, uv2, uv3))
            {
                Vector3 world1 = renderer.transform.TransformPoint(vertices[triangles[i]]);
                Vector3 world2 = renderer.transform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 world3 = renderer.transform.TransformPoint(vertices[triangles[i + 2]]);

                return BarycentricToWorld(uv, uv1, uv2, uv3, world1, world2, world3);
            }
        }

        Debug.LogError("UV coordinate not found in any triangle.");
        return Vector3.zero;
    }

    bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        bool b1 = Sign(p, a, b) < 0.0f;
        bool b2 = Sign(p, b, c) < 0.0f;
        bool b3 = Sign(p, c, a) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    Vector3 BarycentricToWorld(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector3 worldA, Vector3 worldB, Vector3 worldC)
    {
        Vector3 barycentric = new Vector3(
            ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y)),
            ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y)),
            1.0f - (((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y)) + ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y)))
        );

        return worldA * barycentric.x + worldB * barycentric.y + worldC * barycentric.z;
    }
    
}
