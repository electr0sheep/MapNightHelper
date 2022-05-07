using ImGuiNET;
using System;
using System.Numerics;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dalamud.DrunkenToad;
using System.Linq;

namespace MapNightHelper
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Won't change")]
        private Configuration Configuration { get; init; }
        private Plugin Plugin { get; init; }

        private bool trackLinks = false;
        private bool chatLinksVisible = false;

        public bool TrackLinks
        {
            get => trackLinks;
            set => trackLinks = value;
        }

        public bool ChatLinksVisible
        {
            get => chatLinksVisible;
            set => chatLinksVisible = value;
        }

        public PluginUI(Configuration configuration, Plugin plugin)
        {
            Configuration = configuration;
            Plugin = plugin;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.
            DrawChatLinksWindow();
        }

        public void DrawChatLinksWindow()
        {
            if (!chatLinksVisible)
            {
                return;
            }
            try
            {
                ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
                if (ImGui.Begin("Map Night Helper", ref chatLinksVisible))
                {
                    _ = ImGui.Checkbox("Monitor party chat for map links", ref trackLinks);
                    ImGui.Text("");
                    for (int i2 = 0; i2 < Plugin.Links.Count; i2++)
                    {
                        KeyValuePair<string, List<Plugin.MapLink>> element = Plugin.Links.ElementAt(i2);
                        string location = element.Key;
                        if (ImGui.CollapsingHeader($"{location} ({Plugin.Links[location].Count})###{location}"))
                        {
                            if (ImGui.BeginTable("Links", 3, ImGuiTableFlags.Resizable))
                            {
                                ImGui.TableSetupColumn("Name");
                                ImGui.TableSetupColumn("Link");
                                ImGui.TableSetupColumn("");
                                ImGui.TableHeadersRow();
                                for (int i = 0; i < Plugin.Links[location].Count; i++)
                                {
                                    Plugin.MapLink link = Plugin.Links[location][i];
                                    ImGui.TableNextRow();
                                    _ = ImGui.TableNextColumn();
                                    ImGui.Text(link.name);
                                    _ = ImGui.TableNextColumn();
                                    if (ImGui.Button($"{link.link.PlaceName} {link.link.CoordinateString}"))
                                    {
                                        _ = Plugin.GameGui.OpenMapWithMapLink(link.link);
                                    }
                                    _ = ImGui.TableNextColumn();
                                    if (ImGui.Button($"Remove###{i}"))
                                    {
                                        Plugin.Links[location].RemoveAt(i);
                                        if (!Plugin.Links[location].Any())
                                        {
                                            Plugin.Links.Remove(location);
                                        }
                                    }
                                }
                                ImGui.EndTable();
                            }
                        }
                    }
                    ImGui.End();
                }
            }
            catch (KeyNotFoundException)
            {
                // this happens when we remove the last item in a list for a given key in the dictionary
                // we don't want to crash, just keep rendering
            }
        }
    }
}
