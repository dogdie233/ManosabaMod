using System;
using System.Runtime.InteropServices;

using Cysharp.Threading.Tasks;

using GigaCreation.Essentials.AddressablesUtils;

using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;

using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Threading;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ManosabaLoader.Clue;

public class ModThumbnailAddressablesManager : Il2CppObjectBase
{
    private AddressablesManager internalManager;
    
    public ModThumbnailAddressablesManager(IntPtr pointer) : base(pointer)
    {
    }

    public ModThumbnailAddressablesManager(AddressablesManager internalManager) : base(ClassInjector.DerivedConstructorPointer<ModThumbnailAddressablesManager>())
    {
        ClassInjector.DerivedConstructorBody(this);
        this.internalManager = internalManager;
    }

    public bool IsAdditionalCatalogLoaded => internalManager.IsAdditionalCatalogLoaded;

    UniTask<bool> CheckIfMainCatalogExistsAsync(CancellationToken ct)
        => internalManager.CheckIfMainCatalogExistsAsync(ct);

    UniTask<bool> CheckIfAdditionalCatalogExistsAsync(CancellationToken ct)
        => internalManager.CheckIfAdditionalCatalogExistsAsync(ct);

    UniTask<bool> LoadAdditionalCatalogAsync(CancellationToken ct)
        => internalManager.LoadAdditionalCatalogAsync(ct);

    UniTask<List<IResourceLocation>> GetResourceLocationsAsync(CancellationToken ct)
        => internalManager.GetResourceLocationsAsync(ct);

    UniTask<T> GetOrLoadAddressableAsset<T>(string address, [Optional] CancellationToken ct) where T : Il2CppObjectBase
    {
        if (address.StartsWith("@builtin:"))
            return internalManager.GetOrLoadAddressableAsset<T>(address, ct);

        return new UniTask<T>()
            
    }
    
    UniTask<T> GetOrLoadAddressableAsset<T>(AssetReference reference, [Optional] CancellationToken ct)
        => internalManager.GetOrLoadAddressableAsset<T>(reference, ct);
}