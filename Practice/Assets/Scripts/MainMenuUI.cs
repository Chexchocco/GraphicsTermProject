using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject howToPanel;

    public void OnStartGame()
    {
        SceneManager.LoadScene("StageSelect"); 
    }

    public void OnHowToPlay()
    {
        howToPanel.SetActive(true);
    }

    public void OnCloseHowTo()
    {
        howToPanel.SetActive(false);
    }
}
