using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// I believe this comes from @DavidSWu in the Unity forums, thank you!
public class SyncContext : MonoBehaviour
{
    public static TaskScheduler unityTaskScheduler;
    public static int unityThread;
    public static SynchronizationContext unitySynchronizationContext;
    static public Queue<Action> runInUpdate = new Queue<Action>();
    
    public void Awake()
    {
        unitySynchronizationContext = SynchronizationContext.Current;
        unityThread = Thread.CurrentThread.ManagedThreadId;
        unityTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
    }

    public static bool isOnUnityThread => unityThread == Thread.CurrentThread.ManagedThreadId;


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
}