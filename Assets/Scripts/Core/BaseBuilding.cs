using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding : MonoBehaviour
{
    public static BaseBuilding PlayerBase, EnemyBase;
    public Team team;
    public float maxHP = 1500f;
    public float hp;
    public bool IsDead => hp <= 0f;
    public System.Action<BaseBuilding> OnBaseDestroyed;
    public Collider2D col;

    void Awake() {
        // 运行时给满血（防止 0 被判已死）
        if (hp <= 0f) hp = maxHP;

        // 没拖引用就自动找：先本体，再子物体
        if (col == null)
            col = GetComponent<Collider2D>();
        if (col == null)
            col = GetComponentInChildren<Collider2D>();

        if (col == null)
            Debug.LogError($"[BaseBuilding] {name} 没有 Collider2D，请添加 BoxCollider2D 并勾 IsTrigger。");
        else
            col.isTrigger = true; // 保底
    }

    public void TakeDamage(float dmg)
    {
        if (IsDead) return;

        hp -= dmg;   // 这里改成用 dmg，而不是 amount

        if (hp <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        // TODO: 播放爆炸/结算
        gameObject.SetActive(false);
        if (team == Team.Player)
        {
            // 玩家基地死了 → 失败
            GameDirector.isPlayerWin = false;
        }
        else
        {
            // 敌方基地死了 → 胜利
            GameDirector.isPlayerWin = true;
        }

        // 跳转到结束页面
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");

        gameObject.SetActive(false);
    }
}
