using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    [Header("Team & Economy")]
    public Team team = Team.Player;
    public int gold = 150;
    public int incomePerSec = 4;

    [Header("Spawn")]
    public Transform spawnPoint;     // 玩家在左, 敌人在右
    public LayerMask enemyMask;
    public GameObject infantryPrefab, cavalryPrefab, magePrefab;
    public UnitConfig infantryCfg, cavalryCfg, mageCfg;

    [Header("UI")]
    public Text goldText;
    public Button infantryBtn, cavalryBtn, mageBtn;
    public CooldownFill infantryCD, cavalryCD, mageCD; // 冷却遮罩

    [Header("Audio")]
    public AudioClip sfxSpawn;

    Dictionary<UnitType, float> nextReadyTime = new();
    Queue<UnitType> recentPlayerSpawns = new Queue<UnitType>(3);

    void Start()
    {
        InvokeRepeating(nameof(AddIncome), 1f, 1f);
        RefreshUI();
        BindButton(infantryBtn, infantryCfg, infantryPrefab);
        BindButton(cavalryBtn, cavalryCfg, cavalryPrefab);
        BindButton(mageBtn, mageCfg, magePrefab);
    }

    void Update()
    {
        UpdateCooldownUI(infantryCfg, infantryCD);
        UpdateCooldownUI(cavalryCfg, cavalryCD);
        UpdateCooldownUI(mageCfg, mageCD);
    }

    void AddIncome() { gold += incomePerSec; RefreshUI(); }

    void RefreshUI() { if (goldText) goldText.text = gold.ToString(); }

    void BindButton(Button btn, UnitConfig cfg, GameObject prefab)
    {
        btn.onClick.AddListener(() => TrySpawn(cfg, prefab));
    }

    void UpdateCooldownUI(UnitConfig cfg, CooldownFill cd)
    {
        if (cd == null) return;
        nextReadyTime.TryGetValue(cfg.type, out float t);
        float remain = Mathf.Max(0, t - Time.time);
        cd.SetRemaining(remain, cfg.cooldown);
        bool canAfford = gold >= cfg.cost;
        cd.SetInteractable(canAfford && remain <= 0.01f);
    }

    void TrySpawn(UnitConfig cfg, GameObject prefab)
    {
        nextReadyTime.TryGetValue(cfg.type, out float readyAt);
        if (Time.time < readyAt) return;
        if (gold < cfg.cost) return;

        gold -= cfg.cost; RefreshUI();
        nextReadyTime[cfg.type] = Time.time + cfg.cooldown;

        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        var ub = go.GetComponent<UnitBase>();
        ub.config = cfg;
        ub.team = team;
        ub.enemyMask = enemyMask;
        ub.OnKilled += (dead) => GameDirector.Instance.OnUnitKilled(team, cfg.cost);

        if (sfxSpawn) AudioManager.I?.PlaySFX("spawn", sfxSpawn, 0.9f, 0.20f, 3, 0.03f);

        EnqueueRecent(cfg.type);
        GameDirector.Instance.NotifyPlayerSpawn(cfg.type); // 供AI自适应使用
    }

    void EnqueueRecent(UnitType t)
    {
        if (recentPlayerSpawns.Count >= 3) recentPlayerSpawns.Dequeue();
        recentPlayerSpawns.Enqueue(t);
    }
    void SetupMasks(UnitBase ub)
    {
        if (ub.team == Team.Player)
            ub.enemyMask = LayerMask.GetMask("EnemyUnit", "EnemyBase");
        else
            ub.enemyMask = LayerMask.GetMask("PlayerUnit", "PlayerBase");
    }
}
