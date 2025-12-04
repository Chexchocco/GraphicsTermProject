using UnityEngine;

public class MirrorGroundClipper : MonoBehaviour
{
    [Header("Settings")]
    public Camera mirrorCamera;   
    public Transform mirrorPlane;  
    public LayerMask groundLayer;   
    public GameObject groundPrefab; 
    public RenderTexture captureTexture; 

    [Header("Scan Settings")]
    [Range(10, 100)]
    public int scanStep = 40;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SpawnSingleReflectedGround();
        }
    }

    void SpawnSingleReflectedGround()
    {
        int width = captureTexture.width;
        int height = captureTexture.height;

        RaycastHit bestHit = new RaycastHit();
        bool found = false;
        float minDistanceToCenter = float.MaxValue;
        Vector2 screenCenter = new Vector2(width / 2f, height / 2f);

        // 거울에서 ray 쏴서 layer가 ground인 가장 가까운 오브젝트 찾음
        for (int y = 0; y < height; y += scanStep)
        {
            for (int x = 0; x < width; x += scanStep)
            {
                Ray ray = mirrorCamera.ScreenPointToRay(new Vector3(x, y, 0));

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
                {
                    float dist = Vector2.Distance(new Vector2(x, y), screenCenter);
                    if (dist < minDistanceToCenter)
                    {
                        minDistanceToCenter = dist;
                        bestHit = hit;
                        found = true;
                    }
                }
            }
        }

        if (found)
        {
            CreateReflectedObject(bestHit);
        }
    }

    void CreateReflectedObject(RaycastHit hit)// 찾은 ground를 거울에 비친 각도, 위치에 생성
    {

        Vector3 N = mirrorPlane.forward;
        Vector3 O = mirrorPlane.position;

        Vector3 P = hit.point;
        float distToPlane = Vector3.Dot(P - O, N);
        Vector3 reflectedPos = P - 2 * distToPlane * N;

        Vector3 realForward = hit.transform.forward;
        Vector3 realUp = hit.transform.up;

        Vector3 reflectedForward = Vector3.Reflect(realForward, N);
        Vector3 reflectedUp = Vector3.Reflect(realUp, N);

        Quaternion reflectedRot = Quaternion.LookRotation(reflectedForward, reflectedUp);
        Instantiate(groundPrefab, reflectedPos, reflectedRot);
    }
}