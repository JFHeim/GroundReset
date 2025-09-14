namespace GroundReset;

[BepInEx.BepInPlugin(ModGuid, ModName, ModVersion)]
public class Plugin : BepInEx.BaseUnityPlugin
{
    private const string ModName = "GroundReset",
        ModAuthor = "Frogger",
        ModVersion = "2.7.0",
        ModGuid = $"com.{ModAuthor}.{ModName}";
    
    private void Awake()
    {
        if (Helper.IsServerSafe() == false)
        {
            Logger.LogError($"{nameof(ModName)} is fully server-side, do not install it on clients"); 
            return;
        }
        
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGuid);
        
        ConfigsContainer.InitializeConfiguration();
    }
}