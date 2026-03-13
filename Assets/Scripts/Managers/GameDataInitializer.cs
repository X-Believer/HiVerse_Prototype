using System.IO;
using UnityEngine;

public class GameDataInitializer : MonoBehaviour
{
    void Awake()
    {
        CopyDirectory(
            Application.streamingAssetsPath,
            Application.persistentDataPath);
    }

    void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);

        // 复制文件
        foreach (string file in Directory.GetFiles(source))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destination, fileName);

            if (!File.Exists(destFile))
            {
                File.Copy(file, destFile);
            }
        }

        // 递归复制子目录
        foreach (string dir in Directory.GetDirectories(source))
        {
            string dirName = Path.GetFileName(dir);
            string destDir = Path.Combine(destination, dirName);

            CopyDirectory(dir, destDir);
        }
    }
}