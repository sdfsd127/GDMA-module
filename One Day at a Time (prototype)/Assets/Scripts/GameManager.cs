using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private PlayerInfo player;

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
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            LoseGaugeResource(0, 10);
            UpdatePlayerGauges();
        }
            
        if (Input.GetKeyUp(KeyCode.D))
        {
            GainGaugeResource(0, 10);
            UpdatePlayerGauges();
        }
    }

    private void UpdatePlayerGauges()
    {
        Debug.Log(player.GetPercentage("Health"));
        UIManager.Instance.SetGauge(0, player.GetPercentage("Health"));
        UIManager.Instance.SetGauge(1, player.GetPercentage("Hunger"));
        UIManager.Instance.SetGauge(2, player.GetPercentage("Thirst"));
        UIManager.Instance.SetGauge(3, player.GetPercentage("Hygiene"));
    }

    private void LoseGaugeResource(int gaugeIndex, int amount)
    {
        player.LoseResource(gaugeIndex, amount);
    }

    private void GainGaugeResource(int gaugeIndex, int amount)
    {
        player.GainResource(gaugeIndex, amount);
    }
}
