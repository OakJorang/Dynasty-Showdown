using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnitBase : MonoBehaviour
{
    public UnitConfig config;
    public Team team;
    public float hp;
    BaseBuilding EnemyBase => (team == Team.Player) ? BaseBuilding.EnemyBase : BaseBuilding.PlayerBase;
    protected float lastAttackTime;
    protected bool isDead;
    protected bool firstEngage = true; // 骑兵冲锋窗口
    protected Transform target;        // 当前锁定目标
    public System.Action<UnitBase> OnKilled;
    public Transform Target => target;
    public bool HasTarget => target != null;
    public float DistanceToTarget =>
        target ? Vector2.Distance(transform.position, target.position) : float.PositiveInfinity;
    public float AttackRange => config.attackRange;

    [Header("Runtime")]
    public LayerMask enemyMask;        // 设成对方单位与基地层
    public LayerMask groundMask;       // 可选

    protected virtual void Awake()
    {
        hp = config.maxHP;
        if (team == 0) team = config.defaultTeam;
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (config.sprite && sr) sr.sprite = config.sprite;
        enemyMask = (team == Team.Player)
            ? LayerMask.GetMask("EnemyUnit", "EnemyBase")
            : LayerMask.GetMask("PlayerUnit", "PlayerBase");
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // 1) 寻敌（前方小圆或扇形）
        if (target == null || !IsValidTarget(target))
            target = FindFrontTarget();

        // 2) 有目标→进攻；否则向前移动
        if (target)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= config.attackRange) TryAttack();
            else MoveForward();
        }
        else
        {
            MoveForward();
        }
        if (target == null && EnemyBase && EnemyBase.col)
        {
            float distToBaseFront = DistanceToBaseFront(EnemyBase.col);
            if (distToBaseFront <= config.attackRange)
                target = EnemyBase.transform;
        }

        // —— 计算“距离基地前沿”的函数 —— 
        float DistanceToBaseFront(Collider2D baseCol)
        {
            var b = baseCol.bounds;
            // 我方向右，敌方基地在右侧：到“左侧边缘”的水平距离；反之亦然
            float dx = (team == Team.Player) ? (b.min.x - transform.position.x)
                                             : (transform.position.x - b.max.x);
            return Mathf.Max(0f, dx); // 小于0 视为已贴边
        }
    }

    protected virtual void MoveForward()
    {
        float dir = team == Team.Player ? +1f : -1f;

        // 临近敌方基地前沿时钳位，不再前进
        if (EnemyBase && EnemyBase.col)
        {
            var b = EnemyBase.col.bounds;
            float frontX = (team == Team.Player) ? b.min.x : b.max.x; // 敌方基地正面 x
            float stopX = frontX - dir * config.attackRange;         // 停在射程边缘

            // 如果下一帧前进会跨过停点，直接把位置钳到停点
            float nextX = transform.position.x + dir * config.moveSpeed * Time.deltaTime;
            if ((dir > 0 && nextX >= stopX) || (dir < 0 && nextX <= stopX))
            {
                transform.position = new Vector3(stopX, transform.position.y, transform.position.z);
                return;
            }
        }

        transform.Translate(Vector2.right * dir * config.moveSpeed * Time.deltaTime);
    }

    protected virtual Transform FindFrontTarget()
    {
        float dir = team == Team.Player ? +1f : -1f;
        Vector2 origin = (Vector2)transform.position + new Vector2(dir * 0.6f, 0);
        float radius = Mathf.Max(1.2f, config.attackRange + 0.2f);
        var hits = Physics2D.OverlapCircleAll(origin, radius, enemyMask);
        float best = Mathf.Infinity;
        Transform bestT = null;
        foreach (var h in hits)
        {
            if (!IsValidTarget(h.transform)) continue;
            float dx = Mathf.Abs(h.transform.position.x - transform.position.x);
            if (dx < best)
            {
                best = dx; bestT = h.transform;
            }
        }
        return bestT;
    }

    protected bool IsValidTarget(Transform t)
    {
        if (t == null || !t.gameObject.activeInHierarchy) return false;

        var ub = t.GetComponentInParent<UnitBase>();       // ✅ 用 InParent
        if (ub != null) return !ub.isDead && ub.team != this.team;

        var bb = t.GetComponentInParent<BaseBuilding>();   // ✅ 用 InParent
        if (bb != null) return !bb.IsDead && bb.team != this.team;

        return false;
    }

    protected virtual void TryAttack()
    {
        if (Time.time - lastAttackTime < config.attackInterval) return;
        lastAttackTime = Time.time;
        DoAttack();
        firstEngage = false; // 一旦开始攻击，冲锋窗口关闭
    }

    protected virtual void DoAttack() { /* 由子类实现 */ }

    public virtual void TakeDamage(float amount, UnitType attackerType, bool isCharge = false)
    {
        if (isDead) return;
        float mult = CombatResolver.GetCounterMultiplier(config, attackerType, this);
        float final = amount * mult * (isCharge ? config.chargeMultiplier : 1f);
        hp -= final;
        if (hp <= 0f) Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        OnKilled?.Invoke(this);
        Destroy(gameObject);
    }

    public bool IsChargeWindow() => config.hasCharge && firstEngage;

    Vector2 GetFrontOrigin()
    {
        float dir = team == Team.Player ? +1f : -1f;
        return (Vector2)transform.position + new Vector2(dir * 0.6f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        // 只有选中对象时画，避免太乱
        // 1) 前方寻敌圆
        Vector2 origin = GetFrontOrigin();
        float detectRadius = Mathf.Max(1.2f, config.attackRange + 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, detectRadius);

        // 2) 攻击距离（从自身中心画一个半径圈，便于感知）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, config.attackRange);

        // 3) 正前方射线（看方向是否正确）
        float dir = team == Team.Player ? +1f : -1f;
        Vector3 to = (Vector2)transform.position + new Vector2(dir * (config.attackRange + 0.6f), 0f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, to);

#if UNITY_EDITOR
        // 4) 文字标注（可选）
        Handles.color = Color.white;
        Handles.Label(transform.position + Vector3.up * 1.0f,
            $"Mask:{enemyMask.value}\nRange:{config.attackRange:0.00}");
#endif
    }
    static string MaskToLayerNames(int mask)
    {
        var names = new List<string>(); // 如果你喜欢全名：new System.Collections.Generic.List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                string n = LayerMask.LayerToName(i);
                names.Add(string.IsNullOrEmpty(n) ? ("#" + i) : $"{n}(#{i})");
            }
        }
        return string.Join(", ", names);
    }

    // 在 Inspector 里点击三点/齿轮菜单可看到这个项
    [ContextMenu("DEBUG: Print My Enemy Mask")]
    void DebugPrintEnemyMask()
    {
        Debug.Log($"[{name}] enemyMask={enemyMask.value} => {MaskToLayerNames(enemyMask.value)}");
        Debug.Log($"Layer idx: EnemyUnit={LayerMask.NameToLayer("EnemyUnit")}, EnemyBase={LayerMask.NameToLayer("EnemyBase")}");
    }
}

