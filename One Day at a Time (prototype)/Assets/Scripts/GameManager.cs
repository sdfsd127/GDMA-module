using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private bool isMorning;
    private int currentTime;
    private const int MORNING_BEGIN_TIME = 9; // 09:00 am begin
    private const int HOUR_CLOCK = 12; // 12:00 hour clock
    private const int HOUR_INCREMENT = 1; // 01:00 hour increments

    // Cursor Control
    public static bool CURSOR_ACTIVE = true;

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

        // Set time vars
        isMorning = true;
        currentTime = MORNING_BEGIN_TIME;
        UIManager.Instance.SetTime(currentTime, isMorning);

        // Start cursor locked and vanished
        CursorControl.SetCursorState(CursorLockMode.Locked, false);
    }

    private void Update()
    {
        // Mouse Toggle
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            FlipCurrentCursorState();

            if (GetCurrentCursorState())
            {
                CursorControl.SetCursorState(CursorLockMode.None, true);
            }
            else
            {
                CursorControl.SetCursorState(CursorLockMode.Locked, false);
            }
        }
    }

    private void UpdatePlayerGauges()
    {
        UIManager.Instance.SetGauge(0, playerInfo.GetPercentage("Health"));
        UIManager.Instance.SetGauge(1, playerInfo.GetPercentage("Hunger"));
        UIManager.Instance.SetGauge(2, playerInfo.GetPercentage("Thirst"));
        UIManager.Instance.SetGauge(3, playerInfo.GetPercentage("Hygiene"));
    }

    private void LoseGaugeResource(int gaugeIndex, int amount)
    {
        playerInfo.LoseResource(gaugeIndex, amount);

        UpdatePlayerGauges();
    }

    private void GainGaugeResource(int gaugeIndex, int amount)
    {
        playerInfo.GainResource(gaugeIndex, amount);

        UpdatePlayerGauges();
    }

    private void AlterTime(int amount)
    {
        currentTime += amount;

        if (currentTime >= HOUR_CLOCK)
        {
            currentTime -= HOUR_CLOCK;
            isMorning = !isMorning;
        }
                
        UIManager.Instance.SetTime(currentTime, isMorning);
    }

    //
    // EXTERNAL FUNCTIONS
    // 
    public void FillGauge(int gaugeIndex)
    {
        GainGaugeResource(gaugeIndex, 20);
    }

    //
    // STATICS
    //
    public static void FlipCurrentCursorState() { CURSOR_ACTIVE = !CURSOR_ACTIVE; }
    public static bool GetCurrentCursorState() { return CURSOR_ACTIVE; }
}
