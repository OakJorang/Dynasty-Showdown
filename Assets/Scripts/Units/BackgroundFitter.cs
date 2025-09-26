using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFitter : MonoBehaviour
{
    public bool preserveAspect = false; // 关=拉伸填满；开=等比裁剪
    void Start()
    {
        var cam = Camera.main;
        var sr = GetComponent<SpriteRenderer>();
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        var size = sr.sprite.bounds.size;

        if (preserveAspect)
        {
            float s = Mathf.Max(w / size.x, h / size.y); // 等比放大覆盖，可能裁边
            transform.localScale = new Vector3(s, s, 1);
        }
        else
        {
            transform.localScale = new Vector3(w / size.x, h / size.y, 1); // 直接拉伸填满
        }
    }
}
