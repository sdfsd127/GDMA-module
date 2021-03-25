public class Timer
{
    private float elapsed_time;
    private float target_time;

    public float GetPercentElapsed()
    {
        return elapsed_time / target_time;
    }

    public float GetTimeLeft()
    {
        return target_time - elapsed_time;
    }

    public float GetElapsedTime()
    {
        return elapsed_time;
    }

    public float GetTargetTime()
    {
        return target_time;
    }

    public bool HasReachedTarget()
    {
        return elapsed_time >= target_time;
    }

    public void SetElapsedTime(float time)
    {
        elapsed_time = time;
    }

    public void SetTargetTime(float time)
    {
        target_time = time;
    }

    public void AddTime(float time)
    {
        elapsed_time += time;
    }

    public void ResetTimer()
    {
        elapsed_time = 0.0f;
    }
}
