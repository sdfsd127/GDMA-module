using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Player and references to component objects on the player
    private GameObject playerObject;
    private PlayerInfo playerInfo;
    private PlayerMovement playerMovement;
    private PlayerLooking playerLooking;
    private PlayerInteracting playerInteracting;

    // Timing
    private const int HOUR_CLOCK = 12; // 12:00 hour clock
    private const int HOUR_INCREMENT = 1; // 01:00 hour increments
    private const int MORNING_BEGIN_TIME = 9; // 09:00 am begin
    private const int SLEEP_BEGIN_TIME = 11; // 11:00pm night time

    private const int TIME_BETWEEN_TICK = 1; // The realtime (in seconds) between the addition of TIME_ON_TICK to current tick timer
    private const int TIME_PER_TICK = 1; // The time added to the clock per tick
    private const int TICKS_PER_GAUGE_TICK = 3; // How many ticks per resource gauge decrease
    private Timer tickTimer;
    private int tickCounter;

    private bool isMorning;
    private int currentHour;
    private int currentMinute;

    // Gauges
    private const int HEALTH_DRAIN_PER_TICK = 1;
    private const int HUNGER_DRAIN_PER_TICK = 3;
    private const int THIRST_DRAIN_PER_TICK = 4;
    private const int HYGIENE_DRAIN_PER_TICK = 2;

    // Cursor Control
    public static bool CURSOR_ACTIVE = true;

    // Names of the scenes of the minigames
    [SerializeField] private string[] minigames;

    private const float EMPTY_GAUGE_DIFFICULTY_INCREASE = 0.35f; // The value added to the difficulty if a bar was entirely empty

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        // Create player objects
        playerObject = GameObject.Find("Player Root Object");
        playerInfo = playerObject.GetComponentInChildren<PlayerInfo>();
        playerMovement = playerObject.GetComponentInChildren<PlayerMovement>();
        playerLooking = playerObject.GetComponentInChildren<PlayerLooking>();
        playerInteracting = playerObject.GetComponentInChildren<PlayerInteracting>();

        // Load in current time
        currentHour = ConsistentData.m_CurrentHour;
        currentMinute = ConsistentData.m_CurrentMinute;
        isMorning = ConsistentData.m_IsMorning;

        // Setup UI
        SetUITime(currentHour, currentMinute, isMorning);
        UpdatePlayerGauges();

        // Setup game tick timer
        SetupTickTimer();

        // Start cursor locked and vanished
        CursorControl.SetCursorState(CursorLockMode.Locked, false);
    }

    private void SetUITime(int hour, int minute, bool isMorning)
    {
        UIManager.Instance.SetTime(hour, minute, isMorning);
    }

    private void UpdatePlayerGauges()
    {
        UIManager.Instance.SetGauge(0, playerInfo.GetPercentage("Health"));
        UIManager.Instance.SetGauge(1, playerInfo.GetPercentage("Hunger"));
        UIManager.Instance.SetGauge(2, playerInfo.GetPercentage("Thirst"));
        UIManager.Instance.SetGauge(3, playerInfo.GetPercentage("Hygiene"));
    }

    private void SetupTickTimer()
    {
        tickTimer = new Timer();
        tickTimer.SetTargetTime(TIME_BETWEEN_TICK);
        tickTimer.SetElapsedTime(0);
    }

    private void Update()
    {
        // Mouse Toggle
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            FlipCurrentCursorState();

            if (GetCurrentCursorState())
                CursorControl.SetCursorState(CursorLockMode.None, true);
            else
                CursorControl.SetCursorState(CursorLockMode.Locked, false);
        }

        // Game Tick (LOGIC) Handling
        tickTimer.AddTime(Time.deltaTime);

        // Check if time between tick has elapsed
        if (tickTimer.HasReachedTarget())
        {
            tickTimer.ResetTimer();
            tickCounter++;

            // Make time of day tick
            AddMinute(TIME_PER_TICK);

            // Check if time of day has reached night
            CheckForEndOfDay();

            // Make gauges tick every 10 ticks
            if (tickCounter == TICKS_PER_GAUGE_TICK)
            {
                tickCounter = 0;

                LoseGaugeResource("Health", HEALTH_DRAIN_PER_TICK);
                LoseGaugeResource("Hunger", HUNGER_DRAIN_PER_TICK);
                LoseGaugeResource("Thirst", THIRST_DRAIN_PER_TICK);
                LoseGaugeResource("Hygiene", HYGIENE_DRAIN_PER_TICK);
            }
        }
    }

    private void CheckForEndOfDay()
    {
        //if (IsEndOfDay())
            // TODO:
    }

    private bool IsEndOfDay()
    {
        if (currentHour >= SLEEP_BEGIN_TIME)
            return true;
        else
            return false;
    }

    private void LoseGaugeResource(int gaugeIndex, int amount)
    {
        playerInfo.LoseResource(gaugeIndex, amount);

        UpdatePlayerGauges();
    }

    private void LoseGaugeResource(string resourceName, int amount)
    {
        playerInfo.LoseResource(resourceName, amount);

        UpdatePlayerGauges();
    }

    private void GainGaugeResource(int gaugeIndex, int amount)
    {
        playerInfo.GainResource(gaugeIndex, amount);

        UpdatePlayerGauges();
    }

    private void GainGaugeResource(string resourceName, int amount)
    {
        playerInfo.GainResource(resourceName, amount);

        UpdatePlayerGauges();
    }

    private void AddMinute(int amount)
    {
        currentMinute += amount;

        if (currentMinute >= 60)
        {
            currentMinute -= 60;
            AddHour(1);
        }
        else
            SetUITime(currentHour, currentMinute, isMorning);
    }

    private void AddHour(int amount)
    {
        currentHour += amount;

        if (currentHour >= 12)
        {
            currentHour -= 12;
            isMorning = !isMorning;
        }

        SetUITime(currentHour, currentMinute, isMorning);
    }

    private void UpdateDifficulty()
    {
        // Update cross scene data store of difficulty
        ConsistentData.m_MinigameDifficulty = GetCurrentDifficulty();

        Debug.Log(ConsistentData.m_MinigameDifficulty);
    }

    private float GetCurrentDifficulty()
    {
        // Combine all the player basic need stats
        float playerStatsTotal = playerInfo.GetPercentage("Health") + playerInfo.GetPercentage("Hunger") + playerInfo.GetPercentage("Thirst") + playerInfo.GetPercentage("Hygiene");
        float missingPercent = 4 - playerStatsTotal;

        // Add a base value of 1 for the neutral difficulty
        // Add (difference * EMPTY_GAUGE_DIFFICULTY_INCREASE) so full gauges increase the difficulty by 0 and empty gauges by 1
        // Simplified 1 + (difference * EMPTY_GAUGE_DIFFICULTY_INCREASE)
        return 1 + (missingPercent * EMPTY_GAUGE_DIFFICULTY_INCREASE);
    }

    private void UpdateConsistentStorage()
    {
        // Update time
        ConsistentData.m_CurrentHour = currentHour;
        ConsistentData.m_CurrentMinute = currentMinute;
        ConsistentData.m_IsMorning = isMorning;

        // Update gauges
        ConsistentData.m_HealthPercent = playerInfo.GetPercentage("Health");
        ConsistentData.m_HungerPercent = playerInfo.GetPercentage("Hunger");
        ConsistentData.m_ThirstPercent = playerInfo.GetPercentage("Thirst");
        ConsistentData.m_HygienePercent = playerInfo.GetPercentage("Hygiene");
    }

    //
    // EXTERNAL FUNCTIONS
    // 
    public void FillGauge(int gaugeIndex)
    {
        GainGaugeResource(gaugeIndex, 20);
    }

    public void PlayRandomMinigame()
    {
        string minigameName = minigames[Random.Range(0, minigames.Length)];

        PlayMinigame(minigameName);
    }

    public void PlayMinigame(string name)
    {
        // Calculate new difficulty
        UpdateDifficulty();

        // Also update time and gauges
        UpdateConsistentStorage();

        // Load the minigame
        SceneManager.LoadScene(name);
    }

    //
    // STATICS
    //
    public static void FlipCurrentCursorState() { CURSOR_ACTIVE = !CURSOR_ACTIVE; }
    public static bool GetCurrentCursorState() { return CURSOR_ACTIVE; }
}
