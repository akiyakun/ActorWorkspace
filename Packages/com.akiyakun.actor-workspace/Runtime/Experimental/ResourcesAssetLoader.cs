using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace
{
    public class ResourcesAssetLoader<AssetT>
        where AssetT : Object
    {
        public string BasePath = "";

        Dictionary<string, AssetT> cache = new();

        public ResourcesAssetLoader()
        {
        }

        public ResourcesAssetLoader(string basePath)
        {
            BasePath = basePath;
        }

        public AssetT LoadAsset(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "Asset name cannot be null or empty.");

            // string path = Utility.PathCombine(BasePath, Utility.GetPathWithoutExtension(name));
            string path = Utility.PathCombine(BasePath, name);

            if (cache.TryGetValue(path, out AssetT asset))
            {
                return asset;
            }
            else
            {
                var newAsset = Resources.Load<AssetT>(path);
                Debug.Assert(newAsset != null, $"Failed to load asset from: Resources {path}");

                if (newAsset != null)
                {
                    cache.Add(path, newAsset);
                }
            }

            return null;
        }
    }
}
