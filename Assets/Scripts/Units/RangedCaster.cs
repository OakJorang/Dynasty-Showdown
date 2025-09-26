using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RangedCaster : UnitBase
{
    public AudioClip sfxFireballCast;
    protected override void DoAttack()
    {
        if (!config.isRanged || !config.projectilePrefab || !target) return;

        var proj = Instantiate(config.projectilePrefab, transform.position, Quaternion.identity);
        var p = proj.GetComponent<Projectile>();
        p.Init(team, config.type, target, config.projectileSpeed,
               config.dps * config.attackInterval, config.splashRadius, enemyMask);
    }
    protected override void TryAttack()
    {
        if (Time.time - lastAttackTime < config.attackInterval) return;
        lastAttackTime = Time.time;

        GetComponent<UnitAnimatorBridge>()?.TriggerAttack(); // 仅播动画
    }
    public void DoAttack_Public() { if (sfxFireballCast) AudioManager.I.PlaySFX("fireball_cast", sfxFireballCast, 0.8f, 0.06f, 6, 0.03f); DoAttack(); }
}
