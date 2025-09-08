using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace General
{
    public class Logger
    {
        public static void Log(string message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.Log($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }
    
        public static void LogWarning(string message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.LogWarning($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }
    
        public static void LogError(string message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.LogError($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }
    }
}