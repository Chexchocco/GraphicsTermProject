using Unity.VisualScripting;
using UnityEngine;
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



    }
}
