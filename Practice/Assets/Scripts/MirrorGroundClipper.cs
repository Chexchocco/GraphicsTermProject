using UnityEngine;
using System.Collections.Generic;

public class MirrorGroundClipper : MonoBehaviour
{
    [Header("Settings")]
    public Camera mirrorCamera;
    public Transform mirrorPlane;
    public LayerMask groundLayer;
    public Material generatedMeshMaterial;
    public RenderTexture captureTexture;

    [Header("Generation Settings")]
    [Range(1, 20)]
    public int resolutionStep = 1;
    public float maxNeighborDist = 1.0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            GenerateReflectedGroundMesh();
        }
    }

    void GenerateReflectedGroundMesh()
    {
        int width = captureTexture.width;
        int height = captureTexture.height;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int gridW = Mathf.CeilToInt((float)width / resolutionStep);
        int gridH = Mathf.CeilToInt((float)height / resolutionStep);
        int[,] vertexIndices = new int[gridH, gridW];

        Vector3 planeNormal = mirrorPlane.forward;
        Vector3 planePos = mirrorPlane.position;

        for (int y = 0; y < gridH; y++)
        {
            for (int x = 0; x < gridW; x++)
            {
                int px = x * resolutionStep;
                int py = y * resolutionStep;

                Ray ray = mirrorCamera.ScreenPointToRay(new Vector3(px, py, 0));

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
                {
                    Vector3 reflectedPos = CalculateReflectedPoint(hit.point, planePos, planeNormal);
                    vertexIndices[y, x] = vertices.Count;
                    vertices.Add(reflectedPos);
                    uvs.Add(new Vector2((float)px / width, (float)py / height));
                }
                else
                {
                    vertexIndices[y, x] = -1;
                }
            }
        }

        for (int y = 0; y < gridH - 1; y++)
        {
            for (int x = 0; x < gridW - 1; x++)
            {
                int i0 = vertexIndices[y, x];         // Bottom-Left
                int i1 = vertexIndices[y, x + 1];     // Bottom-Right
                int i2 = vertexIndices[y + 1, x];     // Top-Left
                int i3 = vertexIndices[y + 1, x + 1]; // Top-Right

                if (i0 == -1 || i1 == -1 || i2 == -1 || i3 == -1) continue;

                if (Vector3.Distance(vertices[i0], vertices[i1]) > maxNeighborDist ||
                    Vector3.Distance(vertices[i0], vertices[i2]) > maxNeighborDist ||
                    Vector3.Distance(vertices[i1], vertices[i3]) > maxNeighborDist ||
                    Vector3.Distance(vertices[i2], vertices[i3]) > maxNeighborDist) continue;

                    triangles.Add(i0); triangles.Add(i1); triangles.Add(i2);
                    triangles.Add(i2); triangles.Add(i1); triangles.Add(i3);

            }
        }

        CreateMeshObject(vertices, triangles, uvs);
    }

    Vector3 CalculateReflectedPoint(Vector3 hitPoint, Vector3 planePos, Vector3 planeNormal)
    {
        Vector3 P = hitPoint;
        Vector3 O = planePos;
        Vector3 N = planeNormal;
        float distToPlane = Vector3.Dot(P - O, N);
        return P - 2 * distToPlane * N;
    }

    void CreateMeshObject(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        if (vertices.Count == 0) return;

        GameObject meshObj = new GameObject("ReflectedGroundMesh");
        MeshFilter mf = meshObj.AddComponent<MeshFilter>();
        MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
        MeshCollider mc = meshObj.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        mf.mesh = mesh;
        mr.material = generatedMeshMaterial;

        mc.sharedMesh = mesh;
        meshObj.layer = LayerMask.NameToLayer("Ground"); 
    }
}