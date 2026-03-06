using UnityEngine;

public class WorldClock : MonoBehaviour
{
    public static int CurrentHour { get; private set; }
    public static int CurrentMinute { get; private set; }

    [Header("时间加速倍率")]
    public float timeScale = 60f; // 1秒 = 1分钟

    void Update()
    {
        // 总分钟数 = 游戏运行时间 * 时间加速倍率
        int totalMinutes = (int)(Time.time * timeScale) % (24 * 60);

        CurrentHour = totalMinutes / 60;
        CurrentMinute = totalMinutes % 60;
    }
}