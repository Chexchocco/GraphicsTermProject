using System;
using UnityEngine;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;
    private Collider col;
    private float playerHalfHeight;
    private float speed = 10.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        playerHalfHeight = col.bounds.extents.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        keyCheck();
    }







    private void keyCheck()
    {
        /*
         player의 좌표를 screentoworldpoint로 두고
         거기에 dx dz를 더해서 가고 싶은 위치를 찾아
        Camera.ScreenPointToRay(TargetScreenPos)
         */
        float dx = 0, dz = 0;
        if (Input.GetKey(KeyCode.W))
        {
            dz += 333.0f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dz -= 333.0f;

        }
        if (Input.GetKey(KeyCode.A))
        {
            dx -= 333.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dx += 333.0f  ; 

        }
        if (dx == 0 && dz == 0) return;


        Vector3 pos = Camera.main.WorldToScreenPoint(
            new Vector3(this.transform.position.x, 
            this.transform.position.y+0.01f-playerHalfHeight,
            this.transform.position.z));
        
        pos.x += dx * Time.deltaTime;
        pos.y += dz * Time.deltaTime;


        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(pos), out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPos = hit.point + Vector3.up * playerHalfHeight;
            //Vector3 nextPos = Vector3.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
            //부드럽게 움직이려고 이렇게 하면 오히려 문제 생기는듯
            rb.MovePosition(targetPos);
        }
        else 
        {
            //
        }
         



    }
}

/*
 

         player의 좌표를 screentoworldpoint로 두고
         거기에 dx dz를 더해서 가고 싶은 위치를 찾아
        Camera.ScreenPointToRay(TargetScreenPos)
        /
float dx = Input.GetAxis("Horizontal");
float dz = Input.GetAxis("Vertical");
if (dx == 0 && dz == 0) return;

이동거리 보정을 위해 dx dz 를 
카메라와 지면이 이루는 각도로 보정해주기
근데 일딘 지면이 평평하다고 생각하고 대충 구현        



Vector3 camDir = cam.transform.forward;
float angle = 90.0f - Vector3.Angle(camDir, Vector3.up);



Vector3 currentWorldPos = new Vector3(this.transform.position.x,
    this.transform.position.y - playerHalfHeight,
    this.transform.position.z);
Vector3 currentScreenPos = Camera.main.WorldToScreenPoint(currentWorldPos);

currentScreenPos.x += dx;
currentScreenPos.y += dz;


RaycastHit hit;
if (Physics.Raycast(Camera.main.ScreenPointToRay(currentScreenPos), out hit, Mathf.Infinity, groundLayer))
{
    //ray로 쏴서 알아낸 가야할 위치
    Vector3 targetPos = hit.point + Vector3.up * playerHalfHeight;

    // 거리 차이 계산
    float worldDist = Vector3.Distance(currentWorldPos, targetPos);

    // 보정을 위해 화면상 변위 다시 계산
    Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(targetPos);
    float screenDist = Vector3.Distance(currentScreenPos, targetScreenPos);

    if (screenDist < 0.01f) return;

    // 이동거리 보정
    float worldSpeed = moveSpeed * worldDist / screenDist;

    // 8. 이동 실행
    Vector3 nextPos = Vector3.MoveTowards(rb.position, targetPos, worldSpeed * Time.fixedDeltaTime);
    rb.MovePosition(nextPos);


}
else
{
    //
}
*/
