using System;

using GigaCreation.Essentials.Localization;

using HarmonyLib;

using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Il2CppSystem.Collections.Generic;

using WitchTrials.Models;
using WitchTrials.Views;

using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

namespace ManosabaLoader.Clue;

[HarmonyPatch]
public class CluePagePatch
{
    internal static readonly Lazy<Il2CppSystem.Type> ThumbnailResHolderIl2CppType = new(() => Il2CppType.From(typeof(ThumbnailResHolder)));

    [HarmonyPatch(typeof(CluePage._InitializeAsync_d__31), "MoveNext")]
    [HarmonyPostfix]
    private static void SetClueData_Postfix(CluePage._InitializeAsync_d__31 __instance)
    {
        if (__instance.__4__this.TryCast<CluePage>() is not { } page || !__instance.__u__1.IsCompleted)
            return;

        InjectCustomClueItems(page);
    }

    [HarmonyPatch(typeof(CluePage), "RefreshPageContent")]
    [HarmonyPostfix]
    private static void CluePage_RefreshPageContent_Postfix(CluePage __instance, VersionedItem<ClueDataItem> map)
    {
        if (__instance._state.TryCast<ClueState>() == null)
            return;

        var dic = __instance._localizedTextData.TryCast<Dictionary<IdVersionPair, IReadOnlyDictionary<LocaleKind, CluePage.LocalizedTexts>>>();
        // var address = WitchBookDataHelper.BuildClueTextureAddress(map.Id);
    }

    [HarmonyPatch(typeof(WitchBookDataHelper), nameof(WitchBookDataHelper.BuildClueTextureAddress))]
    [HarmonyPrefix]
    private static void WitchBookDataHelper_BuildClueTextureAddress_Prefix(ref string id, ref string __result, ref bool __runOriginal)
    {
        __runOriginal = true;
        var clueLoadService = Plugin.Instance.ClueLoadService;
        var thumbnailAddress = clueLoadService.GetThumbnailAddress(id);
        if (thumbnailAddress == null)  // 是原版资源
            return;

        if (thumbnailAddress.StartsWith("@builtin:"))
        {
            id = thumbnailAddress["@builtin:".Length..];
            __runOriginal = true;
            return;
        }

        if (thumbnailAddress.StartsWith("@mod:"))
        {
            __result = thumbnailAddress;
            __runOriginal = false;
            return;
        }
    }

    [HarmonyPatch(typeof(WitchBookItemThumbnail), "Setup")]
    [HarmonyPrefix]
    private static void WitchBookItemThumbnail_Setup_Prefix(WitchBookItemThumbnail __instance, string address, ref bool __runOriginal)
    {
        if (!address.StartsWith("@mod:"))
        {
            __runOriginal = true;
            return;
        }

        __runOriginal = false;
        __instance._rawImage.texture = __instance._defaultTexture;
        __instance._canvasGroup.alpha = 0f;
        __instance._canvasGroup.interactable = false;
        __instance.gameObject.GetComponent(ThumbnailResHolderIl2CppType.Value).Cast<ThumbnailResHolder>().SetupCustomThumbnail(address["@mod:".Length..]);
    }

    private static void InjectCustomClueItems(CluePage page)
    {
        var clueLoadService = Plugin.Instance.ClueLoadService;

        var map = page._loadedDataItemMap.Cast<List<VersionedItem<ClueDataItem>>>();
        var textData = page._localizedTextData.Cast<Dictionary<IdVersionPair, IReadOnlyDictionary<LocaleKind, CluePage.LocalizedTexts>>>();
        var oldItemIds = page._itemIds;

        clueLoadService.RemoveConflict(map);

        var convertedItems = new System.Collections.Generic.List<VersionedItem<ClueDataItem>>();
        foreach (var customClue in clueLoadService.CustomClueItems)
            foreach (var item in customClue.Versions)
            {
                convertedItems.Add(new VersionedItem<ClueDataItem>(customClue.Id, item.Version, new ClueDataItem(
                    CreateLocalizedTextArray(item.LocalizationName),
                    CreateLocalizedTextArray(item.LocalizationDesc)
                )));
            }

        {
            var idHashSet = new System.Collections.Generic.HashSet<string>();
            foreach (var item in oldItemIds)
                idHashSet.Add(item);
            foreach (var item in convertedItems)
                idHashSet.Add(item.Id);

            var newItemIds = new Il2CppStringArray(idHashSet.Count);
            var i = 0;
            foreach (var item in idHashSet)
                newItemIds[i++] = item;
            page._itemIds = newItemIds;
        }

        foreach (var item in convertedItems)
        {
            map.Add(item);
            textData[item.IdVersionPair] = ConvertToCluePageLocalizedTexts(item).Cast<IReadOnlyDictionary<LocaleKind, CluePage.LocalizedTexts>>();
        }
    }

    // Can be optimized later by caching empty string il2cpp ptr
    private static Il2CppReferenceArray<LocalizedText> CreateLocalizedTextArray(System.Collections.Generic.Dictionary<string, string> customLocalizedDic)
    {
        return new Il2CppReferenceArray<LocalizedText>([
            new LocalizedText(LocaleKind.Ja, CollectionExtensions.GetValueOrDefault(customLocalizedDic, nameof(LocaleKind.Ja), "")),
            new LocalizedText(LocaleKind.ZhHans, CollectionExtensions.GetValueOrDefault(customLocalizedDic, nameof(LocaleKind.ZhHans), "")),
            new LocalizedText(LocaleKind.ZhHant, CollectionExtensions.GetValueOrDefault(customLocalizedDic, nameof(LocaleKind.ZhHant), "")),
            new LocalizedText(LocaleKind.EnUs, CollectionExtensions.GetValueOrDefault(customLocalizedDic, nameof(LocaleKind.EnUs), ""))
        ]);
    }

    // Can be optimized later by re-use il2cpp string pointer
    private static Dictionary<LocaleKind, CluePage.LocalizedTexts> ConvertToCluePageLocalizedTexts(VersionedItem<ClueDataItem> item)
    {
        var result = new Dictionary<LocaleKind, CluePage.LocalizedTexts>();
        foreach (var pair in item.Item.Name.Cast<Il2CppArrayBase<LocalizedText>>())
            result.Add(pair.Locale, new CluePage.LocalizedTexts(pair.Text, null));

        foreach (var pair in item.Item.Description.Cast<Il2CppArrayBase<LocalizedText>>())
        {
            if (result.TryGetValue(pair.Locale, out var texts))
                texts.Description = pair.Text;
            else
                result.Add(pair.Locale, new CluePage.LocalizedTexts(null, pair.Text));
        }
        return result;
    }
}