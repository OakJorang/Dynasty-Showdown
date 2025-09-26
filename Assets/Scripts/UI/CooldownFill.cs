using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownFill : MonoBehaviour
{
    public Image fillMask;   // Ò»¸ö Image£¬Type=Filled, FillMethod=Radial »ò Vertical
    Button btn;

    void Awake() { btn = GetComponent<Button>(); }

    public void SetRemaining(float remain, float total)
    {
        if (!fillMask) return;
        fillMask.fillAmount = Mathf.Clamp01(remain / Mathf.Max(0.001f, total));
    }

    public void SetInteractable(bool v) { if (btn) btn.interactable = v; }
}
