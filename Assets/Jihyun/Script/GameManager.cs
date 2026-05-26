using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isGameStarted = false;
    public GameObject startPanel;

    public void StartGame()
    {
        isGameStarted = true;
        startPanel.SetActive(false);
    }
}
