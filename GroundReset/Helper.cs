using UnityEngine.SceneManagement;

namespace GroundReset;

public static class Helper
{
    public static bool IsMainScene()
    {
        var scene = SceneManager.GetActiveScene();
        var isMainScene = scene.IsValid() && scene.name == Consts.MainSceneName;
        return isMainScene;
    }
    
    public static bool IsServerSafe()
    {
        var znet = ZNet.instance;
        return znet != null && znet.IsServer();
    }
}