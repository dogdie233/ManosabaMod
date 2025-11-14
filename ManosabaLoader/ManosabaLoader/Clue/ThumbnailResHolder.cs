using System;

using BepInEx.Logging;

using Naninovel;

using UnityEngine;

using WitchTrials.Views;

using Logger = BepInEx.Logging.Logger;

namespace ManosabaLoader.Clue;

public class ThumbnailResHolder : MonoBehaviour
{
    private static ManualLogSource logger = Logger.CreateLogSource(nameof(ThumbnailResHolder));
    private ResourceLoader<Texture2D> loader;
    private WitchBookItemThumbnail item;
    private Resource<Texture2D> holdingRes;

    private void Start()
    {
        loader = ModResourceLoader.ThumbnailLoader;
        item = GetComponent<WitchBookItemThumbnail>();
    }

    public void OnSetTexture(Texture2D texture)
    {
        if (holdingRes is { Valid: true } && texture != holdingRes.Object)
            DisposeLoadedRes();
    }

    public void SetupCustomThumbnail(string path)
    {
        if (holdingRes is { Valid: true, Object: not null } && holdingRes.Path == path)
        {
            logger.LogDebug($"Thumbnail from path: {path} is already loaded.");
            return;
        }
        
        logger.LogDebug($"Setting up custom thumbnail from path: {path}");
        loader.Load(path, this)
            .ContinueWith(new Action<Resource<Texture2D>>(res =>
            {
                DisposeLoadedRes();
                if (!res.Valid || res.Object == null)
                {
                    logger.LogWarning($"Failed to load thumbnail from path: {path}");
                    item.SetTexture(item._defaultTexture.Cast<Texture2D>());
                    return;
                }
                
                logger.LogDebug($"Successfully loaded thumbnail from path: {path}");
                holdingRes = res;
                item.SetTexture(res.Object);
            }));
    }

    private void OnDestroy()
    {
        DisposeLoadedRes();
    }

    private void DisposeLoadedRes()
    {
        if (holdingRes == null)
            return;
        
        logger.LogDebug($"Releasing loaded thumbnail resource from path: {holdingRes.Path}");
        loader.Release(holdingRes.Path, this);
        holdingRes = null;
    }
}