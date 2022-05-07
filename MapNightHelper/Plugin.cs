using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Reflection;
using Dalamud.Game.Gui;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState;
using Dalamud.Game;
using System.Threading;
using ImGuiNET;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.DrunkenToad;
using System.Collections.Specialized;

namespace MapNightHelper
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Map Night Helper";

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static GameGui GameGui { get; private set; } = null!;
        [PluginService] public static Dalamud.Data.DataManager DataManager { get; private set; } = null!;
        [PluginService] public static FateTable FateTable { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        [PluginService] public static DalamudPluginInterface DalamudPluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;

        public class MapLink
        {
            public MapLink(string name, string location, MapLinkPayload link)
            {
                this.name = name;
                this.location = location;
                this.link = link;
            }

            public string name;
            public string location;
            public MapLinkPayload link;
        }

        public Dictionary<string, List<MapLink>> Links = new();

        //private List<MapLink>? links = null;
        //public List<MapLink> Links => links ??= new List<MapLink>();


        public Plugin()
        {
            Configuration = DalamudPluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(DalamudPluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
            PluginUi = new PluginUI(Configuration, this);

            _ = CommandManager.AddHandler("/xlmn", new CommandInfo(OnMapNightCommand)
            {
                HelpMessage = "Opens the map night links window",
                ShowInHelp = true
            });

            Chat.ChatMessage += PollChatMessage;

            DalamudPluginInterface.UiBuilder.Draw += DrawUI;
        }

        public void PollChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!PluginUi.TrackLinks)
            {
                return;
            }
            Payload? link = message.Payloads.Find(i => i.Type == PayloadType.MapLink);
            if (link != null)
            {
                if (!Links.ContainsKey(((MapLinkPayload)link).PlaceName))
                {
                    Links.Add(((MapLinkPayload)link).PlaceName, new());
                }
                Links[((MapLinkPayload)link).PlaceName].Add(new MapLink(sender.TextValue[1..], ((MapLinkPayload)link).PlaceName, (MapLinkPayload)link));
            }
        }

        private void OnMapNightCommand(string command, string arguments)
        {
            DrawMapNightUI();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Won't change")]
        public void Dispose()
        {
            PluginUi.Dispose();
            Chat.ChatMessage -= PollChatMessage;
            _ = CommandManager.RemoveHandler("/xlmn");
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }

        private void DrawMapNightUI()
        {
            PluginUi.ChatLinksVisible = true;
        }
    }
}
