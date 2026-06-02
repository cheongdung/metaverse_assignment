using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    GameObject hpText;
    GameObject npcCntTxt;
    GameObject spawner;

    float npcCount;

    public bool isGameStarted = false;

    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;
    public GameObject gamePlayPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpText = GameObject.Find("HP");
        npcCntTxt = GameObject.Find("NPC count");
        spawner = GameObject.Find("NPCSpawner");
        npcCount = spawner.GetComponent<NPCSpawner>().count;
        Debug.Log(npcCount);

        isGameStarted = false;

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameClearPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        npcCntTxt.GetComponent<TMPro.TextMeshProUGUI>().text = (npcCount - ZombieAI.deathCount).ToString() + " / " + npcCount;
        hpText.GetComponent<TMPro.TextMeshProUGUI>().text = (PlayerHealth.currentHealth).ToString() + " / 100";

        if (npcCount - ZombieAI.deathCount <= 0) {
            SceneManager.LoadScene("GameClearScene");
        }
        if (PlayerHealth.currentHealth <= 0) {
            SceneManager.LoadScene("GameOverScene");
        }

    }

    public void StartGame() {
        isGameStarted = true;

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameClearPanel.SetActive(false);
        gamePlayPanel.SetActive(true);
    }

    public void GameOver() {
        isGameStarted = false;

        startPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        gameClearPanel.SetActive(false);
        gamePlayPanel.SetActive(false);

    }

    public void GameClear() {
        isGameStarted = false;

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameClearPanel.SetActive(true);
        gamePlayPanel.SetActive(false);

    }
    public void onStartGame() {
        SceneManager.LoadScene("GameScene");
    }
    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
