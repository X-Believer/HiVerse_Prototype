using System.IO;
using UnityEngine;

public static class DataPath
{
    public static string Root => Application.persistentDataPath;

    public static string Personalities =>
        Path.Combine(Root, "Personalities");

    public static string CityEvents =>
        Path.Combine(Root, "CityEvents");

    public static string NPCSchedules =>
        Path.Combine(Root, "NPCSchedules");
    
    public static string UserProfile =>
        Path.Combine(Application.streamingAssetsPath, "UserProfile");

    public static string Get(string relative)
    {
        return Path.Combine(Root, relative);
    }
}