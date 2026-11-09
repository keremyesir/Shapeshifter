using UnityEditor;
using System.IO;

public class BuildScript
{
    public static void BuildWindows()
    {
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };
        string buildPath = "C:/gitprojeoyun/BulletFighter_Delivery/Oyun_Build/BulletFighter.exe";
        
        // Klasoru olustur
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));
        
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
