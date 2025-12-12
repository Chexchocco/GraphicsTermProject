using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage1GameFlow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private TextMeshProUGUI clearText;
    [SerializeField] private TextMeshProUGUI failText;

    [Header("Timing")]
    [SerializeField] private float introHoldSeconds = 0.0f;  
    [SerializeField] private float introFadeSeconds = 2.0f;

    [Header("Scenes")]
    [SerializeField] private string stageSelectSceneName = "StageSelect";

    [Header("Stop on End")]
    [SerializeField] private MonoBehaviour[] disableScripts;

    private bool gameEnded = false;

    private void Start()
    {
        if (failText != null)
        {
            failText.gameObject.SetActive(false);
        }
        if (clearText != null)
        {
            clearText.gameObject.SetActive(false);
        }
        if(introText != null)
        {
            introText.gameObject.SetActive(true);
            introText.alpha = 1f;
            StartCoroutine(IntroRoutine());
        }
    }

    private IEnumerator IntroRoutine()
    {
        if (introHoldSeconds > 0f) yield return new WaitForSeconds(introHoldSeconds);

        float t = 0f;
        while (t < introFadeSeconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / introFadeSeconds);
            introText.alpha = a;
            yield return null;
        }

        introText.alpha = 0f;
        introText.gameObject.SetActive(false);
    }

    private IEnumerator HideIntroFallback()
    {
        yield return new WaitForSeconds(introFadeSeconds);
        if (introText != null) introText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameEnded) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }

    public void StageClear()
    {
        if (gameEnded) return;
        gameEnded = true;
        StopActors();
        clearText.gameObject.SetActive(true);
    }

    public void StageFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        StopActors();
        failText.gameObject.SetActive(true);
    }
    private void StopActors()
    {
        if (disableScripts != null)
        {
            foreach (var s in disableScripts)
                if (s != null) s.enabled = false;
        }

    }
}

