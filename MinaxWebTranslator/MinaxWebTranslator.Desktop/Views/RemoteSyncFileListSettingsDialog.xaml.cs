using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Minax.Collections;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// RemoteSyncFileListSettings.xaml for remote Glossary FileList combobox settings management
	/// </summary>
	public partial class RemoteSyncFileListSettingsDialog : CustomDialog
	{
		internal int CustomGlossaryFileListCountMax { get; set; } = 10;

		internal MetroWindow ParentWindow { get; set; }

		internal ObservableList<string> CustomGlossaryFileListLocations { get; set; }

		public RemoteSyncFileListSettingsDialog()
		{
			InitializeComponent();
			LvCustom.Items.Clear();
		}

		private readonly MetroDialogSettings mInputSettings = new MetroDialogSettings {
			AffirmativeButtonText = Languages.Global.Str0Ok, DefaultButtonFocus = MessageDialogResult.Affirmative,
			DialogResultOnCancel = MessageDialogResult.Canceled, OwnerCanCloseWithDialog = true,
			NegativeButtonText = Languages.Global.Str0Cancel,
		};
		private HttpClient client = new HttpClient();
		private bool mListChanged = false;

		private void CustomDialog_Loaded( object sender, RoutedEventArgs e )
		{
			TbDefault.Text = Properties.Settings.Default.DefaultGlossaryFileListLocation;

			CustomGlossaryFileListLocations = ProjectManager.Instance.CustomGlossaryFileListLocations;
			LvCustom.ItemsSource = CustomGlossaryFileListLocations;

			LblCustomCount.Content = $"{CustomGlossaryFileListLocations.Count} / {CustomGlossaryFileListCountMax}";
		}

		private async void CustomDialogClose_Click( object sender, RoutedEventArgs e )
		{
			//if( ParentWindow == null )
			//	return;

			if( mListChanged == true ) {
				ProjectManager.Instance.SaveListToSettings();
			}

			if( ParentWindow != null )
				await ParentWindow.HideMetroDialogAsync( this );
			else
				this.OnClose();
		}

		private async Task<bool> _CheckAndTryToFetchFile( string fileListLink )
		{
			try {
				var uri = new Uri( fileListLink );

				// only accept http or https!!
				var response = await client.GetAsync( uri );
				if( response == null || response.IsSuccessStatusCode == false )
					return false;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					return false;

			}
			catch {
				return false;
			}

			return true;
		}

		private void _UpdateCount()
		{
			LblCustomCount.Content = $"{CustomGlossaryFileListLocations.Count} / {CustomGlossaryFileListCountMax}";
			mListChanged = true;
		}

		private async void BtnCustomAdd_Click( object sender, RoutedEventArgs e )
		{
			if( CustomGlossaryFileListLocations.Count >= CustomGlossaryFileListCountMax ) {
				ParentWindow.ShowModalMessageExternal( Languages.ProjectGlossary.Str0GlossaryFileListWarning, Languages.ProjectGlossary.Str0FileListCountHasReachMaximum );
				return;
			}

			var newFileList = ParentWindow.ShowModalInputExternal( Languages.ProjectGlossary.Str0AddGlossaryFileList,
											Languages.ProjectGlossary.Str0FileListLocation, mInputSettings );
			// show warning about OriginalText is all white spaces
			if( string.IsNullOrWhiteSpace( newFileList ) ) {
				// maybe cancelled!!
				//await ParentWindow.ShowMessageAsync( Languages.ProjectGlossary.Str0GlossaryFileListError, "File list location shall not be full of white space!!" );
				return;
			}
			if( CustomGlossaryFileListLocations.Contains( newFileList ) ) {
				ParentWindow.ShowModalMessageExternal( Languages.ProjectGlossary.Str0GlossaryFileListWarning, Languages.ProjectGlossary.Str0FileListLocationIsExisted );
				return;
			}

			// try to fetch file list text file
			if( false == await _CheckAndTryToFetchFile( newFileList ) ) {
				ParentWindow.ShowModalMessageExternal( Languages.ProjectGlossary.Str0GlossaryFileListError, Languages.ProjectGlossary.Str0FileListCantFetchOrInvalid );
				return;
			}

			CustomGlossaryFileListLocations.Add( newFileList );
			_UpdateCount();
		}

		private async void BtnCustomEdit_Click( object sender, RoutedEventArgs e )
		{
			var loc = LvCustom.SelectedItem as string;
			var idx = CustomGlossaryFileListLocations.IndexOf( loc );
			if( string.IsNullOrWhiteSpace( loc ) || idx < 0 )
				return;


			var modifySettings = new MetroDialogSettings {
				AffirmativeButtonText = Languages.Global.Str0Ok, DefaultButtonFocus = MessageDialogResult.Affirmative,
				DialogResultOnCancel = MessageDialogResult.Canceled, OwnerCanCloseWithDialog = true,
				NegativeButtonText = Languages.Global.Str0Cancel,
				DefaultText = loc
			};

			var newFileList = ParentWindow.ShowModalInputExternal( Languages.ProjectGlossary.Str0ModifyGlossaryFileList, Languages.ProjectGlossary.Str0FileListLocation, modifySettings );
			if( newFileList == loc )
				return;

			if( CustomGlossaryFileListLocations.Contains( newFileList ) ) {
				ParentWindow.ShowModalMessageExternal( Languages.ProjectGlossary.Str0GlossaryFileListWarning, Languages.ProjectGlossary.Str0FileListLocationIsExisted );
				return;
			}

			// show warning about OriginalText is all white spaces
			if( string.IsNullOrWhiteSpace( newFileList ) ) {
				// maybe cancelled!!
				//await ParentWindow.ShowMessageAsync( Languages.ProjectGlossary.Str0GlossaryFileListError, "File list location shall not be full of white space!!" );
				return;
			}

			if( false == await _CheckAndTryToFetchFile( newFileList ) ) {
				ParentWindow.ShowModalMessageExternal( Languages.ProjectGlossary.Str0GlossaryFileListError, Languages.ProjectGlossary.Str0FileListCantFetchOrInvalid );
				return;
			}

			CustomGlossaryFileListLocations[idx] = newFileList;
			mListChanged = true;
		}

		private void BtnCustomDelete_Click( object sender, RoutedEventArgs e )
		{
			var loc = LvCustom.SelectedItem as string;
			if( string.IsNullOrWhiteSpace( loc ) ||
				CustomGlossaryFileListLocations.Contains( loc ) == false )
				return;

			CustomGlossaryFileListLocations.Remove( loc );
			_UpdateCount();
		}

		private void BtnCustomMoveUp_Click( object sender, RoutedEventArgs e )
		{
			var idx = CustomGlossaryFileListLocations.IndexOf( LvCustom.SelectedItem as string );
			if( idx < 1 )
				return;

			CustomGlossaryFileListLocations.Move( idx, idx - 1 );
			mListChanged = true;
		}

		private void BtnCustomMoveDown_Click( object sender, RoutedEventArgs e )
		{
			var idx = CustomGlossaryFileListLocations.IndexOf( LvCustom.SelectedItem as string );
			if( idx < 0 || idx >= CustomGlossaryFileListLocations.Count - 1 )
				return;

			CustomGlossaryFileListLocations.Move( idx, idx + 1 );
			mListChanged = true;
		}
	}
}
