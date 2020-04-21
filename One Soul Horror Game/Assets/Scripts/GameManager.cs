using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager p_instance { get; private set; } = null;

    public GameState currentState { get; private set; } = GameState.start;

    public GameObject fadeCover;
    public GameObject sensorObj;

    public Transform journalParentTransform;
    public Transform offscreenCanvasTransform;
    public Transform heartMessagesTransform;
    public Animator doorAnimator;
    public Animator leverAnimator;

    public GameObject menuCamera;
    public GameObject menuCanvas;
    public GameObject mainMenuCanvas;
    public GameObject instructionsMenuCanvas;
    public MessageManager introMessages;

    public GameObject openDoorColliderObj;

    public Transform spawnTransform;
    public Player player;
    public GameObject hudCanvas;

    public VideoPlayer deathVideo;
    public VideoPlayer badEndingVideo;
    public VideoPlayer deathVideoSecondary;
    public VideoPlayer goodEndingVideo;
    public CollectibleManager collectibleManager;
    public List<Enemy> enemies;
    public BoxCollider badEndingCollider;
    public GameObject goodEndingLever;
    public MessageController badEndingMessage;

    public Transform audioTransform;

    private GameObject mainObj;
    private GameObject introObj;

    public GameObject pausedObj { get; private set; }
    private GameObject endingObj;
    private GameObject gameOverObj;
    private MessageController pickupMessage;
    private MessageController interactMessage;
    private MessageController journalEntryMessage;

    private AudioController mainMenuLoop;
    private AudioController introLoop;
    private AudioController ambienceLoop;
    public AudioController wailLoop { get; private set; }
    public AudioController climaxLoop { get; private set; }
    private AudioController deathSound;
    private AudioController badEndingSound;
    private AudioController goodEndingSound;

    private EndType currentEnding = EndType.death;

    public enum EndType
    {
        death,
        badEnding,
        goodEnding
    }

    public enum GameState
    {
        start,
        mainMenu,
        intro,
        playing,
        paused,
        ending,
        gameOver
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize GameManger singleton instance
        if (p_instance == null)
        {
            p_instance = this;
        }
        else
        {
            Destroy(this);
        }

        // Initialize all references to objects in the scene
        mainObj = menuCanvas.transform.Find("Main").gameObject;
        introObj = menuCanvas.transform.Find("Intro").gameObject;

        pickupMessage = hudCanvas.transform.Find("PickupMessage").GetComponent<MessageController>();
        interactMessage = hudCanvas.transform.Find("InteractMessage").GetComponent<MessageController>();
        journalEntryMessage = hudCanvas.transform.Find("JournalEntryMessage").GetComponent<MessageController>();
        pausedObj = hudCanvas.transform.Find("Paused").gameObject;
        endingObj = hudCanvas.transform.Find("Ending").gameObject;
        gameOverObj = menuCanvas.transform.Find("GameOver").gameObject;

        mainMenuLoop = audioTransform.Find("MainMenuLoop").GetComponent<AudioController>();
        introLoop = audioTransform.Find("IntroLoop").GetComponent<AudioController>();
        ambienceLoop = audioTransform.Find("AmbienceLoop").GetComponent<AudioController>();
        wailLoop = audioTransform.Find("WailLoop").GetComponent<AudioController>();
        climaxLoop = audioTransform.Find("ClimaxLoop").GetComponent<AudioController>();
        deathSound = audioTransform.Find("DeathSound").GetComponent<AudioController>();
        badEndingSound = audioTransform.Find("BadEndingSound").GetComponent<AudioController>();
        goodEndingSound = audioTransform.Find("GoodEndingSound").GetComponent<AudioController>();

        // Initialize the state of all variables and set callbacks
        player.OnStart();
        player.gameObject.SetActive(false);
        menuCamera.SetActive(true);
        fadeCover.SetActive(false);
        menuCanvas.SetActive(false);
        mainObj.SetActive(false);
        introObj.SetActive(false);
        pausedObj.SetActive(false);
        endingObj.SetActive(false);
        gameOverObj.SetActive(false);
        instructionsMenuCanvas.SetActive(false);

        deathVideo.loopPointReached += DeathVideoCallback;
        deathVideoSecondary.loopPointReached += DeathVideoSecondaryCallback;
        badEndingVideo.loopPointReached += BadEndingVideoCallback;
        goodEndingVideo.loopPointReached += GoodEndingVideoCallback;
        SetCurrentState(GameState.mainMenu);
    }

    private void StartGame()
    {
        //menuCamera.SetActive(true);
        player.gameObject.SetActive(true);
        spawnTransform.gameObject.SetActive(transform);
        player.Init();
        badEndingCollider.gameObject.SetActive(true);
        spawnTransform.gameObject.SetActive(false);
        hudCanvas.SetActive(true);
        foreach (Enemy enemy in enemies)
        {
            enemy.Init();
        }
        currentEnding = EndType.death;
        leverAnimator.Play("New State");
        collectibleManager.OnStart();
        journalParentTransform.SetParent(offscreenCanvasTransform, false);
        int childrenCount = heartMessagesTransform.childCount;
        for (int i = 0; i < childrenCount; i++)
        {
            heartMessagesTransform.GetChild(i).gameObject.SetActive(true);
            heartMessagesTransform.GetChild(i).GetComponent<HeartMessageColliderScript>().ResetMessage();
        }
        sensorObj.SetActive(true);
        ambienceLoop.StartFade();
        openDoorColliderObj.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GameState.start:
                break;
            case GameState.mainMenu:
                break;
            case GameState.intro:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SetCurrentState(GameState.playing);
                }
                break;
            case GameState.playing:
                player.OnUpdate();

                // Update enemies etc...
                foreach (Enemy enemy in enemies)
                {
                    enemy.OnUpdate();
                }

                bool eDown = false;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    eDown = true;
                }

                if (Vector3.Distance(player.transform.position, goodEndingLever.transform.position) < Player.INTERACT_DISTANCE)
                {
                    if (eDown)
                    {
                        // Start animation for the lever pull
                        // Hide the interact message
                        // Have StartEnding as a call back to the lever pull
                        StartEnding(EndType.goodEnding);
                    }
                    else if (!interactMessage.active &&
                        currentEnding != EndType.goodEnding)
                    {
                        interactMessage.FadeIn(0.0f);
                    }
                }

                int counter = 0;

                foreach (Collectible collectible in collectibleManager.collectibles)
                {
                    if (!collectible.gameObject.activeSelf)
                    {
                        continue;
                    }
                    else if (collectible.collected)
                    {
                        collectible.gameObject.SetActive(false);
                        continue;
                    }

                    if (Vector3.Distance(player.transform.position, collectible.transform.position) < Player.INTERACT_DISTANCE)
                    {
                        if (eDown)
                        {
                            collectibleManager.PickUp(collectible);
                            journalEntryMessage.FadeInOut(1.0f);
                            pickupMessage.FadeOut(0.0f);
                            continue;
                        }
                        if (!pickupMessage.active)
                        {
                            pickupMessage.FadeIn(0.5f);
                        }
                    }
                    else
                    {
                        counter++;
                    }
                }
                if (counter >= collectibleManager.collectibles.Length)
                {
                    pickupMessage.FadeOut(0.5f);
                }

                if (badEndingCollider.bounds.Contains(player.transform.position) &&
                    currentState != GameState.ending)
                {

                    StartEnding(EndType.badEnding);
                }

                if (Input.GetKeyDown(KeyCode.Escape) ||
                    Input.GetKeyDown(KeyCode.F))
                {
                    PauseGame();
                }
                break;
            case GameState.paused:
                if (Input.GetKeyDown(KeyCode.Escape) ||
                    Input.GetKeyDown(KeyCode.F))
                {
                    ResumeGame();
                }
                //for (int i = 0; i < collectibleManager.currentlyCollected; i++)
                //{
                //    if (!collectibleManager.journalEntries[i].activeSelf) collectibleManager.journalEntries[i].SetActive(true);
                //}
                break;
            case GameState.ending:
                if (currentEnding == EndType.death)
                {
                    player.KeepPlayerStatic();
                }
                break;
            case GameState.gameOver:
                break;
            default:
                Debug.LogError("Unrecognized GameState");
                break;
        }
    }

    private void PauseGame()
    {
        SetCurrentState(GameState.paused);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        journalParentTransform.SetParent(hudCanvas.transform, false);
    }

    private void ResumeGame()
    {
        SetCurrentState(GameState.playing);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        journalParentTransform.SetParent(offscreenCanvasTransform, false);
    }

    public void SetCurrentState(GameState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        switch (currentState)
        {
            case GameState.start:
                break;
            case GameState.mainMenu:
                // Disable main menu UI elements
                mainObj.SetActive(false);
                mainMenuLoop.StartFade(mainMenuLoop.GetVolume(), 0.0f);
                break;
            case GameState.intro:
                // Disable intro cinematic elements
                introObj.SetActive(false);
                introLoop.StartFade(introLoop.GetVolume(), 0.0f);
                menuCamera.SetActive(false);
                menuCanvas.SetActive(false);
                introMessages.Stop();
                break;
            case GameState.playing:
                
                player.TogglePlayerMovement(false);
                break;
            case GameState.paused:
                // Disable pause menu UI elements
                pausedObj.SetActive(false);
                break;
            case GameState.ending:
                // Disable ending cinematic elements
                endingObj.SetActive(false);
                break;
            case GameState.gameOver:
                // Disable game over menu UI elements
                // gameOverObj.SetActive(false);
                break;
            default:
                Debug.LogError("Unrecognized GameState");
                break;
        }

        switch (newState)
        {
            case GameState.mainMenu:
                // Playing opening mainMenu animation and enable main menu UI elements
                player.gameObject.SetActive(false);
                mainMenuLoop.StartFade();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                menuCamera.SetActive(true);
                menuCanvas.SetActive(true);
                mainObj.SetActive(true);
                doorAnimator.Play("New State", -1, 0);
                break;
            case GameState.intro:
                // Play the intro cinematic
                introLoop.StartFade();
                introObj.SetActive(true);
                introMessages.Play();
                break;
            case GameState.playing:
                // Initialize the player and enemies etc...
                if (currentState != GameState.paused)
                {
                    StartGame();
                }
                player.TogglePlayerMovement(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case GameState.paused:
                // Enable pause menu UI elements
                pausedObj.SetActive(true);
                break;
            case GameState.ending:
                break;
            case GameState.gameOver:
                player.gameObject.SetActive(false);
                menuCamera.SetActive(true);
                menuCanvas.SetActive(true);

                // Enable gameOver UI elements
                gameOverObj.SetActive(true);
                StartCoroutine(GameOverCoroutine());
                break;
            default:
                Debug.LogError("Unrecognized GameState");
                break;
        }

        currentState = newState;
    }

    public void DisableBadEnding()
    {
        badEndingCollider.gameObject.SetActive(false);
    }

    public void StartEnding(EndType endState)
    {
        SetCurrentState(GameState.ending);
        currentEnding = endState;

        switch (endState)
        {
            case EndType.death:
                climaxLoop.Stop();
                wailLoop.Stop();
                deathVideo.gameObject.SetActive(true);
                deathVideo.Play();
                deathSound.Play();
                break;
            case EndType.badEnding:
                StartBadEndingInGameCoroutine();
                break;
            case EndType.goodEnding:
                StartGoodEndingCoroutine();
                break;
            default:
                climaxLoop.Stop();
                wailLoop.Stop();
                deathVideo.gameObject.SetActive(true);
                deathVideo.Play();
                deathSound.Play();
                break;
        }
    }

    public void StartBadEndingInGameCoroutine()
    {
        player.StartInGameCutsceneCoroutine();
    }

    public void StartBadEndingCutsceneCoroutine()
    {
        player.TogglePlayerMovement(false);
        StartCoroutine(BadEndingCoroutine());
    }

    private void StartGoodEndingCoroutine()
    {
        currentEnding = EndType.goodEnding;
        interactMessage.FadeOut(0.0f);
        leverAnimator.Play("lever_pull");
        player.TogglePlayerMovement(false);
        StartCoroutine(GoodEndingCoroutine());
    }

    private IEnumerator BadEndingCoroutine()
    {
        StartCoroutine(FadeCoverIn(2.0f));
        climaxLoop.StartFade(climaxLoop.GetVolume(), 0.0f, 3.5f);
        yield return new WaitForSeconds(2.0f);
        sensorObj.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        badEndingVideo.gameObject.SetActive(true);
        badEndingSound.StartFade(0.2f, 1.0f, 3.0f);
        badEndingVideo.Play();
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FadeCoverOut(2.5f));
        yield return new WaitForSeconds(3.0f);
        badEndingMessage.FadeInOut(1.0f);
    }

    private IEnumerator GoodEndingCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeCoverIn(2.0f));
        climaxLoop.StartFade(climaxLoop.GetVolume(), 0.0f, 3.5f);
        yield return new WaitForSeconds(2.0f);
        sensorObj.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        goodEndingSound.StartFade(0.2f, 1.0f, 3.0f);
        goodEndingVideo.gameObject.SetActive(true);
        goodEndingVideo.Play();
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FadeCoverOut(2.5f));
    }

    private IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSeconds(3);

        while (true)
        {
            Color newColor = gameOverObj.GetComponentInChildren<Image>().color;
            float alpha = newColor.a;
            alpha -= 1f * Time.deltaTime;
            newColor.a = alpha;
            gameOverObj.GetComponentInChildren<TextMeshProUGUI>().alpha = alpha;
            gameOverObj.GetComponentInChildren<Image>().color = newColor;

            if (alpha <= 0)
            {
                newColor.a = 1;
                gameOverObj.GetComponentInChildren<Image>().color = newColor;
                gameOverObj.GetComponentInChildren<TextMeshProUGUI>().alpha = 1;
                gameOverObj.SetActive(false);
                SetCurrentState(GameState.mainMenu);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator FadeCoverIn(float duration)
    {
        fadeCover.SetActive(true);

        Color color = fadeCover.GetComponent<Image>().color;
        color.a = 0.0f;
        fadeCover.GetComponent<Image>().color = color;
        while (fadeCover.GetComponent<Image>().color.a <= 1)
        {
            float alpha = fadeCover.GetComponent<Image>().color.a;
            alpha += Time.deltaTime / duration;
            color = fadeCover.GetComponent<Image>().color;
            color.a = alpha;
            fadeCover.GetComponent<Image>().color = color;
            yield return null;
        }
    }

    private IEnumerator FadeCoverOut(float duration)
    {
        Color color = fadeCover.GetComponent<Image>().color;
        color.a = 1.0f;
        fadeCover.GetComponent<Image>().color = color;

        while (fadeCover.GetComponent<Image>().color.a >= 0)
        {
            float alpha = fadeCover.GetComponent<Image>().color.a;
            alpha -= Time.deltaTime / duration;
            color = fadeCover.GetComponent<Image>().color;
            color.a = alpha;
            fadeCover.GetComponent<Image>().color = color;
            yield return null;
        }

        fadeCover.SetActive(false);
    }

    // UI button OnClick functions
    public void OnStartClick()
    {
        SetCurrentState(GameState.intro);
    }

    public void OnInstructionsClick()
    {
        mainMenuCanvas.SetActive(false);
        instructionsMenuCanvas.SetActive(true);
    }

    public void OnBackClicked()
    {
        instructionsMenuCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

    public void OnResumeClick()
    {
        ResumeGame();
    }

    public void OnQuitClick()
    {
        SetCurrentState(GameState.mainMenu);
        hudCanvas.SetActive(false);
    }

    // VideoPlayer on complete callbacks
    private void DeathVideoCallback(VideoPlayer vp)
    {
        SetCurrentState(GameState.gameOver);
        deathVideo.gameObject.SetActive(false);
    }

    private void DeathVideoSecondaryCallback(VideoPlayer vp)
    {
        SetCurrentState(GameState.mainMenu);
        deathVideoSecondary.gameObject.SetActive(false);
        badEndingVideo.gameObject.SetActive(false);
    }

    private void BadEndingVideoCallback(VideoPlayer vp)
    {
        deathVideoSecondary.gameObject.SetActive(true);
        deathVideoSecondary.Play();
        deathSound.Play();
    }

    private void GoodEndingVideoCallback(VideoPlayer vp)
    {
        SetCurrentState(GameState.mainMenu);
        goodEndingVideo.gameObject.SetActive(false);
        goodEndingSound.Stop();
    }
}
