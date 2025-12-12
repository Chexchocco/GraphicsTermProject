using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StageSelectController : MonoBehaviour
{
    [Header("Panels (Stage preview UI objects)")]
    [SerializeField] private List<GameObject> stagePanels = new List<GameObject>();

    [Header("Scene Names (must match Build Settings)")]
    [SerializeField] private List<string> stageSceneNames = new List<string>();

    [Header("Input")]
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode confirmKey = KeyCode.Return;   // Enter
    [SerializeField] private KeyCode confirmKey2 = KeyCode.KeypadEnter;

    [Header("Selection")]
    [SerializeField] private int currentIndex = 0;
    [SerializeField] private bool wrapAround = true; // 끝에서 반대편으로 돌아가기

    private float inputCooldown = 0.15f; // 키 꾹 누름 방지용(원하면 0으로)
    private float lastInputTime = -999f;

    private void Start()
    {
        // 패널/씬 리스트 길이 검사
        if (stagePanels.Count == 0)
            Debug.LogWarning("StageSelect: stagePanels is empty.");

        if (stageSceneNames.Count != stagePanels.Count)
            Debug.LogWarning("StageSelect: stageSceneNames count should match stagePanels count.");

        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, stagePanels.Count - 1));
        UpdatePanels();
    }

    private void Update()
    {
        if (Time.unscaledTime - lastInputTime < inputCooldown) return;

        if (Input.GetKeyDown(leftKey))
        {
            Move(-1);
        }
        else if (Input.GetKeyDown(rightKey))
        {
            Move(+1);
        }
        else if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(confirmKey2))
        {
            LoadCurrentStage();
        }
    }

    private void Move(int delta)
    {
        if (stagePanels.Count == 0) return;

        lastInputTime = Time.unscaledTime;

        int next = currentIndex + delta;

        if (wrapAround)
        {
            if (next < 0) next = stagePanels.Count - 1;
            if (next >= stagePanels.Count) next = 0;
        }
        else
        {
            next = Mathf.Clamp(next, 0, stagePanels.Count - 1);
        }

        if (next == currentIndex) return;

        currentIndex = next;
        UpdatePanels();
    }

    private void UpdatePanels()
    {
        for (int i = 0; i < stagePanels.Count; i++)
        {
            if (stagePanels[i] != null)
                stagePanels[i].SetActive(i == currentIndex);
        }
    }

    private void LoadCurrentStage()
    {
        if (stageSceneNames.Count == 0)
        {
            Debug.LogWarning("StageSelect: stageSceneNames is empty.");
            return;
        }

        if (currentIndex < 0 || currentIndex >= stageSceneNames.Count)
        {
            Debug.LogWarning("StageSelect: currentIndex out of range.");
            return;
        }

        string sceneName = stageSceneNames[currentIndex];

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("StageSelect: sceneName is empty.");
            return;
        }

        // Enter 누르면 해당 스테이지 씬으로 이동
        SceneManager.LoadScene(sceneName);
    }
}
