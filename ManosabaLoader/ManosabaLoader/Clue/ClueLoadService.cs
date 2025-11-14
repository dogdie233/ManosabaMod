using System.Collections.Generic;

using BepInEx.Logging;

using HarmonyLib;

using Il2CppInterop.Runtime.Injection;

using ManosabaLoader.ModManager;

using WitchTrials.Models;

using Logger = BepInEx.Logging.Logger;

namespace ManosabaLoader.Clue;

public class ClueLoadService
{
    private readonly record struct ModIdVersionPair(string Id, int Version)
    {
        public static implicit operator ModIdVersionPair(IdVersionPair witchPair)
            => new(witchPair.Id, witchPair.Version);
    }
    
    private ManualLogSource logger;

    public List<CustomClueItem> CustomClueItems { get; }

    public ClueLoadService(Harmony harmony)
    {
        logger = Logger.CreateLogSource($"{MyPluginInfo.PLUGIN_NAME}.{nameof(ClueLoadService)}");

        CustomClueItems = CollectCustomClueItems();
        logger.LogInfo($"Collected {CustomClueItems.Count} custom clue items from mods.");
        
        ClassInjector.RegisterTypeInIl2Cpp<ThumbnailResHolder>();
        harmony.PatchAll(typeof(CluePagePatch));
        harmony.PatchAll(typeof(ThumbnailResBindPatch));
    }
    
    public static List<CustomClueItem> CollectCustomClueItems()
    {
        var list = new List<CustomClueItem>();
        foreach (var mod in ModManager.ModManager.Items.Values)
        {
            if (mod.Description.CustomClues is null) continue;
            list.AddRange(mod.Description.CustomClues);
        }

        if (ScriptWorkingManager.IsEnabled)
            list.AddRange(ScriptWorkingManager.ModInfo.Description.CustomClues);

        return list;
    }

    public string GetThumbnailAddress(string clueId)
        => CustomClueItems.Find(i => i.Id == clueId)?.Thumbnail;

    internal void RemoveConflict(Il2CppSystem.Collections.Generic.List<VersionedItem<ClueDataItem>> builtinItems)
    {
        var set = new HashSet<ModIdVersionPair>();
        foreach (var item in builtinItems)
            set.Add(item.IdVersionPair);
        
        foreach (var clue in CustomClueItems)
        {
            for (var i = 0; i < clue.Versions.Count; i++)
            {
                var version = clue.Versions[i];
                var pair = new ModIdVersionPair(clue.Id, version.Version);
                if (set.Add(pair)) 
                    continue;
                
                logger.LogWarning($"Conflict detected for clue ID '{clue.Id}' version '{version.Version}'.");
                clue.Versions.RemoveAt(i--);
            }
        }

        for (var i = CustomClueItems.Count - 1; i >= 0; i--)
        {
            if (CustomClueItems[i].Versions.Count != 0)
                continue;
            
            logger.LogWarning($"Removing clue ID '{CustomClueItems[i].Id}' due to all versions conflicting.");
            CustomClueItems.RemoveAt(i);
        }
        
        logger.LogInfo($"After conflict removal, {CustomClueItems.Count} custom clue items remain.");
    }
}
