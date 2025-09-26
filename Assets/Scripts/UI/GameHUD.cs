using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Refs")]
    public BaseBuilding playerBase;
    public BaseBuilding enemyBase;

    [Header("HP Bars (Fill Images)")]
    public Image playerHpFill;   // 拖 HPBar_Player/Fill
    public Image enemyHpFill;    // 拖 HPBar_Enemy/Fill

    [Header("Texts")]
    public TextMeshProUGUI timerText; // 拖 HUD/TimerText
    public TextMeshProUGUI goldText;  // 拖 HUD/GoldText（玩家金币）

    [Header("Gold Source")]
    public Spawner playerSpawner; // 负责金币逻辑的脚本（你原来就有 gold）

    float elapsed;

    void Update()
    {
        // HP
        if (playerBase && playerHpFill)
        {
            float f = (playerBase.maxHP > 0f) ? playerBase.hp / playerBase.maxHP : 0f;
            playerHpFill.fillAmount = Mathf.Clamp01(f);
        }
        if (enemyBase && enemyHpFill)
        {
            float f = (enemyBase.maxHP > 0f) ? enemyBase.hp / enemyBase.maxHP : 0f;
            enemyHpFill.fillAmount = Mathf.Clamp01(f);
        }

        // 金币
        if (playerSpawner && goldText)
        {
            goldText.text = playerSpawner.gold.ToString();
        }

        // 计时（你也可以用倒计时）
        elapsed += Time.deltaTime;
        if (timerText)
        {
            int t = Mathf.FloorToInt(elapsed);
            int m = t / 60;
            int s = t % 60;
            timerText.text = $"{m:00}:{s:00}";
        }
    }
}
