using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private float MAX_FLICKER_VOLUME = 0.8f;
    private float FLICKER_SOUND_RANGE = 20.0f;

    public bool flicker = true;
    public float flickerIntensity = 0.5f;

    private AudioSource flickerSound;

    private float initialIntensity = 0.3f;
    private float initialRange = 12.0f;
    private float intensityToRangeMultiplier;

    private float minIntensity;
    private float maxIntensity;
    private int direction = -1;
    private float flickerRate = 0.2f;

    private float targetIntensity;

    // Start is called before the first frame update
    void Start()
    {
        flickerSound = GetComponent<AudioSource>();

        initialIntensity = GetComponent<Light>().intensity;
        initialRange = GetComponent<Light>().range;

        intensityToRangeMultiplier = initialRange / initialIntensity;

        flickerSound.volume = 0.0f;

        SetValues();

        StartCoroutine(FlickerCoroutine());
    }

    public void ToggleFlicker(bool value)
    {
        flicker = value;
        if (flicker) StartCoroutine(FlickerCoroutine());
    }

    private void SetValues()
    {
        minIntensity = initialIntensity - ((initialIntensity - 0.05f * initialIntensity) * flickerIntensity);
        maxIntensity = initialIntensity * 1.2f;
    }

    private void SetLightValues(float intensity, float range)
    {
        GetComponent<Light>().intensity = intensity;
        GetComponent<Light>().range = range;
    }

    private IEnumerator FlickerCoroutine()
    {
        flickerSound.Play();
        flickerSound.volume = 0.0f;

        while (flicker)
        {
            if (GameManager.p_instance == null) yield return null;

            if (GameManager.p_instance.currentState == GameManager.GameState.playing)
            {
                float currentIntensity = GetComponent<Light>().intensity;
                
                float deltaIntensity = direction * flickerRate * Time.deltaTime;
                
                currentIntensity += deltaIntensity;

                if (direction == 1)
                {
                    if (currentIntensity > targetIntensity)
                    {
                        direction = -1;

                        currentIntensity = targetIntensity;

                        targetIntensity = minIntensity + (currentIntensity - minIntensity) * Random.Range(0.1f, 0.7f);
                    }
                } else
                {
                    if (currentIntensity < targetIntensity)
                    {
                        direction = 1;

                        currentIntensity = targetIntensity;

                        targetIntensity = currentIntensity + (initialIntensity - currentIntensity) * Random.Range(0.3f, 0.8f);
                    }
                }

                //Debug.Log("targetIntensity: " + targetIntensity);
                //Debug.Log("initialIntensity: " + initialIntensity);
                //Debug.Log("minIntensity: " + minIntensity);
                //Debug.Log("currentIntensity: " + currentIntensity);
                //Debug.Log("deltaIntensity: " + deltaIntensity);

                targetIntensity = Mathf.Clamp(targetIntensity, minIntensity, initialIntensity);

                float range = currentIntensity * intensityToRangeMultiplier;
                SetLightValues(currentIntensity, range);

                float distance = Vector3.Distance(transform.position, GameManager.p_instance.player.transform.position);

                float volume = 0.0f;

                if (distance < FLICKER_SOUND_RANGE)
                {
                    volume = (1 - (distance / FLICKER_SOUND_RANGE)) * (currentIntensity / maxIntensity) * MAX_FLICKER_VOLUME;
                    Mathf.Clamp(volume, 0, 1);
                }

                flickerSound.volume = volume;
            }

            yield return null;
        }

        yield break;
    }
}
