using System.Diagnostics;
using UnityEngine;

namespace StreetLegends.Core
{
    /// <summary>
    /// Central logging wrapper. Info/Warn are stripped from release builds; Error is always kept.
    /// Routing to analytics/crash reporting is added in later phases without touching call sites.
    /// </summary>
    public static class Log
    {
        [Conditional("UNITY_EDITOR"), Conditional("SL_DEV"), Conditional("SL_STAGING")]
        public static void Info(string category, string message)
        {
            UnityEngine.Debug.Log($"[{category}] {message}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("SL_DEV"), Conditional("SL_STAGING")]
        public static void Warn(string category, string message)
        {
            UnityEngine.Debug.LogWarning($"[{category}] {message}");
        }

        public static void Error(string category, string message, Object context = null)
        {
            UnityEngine.Debug.LogError($"[{category}] {message}", context);
        }
    }
}
