using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SyncContext : MonoBehaviour
{
    public static TaskScheduler unityTaskScheduler;
    public static int unityThread;
    public static SynchronizationContext unitySynchronizationContext;
    static public Queue<Action> runInUpdate = new Queue<Action>();
    static SyncContext _instance;

    public void Awake()
    {
        Debug.Log("awake");
        unitySynchronizationContext = SynchronizationContext.Current;
        unityThread = Thread.CurrentThread.ManagedThreadId;
        unityTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
    }
    public static bool isOnUnityThread => unityThread == Thread.CurrentThread.ManagedThreadId;

    public void Start()
    {
        BMSEngine.DeviceManager.connectBMSServer();
    }
    public static void RunOnUnityThread(Action action)
    {
        // is this right?
        if (unityThread == Thread.CurrentThread.ManagedThreadId)
        {
            action();
        }
        else
        {
            lock (runInUpdate)
            {
                runInUpdate.Enqueue(action);
            }
        }
    }

    private void Update()
    {
        while (runInUpdate.Count > 0)
        {
            Action action = null;
            lock (runInUpdate)
            {
                if (runInUpdate.Count > 0)
                    action = runInUpdate.Dequeue();
            }
            action?.Invoke();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Debug.Log("Initialize");
        if (_instance == null)
        {
            _instance = new GameObject("Networking").AddComponent<SyncContext>();
            DontDestroyOnLoad(_instance.gameObject);
        }

        Application.quitting += BMSEngine.DeviceManager.disconnectBMSServer;
    }
}
