using System.Collections.Generic;
using UnityEngine;

public class ClippingObject : MonoBehaviour
{
    public Camera cam;

    // 자른 결과물을 임시 저장할 데이터 구조 (내부 클래스)
    class MeshData
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();

        public void AddVertex(Vector3 p, Vector3 n, Vector2 u)
        {
            vertices.Add(p);
            normals.Add(n);
            uvs.Add(u);
        }
        public void AddTriangle(int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {//Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
            for (int i = 0; i < 4; ++i)
            {
                Plane plane = GeometryUtility.CalculateFrustumPlanes(cam)[i];
                SliceMesh(plane);
            }

        }
    }

    void SliceMesh(Plane plane)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null)return;

        Mesh originalMesh = mf.mesh; // 원본 메쉬


        MeshData newMeshData = new MeshData();

        Vector3[] verts = originalMesh.vertices;
        int[] tris = originalMesh.triangles;
        Vector3[] normals = originalMesh.normals;
        Vector2[] uvs = originalMesh.uv;

        for (int i = 0; i < tris.Length; i += 3)
        {
            int i1 = tris[i];
            int i2 = tris[i + 1];
            int i3 = tris[i + 2];

            Vector3 v1 = transform.TransformPoint(verts[i1]);
            Vector3 v2 = transform.TransformPoint(verts[i2]);
            Vector3 v3 = transform.TransformPoint(verts[i3]);

            // 평면 기준으로 어디에 있는지 
            bool s1 = plane.GetSide(v1);
            bool s2 = plane.GetSide(v2);
            bool s3 = plane.GetSide(v3);

            if (s1 && s2 && s3) // 셋 다 안쪽 -> 유지
            {
                AddTriangle(newMeshData, verts, normals, uvs, i1, i2, i3);
            }
            else if (!s1 && !s2 && !s3) // 셋 다 바깥쪽 -> 삭제 
            {
                continue;
            }
            else // 걸쳐 있음 
            {
                SplitTriangle(newMeshData, plane,
                    verts[i1], verts[i2], verts[i3],
                    normals[i1], normals[i2], normals[i3],
                    uvs[i1], uvs[i2], uvs[i3],
                    s1, s2, s3);
            }
        }

        Mesh newMesh = new Mesh();
        newMesh.name = "Sliced Mesh";
        newMesh.vertices = newMeshData.vertices.ToArray();
        newMesh.triangles = newMeshData.triangles.ToArray();
        newMesh.normals = newMeshData.normals.ToArray();
        newMesh.uv = newMeshData.uvs.ToArray();
        newMesh.RecalculateBounds();

        mf.mesh = newMesh;
        if (GetComponent<MeshCollider>())
        {
            GetComponent<MeshCollider>().sharedMesh = newMesh;
        }
    }


    void AddTriangle(MeshData data, Vector3[] v, Vector3[] n, Vector2[] u, int a, int b, int c)
    {
        int idx = data.vertices.Count;
        data.AddVertex(v[a], n[a], u[a]);
        data.AddVertex(v[b], n[b], u[b]);
        data.AddVertex(v[c], n[c], u[c]);
        data.AddTriangle(idx, idx + 1, idx + 2);
    }

    void SplitTriangle(MeshData data, Plane plane,
        Vector3 v1, Vector3 v2, Vector3 v3,
        Vector3 n1, Vector3 n2, Vector3 n3,
        Vector2 uv1, Vector2 uv2, Vector2 uv3,
        bool s1, bool s2, bool s3)
    {
        if (s1 == s2)
        {
            Rotate(ref v1, ref v2, ref v3, ref n1, ref n2, ref n3, ref uv1, ref uv2, ref uv3, ref s1, ref s2, ref s3);
            Rotate(ref v1, ref v2, ref v3, ref n1, ref n2, ref n3, ref uv1, ref uv2, ref uv3, ref s1, ref s2, ref s3);
        }
        else if (s2 == s3)
        {
        }
        else
        {
            Rotate(ref v1, ref v2, ref v3, ref n1, ref n2, ref n3, ref uv1, ref uv2, ref uv3, ref s1, ref s2, ref s3);
        }

        Vector3 wV1 = transform.TransformPoint(v1);
        Vector3 wV2 = transform.TransformPoint(v2);
        Vector3 wV3 = transform.TransformPoint(v3);

        float enter1, enter2;
        new Ray(wV1, (wV2 - wV1).normalized).AsRay(plane, out enter1, Vector3.Distance(wV1, wV2));
        new Ray(wV1, (wV3 - wV1).normalized).AsRay(plane, out enter2, Vector3.Distance(wV1, wV3));

        float t1 = enter1 / Vector3.Distance(wV1, wV2);
        float t2 = enter2 / Vector3.Distance(wV1, wV3);

        Vector3 newV1 = Vector3.Lerp(v1, v2, t1);
        Vector3 newV2 = Vector3.Lerp(v1, v3, t2);
        Vector3 newN1 = Vector3.Lerp(n1, n2, t1);
        Vector3 newN2 = Vector3.Lerp(n1, n3, t2);
        Vector2 newUV1 = Vector2.Lerp(uv1, uv2, t1);
        Vector2 newUV2 = Vector2.Lerp(uv1, uv3, t2);

        if (s1) 
        {
            int idx = data.vertices.Count;
            data.AddVertex(v1, n1, uv1);
            data.AddVertex(newV1, newN1, newUV1);
            data.AddVertex(newV2, newN2, newUV2);
            data.AddTriangle(idx, idx + 1, idx + 2);
        }
        else 
        {
            int idx = data.vertices.Count;
            data.AddVertex(newV1, newN1, newUV1);
            data.AddVertex(v2, n2, uv2);
            data.AddVertex(v3, n3, uv3);
            data.AddVertex(newV2, newN2, newUV2);
            data.AddTriangle(idx, idx + 1, idx + 2);
            data.AddTriangle(idx, idx + 2, idx + 3);
        }
    }

    void Rotate<T>(ref T a, ref T b, ref T c, ref Vector3 n1, ref Vector3 n2, ref Vector3 n3, ref Vector2 u1, ref Vector2 u2, ref Vector2 u3, ref bool s1, ref bool s2, ref bool s3)
    {
        T tmp = a; a = b; b = c; c = tmp;
        Vector3 tmpN = n1; n1 = n2; n2 = n3; n3 = tmpN;
        Vector2 tmpU = u1; u1 = u2; u2 = u3; u3 = tmpU;
        bool tmpS = s1; s1 = s2; s2 = s3; s3 = tmpS;
    }
}

public static class RayExtension
{
    public static void AsRay(this Ray ray, Plane plane, out float enter, float maxDist)
    {
        plane.Raycast(ray, out enter);
    }
}