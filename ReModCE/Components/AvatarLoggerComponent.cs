using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using VRC.Core;
using VRC;

namespace ReModCE.Components
{
    internal class AvatarLoggerComponent : ModComponent
    {
        private ConfigValue<bool> AvatarLoggerEnabled;
        private ConfigValue<string> AvatarLoggerExternalCheckFile;
        private ReMenuToggle _AvatarLoggerToggle;
        private readonly List<string> seenAvatars = new();
        private long lastUpdateFrame = 0;
        private string[] ignoredIds = new string[] { "avtr_c38a1615-5bf5-42b4-84eb-a8b6c37cbd11" };

        public AvatarLoggerComponent()
        {
            AvatarLoggerEnabled = new ConfigValue<bool>(nameof(AvatarLoggerEnabled), true);
            AvatarLoggerEnabled.OnValueChanged += () => _AvatarLoggerToggle.Toggle(AvatarLoggerEnabled);
            AvatarLoggerExternalCheckFile = new ConfigValue<string>(nameof(AvatarLoggerExternalCheckFile), "");

            if (!string.IsNullOrEmpty(AvatarLoggerExternalCheckFile) && File.Exists(AvatarLoggerExternalCheckFile))
            {
                ReLogger.Msg(ConsoleColor.Cyan, "Using additional file for avatar id existence checks: " + AvatarLoggerExternalCheckFile);
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Logging");
            _AvatarLoggerToggle = menu.AddToggle("Avatar Logs",
                "Enable whether any new avatar spotted should be logged to file.", AvatarLoggerEnabled.SetValue,
                AvatarLoggerEnabled);
        }

        public override void OnUpdate()
        {
            if (!AvatarLoggerEnabled) return;

            if (lastUpdateFrame < 1000)
            {
                lastUpdateFrame++;
                return;
            }
            lastUpdateFrame = 0;

            var playerManager = PlayerManager.field_Private_Static_PlayerManager_0;
            if (playerManager == null) return;

            foreach (var player in playerManager.GetPlayers())
            {
                var avatar = player.GetApiAvatar();
                if (avatar == null) continue;

                if (ignoredIds.Contains(avatar.id) || avatar.id.StartsWith("local:"))
                {
                    seenAvatars.Add(avatar.id);
                    continue;
                }

                if (!seenAvatars.Contains(avatar.id))
                {
                    seenAvatars.Add(avatar.id);
                    if (avatar.releaseStatus == "private") continue;

                    SaveAvatarId(avatar, player);
                }
            }
        }

        // @TODO figure how to implement this in ReMod.cs...
        /*public override bool OnDownloadAvatar(ApiAvatar apiAvatar)
        {
            ReLogger.Msg(ConsoleColor.White, "OnDownloadAvatar called for Avatar ID: " + apiAvatar.id);

            if (AvatarLoggerEnabled && apiAvatar.releaseStatus != "private")
            {
                SaveAvatarId(apiAvatar);
            }

            return base.OnDownloadAvatar(apiAvatar);
        }*/

        private void SaveAvatarId(ApiAvatar avatar, Player player = null)
        {
            string file = "UserData/ReModCE/avatars/" + (DateTime.Now).ToString("yyyy-MM-dd") + ".txt";
            string line = avatar.id + " " + avatar.name + (player != null ? " (" + player.field_Private_APIUser_0.displayName + ")" : "") + " - https://vrchat.com/home/avatar/" + avatar.id;

            if (!Directory.Exists("UserData/ReModCE/avatars")) Directory.CreateDirectory("UserData/ReModCE/avatars");

            if (File.Exists(file))
            {
                if (File.ReadAllText(file).Contains(avatar.id)) return;
            }

            if (!string.IsNullOrEmpty(AvatarLoggerExternalCheckFile) && File.Exists(AvatarLoggerExternalCheckFile))
            {
                if (File.ReadAllText(AvatarLoggerExternalCheckFile).Contains(avatar.id)) return;
            }

            using (StreamWriter sw = File.AppendText(file))
            {
                sw.WriteLine(line);
                ReLogger.Msg(ConsoleColor.Cyan, "Saving avatar ID: " + avatar.id);
            }
        }
    }
}
