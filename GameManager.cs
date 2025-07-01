using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string transitionedFromScene;

    public Vector2 platformingRespawnPoint;
    public Vector2 respawnPoint;
    [SerializeField] Vector2 defaultRespawnPoint;
    [SerializeField] Bench bench;

    public GameObject shade;

    public bool THKDefeated = false;

    [SerializeField] FadeUI pauseMenu;
    [SerializeField] float fadeTime;
    public bool isPaused;
    float lastTimeScale = -1;
    static Coroutine stopGameCoroutine;
    public static bool isStopped { get { return stopGameCoroutine != null; } }

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        SaveData.Instance.Initialize();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (PlayerController.Instance != null)
        {
            if (PlayerController.Instance.halfMana)
            {
                SaveData.Instance.LoadShadeData();
                if (SaveData.Instance.sceneWithShade == SceneManager.GetActiveScene().name || SaveData.Instance.sceneWithShade == "")
                {
                    Instantiate(shade, SaveData.Instance.shadePos, SaveData.Instance.shadeRot);
                }
            }
        }

        SaveScene();
        DontDestroyOnLoad(gameObject);
        bench = FindObjectOfType<Bench>();

        SaveData.Instance.LoadBossData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveData.Instance.SavePlayerData();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause(!isPaused);
        }
    }

    public void Pause(bool b)
    {
        if (b)
        {
            // Save the timescale we will restore to.
            if (lastTimeScale < 0)
                lastTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            if (!isStopped)
            {
                Time.timeScale = lastTimeScale;
                lastTimeScale = -1;
            }
        }
        pauseMenu.Fade(fadeTime, b);
        isPaused = b;
    }

    public static void Stop(float duration = .5f, float restoreDelay = .1f, float slowMultiplier = 0f)
    {
        if (stopGameCoroutine != null) return;
        stopGameCoroutine = Instance.StartCoroutine(HandleStopGame(duration, restoreDelay, slowMultiplier));
    }

    // Used to create the hit stop effect. 
    // <duration> specifies how long it lasts for.
    // <restoreDelay> specifies how quickly we go back to the original time scale.
    // <stopMultiplier> lets us control how much the stop is.
    static IEnumerator HandleStopGame(float duration, float restoreDelay, float slowMultiplier = 0f)
    {
        if (Instance.lastTimeScale < 0)
            Instance.lastTimeScale = Time.timeScale; // Saves the original time scale for restoration later.

        Time.timeScale = Instance.lastTimeScale * slowMultiplier;

        // Counts down every frame until the stop game is finished.
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        while (duration > 0)
        {
            // Don't count down if the game is paused, and don't loop as well.
            if (Instance.isPaused)
            {
                yield return w;
                continue;
            }

            // Set the time back to zero, since unpausing sets it back to 1.
            Time.timeScale = Instance.lastTimeScale * slowMultiplier;

            // Counts down.
            duration -= Time.unscaledDeltaTime;
            yield return w;
        }

        // Save the last time scale we want to restore to.
        float timeScaleToRestore = Instance.lastTimeScale;

        // Signal that the stop is finished.
        Instance.lastTimeScale = -1;
        stopGameCoroutine = null;

        // If a restore delay is set, restore the time scale gradually.
        if (restoreDelay > 0)
        {
            // Moves the timescale from the value it is set to, to its original value.
            float currentTimeScale = timeScaleToRestore * slowMultiplier;
            float restoreSpeed = (timeScaleToRestore - currentTimeScale) / restoreDelay;
            while (currentTimeScale < timeScaleToRestore)
            {
                // Stop this if the game is paused.
                if (Instance.isPaused)
                {
                    yield return w;
                    continue;
                }

                // End this coroutine if another stop has started.
                if (isStopped) yield break;

                // Set the timescale to the current value this frame.
                currentTimeScale += restoreSpeed * Time.unscaledDeltaTime;
                Time.timeScale = currentTimeScale;

                // Wait for a frame.
                yield return w;
            }
        }

        // Only restore the timeScale if it is not stopped again.
        // Can happen if another stop fires while restoring the time scale.
        if (!isStopped) Time.timeScale = timeScaleToRestore;
    }

    public void SaveGame()
    {
        SaveData.Instance.SavePlayerData();
    }

    public void SaveScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveData.Instance.sceneNames.Add(currentSceneName);
    }

    public void RespwanPlayer()
    {
        SaveData.Instance.LoadBench();
        if (SaveData.Instance.benchSceneName != null) //load the bench's scene if it exists.
        {
            SceneManager.LoadScene(SaveData.Instance.benchSceneName);
        }

        if (SaveData.Instance.benchPos != null) //set the respawn point to the bench's position.
        {
            respawnPoint = SaveData.Instance.benchPos;
        }
        else
        {
            respawnPoint = defaultRespawnPoint;
        }

        PlayerController.Instance.transform.position = respawnPoint;
        StartCoroutine(UIManager.Instance.DeactivateDeathScreen());
        PlayerController.Instance.Respawned();
    }
}
