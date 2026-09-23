using System;
using System.Collections;
using System.IO;
using Animations;
using Data.ChannelTargets;
using Misc;
using SFB;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
	public class SetupEmulatorSettingView : AbstractSetupSettingView
	{
		public SetupEmulatorSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_EMULATOR)
		{
		}

		public override void Show(Channel channel)
		{
			base.Show(channel);

			// we default to Revo's VC animations
			channel.SetIconAnimation(ChannelController._instance.GetOrLoadChannelAnimation(SaveManager.boxArtIcon));
			channel.SetBannerAnimation(ChannelController._instance.GetOrLoadChannelAnimation(SaveManager.boxArtBanner));

			// and we load a file picker to choose the file
			if (!Application.isMobilePlatform)
			{
				LoadFilePicker(path =>
				{
					GenerateData(path);
				});
			}
			else
			{
				// Time to cheat. Thanks Android.
				ShowLoadDialog();
			}
		}

		private void GenerateData(string originalPath)
		{
			if (currentChannel.GetTarget() is ChannelEmulatorTarget target)
			{
				// we generate the metadata
				GameMetadata metadata;
				try
				{
					metadata = MetadataController._instance.GenerateMetadata(originalPath, currentChannel);
				}
				catch (Exception e)
				{
					// an error generating the metadata. let's abort
					metadata = null;
				}
				
				target.metadata = metadata;

				// we try to assign the app
				_instance.setupEmulatorPathSettingView.Show(currentChannel);
			}
		}

		public void ShowLoadDialog()
		{
			SettingsController._instance.StartCoroutine(ShowLoadDialogCoroutine());
		}
		
		private IEnumerator ShowLoadDialogCoroutine()
		{
			FileBrowser.SingleClickMode = false;
			FadeController._instance.FadeIn(0.1f);
			yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select your game file!", "Select");

			// Dialog is closed now
			FadeController._instance.FadeOut(0.1f);

			if (FileBrowser.Success)
			{
				PopupController.ShowPopup("popup.loadinggame", null);
				yield return new WaitForSeconds(0.6f);
				
				// We debug the path we've chosen
				string originalPath = FileBrowser.Result[0];
				Debug.Log($"The path chosen for the game is: {originalPath}");

				// Read the bytes of the first file via FileBrowserHelpers
				// Contrary to File.ReadAllBytes, this function works on Android 10+, as well
				// byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

				// We copy it to persistentDataPath in order to be able to read it
				// string destinationPath = Path.Combine(SaveManager.TEMP_GAMES, FileBrowserHelpers.GetFilename(originalPath));
				// FileBrowserHelpers.CopyFile(originalPath, destinationPath);
				
				// And we finish it
				// In theory now we have perms to read from this path... let's try
				PopupController.ClosePopup();
				GenerateData(originalPath);
			}
			else
			{
				ClickedBack();
			}
			
		}
	}
}