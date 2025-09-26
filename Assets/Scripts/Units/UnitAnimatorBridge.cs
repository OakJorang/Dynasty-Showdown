using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimatorBridge : MonoBehaviour
{
    Animator anim;
    UnitBase ent;
    SpriteRenderer sr;
    MeleeFighter melee;      // 近战（步兵/骑兵）
    RangedCaster ranged;     // 远程（法师）

    static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    static readonly int HashAttack = Animator.StringToHash("Attack");

    void Awake()
    {
        anim = GetComponent<Animator>();
        ent = GetComponent<UnitBase>();
        sr = GetComponentInChildren<SpriteRenderer>();
        melee = GetComponent<MeleeFighter>();
        ranged = GetComponent<RangedCaster>();

        // 敌人朝左（可选）
        if (sr != null && ent != null)
            sr.flipX = (ent.team == Team.Enemy);
    }

    void Update()
    {
        if (!anim || ent == null) return;

        // 没目标 或 目标不在射程内 → 视为在移动
        bool moving = true;
        if (ent.HasTarget) moving = ent.DistanceToTarget > ent.AttackRange;

        anim.SetBool(HashIsMoving, moving);
    }

    // 攻击时仅触发动画，由动画事件再结算
    public void TriggerAttack() => anim?.SetTrigger(HashAttack);

    // ------- 动画事件回调（Animation Event 里选它们） -------
    public void Anim_AttackHit() { melee?.DoAttack_Public(); }  // 近战命中帧
    public void Anim_Shoot() { ranged?.DoAttack_Public(); }  // 远程发射帧
}
