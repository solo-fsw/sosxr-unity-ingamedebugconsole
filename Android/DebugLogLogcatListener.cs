#if UNITY_EDITOR || UNITY_ANDROID
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;


// Credit: https://stackoverflow.com/a/41018028/2373034
namespace IngameDebugConsole
{
    /// <summary>
    ///     Android-only bridge that receives logcat output from the native
    ///     <c>com.yasirkula.unity.DebugConsoleLogcatLogger</c> Java plugin and buffers the messages
    ///     in a thread-safe <see cref="System.Collections.Generic.Queue{T}" /> for polling by
    ///     <see cref="DebugLogManager" /> on the main thread.
    /// </summary>
    public class DebugLogLogcatListener : AndroidJavaProxy
    {
        private readonly Queue<string> queuedLogs;
        private AndroidJavaObject nativeObject;


        public DebugLogLogcatListener() : base("com.yasirkula.unity.DebugConsoleLogcatLogReceiver")
        {
            queuedLogs = new Queue<string>(16);
        }


        ~DebugLogLogcatListener()
        {
            Stop();

            nativeObject?.Dispose();
        }


        /// <summary>Starts logcat capture with the given <paramref name="arguments" /> filter string (e.g. <c>"*:W"</c>).</summary>
        public void Start(string arguments)
        {
            if (nativeObject == null)
            {
                nativeObject = new AndroidJavaObject("com.yasirkula.unity.DebugConsoleLogcatLogger");
            }

            nativeObject.Call("Start", this, arguments);
        }


        /// <summary>Stops the native logcat capture. Safe to call even if capture was never started.</summary>
        public void Stop()
        {
            nativeObject?.Call("Stop");
        }


        /// <summary>Called by the native plugin on a background thread when a new logcat line arrives; enqueues the message for main-thread consumption.</summary>
        [Preserve]
        public void OnLogReceived(string log)
        {
            queuedLogs.Enqueue(log);
        }


        /// <summary>Dequeues and returns the oldest buffered logcat message, or <c>null</c> if the queue is empty.</summary>
        public string GetLog()
        {
            if (queuedLogs.Count > 0)
            {
                return queuedLogs.Dequeue();
            }

            return null;
        }
    }
}
#endif