/*
    ------------------- Code Monkey -------------------

    Thank you for downloading the Code Monkey Utilities
    I hope you find them useful in your projects
    If you have any questions use the contact form
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

// ReSharper disable once CheckNamespace
namespace CodeMonkey;

/*
 * Calls function on every Update until it returns true
 * */
public class FunctionUpdater
{
    private static List<FunctionUpdater>? UpdaterList; // Holds a reference to all active updaters

    private static GameObject? InitGameObject; // Global game object used for initializing class, is destroyed on scene change

    private readonly string functionName;


    private readonly GameObject gameObject;
    private readonly Func<bool> updateFunc; // Destroy Updater if return true;

    private bool active;

    public FunctionUpdater(GameObject gameObject, Func<bool> updateFunc, string functionName, bool active)
    {
        this.gameObject = gameObject;
        this.updateFunc = updateFunc;
        this.functionName = functionName;
        this.active = active;
    }

    private static void InitIfNeeded()
    {
        if (InitGameObject == null)
        {
            InitGameObject = new GameObject("FunctionUpdater_Global");
            UpdaterList = new List<FunctionUpdater>();
        }
    }


    public static FunctionUpdater Create(Action updateFunc)
    {
        return Create(() =>
        {
            updateFunc();
            return false;
        }, "", true, false);
    }

    public static FunctionUpdater Create(Action updateFunc, string functionName)
    {
        return Create(() =>
        {
            updateFunc();
            return false;
        }, functionName, true, false);
    }

    public static FunctionUpdater Create(Func<bool> updateFunc) { return Create(updateFunc, "", true, false); }

    public static FunctionUpdater Create(Func<bool> updateFunc, string functionName)
    {
        return Create(updateFunc, functionName, true, false);
    }

    public static FunctionUpdater Create(Func<bool> updateFunc, string functionName, bool active)
    {
        return Create(updateFunc, functionName, active, false);
    }

    public static FunctionUpdater Create(Func<bool> updateFunc, string functionName, bool active,
        bool stopAllWithSameName)
    {
        InitIfNeeded();

        if (stopAllWithSameName) StopAllUpdatersWithName(functionName);

        var gameObject = new GameObject("FunctionUpdater Object " + functionName, typeof(MonoBehaviourHook));
        var functionUpdater = new FunctionUpdater(gameObject, updateFunc, functionName, active);
        gameObject.GetComponent<MonoBehaviourHook>().OnUpdate = functionUpdater.Update;

        UpdaterList!.Add(functionUpdater);
        return functionUpdater;
    }

    private static void RemoveUpdater(FunctionUpdater funcUpdater)
    {
        InitIfNeeded();
        UpdaterList!.Remove(funcUpdater);
    }

    public static void DestroyUpdater(FunctionUpdater? funcUpdater)
    {
        InitIfNeeded();
        funcUpdater?.DestroySelf();
    }

    public static void StopUpdaterWithName(string functionName)
    {
        InitIfNeeded();
        for (var i = 0; i < UpdaterList!.Count; i++)
            if (UpdaterList[i].functionName == functionName)
            {
                UpdaterList[i].DestroySelf();
                return;
            }
    }

    public static void StopAllUpdatersWithName(string functionName)
    {
        InitIfNeeded();
        for (var i = 0; i < UpdaterList!.Count; i++)
            if (UpdaterList[i].functionName == functionName)
            {
                UpdaterList[i].DestroySelf();
                i--;
            }
    }

    public void Pause() { active = false; }

    public void Resume() { active = true; }

    private void Update()
    {
        if (!active) return;
        if (updateFunc()) DestroySelf();
    }

    public void DestroySelf()
    {
        RemoveUpdater(this);
        if (gameObject != null) Destroy(gameObject);
    }

    /*
     * Class to hook Actions into MonoBehaviour
     * */
    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action? OnUpdate;

        private void Update()
        {
            if (OnUpdate != null) OnUpdate();
        }
    }
}