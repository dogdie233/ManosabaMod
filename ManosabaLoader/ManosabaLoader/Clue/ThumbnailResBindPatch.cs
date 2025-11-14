using HarmonyLib;

using UnityEngine;

using WitchTrials.Views;

namespace ManosabaLoader.Clue;

[HarmonyPatch]
public class ThumbnailResBindPatch
{
    [HarmonyPatch(typeof(WitchBookItemThumbnail), nameof(WitchBookItemThumbnail.Awake))]
    [HarmonyPostfix]
    private static void AwakePostfix(WitchBookItemThumbnail __instance)
    {
        __instance.gameObject.AddComponent<ThumbnailResHolder>();
    }
    
    [HarmonyPatch(typeof(WitchBookItemThumbnail), nameof(WitchBookItemThumbnail.SetTexture))]
    [HarmonyPostfix]
    private static void SetTexturePostfix(WitchBookItemThumbnail __instance, Texture2D tex)
    {
        var holder = __instance.gameObject.GetComponent(CluePagePatch.ThumbnailResHolderIl2CppType.Value).Cast<ThumbnailResHolder>();
        holder.OnSetTexture(tex);
    }
}