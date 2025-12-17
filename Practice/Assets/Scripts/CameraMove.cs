using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private GameObject pivot; //기준점 같은거, 플레이어 위치에 따라 움직이게 수정해줘야 버그 생길 여지가 적을듯

    public float rotateSpeed = 5.0f;
    public float panSpeed = 0.5f;
    public float zoomSpeed = 5.0f;
    public bool onMove = false;
    private float yaw = 0.0f;   // 좌우 회전 (Y축)
    private float pitch = 0.0f; // 상하 회전 (X축)
    private float distance = 10.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        if (pivot != null)
        {
            distance = Vector3.Distance(transform.position, pivot.transform.position);
        }



    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!pivot) return;
        if (!GameManager.ScreenMove) return;
        if (onMove) return;
        if (Input.GetMouseButton(1)) // 우
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;

            pitch = Mathf.Clamp(pitch, 40f, 80f); // 땅 밑으로 안 가게 제한 + 너무 가파르게 보면 속도가 어쩔 수 없이 너무 빠른거 떔에 수정함
        }

        if (Input.GetMouseButton(0)) // 좌
        {
            float dx = -Input.GetAxis("Mouse X") * panSpeed;
            float dz = -Input.GetAxis("Mouse Y") * panSpeed;

            Vector3 right = transform.right;
            Vector3 up = transform.up;
            
            // 마우스 이동 반대 방향으로 pivot을 움직임
            Vector3 moveDir = (right * dx) + (up * dz);
            pivot.transform.position += moveDir;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, 2f, 50f); 

        //최종 변환 적용
        //Quaternion생성
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 위치 계산: pivot 위치에서 회전 방향으로 거리만큼 뒤로 물러난 곳
        Vector3 position = pivot.transform.position - (rotation * Vector3.forward * distance);

        // 적용
        transform.rotation = rotation;
        transform.position = position;


    }
}
