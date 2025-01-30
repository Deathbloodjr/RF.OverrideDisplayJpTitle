using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static MusicDataInterface;

namespace OverrideDisplayJpTitle.Plugins
{
    internal class OverridePatch
    {
        static Dictionary<string, bool> Overrides = new Dictionary<string, bool>();


        public static void InitializeSongsToOverride()
        {
            var jsonPath = Plugin.Instance.ConfigOverridesJsonPath.Value;
            if (!File.Exists(jsonPath))
            {
                // Create a blank one with an example entry
                CreateDefaultJson();
            }

            try
            {
                var node = JsonNode.Parse(File.ReadAllText(jsonPath));
                var array = node.AsArray();
                for (int i = 0; i < array.Count; i++)
                {
                    var songId = array[i]["SongId"].GetValue<string>();
                    var displayJpTitle = array[i]["DisplayJpTitle"].GetValue<bool>();
                    if (!Overrides.ContainsKey(songId))
                    {
                        Overrides.Add(songId, displayJpTitle);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, LogType.Error);
                throw;
            }
        }

        public static void CreateDefaultJson()
        {
            JsonArray node = new JsonArray();


            List<(string songId, bool displayJpTitle)> defaultOverrides = new List<(string, bool)>()
            {
                ("mmkami", true),
                ("clsaip", true),
                ("clscif", true),
                ("clsmrs", true),
                ("clscpr", true),
                ("clspvn", true),
                ("clskum", true),
                ("clsca", true),
                ("cls10", true),
                ("clsrou", true),
                ("jaznoc", true),
                ("cls7", true),
                ("jazmen", true),
                ("whowho", false),
                ("lm7708", false),
                ("shabra", true),
                ("ekiben", false),
            };

            for (int i = 0; i < defaultOverrides.Count; i++)
            {
                JsonObject entry = new JsonObject()
                {
                    ["SongId"] = defaultOverrides[i].songId,
                    ["DisplayJpTitle"] = defaultOverrides[i].displayJpTitle,
                };
                node.Add(entry);
            }


            var jsonPath = Plugin.Instance.ConfigOverridesJsonPath.Value;
            if (!Directory.Exists(Path.GetDirectoryName(jsonPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            }

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };

            File.WriteAllText(jsonPath, node.ToJsonString(options));
        }


        [HarmonyPatch(typeof(MusicInfoAccesser))]
        [HarmonyPatch(nameof(MusicInfoAccesser.SetMusicInfo))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void MusicInfoAccesser_SetMusicInfo_Postfix(MusicInfoAccesser __instance, MusicDataInterface.MusicInfo musicinfo, int katsuMask)
        {
            if (Overrides.ContainsKey(__instance.Id))
            {
                __instance.IsDispJpSongName = Overrides[__instance.Id];
            }
        }
    }
}
