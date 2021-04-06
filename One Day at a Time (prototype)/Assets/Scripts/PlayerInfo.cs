using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public BasicNeed[] playerNeeds;

    private const int MAX_VALUE = 100;

    public PlayerInfo()
    {
        playerNeeds = new BasicNeed[] { new BasicNeed("Health"), new BasicNeed("Hunger"), new BasicNeed("Thirst"), new BasicNeed("Hygiene") };
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
