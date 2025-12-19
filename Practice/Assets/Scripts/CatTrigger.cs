using UnityEngine;

public class CatTrigger : MonoBehaviour
{
    [SerializeField] private Stage1GameFlow flow;

    private void Start()
    {
        // 자동으로 찾기(가능하면)
        flow = FindFirstObjectByType<Stage1GameFlow>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (flow == null) { flow = FindFirstObjectByType<Stage1GameFlow>(); return; }

        if (other.CompareTag("Fish"))
        {
            flow.StageClear();
        }
        else if (other.CompareTag("Ghost"))
        {
            flow.StageFail();
        }
    }
}
