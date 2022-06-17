using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using VRC.Core;

namespace ReModCE.Components
{
    internal class WorldLoggerComponent : ModComponent
    {
        private ConfigValue<bool> WorldLoggerEnabled;
        private ReMenuToggle _WorldLoggerToggle;
        private readonly List<string> seenWorlds = new();

        public WorldLoggerComponent()
        {
            WorldLoggerEnabled = new ConfigValue<bool>(nameof(WorldLoggerEnabled), true);
            WorldLoggerEnabled.OnValueChanged += () => _WorldLoggerToggle.Toggle(WorldLoggerEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Logging");
            _WorldLoggerToggle = menu.AddToggle("World Logs",
                "Enable whether any new world entered should be logged to file.", WorldLoggerEnabled.SetValue,
                WorldLoggerEnabled);
        }

        public override void OnEnterWorld(ApiWorld world, ApiWorldInstance instance)
        {
            if (!WorldLoggerEnabled) return;

            if (!seenWorlds.Contains(world.id))
            {
                seenWorlds.Add(world.id);

                SaveWorldId(world);
            }
        }
        
        private void SaveWorldId(ApiWorld world)
        {
            string file = "UserData/ReModCE/worlds/" + (DateTime.Now).ToString("yyyy-MM-dd") + ".txt";
            string line = $"{world.name} - https://vrchat.com/home/world/{world.id}";

            if (!Directory.Exists("UserData/ReModCE/worlds"))
                Directory.CreateDirectory("UserData/ReModCE/worlds");

            if (File.Exists(file))
                if (File.ReadAllText(file).Contains(world.id)) return;

            using (StreamWriter sw = File.AppendText(file))
            {
                sw.WriteLine(line);
                ReLogger.Msg(ConsoleColor.Cyan, $"Saving world ID: {world.id}");
            }
        }
    }
}
