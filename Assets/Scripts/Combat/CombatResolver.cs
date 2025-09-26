using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CombatResolver
{
    // 根据受击者类型决定乘数（这里用“攻击者 vs 受击者类型”）
    public static float GetCounterMultiplier(UnitConfig defenderCfg, UnitType attackerType, UnitBase defender)
    {
        switch (defenderCfg.type)
        {
            case UnitType.Infantry: // 法师克步兵
                return attackerType == UnitType.Mage ? 1.4f : 1f;
            case UnitType.Cavalry: // 步兵克骑兵
                return attackerType == UnitType.Infantry ? 1.5f : 1f;
            case UnitType.Mage: // 骑兵克法师
                return attackerType == UnitType.Cavalry ? 1.5f : 1f;
            default: return 1f;
        }
    }
}
