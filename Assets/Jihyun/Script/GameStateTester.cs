using UnityEngine;

public class GameStateTester : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            gameManager.GameOver();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            gameManager.GameClear();
        }
    }
}
