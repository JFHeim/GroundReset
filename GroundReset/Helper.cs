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

    public static bool IsServer(bool logIsShouldBeOnServerOnly = false)
    {
        var gameState = GetGameServerClientState();
        if (logIsShouldBeOnServerOnly && gameState is GameServerClientState.Client)
            LogError($"{nameof(ModName)} is fully server-side, do not install it on clients");
        return gameState is GameServerClientState.Server;
    }

    public static GameServerClientState GetGameServerClientState() => ZNet.instance?.IsServer() switch
    {
        null => GameServerClientState.Unknown,
        false => GameServerClientState.Client,
        true => GameServerClientState.Server
    };
}

public enum GameServerClientState
{
    Unknown,
    Client,
    Server
}