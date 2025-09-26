using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // 初始化参数（与 RangedCaster.Init 调用一一对应）
    Team team;                    // 发射者阵营
    UnitType attackerType;        // 发射者兵种类型（用于克制倍率）
    Transform target;             // 追踪目标（单位或基地）
    float speed = 7f;             // 飞行速度
    float damage = 10f;           // 这颗弹体造成的伤害（你传的是 dps * 攻速）
    float splashRadius = 0f;      // 0 表示无溅射
    LayerMask enemyMask;          // 只打这些层（对方单位+对方基地）

    // 组件
    Rigidbody2D rb;
    Animator anim;
    Collider2D col;
    SpriteRenderer sr;

    // 运行时
    bool exploded = false;
    float life = 0f;
    public float maxLife = 4f;    // 保险寿命（避免永远飞）
    public float hitDistance = 0.18f; // 贴近多少算命中（对“只追 transform”的情况）

    public void Init(
        Team team, UnitType attackerType, Transform target,
        float speed, float damage, float splashRadius, LayerMask enemyMask)
    {
        this.team = team;
        this.attackerType = attackerType;
        this.target = target;
        this.speed = speed;
        this.damage = damage;
        this.splashRadius = splashRadius;
        this.enemyMask = enemyMask;

        // 初始朝向（翻转）
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr && target != null) sr.flipX = (target.position.x < transform.position.x);

        // 运动由 Update 逐帧朝目标推进（轻度“追踪”效果）
        if (rb != null) rb.isKinematic = true; // 我们用 transform 推进，也可改为 rb.velocity
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (col) col.isTrigger = true; // 触发器命中
    }

    void Update()
    {
        if (exploded) return;

        life += Time.deltaTime;
        if (life >= maxLife) { Explode(); return; }

        // 目标丢失：直接爆炸/销毁（也可选择继续直线飞）
        if (target == null) { Explode(); return; }

        // 朝目标推进（追踪）
        Vector3 dir = (target.position - transform.position);
        float dist = dir.magnitude;
        if (dist < hitDistance) { OnHitTarget(target); return; }

        Vector3 step = dir.normalized * speed * Time.deltaTime;
        transform.position += step;

        // 动态翻转朝向（可选）
        if (sr) sr.flipX = (step.x < 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded) return;

        // 只对敌对层生效
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;

        // 单位命中
        var ub = other.GetComponentInParent<UnitBase>();
        if (ub != null && ub.team != team)
        {
            OnHitUnit(ub);
            return;
        }
        // 基地命中
        var bb = other.GetComponentInParent<BaseBuilding>();
        if (bb != null && bb.team != team)
        {
            OnHitBase(bb);
            return;
        }
    }

    void OnHitTarget(Transform t)
    {
        var ub = t.GetComponentInParent<UnitBase>();
        if (ub != null && ub.team != team) { OnHitUnit(ub); return; }

        var bb = t.GetComponentInParent<BaseBuilding>();
        if (bb != null && bb.team != team) { OnHitBase(bb); return; }

        // 目标恰好不是敌方（或层不在 mask）→ 直接爆炸
        Explode();
    }

    void OnHitUnit(UnitBase ub)
    {
        ub.TakeDamage(damage, attackerType);
        DoSplash();
        Explode();
    }

    void OnHitBase(BaseBuilding bb)
    {
        bb.TakeDamage(damage);
        DoSplash();
        Explode();
    }

    void DoSplash()
    {
        if (splashRadius <= 0f) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, splashRadius, enemyMask);
        foreach (var h in hits)
        {
            var u = h.GetComponentInParent<UnitBase>();
            if (u != null && u.team != team)
                u.TakeDamage(damage, attackerType);

            var b = h.GetComponentInParent<BaseBuilding>();
            if (b != null && b.team != team)
                b.TakeDamage(damage);
        }
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (col) col.enabled = false;
        // 停止移动
        if (rb && !rb.isKinematic) rb.velocity = Vector2.zero;

        // 触发爆炸动画（控制器里 Any State → Explode/Hit 触发）
        if (anim) anim.SetTrigger("Hit");
        else Destroy(gameObject);
    }

    // 绑定到“爆炸”动画最后一帧的 Animation Event
    public void Anim_ExplodeEnd()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (splashRadius > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }
}
