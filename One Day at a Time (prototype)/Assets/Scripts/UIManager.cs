using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Image[] gaugeImages;

    [SerializeField] private Text timeText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void SetGauge(int gaugeIndex, float percent)
    {
        gaugeImages[gaugeIndex].fillAmount = Mathf.Clamp(percent, 0.0f, 1.0f);
    }

    public void SetTime(int currentTime, bool am)
    {
        timeText.text = currentTime.ToString("00") + ":00" + ((am) ? "am" : "pm");
    }

    public void FillGaugeButton(int gaugeIndex)
    {
        GameManager.Instance.FillGauge(gaugeIndex);
    }
}
