using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Background (always same)")]
    public Image bgImage;        // 拖 Canvas/BG (Image)
    public Sprite commonBG;      // 拖 Main.jpg

    [Header("Result Overlay (win / lose)")]
    public Image resultImage;    // 拖 Canvas/ResultImage (Image)
    public Sprite winSprite;     // 拖 win.png
    public Sprite loseSprite;    // 拖 lose.png

    [Header("Buttons")]
    public Button retryBtn;      // 返回标题
    public Button quitBtn;       // 退出

    [Header("Audio (optional)")]
    public AudioSource sfxSource;   // 可留空，脚本会自动 Add
    public AudioClip winSfx;
    public AudioClip loseSfx;
    public AudioClip clickSfx;

    void Awake()
    {
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // 1) 固定背景
        if (bgImage && commonBG) bgImage.sprite = commonBG;

        // 2) 根据胜负切前景徽章 + 音效
        bool win = GameDirector.isPlayerWin;
        if (resultImage)
            resultImage.sprite = win ? winSprite : loseSprite;

        PlayOnce(win ? winSfx : loseSfx, 1f);

        // 3) 按钮
        if (retryBtn) retryBtn.onClick.AddListener(OnRetry);
        if (quitBtn) quitBtn.onClick.AddListener(OnQuit);

        // 4) （可选）做个淡入更有仪式感
        TryFadeIn(resultImage, 0.45f);
    }

    void PlayOnce(AudioClip clip, float vol = 1f)
    {
        if (clip) sfxSource.PlayOneShot(clip, vol);
    }

    void OnRetry()
    {
        PlayOnce(clickSfx, 0.8f);
        SceneManager.LoadScene("StartScene"); // 你的标题场景名
    }

    void OnQuit()
    {
        PlayOnce(clickSfx, 0.8f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 可选淡入
    void TryFadeIn(Graphic g, float dur)
    {
        if (!g) return;
        var cg = g.GetComponent<CanvasGroup>();
        if (!cg) cg = g.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        g.StartCoroutine(FadeRoutine(cg, dur));
    }

    System.Collections.IEnumerator FadeRoutine(CanvasGroup cg, float dur)
    {
        float t = 0;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, t / dur);
            yield return null;
        }
        cg.alpha = 1f;
    }
}
