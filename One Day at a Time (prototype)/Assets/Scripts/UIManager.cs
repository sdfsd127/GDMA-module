using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Image[] gaugeImages;

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
}
