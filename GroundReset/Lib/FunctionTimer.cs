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
 * Triggers a Action after a certain time
 * */
public class FunctionTimer
{
    private static List<FunctionTimer>? TimerList; // Holds a reference to all active timers

    private static GameObject? InitGameObject; // Global game object used for initializing class, is destroyed on scene change

    // ReSharper disable once MemberInitializerValueIgnored
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
        if (InitGameObject == null)
        {
            InitGameObject = new GameObject("FunctionTimer_Global");
            TimerList = [];
        }
    }


    public static FunctionTimer? Create(Action action, float timer) { return Create(action, timer, "", false, false); }

    public static FunctionTimer? Create(Action action, float timer, string functionName)
    {
        return Create(action, timer, functionName, false, false);
    }

    public static FunctionTimer? Create(Action action, float timer, string functionName, bool useUnscaledDeltaTime)
    {
        return Create(action, timer, functionName, useUnscaledDeltaTime, false);
    }

    public static FunctionTimer Create(Action? action, float timer, string functionName, bool useUnscaledDeltaTime, bool stopAllWithSameName)
    {
        if(TimerList is null) throw new NullReferenceException(nameof(TimerList));
        if (action is null) throw new ArgumentNullException(nameof(action));
        InitIfNeeded();

        if (stopAllWithSameName) StopAllTimersWithName(functionName);

        var obj = new GameObject("FunctionTimer Object " + functionName, typeof(MonoBehaviourHook));
        var funcTimer = new FunctionTimer(obj, action, timer, functionName, useUnscaledDeltaTime);
        obj.GetComponent<MonoBehaviourHook>().OnUpdate = funcTimer.Update;

        TimerList.Add(funcTimer);
        return funcTimer;
    }

    public static void RemoveTimer(FunctionTimer funcTimer)
    {
        InitIfNeeded();
        TimerList!.Remove(funcTimer);
    }

    public static void StopAllTimersWithName(string functionName)
    {
        InitIfNeeded();
        for (var i = 0; i < TimerList!.Count; i++)
            if (TimerList[i].functionName == functionName)
            {
                TimerList[i].DestroySelf();
                i--;
            }
    }

    public static void StopFirstTimerWithName(string functionName)
    {
        InitIfNeeded();
        for (var i = 0; i < TimerList!.Count; i++)
            if (TimerList[i].functionName == functionName)
            {
                TimerList[i].DestroySelf();
                return;
            }
    }

    private void Update()
    {
        if (useUnscaledDeltaTime) Timer -= Time.unscaledDeltaTime;
        else Timer -= Time.deltaTime;
        
        if (Timer <= 0)
        {
            // Timer complete, trigger Action
            onEndAction.Invoke();
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
        public Action? OnUpdate;

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