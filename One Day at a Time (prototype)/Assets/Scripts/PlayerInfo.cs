using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public BasicNeed[] playerNeeds;

    private const float FULL_PERCENT = 1.0f;

    public PlayerInfo()
    {
        playerNeeds = new BasicNeed[] { new BasicNeed("Health"), new BasicNeed("Hunger"), new BasicNeed("Thirst"), new BasicNeed("Hygiene") };

        LoadPreviousValues();
    }

    private void LoadPreviousValues()
    {
        if (ConsistentData.m_HealthPercent != FULL_PERCENT)
            playerNeeds[0].SetPercentage(ConsistentData.m_HealthPercent);
        if (ConsistentData.m_HungerPercent != FULL_PERCENT)
            playerNeeds[1].SetPercentage(ConsistentData.m_HungerPercent);
        if (ConsistentData.m_ThirstPercent != FULL_PERCENT)
            playerNeeds[2].SetPercentage(ConsistentData.m_ThirstPercent);
        if (ConsistentData.m_HygienePercent != FULL_PERCENT)
            playerNeeds[3].SetPercentage(ConsistentData.m_HygienePercent);
    }

    public float GetPercentage(string needName)
    {
        for (int i = 0; i < playerNeeds.Length; i++)
            if (playerNeeds[i].m_RequirementName == needName)
                return playerNeeds[i].GetPercentage();

        return -1.0f;
    }

    public void LoseResource(int needIndex, int amount)
    {
        playerNeeds[needIndex].RemoveResource(amount);
    }

    public void LoseResource(string resourceName, int amount)
    {
        for (int i = 0; i < playerNeeds.Length; i++)
        {
            if (playerNeeds[i].m_RequirementName == resourceName)
            {
                LoseResource(i, amount);
                break;
            }
        }
    }

    public void GainResource(int needIndex, int amount)
    {
        playerNeeds[needIndex].AddResource(amount);
    }

    public void GainResource(string resourceName, int amount)
    {
        for (int i = 0; i < playerNeeds.Length; i++)
        {
            if (playerNeeds[i].m_RequirementName == resourceName)
            {
                GainResource(i, amount);
                break;
            }
        }
    }
}