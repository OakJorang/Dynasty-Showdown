using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSfx : MonoBehaviour
{
    public AudioSource source;     // ÍÏ Audio_StartScene/UI_Source
    public AudioClip clip;         // ÍÏ°´Å¥µã»÷ÒôÐ§
    public float volume = 1f;

    void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(Play);
        if (!source) source = FindObjectOfType<AudioSource>(); // ¶µµ×
    }

    public void Play()
    {
        if (source && clip) source.PlayOneShot(clip, volume);
    }
}
