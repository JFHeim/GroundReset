/*
    ------------------- Code Monkey -------------------

    Thank you for downloading the Code Monkey Utilities
    I hope you find them useful in your projects
    If you have any questions use the contact form
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */


namespace GroundReset;

/*
 * Triggers a Action after a certain time
 * */
public class FunctionTimer
{
    private static List<FunctionTimer> timerList; // Holds a reference to all active timers

    private static GameObject
        initGameObject; // Global game object used for initializing class, is destroyed on scene change

    private readonly string functionName = "NoneNameTimer";


    private readonly GameObject gameObject;
    private readonly Action onEndAction;
    private readonly bool useUnscaledDeltaTime;


    public FunctionTimer(GameObject gameObject, Action action, float timer, string functionName,
        bool useUnscaledDeltaTime)
    {
        this.gameObject = gameObject;
        onEndAction = action;
        Timer = timer;
        this.functionName = functionName;
        this.useUnscaledDeltaTime = useUnscaledDeltaTime;
    }

    public float Timer { get; private set; }

    private static void InitIfNeeded()
    {
        if (initGameObject == null)
        {
            initGameObject = new GameObject("FunctionTimer_Global");
            timerList = new List<FunctionTimer>();
        }
    }


    public static FunctionTimer Create(Action action, float timer) { return Create(action, timer, "", false, false); }

    public static FunctionTimer Create(Action action, float timer, string functionName)
    {
        return Create(action, timer, functionName, false, false);
    }

    public static FunctionTimer Create(Action action, float timer, string functionName, bool useUnscaledDeltaTime)
    {
        return Create(action, timer, functionName, useUnscaledDeltaTime, false);
    }

    public static FunctionTimer Create(Action action, float timer, string functionName, bool useUnscaledDeltaTime,
        bool stopAllWithSameName)
    {
        if (!ZNet.m_isServer) return null;
        InitIfNeeded();

        if (stopAllWithSameName) StopAllTimersWithName(functionName);

        var obj = new GameObject("FunctionTimer Object " + functionName, typeof(MonoBehaviourHook));
        var funcTimer = new FunctionTimer(obj, action, timer, functionName, useUnscaledDeltaTime);
        obj.GetComponent<MonoBehaviourHook>().OnUpdate = funcTimer.Update;

        timerList.Add(funcTimer);

        Plugin.timer = funcTimer;
        Debug($"Timer was successfully created. Interval was set to {timer} seconds.");
        return funcTimer;
    }

    public static void RemoveTimer(FunctionTimer funcTimer)
    {
        InitIfNeeded();
        timerList.Remove(funcTimer);
    }

    public static void StopAllTimersWithName(string functionName)
    {
        if (Plugin.timer != null && Plugin.timer.functionName == functionName) Plugin.timer = null;

        InitIfNeeded();
        for (var i = 0; i < timerList.Count; i++)
            if (timerList[i].functionName == functionName)
            {
                timerList[i].DestroySelf();
                i--;
            }
    }

    public static void StopFirstTimerWithName(string functionName)
    {
        InitIfNeeded();
        for (var i = 0; i < timerList.Count; i++)
            if (timerList[i].functionName == functionName)
            {
                timerList[i].DestroySelf();
                return;
            }
    }

    private void Update()
    {
        if (useUnscaledDeltaTime)
            Timer -= Time.unscaledDeltaTime;
        else
            Timer -= Time.deltaTime;

        if (Timer <= 0)
        {
            // Timer complete, trigger Action
            onEndAction();
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        RemoveTimer(this);
        if (gameObject != null) Destroy(gameObject);
    }

    // Create a Object that must be manually updated through Update();
    public static FunctionTimerObject CreateObject(Action callback, float timer)
    {
        return new FunctionTimerObject(callback, timer);
    }

    /*
     * Class to hook Actions into MonoBehaviour
     * */
    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action OnUpdate;

        private void Update() { OnUpdate?.Invoke(); }
    }


    /*
     * Class to trigger Actions manually without creating a GameObject
     * */
    public class FunctionTimerObject
    {
        private readonly Action callback;
        private float timer;

        public FunctionTimerObject(Action callback, float timer)
        {
            this.callback = callback;
            this.timer = timer;
        }

        public bool Update() { return Update(Time.deltaTime); }

        public bool Update(float deltaTime)
        {
            timer -= deltaTime;
            if (timer <= 0)
            {
                callback();
                return true;
            }

            return false;
        }
    }
}