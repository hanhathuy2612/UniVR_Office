using System;
using System.Runtime.CompilerServices;

namespace SlivingDeviceSim
{
    public static class Log
    {
        public static void Debug(string msg,
    [CallerLineNumber] int lineNumber = 0,
    [CallerMemberName] string caller = null)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
            UnityEngine.Debug.Log(msg);
#else
            Console.WriteLine(caller + ":" + lineNumber.ToString() + " " + msg + Environment.NewLine);
            #endif
        }
    }
}
