using HarmonyLib;
using manosaba_mod;
using Naninovel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WitchTrials.Models;
using WitchTrials.Views;

namespace ManosabaLoader
{
    public static class ModDebugTools
    {
        public static Action<string> ModDebugToolsLogMessage;
        public static Action<string> ModDebugToolsLogDebug;
        public static Action<string> ModDebugToolsLogWarning;
        public static Action<string> ModDebugToolsLogError;
        public static string StackTraceToString()
        {
            StringBuilder sb = new StringBuilder(256);
            var frames = new System.Diagnostics.StackTrace().GetFrames();
            for (int i = 1; i < frames.Length; i++) /* Ignore current StackTraceToString method...*/
            {
                var currFrame = frames[i];
                var method = currFrame.GetMethod();
                sb.AppendLine(string.Format("{0}:{1}",
                    method.ReflectedType != null ? method.ReflectedType.Name : string.Empty,
                    method.Name));
            }
            return sb.ToString();
        }

        public static void ReleaseAllScript()
        {
            var service = Engine.GetServiceOrErr<WitchTrialsScriptPlayer>();
            foreach(var script in service.scripts.ScriptLoader.GetAllLoaded().Cast<Il2CppSystem.Collections.Generic.List<Resource<Script>>>())
            {
                if (script.Path.Contains(ModResourceLoader.modScriptPrefix))
                {
                    continue;
                }
                if(!service.PlayedScript.Equals(script.Object)) 
                {
                    UnityEngine.Object.Destroy(script.Object);
                }
            }
        }

        public static void ShowConsole()
        {
            ConsoleGUI.Show();
        }

        public static void Init()
        {
            var instance = new Harmony(MyPluginInfo.PLUGIN_NAME);
            instance.PatchAll(typeof(UnityLogger_Patch));
            instance.PatchAll(typeof(ResourceProvider_Patch));
        }
    }

    [HarmonyPatch]
    class UnityLogger_Patch
    {
        [HarmonyPatch(typeof(UnityLogger), nameof(UnityLogger.Log))]
        [HarmonyPrefix]
        static bool UnityLogger_Log_Patch(string message)
        {
            ModDebugTools.ModDebugToolsLogMessage(message);
            return false;
        }

        [HarmonyPatch(typeof(UnityLogger), nameof(UnityLogger.Warn))]
        [HarmonyPrefix]
        static bool UnityLogger_Warn_Patch(string message)
        {
            ModDebugTools.ModDebugToolsLogWarning(message);
            return false;
        }

        [HarmonyPatch(typeof(UnityLogger), nameof(UnityLogger.Err))]
        [HarmonyPrefix]
        static bool UnityLogger_Err_Patch(string message)
        {
            ModDebugTools.ModDebugToolsLogError(message);
            return false;
        }
    }

    // 打印资源调用信息
    [HarmonyPatch]
    class ResourceProvider_Patch
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            return typeof(ResourceProvider).GetMethod("ResourceLoaded", 0, new Type[] { typeof(string) });
        }
        public static void Postfix(string path)
        {
            ModDebugTools.ModDebugToolsLogDebug(string.Format("ResourceProvider.ResourceLoaded: {0}", path));
        }
    }
}
