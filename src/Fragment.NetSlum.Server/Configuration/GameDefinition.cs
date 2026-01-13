using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fragment.NetSlum.Server.Configuration
{
    public class GameDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("mode")]
        public GameMode Mode { get; set; }

        // Optional per-game common key to use instead of the global Crypto.CommonKey
        [JsonPropertyName("commonKey")]
        public string? CommonKey { get; set; }

        // Optional list of aliases or short names for this game (e.g. "SBOL" -> "Shutokou Battle Online")
        [JsonPropertyName("aliases")]
        public List<string>? Aliases { get; set; }

        // Optional list of compatibility adapters to enable for this game
        [JsonPropertyName("compatibility")]
        public List<string>? Compatibilities { get; set; }
    }
}
