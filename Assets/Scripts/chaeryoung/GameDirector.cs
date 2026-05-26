using TMPro;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    GameObject hpText;
    GameObject npcCntTxt;
    GameObject spawner;

    float npcCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpText = GameObject.Find("HP");
        npcCntTxt = GameObject.Find("NPC count");
        spawner = GameObject.Find("NPCSpawner");
        npcCount = spawner.GetComponent<NPCSpawner>().count;
        Debug.Log(npcCount);
    }

    // Update is called once per frame
    void Update()
    {
        npcCntTxt.GetComponent<TMPro.TextMeshProUGUI>().text = (npcCount - ZombieAI.deathCount).ToString() + " / " + npcCount;
        hpText.GetComponent<TMPro.TextMeshProUGUI>().text = (PlayerHealth.currentHealth).ToString() + " / 100";
    }
}
