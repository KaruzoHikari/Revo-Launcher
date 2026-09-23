using System;
using System.Collections;
using System.IO;
using Animations;
using Data;
using Data.ChannelTargets;
using Misc;
using SFB;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
	public class SetupSteamSettingView : AbstractSetupSettingView
	{
		public SetupSteamSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_STEAM, ThemeColor.SettingsBackgroundLight)
		{
		}

		public override void Show(Channel channel)
		{
			base.Show(channel);

			// we default to Revo's VC animations
			channel.SetIconAnimation(ChannelController._instance.GetOrLoadChannelAnimation(SaveManager.boxArtIcon));
			channel.SetBannerAnimation(ChannelController._instance.GetOrLoadChannelAnimation(SaveManager.boxArtBanner));

			// we ask for Steam's file location if it doesn't exist. otherwise we just load the game picker
			string loc = PREFS.SteamLocation.GetString();
			if (string.IsNullOrEmpty(loc))
			{
				// first we try to see if it's installed in the default path
				string def = @"C:\Program Files (x86)\Steam\steam.exe";
				if (File.Exists(def))
				{
					Debug.Log("Found Steam in the default path!");
					PREFS.SteamLocation.SetString(def);
					loc = def;
				}
			}
			
			// we check again
			if (string.IsNullOrEmpty(loc))
			{
				PopupController.ShowPopup("popup.findsteam", () =>
				{
					LoadFilePicker(path =>
					{
						PREFS.SteamLocation.SetString(path);
						// todo holding button should reset its position
						SettingsController._instance.SetupSteamSelection_Editor(GenerateData);
					});
				});
			}
			else
			{
				SettingsController._instance.SetupSteamSelection_Editor(GenerateData);
			}
		}

		private void GenerateData(SteamGame game)
		{
			if (currentChannel.GetTarget() is ChannelSteamTarget target)
			{
				// we generate the metadata
				GameMetadata metadata = MetadataController._instance.GenerateSteamMetadata(game, currentChannel);
				target.metadata = metadata;

				// initialize the channel
				if (!currentChannel.isFullySetup)
				{
					currentChannel.isFullySetup = true;
					ChannelController._instance.InitializeNewChannel(currentChannel);
					_instance.ExitSettings(true, false);
                
					// and we refresh the metadata just in case!
					target.metadata?.RefreshMetadata();
				}
				else
				{
					// we just go back to edit
					BackToEdit();
				}
			}
		}
	}
}