using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageController : MonoBehaviour
{
    public TextMeshProUGUI textMesh { get; private set; }
    public float holdTime = 5.0f;
    public bool active = false;

    public void FadeInOut(float fadeDuration)
    {
        if (!textMesh) textMesh = GetComponent<TextMeshProUGUI>();
        if (gameObject.activeSelf) return;
        gameObject.SetActive(true);
        Stop();
        StartCoroutine(FadeInOutCoroutine(fadeDuration));
    }

    public void FadeIn(float fadeDuration)
    {
        gameObject.SetActive(true);
        if (!textMesh) textMesh = GetComponent<TextMeshProUGUI>();
        Stop();
        active = true;
        textMesh.alpha = 0.0f;
        StartCoroutine(FadeInCoroutine(fadeDuration, 1.0f));
    }

    public void FadeOut(float fadeDuration)
    {
        if (!textMesh) textMesh = GetComponent<TextMeshProUGUI>();
        if (!gameObject.activeSelf) return;
        else if (textMesh.alpha <= 0)
        {
            textMesh.alpha = 0;
            gameObject.SetActive(false);
            return;
        }

        Stop();

        float currentAlpha = textMesh.alpha;
        StartCoroutine(FadeOutCoroutine(fadeDuration, currentAlpha));
    }

    public void Stop()
    {
        StopCoroutine("FadeIn");
        StopCoroutine("FadeOut");
    }

    private IEnumerator FadeInCoroutine(float fadeDuration, float alphaChange)
    {
        while (true)
        {
            textMesh.alpha += alphaChange * (Time.deltaTime / fadeDuration);
            //Debug.Log(textMesh.alpha);
            if (textMesh.alpha >= 1.0f)
            {
                textMesh.alpha = 1.0f;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator FadeOutCoroutine(float fadeDuration, float alphaChange)
    {
        while (true)
        {
            textMesh.alpha -= alphaChange * (Time.deltaTime / fadeDuration);

            if (textMesh.alpha <= 0.0f)
            {
                textMesh.alpha = 0.0f;
                gameObject.SetActive(false);
                active = false;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator FadeInOutCoroutine(float fadeDuration)
    {
        FadeIn(fadeDuration);
        yield return new WaitForSeconds(fadeDuration + holdTime);
        FadeOut(fadeDuration);
    }
}
