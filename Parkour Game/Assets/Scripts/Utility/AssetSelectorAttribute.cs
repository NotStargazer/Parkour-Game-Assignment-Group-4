using UnityEngine;

namespace Utility
{
    public class AssetSelectorAttribute : PropertyAttribute
    {
        public readonly string Path;
        public readonly string AssetType;
        public AssetSelectorAttribute(string path, string assetType)
        {
            Path = path;
            AssetType = assetType;
        }

    }
}