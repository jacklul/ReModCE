using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace ReModCE.Components
{
    internal class AvatarLoggerComponent : ModComponent
    {
        private ConfigValue<bool> AvatarLoggerEnabled;
        private ConfigValue<string> AvatarLoggerExternalCheckFile;
        private ReMenuToggle _AvatarLoggerToggle;
        private readonly List<string> seenAvatars = new();
        private string[] ignoredIds = new string[] { "avtr_c38a1615-5bf5-42b4-84eb-a8b6c37cbd11" };

        public AvatarLoggerComponent()
        {
            AvatarLoggerEnabled = new ConfigValue<bool>(nameof(AvatarLoggerEnabled), true);
            AvatarLoggerEnabled.OnValueChanged += () => _AvatarLoggerToggle.Toggle(AvatarLoggerEnabled);
            AvatarLoggerExternalCheckFile = new ConfigValue<string>(nameof(AvatarLoggerExternalCheckFile), "");
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Logging");
            _AvatarLoggerToggle = menu.AddToggle("Avatar Logs",
                "Enable whether any new avatar spotted should be logged to file.", AvatarLoggerEnabled.SetValue,
                AvatarLoggerEnabled);
        }

        /*
        private long lastUpdateFrame;
        
        public override void OnLateUpdate()
        {
            if (!AvatarLoggerEnabled) return;

            if (lastUpdateFrame < 10)
            {
                lastUpdateFrame++;
                return;
            }
            lastUpdateFrame = 0;

            PlayerManager playerManager = PlayerManager.field_Private_Static_PlayerManager_0;
            if (playerManager == null) return;

            foreach (Player player in playerManager.GetPlayers())
            {
                ApiAvatar avatar = player.GetApiAvatar();
                if (avatar == null) continue;

                SaveAvatarId(avatar, player);
            }
        }*/

        /*public override void OnPlayerJoined(Player player)
        {
            if (!AvatarLoggerEnabled) return;

            ApiAvatar avatar = player.GetApiAvatar();

            if (avatar != null)
                SaveAvatarId(avatar, player);
        }

        public override void OnPlayerLeft(Player player)
        {
            if (!AvatarLoggerEnabled) return;

            ApiAvatar avatar = player.GetApiAvatar();

            if (avatar != null)
                SaveAvatarId(avatar, player);
        }*/

        public override void OnAvatarIsReady(VRCPlayer vrcPlayer)
        {
            Player player = vrcPlayer.GetPlayer();

            if (player == null) return;

            ApiAvatar avatar = player.GetApiAvatar();

            if (avatar != null)
                SaveAvatarId(avatar, player);
        }

        private void SaveAvatarId(ApiAvatar avatar, Player player = null)
        {
            if (seenAvatars.Contains(avatar.id) || ignoredIds.Contains(avatar.id) || avatar.id.StartsWith("local:")) return;

            seenAvatars.Add(avatar.id);
            if (avatar.releaseStatus == "private") return;

            string file = "UserData/ReModCE/avatars/" + (DateTime.Now).ToString("yyyy-MM-dd") + ".txt";
            string line = $"{avatar.id} {avatar.name}{(player != null ? $" ({player.field_Private_APIUser_0.displayName})" : "")} - https://vrchat.com/home/avatar/{avatar.id}";

            if (!Directory.Exists("UserData/ReModCE/avatars"))
                Directory.CreateDirectory("UserData/ReModCE/avatars");

            if (File.Exists(file))
                if (File.ReadAllText(file).Contains(avatar.id)) return;

            if (!string.IsNullOrEmpty(AvatarLoggerExternalCheckFile) && File.Exists(AvatarLoggerExternalCheckFile))
                if (File.ReadAllText(AvatarLoggerExternalCheckFile).Contains(avatar.id)) return;

            using (StreamWriter sw = File.AppendText(file))
            {
                sw.WriteLine(line);
                ReLogger.Msg(ConsoleColor.Cyan, $"Saving avatar ID: {avatar.id}{(player != null ? $" ({player.field_Private_APIUser_0.displayName})" : "")}");
            }
        }
    }
}
