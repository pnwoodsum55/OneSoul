using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public const float INTERACT_DISTANCE = 1.0f;
    private const float HEART_MAX_WAIL_VOLUME = 0.6f;
    private const float HEART_WAIL_LOOP_RANGE = 40.0f;
    private const float ENEMY_MAX_WAIL_VOLUME = 1.0f;
    private const float ENEMY_WAIL_LOOP_RANGE = 15.0f;
    private const float MIN_VISUAL_EFFECT_RANGE = 10.0f;
    private const float VISUAL_EFFECT_RANGE = 55.0f;
    private const float MIN_GRAIN = 0.2f;
    private const float MAX_GRAIN = 1.0f;
    private const float MIN_CHROM_AB = 0.1f;
    private const float MAX_CHROM_AB = 1.0f;
    private const float MIN_VIGNETTE = 0.0f;
    private const float MAX_VIGNETTE = 0.4f;
    private const float MIN_LENS_DIST = 0.0f;
    private const float MAX_LENS_DIST = 50.0f;


    public Transform badEndingPointOne;
    public Transform badEndingPointTwo;
    public bool forceMovement = false;
    private Vector2 forcedMovementDirection = Vector2.zero;
    private bool looking = false;
    private bool forceCrouch = false;

    public BoxCollider chairCollider;
    public BoxCollider bigCollider;

    // Variables controlling the ring of the heartbeat sensor
    const int RING_VERTICES = 30;
    const float RING_WIDTH = .005f;
    const float MIN_RING_ALPHA = 0.05f;
    const float MAX_RING_ALPHA = 1.0f;
    const float MIN_RING_WIDTH = .0005f;
    const float MAX_RING_WIDTH = .005f;
    const float SENSOR_RANGE = 20.0f;
    const float FADE_SPEED = 1.0f;

    public Transform heart;
    public AudioClip heartbeatSound;
    public Animator heartbeatAnimator;
    public Transform cameraTransform;
    public GameObject targetObject;
    public LineRenderer lineRenderer;

    public Transform spawnTransform;
    public Transform hitParent;
    public PostProcessVolume volume;

    private float startSpeed;
    private float startInternalSpeed;
    private Grain grainEffect;
    private ChromaticAberration chromAbEffect;
    private Vignette vignetteEffect;
    private LensDistortion lensDistEffect;
    private List<GameObject> hits = new List<GameObject>();
    private GameObject hitObject;

    private List<Enemy> hitEnemies = new List<Enemy>();

    private Circle circle;
    private bool beating = false;
    private float beatTimer = 0.0f;
    private float beatDelay;
    private float beatDuration;
    private float currentRadius = 0.0f;
    private float maxRadius = 50.0f;

    private FirstPersonAIO firstPersonScript;

    private Vector3 deathPosition = new Vector3(0.0f, 0.0f, 0.0f);

    // Start is called before the first frame update
    public void OnStart()
    {
        firstPersonScript = GetComponent<FirstPersonAIO>();
        startInternalSpeed = firstPersonScript.walkSpeedInternal;
        startSpeed = firstPersonScript.speed;
        forceMovement = false;
        firstPersonScript.forcedMovement = false;
        TogglePlayerMovement(false);

        volume.profile.TryGetSettings(out grainEffect);
        volume.profile.TryGetSettings(out chromAbEffect);
        volume.profile.TryGetSettings(out vignetteEffect);
        volume.profile.TryGetSettings(out lensDistEffect);

        hitObject = Resources.Load<GameObject>("Prefabs/Hit");

        maxRadius = targetObject.GetComponent<RectTransform>().rect.width / 2;

        circle = new Circle(0.0f, RING_VERTICES);

        lineRenderer.loop = true;
        lineRenderer.positionCount = RING_VERTICES;
        lineRenderer.SetPositions(circle.GetPoints());
        lineRenderer.startWidth = RING_WIDTH;
        lineRenderer.endWidth = RING_WIDTH;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        lineRenderer.gameObject.SetActive(false);
    }

    public void Init()
    {
        currentRadius = 0.0f;
        beating = false;
        beatTimer = 0.0f;
        beatDelay = 1.4f;
        beatDuration = 0.9f;
        currentRadius = 0.0f;
        gameObject.SetActive(true);

        grainEffect.intensity.value = MIN_GRAIN;
        chromAbEffect.intensity.value = MIN_CHROM_AB;
        vignetteEffect.intensity.value = MIN_VIGNETTE;
        lensDistEffect.intensity.value = MIN_LENS_DIST;
        firstPersonScript.walkSpeedInternal = startInternalSpeed;
        firstPersonScript.speed = startSpeed;
        chairCollider.enabled = true;
        bigCollider.enabled = true;
        firstPersonScript.enabled = true;
        transform.position = spawnTransform.position;
        cameraTransform.rotation = Quaternion.Euler(0, 0, 0);
        transform.LookAt(spawnTransform.Find("Forward"), new Vector3(0.0f, 1.0f, 0.0f));
        firstPersonScript.cameraStartingPosition = cameraTransform.transform.localPosition;
        firstPersonScript.followAngles = firstPersonScript.targetAngles = transform.forward;
        firstPersonScript.originalRotation = transform.eulerAngles;
    }

    public void StartInGameCutsceneCoroutine()
    {
        TogglePlayerMovement(true);
        firstPersonScript.mouseSensitivity = 0.0f;
        firstPersonScript.speed = 0.5f;
        forceMovement = true;
        chairCollider.enabled = false;
        bigCollider.enabled = false;
        firstPersonScript.forcedMovement = true;
        
        StartCoroutine(InGameCutsceneCoroutine());
    }
    
    private IEnumerator InGameCutsceneCoroutine()
    {
        float distance = 100.0f;

        Vector3 lookAngle;
        Vector3 playerPosition;
        Vector3 targetPosition;

        while (distance > 0.3f)
        {
            playerPosition = transform.position;
            targetPosition = badEndingPointOne.position;

            targetPosition.y = playerPosition.y;

            //cameraTransform.rotation = Quaternion.Euler(0, 0, 0);
            transform.LookAt(targetPosition, new Vector3(0.0f, 1.0f, 0.0f));
            //transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), -90);
            firstPersonScript.followAngles = firstPersonScript.targetAngles = transform.forward;
            firstPersonScript.originalRotation = transform.eulerAngles;
            forcedMovementDirection = new Vector2(0.0f, 1.0f);
            distance = Vector3.Distance(playerPosition, targetPosition);
            yield return null;
        }


        forcedMovementDirection = Vector2.zero;
        
        looking = true;
        LeanTween.rotateAround(gameObject, Vector3.up, -90.0f, 1.5f).setOnComplete(() =>
        {
            looking = false;
        });

        float previousAngle = 120.0f;

        while (looking)
        {
            //transform.Rotate(Vector3.up, 90.0f);
            lookAngle = transform.forward;
            Vector3 pForward = transform.forward;
            Vector3 targetPos = badEndingPointTwo.position;
            targetPos.y = transform.position.y;
            Vector3 targetDirection = targetPos - transform.position;

            firstPersonScript.followAngles = firstPersonScript.targetAngles = lookAngle;
            firstPersonScript.originalRotation = transform.eulerAngles;

            float angle = Vector3.Angle(pForward, targetDirection);
            Debug.Log(angle);
            if (Vector3.Angle(pForward, targetDirection) < 0.5f ||
                angle > previousAngle)
            {
                looking = false;
                LeanTween.cancel(gameObject);
            }
            previousAngle = angle;
            yield return null;
        }


        distance = 100.0f;

        while (distance > 0.8f)
        {
            playerPosition = transform.position;
            targetPosition = badEndingPointTwo.position;

            targetPosition.y = playerPosition.y;

            cameraTransform.rotation = Quaternion.Euler(0, 0, 0);
            transform.LookAt(badEndingPointTwo, new Vector3(0.0f, 1.0f, 0.0f));
            //transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), 90);
            lookAngle = transform.forward;
            lookAngle.z = -10.0f;
            firstPersonScript.followAngles = firstPersonScript.targetAngles = lookAngle;
            firstPersonScript.originalRotation = transform.eulerAngles;
            forcedMovementDirection = new Vector2(0.0f, 1.0f);
            distance = Vector3.Distance(playerPosition, targetPosition);
            yield return null;
        }

        forcedMovementDirection = Vector2.zero;
        looking = true;
        LeanTween.rotateAround(gameObject, Vector3.up, 120.0f, 1.5f).setOnComplete(() =>
        {
            looking = false;
        });

        previousAngle = 120.0f;

        while (looking)
        {
            lookAngle = transform.forward;
            Vector3 pForward = transform.forward;
            Vector3 targetPos = heart.position;
            targetPos.y = transform.position.y;
            Vector3 targetDirection = targetPos - transform.position;

            firstPersonScript.followAngles = firstPersonScript.targetAngles = lookAngle;
            firstPersonScript.originalRotation = transform.eulerAngles;

            float angle = Vector3.Angle(pForward, targetDirection);
            Debug.Log(angle);
            if (Vector3.Angle(pForward, targetDirection) < 0.5f ||
                angle > previousAngle)
            {
                looking = false;
                LeanTween.cancel(gameObject);
            }
            previousAngle = angle;
            yield return null;
        }

        playerPosition = transform.position;
        targetPosition = heart.position;
        targetPosition.y = playerPosition.y;
        cameraTransform.rotation = Quaternion.Euler(0, 0, 0);
        transform.LookAt(heart, new Vector3(0.0f, 1.0f, 0.0f));
        lookAngle = transform.forward;
        firstPersonScript.followAngles = firstPersonScript.targetAngles = lookAngle;
        firstPersonScript.originalRotation = transform.eulerAngles;

        yield return new WaitForSeconds(0.5f);

        forceCrouch = true;

        yield return new WaitForSeconds(2.5f);

        GameManager.p_instance.StartBadEndingCutsceneCoroutine();
    }

    private void Update()
    {
        if (forceMovement)
        {
            firstPersonScript.ForceMoveUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (forceMovement)
        {
            firstPersonScript.ForceMoveFixedUpdate(forcedMovementDirection, forceCrouch);
        }
    }

    public void TogglePlayerMovement(bool value)
    {
        firstPersonScript.enableCameraMovement = value;
        firstPersonScript.playerCanMove = value;
    }

    public void EnemyCollision(Transform enemyTransform)
    {
        Vector3 currentDirection = transform.forward;
        Vector3 targetPos = new Vector3(enemyTransform.position.x, transform.position.y, enemyTransform.position.z);
        Quaternion currentRotation = transform.rotation;
        transform.LookAt(targetPos);
        Vector3 targetDirection = transform.forward;
        transform.rotation = currentRotation;
        float angle = Vector3.Angle(targetDirection, currentDirection);
        Debug.Log(angle);
        Vector3 playerRight = Vector3.Normalize(new Vector3(transform.right.x, 0, transform.right.z));

        TogglePlayerMovement(false);
        firstPersonScript.enabled = false;

        StartCoroutine(CubicRotateCoroutine(angle, 0.3f));
    }

    private IEnumerator CubicRotateCoroutine(float totalAngle, float duration)
    {
        deathPosition = transform.position;

        float timer = 0.0f;

        float deltaAngle = 0.0f;

        float previousAngle = 0.0f;

        float c = totalAngle / Mathf.Pow(duration, 3);

        while (timer < duration)
        {
            float currentAngle = c * Mathf.Pow(timer, 3);

            //deltaAngle = totalAngle * (Time.deltaTime / duration);

            deltaAngle = currentAngle - previousAngle;

            //Debug.Log(deltaAngle);

            previousAngle = currentAngle;

            transform.Rotate(Vector3.up, deltaAngle);

            timer += Time.deltaTime;

            firstPersonScript.followAngles = firstPersonScript.targetAngles = transform.forward;
            firstPersonScript.originalRotation = transform.eulerAngles;

            transform.position = deathPosition;

            yield return null;
        }

        GameManager.p_instance.StartEnding(GameManager.EndType.death);
    }

    public void KeepPlayerStatic()
    {
        transform.position = deathPosition;
    }

    // OnUpdate is called in the GameManager
    public void OnUpdate()
    {
        float heartDistance = Vector3.Distance(heart.position, transform.position);
        if (heartDistance < VISUAL_EFFECT_RANGE)
        {
            Mathf.Clamp(heartDistance, MIN_VISUAL_EFFECT_RANGE, VISUAL_EFFECT_RANGE);
            float ratio = 1 - ((heartDistance - MIN_VISUAL_EFFECT_RANGE) / (VISUAL_EFFECT_RANGE - MIN_VISUAL_EFFECT_RANGE));
            float value = ratio * (MAX_GRAIN - MIN_GRAIN) + MIN_GRAIN;
            grainEffect.intensity.value = value;
            value = ratio * (MAX_CHROM_AB - MIN_CHROM_AB) + MIN_CHROM_AB;
            chromAbEffect.intensity.value = value;
            value = ratio * (MAX_VIGNETTE - MIN_VIGNETTE) + MIN_VIGNETTE;
            vignetteEffect.intensity.value = value;
            value = ratio * (MAX_LENS_DIST - MIN_LENS_DIST) + MIN_LENS_DIST;
            lensDistEffect.intensity.value = value;
        }

        if (!beating)
        {
            beatTimer += Time.deltaTime;

            if (beatTimer > beatDelay)
            {
                beatTimer = 0.0f;

                GetComponent<AudioSource>().PlayOneShot(heartbeatSound, 0.5f);

                heartbeatAnimator.Play("heartbeat", -1, 0.25f);

                hitEnemies = new List<Enemy>();

                StartCoroutine(BeatCoroutine());
            }
        }

        for (int i = hits.Count - 1; i >= 0; i--)
        {
            float alpha = hits[i].GetComponent<Image>().color.a;
            alpha -= FADE_SPEED* Time.deltaTime;
            if (alpha <= 0.05)
            {
                Destroy(hits[i]);
                hits.Remove(hits[i]);
            } 
            else
            {
                Color color = hits[i].GetComponent<Image>().color;
                color.a = alpha;
                hits[i].GetComponent<Image>().color = color;
            }
        }

        // Handle the current volume of the wailing
        float distance = Vector3.Distance(transform.position, heart.position);
        float wailVolume = 0.0f;

        if (distance < HEART_WAIL_LOOP_RANGE)
        {
            float ratio = 1 - (distance / HEART_WAIL_LOOP_RANGE);

            Mathf.Clamp(ratio, 0, 1);

            //Debug.Log(ratio);

            wailVolume = ratio * HEART_MAX_WAIL_VOLUME;
        }
        else
        {
            foreach (Enemy enemy in GameManager.p_instance.enemies)
            {
                distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < ENEMY_WAIL_LOOP_RANGE)
                {
                    float ratio = 1 - (distance / ENEMY_WAIL_LOOP_RANGE);

                    Mathf.Clamp(ratio, 0, 1);


                    wailVolume = ratio * ENEMY_MAX_WAIL_VOLUME;
                }
            }
        }

        GameManager.p_instance.wailLoop.SetVolume(wailVolume);
    }

    private IEnumerator BeatCoroutine()
    {
        lineRenderer.gameObject.SetActive(true);
        currentRadius = 1.0f;
        beating = true;
        while (true)
        {
            currentRadius += Time.deltaTime / beatDuration * maxRadius;

            if (currentRadius >= maxRadius)
            {
                lineRenderer.gameObject.SetActive(false);
                beating = false;
                yield break;
            }

            foreach (Enemy enemy in GameManager.p_instance.enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < (currentRadius / maxRadius) * SENSOR_RANGE &&
                    !hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);

                    GameObject newHit = Instantiate(hitObject, hitParent);

                    Vector3 enemyDir = enemy.transform.position - transform.position;
                    enemyDir.y = 0;
                    Vector3.Normalize(enemyDir);
                    Vector3 playerForward = Vector3.Normalize (new Vector3(transform.forward.x, 0, transform.forward.z));
                    Vector3 playerRight = Vector3.Normalize(new Vector3(transform.right.x, 0, transform.right.z));
                    
                    float angle = Vector3.Angle(playerForward, enemyDir);

                    if (Vector3.Angle(playerRight, enemyDir) < 90)
                    {
                        angle = 360 - angle;
                    }

                    Vector3 forwardDir = new Vector3(0, 1.0f, 0);

                    Quaternion quaternion = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1.0f));

                    Vector3 hitDir = quaternion * forwardDir;

                    Vector3 hitPos = (distance / SENSOR_RANGE) * maxRadius * hitDir;

                    newHit.GetComponent<RectTransform>().localPosition = hitPos;

                    hits.Add(newHit);
                }
            }
            
            UpdateRing();

            yield return null;
        }
    }

    private void UpdateRing()
    {
        circle.SetRadius(currentRadius);

        float inverseRadius = (maxRadius - currentRadius) / maxRadius;

        float alpha = inverseRadius * (MAX_RING_ALPHA - MIN_RING_ALPHA) + MIN_RING_ALPHA;
        lineRenderer.startColor = new Color(1.0f, 1.0f, 1.0f, alpha);
        lineRenderer.endColor = new Color(1.0f, 1.0f, 1.0f, alpha);

        float width = inverseRadius * (MAX_RING_WIDTH - MIN_RING_WIDTH) + MIN_RING_WIDTH;

        lineRenderer.SetPositions(circle.GetPoints());
    }
}
