using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool isGameStarted = false;

    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;

    void Start()
    {
        isGameStarted = false;

        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        gameClearPanel.SetActive(false);
    }

    public void StartGame()
    {
        isGameStarted = true;

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameClearPanel.SetActive(false);
    }

    public void GameOver()
    {
        isGameStarted = false;

        startPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        gameClearPanel.SetActive(false);

    }

    public void GameClear()
    {
        isGameStarted = false;

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameClearPanel.SetActive(true);

    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
