public class Timer
{
    private float m_ElapsedTime;
    private float m_TargetTime;

    public Timer()
    {
        m_ElapsedTime = 0.0f;
        m_TargetTime = 0.0f;
    }

    public Timer(float targetTime)
    {
        m_ElapsedTime = 0.0f;
        m_TargetTime = targetTime;
    }

    public float GetPercentElapsed()
    {
        return m_ElapsedTime / m_TargetTime;
    }

    public float GetTimeLeft()
    {
        return m_TargetTime - m_ElapsedTime;
    }

    public float GetElapsedTime()
    {
        return m_ElapsedTime;
    }

    public float GetTargetTime()
    {
        return m_TargetTime;
    }

    public bool HasReachedTarget()
    {
        return m_ElapsedTime >= m_TargetTime;
    }

    public void SetElapsedTime(float time)
    {
        m_ElapsedTime = time;
    }

    public void SetTargetTime(float time)
    {
        m_TargetTime = time;
    }

    public void AddTime(float time)
    {
        m_ElapsedTime += time;
    }

    public void ResetTimer()
    {
        m_ElapsedTime = 0.0f;
    }
}
