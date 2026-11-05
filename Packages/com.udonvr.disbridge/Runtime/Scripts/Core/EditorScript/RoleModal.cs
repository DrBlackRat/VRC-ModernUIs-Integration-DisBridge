//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using Newtonsoft.Json;
using UnityEngine;

namespace UdonVR.DisBridge
{
    public class RoleModal
    {
        public string roleId;
        public string roleName;
        public string roleColor;
        public bool isBooster;

        public Color32 GetColor()
        {
            ColorUtility.TryParseHtmlString(roleColor, out Color color);
            return color;
        }
    }

    public class DisbridgeKey
    {
        public string key;
        [JsonProperty("keyJsonUrl")] public string keyJsonUrl;
        public string serverName;
        public RoleModal[] roles;
        public int version;
        [JsonProperty("fallbackUrl")] public string fallbackUrl;
    }

    public class PackageInfo
    {
        [JsonProperty("version")] public string version;
    }
}