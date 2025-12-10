using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ShadowColllider : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Transform lightSource;
    public GameObject originObject;
    [SerializeField] private LayerMask falseGroundLayer;

    private Mesh srcMesh;              // 원본 물체의 메쉬
    private Mesh shadowMesh;           // 새로 만들 그림자 메쉬
    private MeshFilter mf;
    private MeshCollider mc;
    public Material shadowMaterial;
    private List<GameObject> shadowPool = new List<GameObject>();

    private int gridResolution = 4;
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();
        srcMesh = originObject.GetComponent<MeshFilter>().sharedMesh;

        shadowMesh = new Mesh();
        shadowMesh.name = "Generated Shadow Mesh";
        shadowMesh.MarkDynamic();




        MakeShadow(); //일단 광원 변경없이 시작시 생성되는걸로만

    }

    // Update is called once per frame
    void Update()
    {




    }



    List<Vector3> GridMaking(Transform origin)
    {
        /*
         그림자 생성용 발판에서 빛을 쏠 때
        쏘는 위치를 grid 화 해서 여러개 만들도록 하는 함수
        transform 데이터를 바탕으로 
        윗면 아랫면의 grid를 제작
        일단 발판이 cube 형임을 가정하고 제작
        만약 다른 모양의 발판으로 만들고 싶으면 흠...

        네모로 조개서 나눠서 시행시키거나
        애도 grid 로 조갠담에 snapping 시키기?         
         */

          

        List<Vector3> points = new List<Vector3>();

        float step = 1.0f / (gridResolution - 1); // 간격 계산

        // 평면을 기준으로 생성
        //
        for (int x = 0; x < gridResolution; ++x)
        {
            for (int z = 0; z < gridResolution; ++z)
            {
                // -0.5 ~ 0.5 (로컬 좌표계) 사이의 좌표 생성
                float localX = -0.5f + (x * step);
                float localZ = -0.5f + (z * step);

                // 윗면 아랫면에서 각각 발사하게 설정
                points.Add(new Vector3(localX, -0.5f, localZ));
                points.Add(new Vector3(localX, 0.5f, localZ));
            }
        }



        return points;
        // 반환 값 : 해당 grid 데이터 (vector list)

    }
    List<Vector3> GetConvexHull(List<Vector3> points, Vector3 upAxis)
    { 
        if (points.Count <= 3) return points;
        // point가 4개는 있어야함. 근데 없다는 거는 
        points = points.OrderBy(v => v.x).ThenBy(v => v.z).Distinct().ToList();


        List<Vector3> upper = new List<Vector3>();
        List<Vector3> lower = new List<Vector3>();

        // Lower Hull
        foreach (var p in points)
        {
            while (lower.Count >= 2 && IsClockwise(lower[lower.Count - 2], lower[lower.Count - 1], p, upAxis))
            {
                lower.RemoveAt(lower.Count - 1);
            }
            lower.Add(p);
        }

        // Upper Hull
        for (int i = points.Count - 1; i >= 0; i--)
        {
            var p = points[i];
            while (upper.Count >= 2 && IsClockwise(upper[upper.Count - 2], upper[upper.Count - 1], p, upAxis))
            {
                upper.RemoveAt(upper.Count - 1);
            }
            upper.Add(p);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        lower.AddRange(upper);
        return lower;

    }
    bool IsClockwise(Vector3 a, Vector3 b, Vector3 c, Vector3 up)
    {
        Vector3 cross = Vector3.Cross(b - a, c - a);
        return Vector3.Dot(cross, up) < 0; // Up 벡터 반대 방향이면 시계 방향
    }

    void MakeShadow()
    {
        Vector3[] originalVertices = srcMesh.vertices;
        Vector3[] newVertices = new Vector3[originalVertices.Length];

        Dictionary<Transform, List<Vector3>> hitGroups = new Dictionary<Transform, List<Vector3>>();
        //빛 발사 후 명중한 곳 기록용
        Transform worldPos = originObject.transform;
        Vector3 lightDir = lightSource.forward; //일단 directional light만 생각
        /*
         이게 vertex들에 대해서만 하면
        ㅁ ㅁ ㅁ 이런식으로 띄엄 띄엄 되잇는 경우가 감지가 안 될 수 있어서
        좀 큐브 기준으로 6면에 대해 좀 쪼개서 빛을 개많이 보내는걸로
        그래도 이제 엄청 오버로드가 큰건 아니니까

        근데 이제 걸리는건 불필요한게 생기기 마련이긴한데,
        일단 이 방식으로 만드는게 추후에 확장하기 더 좋아보여씀

         */
        List<Vector3> points = GridMaking(originObject.transform);

        // 윗 면 4x4 배열 아랫 면 4x4 배열로 해서 point 받아오기

        hitGroups.Clear();

        if (shadowPool != null)
        {
            foreach (var obj in shadowPool)
            {
                if (obj != null)
                {

                    obj.SetActive(false);
                }
            }
        }   

        int poolIndex = 0;


        foreach (var vertex in points)
        {
            Vector3 verPos = originObject.transform.TransformPoint(vertex); 
            // originObject (그림자 생성할 놈
            // 기준으로 local 좌표 설정된걸 world 좌표로 바꿔주기

            Ray ray = new Ray(verPos, lightDir);
            // 빛 발사
            
            RaycastHit hit;
            if (Physics.Raycast(verPos, lightDir, out hit, Mathf.Infinity, falseGroundLayer))
            {
                // 부딪히는 물체 세분화
                if (!hitGroups.ContainsKey(hit.transform))
                {//부딪힌 물체의 transform 정보를 key 값으로
                    hitGroups[hit.transform] = new List<Vector3>();
                }
                /*
                 일단 여기서 빛을 개많이 쏴도 이 그림자라는게
                네모 발판 안에 포함되있쓸 수도있고, 그 점이 하나만 있을 수 도있음
                 그래서 좀 더 고려를 하면
                일단 충돌한 물체만 저장
                그 담에 충돌한 물체에 대해서 8개의 꼭짓점으로 다시 한번 발사
                이때 충돌물체가 x,z가 무한하다고 가정(즉 동일 y지점을 찾음, 평면이라고 가정했으니 가능)
                그 담에 그 x,z 를 그 오브젝트 기준 가장 가까운 점으로 보냄

                그렇게 해서 8개의 점을 넣은후
                그 점들로 convexhull 알고리즘을 적용해서
                그림자 평면 제각각 구해주고

                그 그림자 평면을 표현하기 위한 mesh를 여러개 pooling해서 세팅해주기
                 */



            }
        }


        foreach (var group in hitGroups)
        {
            Transform hitPlatform = group.Key;
            Collider platCol = hitPlatform.GetComponent<Collider>();
            if (platCol == null) continue;

            List<Vector3> projectedPoints = new List<Vector3>();

            // 1) 8개 꼭짓점 투영 & 스냅
            foreach (var v in originalVertices)
            {
                Vector3 worldV = originObject.transform.TransformPoint(v);

                // 투영: 무한 평면에 맺힐 위치 계산
                Vector3 projected = PointCalculate(worldV, lightDir, hitPlatform);

                // 스냅: 발판 밖이면 모서리로, 안이면 그대로
                //Vector3 snapped  = platCol.ClosestPoint(projected);
           
                /*
                 이렇게 하면 대각선인 경우 찌그러짐

                그래서 다음 방법으로 
                 
                1. closest point 함수를 쓰는게 아닌
                직접 높이 설정후 빛 방향대로 x,z 움직여서 만나는 지점 찾기
                 */
                if(IsPointInsidePlatform(projected, hitPlatform))
                {
                    projectedPoints.Add(projected);

                }
                else
                {
                    //if (platCol == null) Debug.Log("platcol");
                    Vector3 snapped = getClosestPoint(projected, worldPos, platCol);

                    projectedPoints.Add(snapped);
                }



                //projectedPoints.Add(snapped);
            }

            List<Vector3> hullPoints = GetConvexHull(projectedPoints, hitPlatform.up);

            if (hullPoints.Count < 3) continue;

            // 메쉬 생성 및 풀링
            GameObject shadowObj = GetShadowFromPool(poolIndex++);
            UpdateMesh(shadowObj, hullPoints);
        }
    }
    Vector3 getClosestPoint(Vector3 originVertex, Transform trans, Collider platCol)
    {
        // 살짝 야매긴한데 forward 와 right 갖고 와서 이동시키기

        Vector3 snapped = platCol.ClosestPoint(originVertex);

        Vector3 dir = (snapped - originVertex).normalized;

        if (dir == Vector3.zero)
        {
            return originVertex;
        }
        Vector3 dA = trans.forward;

        Vector3 dB = trans.right;

        Vector3 newdir = trans.forward ;
        if (Vector3.Dot(dA, dir) > 0)
        {
            newdir = dA;
        }
        else if (Vector3.Dot(-dA, dir) > 0)
        {
            newdir = -dA;
        }
        


        Ray ray = new Ray(originVertex, newdir);
        RaycastHit hit;

        if (platCol.Raycast(ray, out hit, 1000f))
        {
            return hit.point;
        }


        if (Vector3.Dot(dB, dir) > 0)
        {
            newdir = dB;
        }
        else if (Vector3.Dot(-dB, dir) > 0)
        {
            newdir = -dB;
        }
        ray = new Ray(originVertex, newdir);

        if (platCol.Raycast(ray, out hit, 1000f))
        {
            return hit.point;
        }

        return snapped;

    }

    bool IsPointInsidePlatform(Vector3 worldPoint, Transform platformTr)
    {
        // 1. 박스 콜라이더 가져오기 (없으면 false)
        BoxCollider box = platformTr.GetComponent<BoxCollider>();
        if (box == null) return false;

        // 2. 월드 좌표 -> 발판 기준 로컬 좌표로 변환
        Vector3 localPoint = platformTr.InverseTransformPoint(worldPoint);

        // 3. 로컬 기준 박스 범위 계산 (Center ± Size/2)
        // (Y축은 무시하거나 아주 넉넉하게 체크)
        float halfX = box.size.x * 0.5f;
        float halfZ = box.size.z * 0.5f;

        // box.center가 (0,0,0)이 아닐 수도 있으니 고려
        float minX = box.center.x - halfX;
        float maxX = box.center.x + halfX;
        float minZ = box.center.z - halfZ;
        float maxZ = box.center.z + halfZ;

        // 4. 범위 검사 (XZ 평면)
        if (localPoint.x >= minX && localPoint.x <= maxX &&
            localPoint.z >= minZ && localPoint.z <= maxZ)
        {
            return true; // 안에 있음
        }

        return false; // 밖에 있음
    }



    Vector3 PointCalculate(Vector3 rayOrigin, Vector3 rayDir, Transform planeTr)
    {
        float halfHeight = planeTr.GetComponent<BoxCollider>().size.y * 0.5f * planeTr.localScale.y;
        Vector3 surfacePos = planeTr.position + planeTr.up * halfHeight;

        Plane plane = new Plane(planeTr.up, surfacePos);
        Ray ray = new Ray(rayOrigin, rayDir);
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return rayOrigin + rayDir * 10f; // 거의 없는 경우
    }



    GameObject GetShadowFromPool(int index)
    {
        if (index >= shadowPool.Count)
        {
            GameObject obj = new GameObject("Shadow_Pooled_" + index);
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();

            // 그림자처럼 보이게 검은색 반투명 재질 설정 (임시)
            obj.GetComponent<MeshRenderer>().material = shadowMaterial;
            obj.AddComponent<MeshCollider>();
            obj.layer = LayerMask.NameToLayer("Ground"); // 플레이어가 밟을 수 있게 레이어 설정

            shadowPool.Add(obj);
        }

        GameObject target = shadowPool[index];
        target.SetActive(true);
        return target;
    }



    void UpdateMesh(GameObject obj, List<Vector3> points)
    {
        Mesh mesh = new Mesh();

        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;

        Vector3[] vertices = points.ToArray();

        int[] triangles = new int[(points.Count - 2) * 3];
        for (int i = 0; i < points.Count - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}


/*
    void MakeShadow()
    {
        Vector3[] originalVertices = srcMesh.vertices;
        Vector3[] newVertices = new Vector3[originalVertices.Length];


        Transform worldPos = originObject.transform;
        Vector3 lightDir = lightSource.forward; //일단 directional light만 생각

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 worldV = worldPos.TransformPoint(originalVertices[i]);

                      

            Ray ray = new Ray(worldV, lightDir);
            RaycastHit hit;
            //빛 발사
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, falseGroundLayer))
            {//여기선 false ground로 설정해서 발판 만들기 
                newVertices[i] = transform.InverseTransformPoint(hit.point);

                // 맞은 지점 vertex로 저장
            }
            else
            {
                // 바닥에 안 닿으면 일단 임의로 더 보내기 안 만들어질까봐
                newVertices[i] = transform.InverseTransformPoint(worldV  + lightDir * 10f); 
            
            }

            

        }
        shadowMesh.vertices = newVertices;
        shadowMesh.triangles = srcMesh.triangles;

        mf.mesh = shadowMesh;
        mc.sharedMesh = shadowMesh; 
         * 이 방식 단점
         * 1. 그림자 모양대로 발판이 안 생김
         * 2. 경계면 감안이 안 되어있음

    }
}
*/