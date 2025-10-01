namespace GroundReset;

[BepInEx.BepInPlugin(ModGuid, ModName, ModVersion)]
public class Plugin : BepInEx.BaseUnityPlugin
{
    private const string ModName = "GroundReset",
        ModAuthor = "Frogger",
        ModVersion = "2.7.1",
        ModGuid = $"com.{ModAuthor}.{ModName}";
    
    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGuid);
        ConfigsContainer.InitializeConfiguration();
    }
}