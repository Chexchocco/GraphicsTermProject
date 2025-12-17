using UnityEngine;

public class GhostChase : MonoBehaviour
{
    [SerializeField] private Transform target; // Cat 넣기
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float turnSpeed = 6f;
    [SerializeField] private Stage1GameFlow flow;

    private bool stopped = false;

    private void Reset()
    {
        flow = FindFirstObjectByType<Stage1GameFlow>();
    }

    private void Update()
    {
        if (stopped) return;
        if (target == null) return;

        Vector3 to = target.position - transform.position;
        //to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;

        // 회전
        Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        // 이동
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    public void Stop()
    {
        stopped = true;
    }
}
