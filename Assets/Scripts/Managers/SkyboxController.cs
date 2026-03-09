using UnityEngine;
using Pinwheel.Jupiter;

public class SkyboxController : MonoBehaviour
{
    public JDayNightCycle dnc;

    void Start()
    {
        if (dnc != null)
        {
            dnc.AutoTimeIncrement = false; // 关闭自动时间
            dnc.StartTime = WorldClock.Instance.startHour;
        }

        // 订阅时间更新事件
        WorldClock.OnMinuteChanged += UpdateSkyboxTime;
    }

    void OnDestroy()
    {
        WorldClock.OnMinuteChanged -= UpdateSkyboxTime;
    }

    void UpdateSkyboxTime()
    {
        if (dnc == null || WorldClock.Instance == null) return;

        // 计算当前时间比例
        float t = WorldClock.Instance.CurrentHour + WorldClock.Instance.CurrentMinute / 60f;
        if (WorldClock.Instance.CurrentMinute == 0) t += 1f;
        dnc.Time = t;
        
    }
}