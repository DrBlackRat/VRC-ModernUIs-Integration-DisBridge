//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UdonVR.DisBridge.Editors.JsonImport
{
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class Role
    {
        [JsonProperty("SuppRoleId")]
        public string SuppRoleId { get; }

        [JsonProperty("Members")]
        public List<string> Members { get; } = new List<string>();
    }

    public class Root
    {
        [JsonProperty("Roles")]
        public List<Role> Roles { get; } = new List<Role>();

        [JsonProperty("Mods")]
        public JToken Mods { get; }
    }


}