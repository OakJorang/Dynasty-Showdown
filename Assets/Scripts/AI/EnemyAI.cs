using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Team & Economy")]
    public Team team = Team.Enemy;
    public int gold = 150;
    public int incomePerSec = 4;

    [Header("Spawn")]
    public Transform spawnPoint;   // 敌方右侧
    public LayerMask enemyMask;
    public GameObject infantryPrefab, cavalryPrefab, magePrefab;
    public UnitConfig infantryCfg, cavalryCfg, mageCfg;

    float nextTick;
    float tickInterval = 2f;

    void Start()
    {
        InvokeRepeating(nameof(AddIncome), 1f, 1f);
    }

    void Update()
    {
        if (Time.time >= nextTick)
        {
            nextTick = Time.time + tickInterval;
            TryAutoSpawn();
        }
    }

    void AddIncome() { gold += incomePerSec; }

    void TryAutoSpawn()
    {
        // 基础权重
        float wInf = 0.4f, wCav = 0.35f, wMag = 0.25f;

        // 自适应：读玩家最近3次
        var (pi, pc, pm) = GameDirector.Instance.GetPlayerRecentCounts();
        if (pi >= 2)
        { /* 玩家步兵多 → 我出克制的法师 */
            wMag += 0.2f; float d = 0.2f; wInf -= d * 0.5f; wCav -= d * 0.5f;
        }
        if (pc >= 2)
        { /* 玩家骑兵多 → 我出步兵 */
            wInf += 0.2f; float d = 0.2f; wCav -= d * 0.5f; wMag -= d * 0.5f;
        }
        if (pm >= 2)
        { /* 玩家法师多 → 我出骑兵 */
            wCav += 0.2f; float d = 0.2f; wInf -= d * 0.5f; wMag -= d * 0.5f;
        }

        // 选型
        UnitConfig pick = WeightedPick(wInf, wCav, wMag);
        if (pick == null) return;
        if (gold < pick.cost) return;

        Spawn(pick);
    }

    UnitConfig WeightedPick(float a, float b, float c)
    {
        float sum = Mathf.Max(0.01f, a + b + c);
        float r = Random.value * sum;
        if (r < a) return infantryCfg;
        if (r < a + b) return cavalryCfg;
        return mageCfg;
    }

    void Spawn(UnitConfig cfg)
    {
        gold -= cfg.cost;
        GameObject prefab = cfg.type switch
        {
            UnitType.Infantry => infantryPrefab,
            UnitType.Cavalry => cavalryPrefab,
            UnitType.Mage => magePrefab,
            _ => infantryPrefab
        };
        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        var ub = go.GetComponent<UnitBase>();
        ub.config = cfg;
        ub.team = team;
        ub.enemyMask = enemyMask;
        ub.OnKilled += (dead) => GameDirector.Instance.OnUnitKilled(team, cfg.cost);
    }
    void SetupMasks(UnitBase ub)
    {
        if (ub.team == Team.Player)
            ub.enemyMask = LayerMask.GetMask("EnemyUnit", "EnemyBase");
        else
            ub.enemyMask = LayerMask.GetMask("PlayerUnit", "PlayerBase");
    }
}
