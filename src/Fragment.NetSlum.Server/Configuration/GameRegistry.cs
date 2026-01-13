using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace Fragment.NetSlum.Server.Configuration
{
    public class GameRegistry
    {
        private readonly ConcurrentDictionary<string, GameDefinition> _map = new(StringComparer.OrdinalIgnoreCase);

        public GameRegistry(IHostEnvironment env, string fileName = "serverGames.json")
        {
            var path = Path.Combine(env.ContentRootPath, fileName);
            if (!File.Exists(path))
                return;

            try
            {
                var json = File.ReadAllText(path);
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var list = JsonSerializer.Deserialize<List<GameDefinition>>(json, opts) ?? new List<GameDefinition>();
                foreach (var g in list)
                {
                    if (string.IsNullOrWhiteSpace(g.Name))
                        continue;

                    // Register by canonical name
                    _map[g.Name] = g;

                    // Register aliases (if any) to point to the same definition
                    if (g.Aliases != null)
                    {
                        foreach (var a in g.Aliases)
                        {
                            if (string.IsNullOrWhiteSpace(a))
                                continue;

                            // avoid clobbering existing explicit entries
                            _map.TryAdd(a, g);
                        }
                    }
                }
            }
            catch
            {
                // swallow parse errors; registry will be empty
            }
        }

        public IEnumerable<GameDefinition> AllGames => _map.Values;

        public bool TryGet(string name, out GameDefinition? def)
        {
            if (name == null) { def = null; return false; }
            return _map.TryGetValue(name, out def!);
        }
    }
}
