using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Animations;
using Data;
using Data.ChannelTargets;
using Misc;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewEmulatorSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewEmulatorSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_EMULATOR)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            ChannelTarget channelTarget = channel.GetTarget();
            if (channelTarget is HasGameMetadata hasMetadata)
            {
                if (channelTarget is ChannelEmulatorTarget target)
                {
                    OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.emulatorargs", true);
                    if (Application.isMobilePlatform)
                    {
                        // we setup the emulator app
                        GameObject app = Spawn(_instance.appSourcePrefab);
                        if (channel.info != null)
                        {
                            app.transform.Find("Image").GetComponent<RawImage>().texture = channel.info.icon;
                            app.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text =
                                channel.info.name;

                            // now we check if the emulator is ours, if not we need to spawn the activity selector
                            if (!target.IsSupportedByDefault())
                            {
                                if (target.IsRetroArch())
                                {
                                    Option extra = Option.Create("settings.target.retrocore",
                                        () => target.retroarchCore, x => target.SetRetroCore(x));
                                    OptionsSpawner.SetupOption(extra, _instance.optionsHolder);
                                }
                                else
                                {
                                    // we setup the activity selector. defaults to.. default
                                    List<string> activities = new List<string>();
                                    activities.Add("Default");
                                    activities.AddRange(AndroidLinker.GetPackageActivities(channel.info.packageName));
                                    Option<string> allowedActivities = Option.Create("settings.target.activity",
                                        () => target.overrideAndroidActivity ?? "Default", x =>
                                        {
                                            // we set the new activity
                                            target.SetActivity(x.Equals("Default") ? null : x);
                                            _instance.setupOverviewAppSettingView.Show(channel);
                                        });
                                    OptionsSpawner.SummonOption_DropdownString(allowedActivities,
                                        _instance.optionsHolder, activities);

                                    Option extra = Option.Create("settings.target.extra",
                                        () => target.overrideAndroidExtra, x => target.SetExtra(x));
                                    OptionsSpawner.SetupOption(extra, _instance.optionsHolder);
                                }
                            }
                        }

                        app.transform.Find("NameAndChange").Find("Button").GetComponent<Button>().onClick
                            .AddListener(() => _instance.setupAppSettingView.Show(channel));
                    }
                    else
                    {
                        // we setup the file path with the startup args
                        GameObject emulator = Spawn(_instance.pathSourcePrefab);
                        emulator.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text =
                            target.emulatorPath ?? "None";
                        emulator.transform.Find("NameAndChange").Find("Button").GetComponent<Button>().onClick
                            .AddListener(() => _instance.setupEmulatorPathSettingView.OverrideApp(channel));

                        Option args = Option.Create("settings.target.args", () => target.startupArgs,
                            x => target.SetStartupArgs(x));
                        OptionsSpawner.SetupOption(args, _instance.optionsHolder);
                    }
                }
                
                // now we setup the game
                SetupGame(channel, hasMetadata);
            }

            SpawnDefaultOptions();
        }

        protected void SetupGame(Channel channel, HasGameMetadata target)
        {
            // now we setup the game's path
            GameMetadata metadata = target.GetGameMetadata();
            bool isSteam = target is ChannelSteamTarget;
            
            OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.gameargs", true);
            GameObject path = Spawn(_instance.pathSourcePrefab);
            path.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text =
                metadata != null ? (isSteam ? metadata.title : metadata.filePath) : "None";
            path.transform.Find("NameAndChange").Find("Button").GetComponent<Button>().onClick
                .AddListener(() =>
                {
                    AbstractSetupSettingView setup = isSteam ? _instance.setupSteamView : _instance.setupEmulatorSettingView;
                    setup.Show(channel);
                });

            if (metadata != null)
            {
                if (!isSteam)
                {
                    // we setup the console type, game ID and box color
                    Option<string> platform = Option.Create("settings.target.platform",
                        () => metadata.console?.name ?? "Unknown", x =>
                        {
                            Console console = CONSOLES.GetConsole(x);
                            metadata.SetConsole(console);
                            // and now we try to find a proper app for this console
                            _instance.setupEmulatorPathSettingView.Show(channel);
                        });
                    OptionsSpawner.SummonOption_DropdownString(platform, _instance.optionsHolder,
                        CONSOLES.GetAllConsoleIds());
                }

                // we setup the cover source
                Dictionary<string, COVERSOURCE> sourceNames = GenerateCoverStrings(isSteam);
                string currentCover = GetCurrentCoverString(metadata, sourceNames);
                Option<string> coverSource = Option.Create("settings.target.coversource", () => currentCover, x =>
                {
                    COVERSOURCE source = sourceNames[x];
                    metadata.SetCoverSource(source);
                    // and we reload it to setup the new params
                    _instance.setupOverviewEmulatorSettingView.Show(channel);
                });
                OptionsSpawner.SummonOption_DropdownString(coverSource, _instance.optionsHolder, sourceNames.Keys.ToList());

                // if it's a custom one, then we spawn the image selectors
                if (metadata.coverSource == COVERSOURCE.CUSTOM)
                {
                    // first the full one, if it supports it
                    if (metadata.SupportsFullTextures())
                    {
                        SpawnCoverButton(metadata, "full");
                    }

                    // then the front and back
                    SpawnCoverButton(metadata, "front");
                    SpawnCoverButton(metadata, "back");
                }

                if (!isSteam)
                {
                    // we setup game id
                    string idName = metadata.coverSource == COVERSOURCE.GAMETDB || metadata.coverSource == COVERSOURCE.STEAM
                        ? "settings.target.gameid"
                        : "settings.target.gamename";
                    Option id = Option.Create(idName, () => metadata.gameId, x => metadata.SetGameId(x));
                    OptionsSpawner.SetupOption(id, _instance.optionsHolder);
                }

                // finally we spawn box color + whether it should rotate
                Option boxColor = Option.Create("settings.target.boxcolor", () => metadata.boxColor,
                    x => metadata.SetBoxColor(x));
                OptionsSpawner.SetupOption(boxColor, _instance.optionsHolder);
                Option rotateBox = Option.Create("settings.target.rotatebox", () => metadata.rotateBox,
                    x => metadata.rotateBox = x);
                OptionsSpawner.SetupOption(rotateBox, _instance.optionsHolder);
            }
        }

        private void SpawnCoverButton(GameMetadata data, string suffix)
        {
            GameObject full = Spawn(_instance.coverSourcePrefab);
            full.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text =
                TextController.GetTranslation("settings.target.cover" + suffix);
            full.transform.Find("NameAndChange").Find("Buttons").Find("Change").GetComponent<Button>().onClick
                .AddListener(() => RequestNewCover(data,suffix));
            full.transform.Find("NameAndChange").Find("Buttons").Find("Delete").GetComponent<Button>().onClick
                .AddListener(() => DeleteCover(data,suffix));
            RawImage image = full.transform.Find("Image").GetComponent<RawImage>();
            data.GetCover(suffix, tex =>
            {
                if (tex != null)
                {
                    image.texture = tex;
                }
            });
        }

        private void RequestNewCover(GameMetadata data, string suffix)
        {
            FileManager.RequestFile("Select a new cover", FILETYPE.IMAGES_ONLY, path =>
            {
                // We just save it in our cache
                // todo we should also delete this when deleting the channel!
                string savePath = SaveManager.SAVE_FOLDER_GAMECOVERS + data.GetCoverPath(suffix);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
                FileManager.CopyFile(path, savePath, false);
                data.SetCover(suffix, null);
                
                // And we reload the menu! it'll automatically bring the new data here
                _instance.setupOverviewEmulatorSettingView.Show(data.linkedChannel);
            });
        }
        
        private void DeleteCover(GameMetadata data, string suffix)
        {
            // we delete it and reload the menu
            string savePath = SaveManager.SAVE_FOLDER_GAMECOVERS + data.GetCoverPath(suffix);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            data.SetCover(suffix, null);
            _instance.setupOverviewEmulatorSettingView.Show(data.linkedChannel);
        }
        
        public override void ClickedBack()
        {
            // we reset the metadata of the channel (so we don't download it 500 times)
            if (currentChannel != null && currentChannel.target is ChannelEmulatorTarget target)
            {
                target.metadata?.RefreshMetadata();
            }
            
            base.ClickedBack();
        }
        
        private Dictionary<string,COVERSOURCE> GenerateCoverStrings(bool isSteam)
        {
            Dictionary<string, COVERSOURCE> covers = new Dictionary<string, COVERSOURCE>();
            if (isSteam)
            {
                covers.Add("Steam", COVERSOURCE.STEAM);
            }
            else
            {
                covers.Add("GameTDB", COVERSOURCE.GAMETDB);
                covers.Add("LibRetro", COVERSOURCE.LIBRETRO);
            }
            covers.Add("Custom", COVERSOURCE.CUSTOM);
            return covers;
        }

        private string GetCurrentCoverString(GameMetadata data, Dictionary<string,COVERSOURCE> sourceNames)
        {
            foreach (string str in sourceNames.Keys)
            {
                if (sourceNames[str] == data.coverSource)
                {
                    return str;
                }
            }

            return "Unknown";
        }
    }
}