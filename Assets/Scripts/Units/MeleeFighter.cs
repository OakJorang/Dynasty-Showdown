using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MeleeFighter : UnitBase
{
    public AudioClip sfxMelee;
    protected override void DoAttack()
    {
        if (sfxMelee) AudioManager.I.PlaySFX("melee_hit", sfxMelee, 0.85f, 0.10f, 5, 0.04f);
        if (!target) return;

        var ub = target.GetComponentInParent<UnitBase>();
        if (ub != null && ub.team != team)
        {
            bool charge = IsChargeWindow(); // 如有
            ub.TakeDamage(config.dps, config.type, charge);
            firstEngage = false;
            return;
        }

        var bb = target.GetComponentInParent<BaseBuilding>();
        if (bb != null && bb.team != team)
        {
            bb.TakeDamage(config.dps);
            firstEngage = false;
            return;
        }
    }
    protected override void TryAttack()
    {
        if (Time.time - lastAttackTime < config.attackInterval) return;
        lastAttackTime = Time.time;

        GetComponent<UnitAnimatorBridge>()?.TriggerAttack(); // 仅播动画
        firstEngage = false; // 你的冲锋窗口逻辑
    }
    public void DoAttack_Public() { DoAttack(); }
}
