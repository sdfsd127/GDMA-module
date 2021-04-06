using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Image[] gaugeImages;

    [SerializeField] private Text timeText;
    [SerializeField] private GameObject interactTextObject;
    [SerializeField] private Text interactText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    //
    // EXTERNAL FUNCTIONS
    //
    public void SetGauge(int gaugeIndex, float percent)
    {
        gaugeImages[gaugeIndex].fillAmount = Mathf.Clamp(percent, 0.0f, 1.0f);
    }

    public void SetTime(int hour, int minute, bool am)
    {
        if (timeText != null && timeText.text != null)
            timeText.text = hour.ToString("00") + ":" + minute.ToString("00") + ((am) ? "am" : "pm");
    }

    public void ShowWithinInteractRange()
    {
        if (interactTextObject != null)
            interactTextObject.SetActive(true);
    }

    public void HideWithinInteractRange()
    {
        if (interactTextObject != null)
            interactTextObject.SetActive(false);
    }

    // 
    // EXTERNAL BUTTONS
    //
    public void FillGaugeButton(int gaugeIndex)
    {
        GameManager.Instance.FillGauge(gaugeIndex);
    }
}
