using Minax.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class RemoteSyncFileListSettingsPage : ContentPage
	{
		public const int CustomGlossaryFileListCountMax = 10;

		internal ObservableList<string> CustomGlossaryFileListLocations { get; private set; }

		public RemoteSyncFileListSettingsPage( MainPage mainPage )
		{
			mMainPage = mainPage;

			InitializeComponent();

			mVm = new ViewModels.BaseViewModel {
				IsDataEmpty = true,
			};
			this.BindingContext = mVm;

			LvCustom.ItemSelected += ( s1, e1 ) => {
				if( LvCustom.SelectedItem == null ) {
					BtnCustomDelete.IsEnabled = false;
					BtnCustomEdit.IsEnabled = false;
					BtnCustomMoveDown.IsEnabled = false;
					BtnCustomMoveUp.IsEnabled = false;
				}
				else {
					BtnCustomDelete.IsEnabled = true;
					BtnCustomEdit.IsEnabled = true;
					BtnCustomMoveDown.IsEnabled = true;
					BtnCustomMoveUp.IsEnabled = true;
				}
			};
		}

		private MainPage mMainPage;
		private ViewModels.BaseViewModel mVm;
		private HttpClient client = new HttpClient();
		private volatile bool mListChanged = false;

		private async Task<bool> _CheckAndTryToFetchFile( string fileListLink )
		{
			try {
				var uri = new Uri( fileListLink );

				// only accept http or https!!
				var response = await client.GetAsync( uri );
				if( response == null || response.IsSuccessStatusCode == false )
					return false;

				// response.Content.Headers.LastModified almost can't get...
				string responseString = await response.Content.ReadAsStringAsync();
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
			LblCustomCount.Text = $"{CustomGlossaryFileListLocations.Count} / {CustomGlossaryFileListCountMax}";
			mListChanged = true;
			mVm.IsDataEmpty = CustomGlossaryFileListLocations.Count <= 0;
		}

		private void ContentPage_Appearing( object sender, EventArgs e )
		{
			LblDefault.Text = Properties.Settings.Default.DefaultGlossaryFileListLocation;

			CustomGlossaryFileListLocations = ProjectManager.Instance.CustomGlossaryFileListLocations;
			LvCustom.ItemsSource = CustomGlossaryFileListLocations;
			mVm.IsDataEmpty = CustomGlossaryFileListLocations.Count <= 0;

			LblCustomCount.Text = $"{CustomGlossaryFileListLocations.Count} / {CustomGlossaryFileListCountMax}";
		}

		private void ContentPage_Disappearing( object sender, EventArgs e )
		{
			if( mListChanged == true ) {
				ProjectManager.Instance.SaveListToSettings();
			}
		}

		private async void BtnCustomAdd_Clicked( object sender, EventArgs e )
		{
			if( CustomGlossaryFileListLocations.Count >= CustomGlossaryFileListCountMax ) {
				await DisplayAlert( Languages.ProjectGlossary.Str0GlossaryFileListWarning, Languages.ProjectGlossary.Str0FileListCountHasReachMaximum, Languages.Global.Str0Ok );
				return;
			}

			var newFileList = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync( Languages.ProjectGlossary.Str0FileListLocationField, "", "https://" );
			if( string.IsNullOrWhiteSpace( newFileList ) ) {
				// maybe cancelled!!
				return;
			}
			if( CustomGlossaryFileListLocations.Contains( newFileList ) ) {
				await DisplayAlert( Languages.ProjectGlossary.Str0GlossaryFileListWarning, Languages.ProjectGlossary.Str0FileListLocationIsExisted, Languages.Global.Str0Ok );
				return;
			}

			// try to fetch file list text file
			if( false == await _CheckAndTryToFetchFile( newFileList ) ) {
				await DisplayAlert( Languages.ProjectGlossary.Str0GlossaryFileListError,
						Languages.ProjectGlossary.Str0FileListCantFetchOrInvalid, Languages.Global.Str0Ok );
				return;
			}

			CustomGlossaryFileListLocations.Add( newFileList );
			_UpdateCount();
		}

		private void BtnCustomDelete_Clicked( object sender, EventArgs e )
		{
			var loc = LvCustom.SelectedItem as string;
			if( string.IsNullOrWhiteSpace( loc ) ||
				CustomGlossaryFileListLocations.Contains( loc ) == false )
				return;

			CustomGlossaryFileListLocations.Remove( loc );
			_UpdateCount();
		}

		private void BtnCustomMoveUp_Clicked( object sender, EventArgs e )
		{
			var idx = CustomGlossaryFileListLocations.IndexOf( LvCustom.SelectedItem as string );
			if( idx < 1 )
				return;

			CustomGlossaryFileListLocations.Move( idx, idx - 1 );
			mListChanged = true;
		}

		private void BtnCustomMoveDown_Clicked( object sender, EventArgs e )
		{
			var idx = CustomGlossaryFileListLocations.IndexOf( LvCustom.SelectedItem as string );
			if( idx < 0 || idx >= CustomGlossaryFileListLocations.Count - 1 )
				return;

			CustomGlossaryFileListLocations.Move( idx, idx + 1 );
			mListChanged = true;
		}

		private async void BtnCustomEdit_Clicked( object sender, EventArgs e )
		{
			var loc = LvCustom.SelectedItem as string;
			var idx = CustomGlossaryFileListLocations.IndexOf( loc );
			if( string.IsNullOrWhiteSpace( loc ) || idx < 0 )
				return;

			var newFileList = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync( Languages.ProjectGlossary.Str0FileListLocationField, "", loc );
			if( newFileList == loc )
				return;

			if( CustomGlossaryFileListLocations.Contains( newFileList ) ) {
				await DisplayAlert( Languages.ProjectGlossary.Str0GlossaryFileListWarning,
						Languages.ProjectGlossary.Str0FileListLocationIsExisted, Languages.Global.Str0Ok );
				return;
			}

			// show warning about OriginalText is all white spaces
			if( string.IsNullOrWhiteSpace( newFileList ) ) {
				// maybe cancelled!!
				return;
			}

			if( false == await _CheckAndTryToFetchFile( newFileList ) ) {
				await DisplayAlert( Languages.ProjectGlossary.Str0GlossaryFileListError,
						Languages.ProjectGlossary.Str0FileListCantFetchOrInvalid, Languages.Global.Str0Ok );
				return;
			}

			CustomGlossaryFileListLocations[idx] = newFileList;
			mListChanged = true;
		}
	}
}
