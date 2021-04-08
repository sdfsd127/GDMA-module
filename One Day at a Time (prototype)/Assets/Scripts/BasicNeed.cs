public class BasicNeed
{
    public string m_RequirementName;
    public int m_MaxValue;
    public int m_CurrentValue;

    public BasicNeed(string name, int maxValue = 100)
    {
        m_RequirementName = name;

        m_CurrentValue = maxValue;
        m_MaxValue = maxValue;
    }

    public void SetPercentage(float percent) 
    {
        m_CurrentValue = (int)(m_MaxValue * percent);
    }

    public float GetPercentage()
    {
        return (float)m_CurrentValue / (float)m_MaxValue;
    }

    public void RemoveResource(int amount)
    {
        m_CurrentValue -= amount;

        BalanceAmount();
    }

    public void AddResource(int amount)
    {
        m_CurrentValue += amount;

        BalanceAmount();
    }

    private void BalanceAmount()
    {
        if (m_CurrentValue > m_MaxValue)
            m_CurrentValue = m_MaxValue;
        else if (m_CurrentValue < 0)
            m_CurrentValue = 0;
    }
}
