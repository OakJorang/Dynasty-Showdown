using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;

    [Header("Mixer Routing (optional)")]
    public AudioMixerGroup sfxGroup;

    [Header("Pool")]
    public int poolSize = 12;

    private readonly Queue<AudioSource> idle = new();
    private readonly HashSet<AudioSource> busy = new();

    // Rate limit & concurrency
    private readonly Dictionary<string, float> lastPlay = new();
    private readonly Dictionary<string, int> playingCount = new();

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // build pool
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SFX_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f; // 2D
            if (sfxGroup) src.outputAudioMixerGroup = sfxGroup;
            idle.Enqueue(src);
        }
    }

    void Update()
    {
        // recycle finished sources
        if (busy.Count == 0) return;
        var done = new List<AudioSource>();
        foreach (var src in busy)
            if (!src.isPlaying) done.Add(src);

        foreach (var src in done)
        {
            busy.Remove(src);
            idle.Enqueue(src);
        }
    }

    /// <summary>
    /// 播放短促 SFX（带限频与并发限制）
    /// </summary>
    public void PlaySFX(string key, AudioClip clip, float vol = 1f,
                        float minInterval = 0.08f, int maxSimul = 4,
                        float pitchJitter = 0.04f)
    {
        if (clip == null) return;

        // rate limit
        if (lastPlay.TryGetValue(key, out float t))
        {
            if (Time.time - t < minInterval) return;
        }

        // polyphony limit
        playingCount.TryGetValue(key, out int count);
        if (count >= maxSimul) return;

        if (!TryGetSource(out var src)) return;

        // slight random pitch
        src.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        src.volume = vol;
        src.clip = clip;

        src.Play();

        lastPlay[key] = Time.time;
        playingCount[key] = count + 1;
        StartCoroutine(DecAfter(src, key, clip.length));
    }

    System.Collections.IEnumerator DecAfter(AudioSource src, string key, float len)
    {
        busy.Add(src);
        yield return new WaitForSeconds(len);
        playingCount[key] = Mathf.Max(0, playingCount[key] - 1);
    }

    bool TryGetSource(out AudioSource src)
    {
        if (idle.Count > 0)
        {
            src = idle.Dequeue();
            return true;
        }
        src = null;
        return false;
    }
}
