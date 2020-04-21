using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource source { get; private set; }
    private Coroutine coroutine;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play(float volume = 1.0f, bool restart = true)
    {
        source.volume = volume;
        if (restart) source.time = 0;
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }

    public float GetVolume()
    {
        return source.volume;
    }

    public void SetVolume(float value)
    {
        source.volume = value;
    }

    public void StartFade(float start = 0.0f, float target = 1.0f, float duration = 2.0f)
    {
        if (coroutine != null) StopCoroutine(coroutine);

        coroutine = StartCoroutine(SoundFadeCoroutine(start, target, duration));
    }

    private IEnumerator SoundFadeCoroutine(float start, float target, float duration)
    {
        if (!source.isPlaying)
        {
            source.time = 0;
            source.Play();
        }

        source.volume = start;

        int direction = 1;

        if (source.volume == target)
        {
            yield break;
        }
        else if (target < start)
        {
            direction = -1;
        }

        while (true)
        {
            source.volume += direction * (Time.deltaTime / duration);

            if (direction == 1 &&
                source.volume >= target)
            {
                source.volume = target;
                yield break;
            }
            else if (direction == -1 &&
                source.volume <= target)
            {
                source.volume = target;
                yield break;
            }

            yield return null;
        }
    }
}
