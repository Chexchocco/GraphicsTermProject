using Unity.VisualScripting;
using UnityEngine;

namespace PlayerWithAnimation
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMover_PlayerWithAnimation : MonoBehaviour
    {
        [Header("Movement")]
        public float screenMoveSpeed = 150f;  
        public float rayDistance = 100f;
        public LayerMask groundLayer;
        private Collider col;
        [Header("Animation")]
        public string verticalID = "Vert";
        public string stateID = "State";

        private Rigidbody rb;
        private Animator animator;
        private Camera cam;
        private float halfHeight;

        [SerializeField] CameraMove cm;


        // 입력 저장용
        private Vector2 inputAxis = Vector2.zero;

        // 애니메이션용
        private float animValue = 0f;
        private float animFlow = 6f; 
       
        float footOffset;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            cam = Camera.main;

            col = GetComponent<Collider>();
            //halfHeight = col.bounds.extents.y;
            footOffset = transform.position.y - col.bounds.min.y;

            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        void Update()
        {
            float dx = 0f, dz = 0f;

            if (!GameManager.CatMove) return;

            if (Input.GetKey(KeyCode.W)) dz += 1f;
            if (Input.GetKey(KeyCode.S)) dz -= 1f;
            if (Input.GetKey(KeyCode.A)) dx -= 1f;
            if (Input.GetKey(KeyCode.D)) dx += 1f;
            
            inputAxis = new Vector2(dx, dz);
        }

        void FixedUpdate()
        {
            HandleMove(Time.fixedDeltaTime);
        }

        void LateUpdate()
        {
            HandleAnimation(Time.deltaTime);
        }

        private void HandleMove(float dt)
        {
            if (inputAxis == Vector2.zero)
            {
                // 입력 없으면 애니메이션만 멈추게
                SetAnimMoving(false);
                cm.onMove = false;
                return;
            }
            cm.onMove = true;
            // 스크린 좌표 계산 (발밑 근처 기준)
            Vector3 screenPos = cam.WorldToScreenPoint(
                new Vector3(
                    transform.position.x,
                    transform.position.y - footOffset + 0.01f,
                    transform.position.z
                )
            );

            // 화면 상에서 WASD 방향으로 이동
            screenPos.x += inputAxis.x * screenMoveSpeed * dt;
            screenPos.y += inputAxis.y * screenMoveSpeed * dt;


            Vector3 camFwd = cam.transform.forward;
            Vector3 camRight = cam.transform.right;
            camFwd.y = 0; camRight.y = 0;
            camFwd.Normalize(); camRight.Normalize();

            Vector3 moveDir = (camFwd * inputAxis.y + camRight * inputAxis.x).normalized;
            RaycastHit hit;
            // Collider currentGround = CurrentGround();


            // 레이캐스트로 "현재 카메라에서 봤을 때 이어져 보이는 땅" 찾기
            if (Physics.Raycast(
                cam.ScreenPointToRay(screenPos),
                out hit,
                rayDistance,
                groundLayer))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) < 0.7f)
                {
                    SetAnimMoving(false);
                    Debug.Log("Side detection");
                    return; // 옆면(벽)이나 천장이므로 이동 불가
                }
                Vector3 targetPos = hit.point + Vector3.up * footOffset;
                if (Mathf.Abs(transform.position.y - targetPos.y) <= 1.0f)
                {//같은 지면 일때를 대충 구현
                    //Debug.Log("same");
                    MoveToTarget(targetPos);
                }
                else
                {
                    Debug.Log("telpo");
                    rb.MovePosition(targetPos);


                }

                // 이동 방향 기준으로 회전
                Vector3 flatDir = targetPos - transform.position;
                flatDir.y = 0f;
                if (flatDir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.2f);
                }
                SetAnimMoving(true);

            }
            else
            {
                SetAnimMoving(false);
            }
        }


        private Collider CurrentGround()
        {
            float checkRadius = footOffset;
            Vector3 origin = transform.position + Vector3.up * 1.0f;

            float maxDist = 1.0f - checkRadius + 0.2f;

            if (Physics.SphereCast(origin, checkRadius, Vector3.down, out RaycastHit hit, maxDist, groundLayer))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) > 0.5f)
                {
                    return hit.collider;
                }
            }

            return null;
        }
        private void MoveToTarget(Vector3 targetPos)
        {
            Vector3 currentPos = rb.position;
            Vector3 moveDelta = targetPos - currentPos;
            
            moveDelta.y = 0.0f;
            Vector3 moveDir = moveDelta.normalized;
            float originMag = moveDelta.magnitude;
            /*if (rb.SweepTest(moveDir, out RaycastHit hit, moveDelta.magnitude + 0.2f))
            {
                Debug.Log("222");
                float safeDist = Mathf.Max(0, hit.distance-0.3f);
                moveDelta = moveDir * safeDist;
            }*/
            if(Physics.Raycast(currentPos, moveDir, out RaycastHit hit, originMag +0.4f))
            {
                Debug.Log("222");

                float safeDist = Mathf.Max(0, hit.distance - 0.9f);
                moveDelta = moveDir * safeDist;
            }


            rb.MovePosition(currentPos + moveDelta);
        }


        private void SetAnimMoving(bool isMoving)
        {
            float target = isMoving ? 1f : 0f;
            if (isMoving)
                animValue = Mathf.Clamp01(animValue + Time.deltaTime * animFlow);
            else
                animValue = Mathf.Clamp01(animValue - Time.deltaTime * animFlow);
        }

        private void HandleAnimation(float dt)
        {
            animator.SetFloat(verticalID, animValue);
            animator.SetFloat(stateID, animValue);
        }
    }
}
