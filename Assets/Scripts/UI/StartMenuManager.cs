using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnStart;
    public Button btnQuit;

    [Header("Instruction")]
    public CanvasGroup instructionGroup;   // InstructionPanel 的 CanvasGroup
    public TMP_Text instructionText;
    public float showDuration = 2.0f;      // 说明停留时间
    public float fadeTime = 0.8f;          // 淡出时长
    public string gameSceneName = "SampleScene";

    void Awake()
    {
        if (btnStart) btnStart.onClick.AddListener(OnStartClicked);
        if (btnQuit) btnQuit.onClick.AddListener(OnQuitClicked);
        if (instructionGroup)
        {
            instructionGroup.alpha = 0f;
            instructionGroup.blocksRaycasts = false;
            instructionGroup.interactable = false;
        }
    }

    void OnStartClicked()
    {
        StartCoroutine(ShowInstructionThenLoad());
    }

    IEnumerator ShowInstructionThenLoad()
    {
        // 说明内容（可直接在 Inspector 里写，就不用这里赋值）
        // if (instructionText) instructionText.text = "...";

        // 先淡入说明
        yield return Fade(instructionGroup, 0f, 1f, 0.3f);
        yield return new WaitForSeconds(showDuration);
        // 再淡出说明
        yield return Fade(instructionGroup, 1f, 0f, fadeTime);

        SceneManager.LoadScene(gameSceneName);
    }

    IEnumerator Fade(CanvasGroup g, float a, float b, float t)
    {
        if (!g) yield break;
        g.blocksRaycasts = true; g.interactable = true;
        float time = 0f;
        while (time < t)
        {
            time += Time.deltaTime;
            g.alpha = Mathf.Lerp(a, b, time / t);
            yield return null;
        }
        g.alpha = b;
        if (b == 0f) { g.blocksRaycasts = false; g.interactable = false; }
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
