using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace General.Logging
{
    public sealed class VandullLogger : ILogger
    {
        public void Log(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.Log($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }

        public void LogWarning(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.LogWarning($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }

        public void LogError(object message,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Debug.LogError($"[{Path.GetFileName(file)}:{line} - {member}] {message}");
        }
    }
}