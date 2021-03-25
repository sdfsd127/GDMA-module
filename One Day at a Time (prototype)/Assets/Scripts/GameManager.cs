using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Player and Gauges
    private PlayerInfo player;

    // Timing
    private bool isMorning;
    private int currentTime;
    private const int MORNING_BEGIN_TIME = 9; // 09:00 am begin
    private const int HOUR_CLOCK = 12; // 12:00 hour clock
    private const int HOUR_INCREMENT = 1; // 01:00 hour increments

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        player = new PlayerInfo();

        isMorning = true;
        currentTime = MORNING_BEGIN_TIME;
        UIManager.Instance.SetTime(currentTime, isMorning);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            LoseGaugeResource(0, 10);
        }
            
        if (Input.GetKeyUp(KeyCode.D))
        {
            GainGaugeResource(0, 10);
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            AlterTime(HOUR_INCREMENT);
        }
    }

    private void UpdatePlayerGauges()
    {
        UIManager.Instance.SetGauge(0, player.GetPercentage("Health"));
        UIManager.Instance.SetGauge(1, player.GetPercentage("Hunger"));
        UIManager.Instance.SetGauge(2, player.GetPercentage("Thirst"));
        UIManager.Instance.SetGauge(3, player.GetPercentage("Hygiene"));
    }

    private void LoseGaugeResource(int gaugeIndex, int amount)
    {
        player.LoseResource(gaugeIndex, amount);

        UpdatePlayerGauges();
    }

    private void GainGaugeResource(int gaugeIndex, int amount)
    {
        player.GainResource(gaugeIndex, amount);

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

    public void FillGauge(int gaugeIndex)
    {
        GainGaugeResource(gaugeIndex, 20);
    }
}
