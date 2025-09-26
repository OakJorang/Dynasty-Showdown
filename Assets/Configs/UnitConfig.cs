using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Unit Config", fileName = "UnitConfig")]
public class UnitConfig : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public UnitType type;
    public Team defaultTeam = Team.Player;
    public Sprite sprite;

    [Header("Economy")]
    public int cost = 50;
    public float cooldown = 3f;

    [Header("Stats")]
    public float maxHP = 220f;
    public float dps = 22f;         // 每秒伤害（近战按频率转化）
    public float attackInterval = 0.6f; // 每次攻击间隔
    public float attackRange = 1.2f;    // 近战=短，法师=长
    public float moveSpeed = 1.0f;

    [Header("Counter Multipliers (vs target type)")]
    public float vsInfantry = 1.0f;
    public float vsCavalry = 1.0f;
    public float vsMage = 1.0f;

    [Header("Cavalry Charge")]
    public bool hasCharge = false;
    public float chargeWindow = 1.0f;
    public float chargeMultiplier = 1.6f;

    [Header("Ranged / Projectile")]
    public bool isRanged = false;
    public GameObject projectilePrefab;   // 仅法师
    public float projectileSpeed = 7f;
    public float splashRadius = 0f;       // 法师设为 >0 即有溅射
}
