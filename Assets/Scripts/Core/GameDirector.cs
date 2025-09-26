using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance { get; private set; }

    public BaseBuilding playerBase, enemyBase;
    public float timeLimit = 180f;
    float timer;
    public Text timerText;
    public GameObject winPanel, losePanel;
    public static bool isPlayerWin = false;

    Queue<UnitType> playerRecent = new Queue<UnitType>(3);

    void Awake()
    {
        Instance = this;
        timer = timeLimit;
        playerBase.OnBaseDestroyed += OnBaseDestroyed;
        enemyBase.OnBaseDestroyed += OnBaseDestroyed;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(timer).ToString();
        if (timer <= 0f) JudgeByHP();
    }

    void JudgeByHP()
    {
        if (playerBase.hp > enemyBase.hp) ShowWin();
        else ShowLose();
    }

    void OnBaseDestroyed(BaseBuilding b)
    {
        if (b.team == Team.Enemy) ShowWin();
        else ShowLose();
    }

    void ShowWin() { if (winPanel) winPanel.SetActive(true); Time.timeScale = 0; }
    void ShowLose() { if (losePanel) losePanel.SetActive(true); Time.timeScale = 0; }

    public void OnUnitKilled(Team killerTeam, int victimCost)
    {
        // 可在此加赏金逻辑（示例：killerTeam 获得 victimCost*0.4）
        int bounty = Mathf.RoundToInt(victimCost * 0.4f);
        if (killerTeam == Team.Player) FindObjectOfType<Spawner>().gold += bounty;
        else FindObjectOfType<EnemyAI>().gold += bounty;
    }

    public void NotifyPlayerSpawn(UnitType t)
    {
        if (playerRecent.Count >= 3) playerRecent.Dequeue();
        playerRecent.Enqueue(t);
    }

    public (int inf, int cav, int mag) GetPlayerRecentCounts()
    {
        int a = 0, b = 0, c = 0;
        foreach (var t in playerRecent)
        {
            if (t == UnitType.Infantry) a++;
            else if (t == UnitType.Cavalry) b++;
            else c++;
        }
        return (a, b, c);
    }

    public void Restart() { Time.timeScale = 1; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
}
