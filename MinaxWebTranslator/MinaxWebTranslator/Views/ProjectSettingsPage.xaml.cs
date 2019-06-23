using Minax.Domain.Translation;
using MinaxWebTranslator.Models;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class ProjectSettingsPage : ContentPage
	{
		internal ProjectModel CurrentProject {
			get => mProject;
			set {
				mProject = value;
				if( mProject == null )
					return;

				EtProjectName.Text = mProject.ProjectName;

				switch( mProject.Project.SourceLanguage ) {
					case SupportedSourceLanguage.Japanese:
						PkSourceLang.SelectedIndex = 0;
						break;
					case SupportedSourceLanguage.English:
						PkSourceLang.SelectedIndex = 1;
						break;
				}

				switch( mProject.Project.TargetLanguage ) {
					case SupportedTargetLanguage.ChineseTraditional:
						PkTargetLang.SelectedIndex = 0;
						break;
					case SupportedTargetLanguage.English:
						PkTargetLang.SelectedIndex = 1;
						break;
				}
			}

		}

		public string CurrentProjectName => EtProjectName.Text;

		public SupportedSourceLanguage CurrentSourceLanguage { get; private set; }
		public SupportedTargetLanguage CurrentTargetLanguage { get; private set; }

		internal bool NeedReloading { get; set; }

		public ProjectSettingsPage( MainPage mainPage )
		{
			mMainPage = mainPage;

			InitializeComponent();

			CbGlossarySyncFile.Items.Clear();
			CbGlossaryOverwritePolicy.SelectedIndex = 0;

			sNetProgrss.ProgressChanged += async ( s1, e1 ) => {
				// TODO: show download progress message...
				await MessageHub.SendMessageAsync( this, MessageType.NetProgress, e1 );
			};
		}

		private MainPage mMainPage;
		private ProjectModel mProject;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		private static readonly Progress<Minax.ProgressInfo> sNetProgrss = new Progress<Minax.ProgressInfo>();

		private void ContentPage_Appearing( object sender, EventArgs e )
		{
			var glossaryFileList = ProjectManager.Instance.CustomGlossaryFileListLocations.ToList();

			glossaryFileList.Insert( 0, Properties.Settings.Default.DefaultGlossaryFileListLocation );
			CbGlossarySyncFile.SelectedItem = null;
			CbGlossarySyncFile.ItemsSource = glossaryFileList;
			CbGlossarySyncFile.SelectedIndex = 0;

			if( ProjectManager.Instance.MappingMonitor != null )
				TbGlossaryPath.Text = ProjectManager.Instance.MappingMonitor.GlossaryPath;
			else
				TbGlossaryPath.Text = "";

		}

		private async void ContentPage_Disappearing( object sender, EventArgs e )
		{
			// only send message when this page pop up from top of nav. stack!
			if( mMainPage.Detail is NavigationPage rootNav && rootNav.CurrentPage is NavigationPage projSettingNav &&
				projSettingNav.RootPage == this )
				await MessageHub.SendMessageAsync( this, MessageType.MenuNavigate, MenuItemType.ProjectSettingsClosed );
		}

		private void PkSourceLang_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( mProject == null || mProject.Project == null )
				return;

			var newLang = SupportedSourceLanguage.Japanese;
			switch( PkSourceLang.SelectedIndex ) {
				case 1:
					newLang = SupportedSourceLanguage.English;
					break;
			}
			CurrentSourceLanguage = newLang;
		}

		private void PkTargetLang_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( mProject == null || mProject.Project == null )
				return;

			var newLang = SupportedTargetLanguage.ChineseTraditional;
			switch( PkTargetLang.SelectedIndex ) {
				case 1:
					newLang = SupportedTargetLanguage.English;
					break;
			}
			CurrentTargetLanguage = newLang;
		}

		private void BtnGlossaryCreateEmptyFolders_Clicked( object sender, EventArgs e )
		{
			var result = ProjectManager.Instance.CreateProjectFolders( ProjectManager.Instance.MappingMonitor.BaseProjectPath );
			CrossToastPopUp.Current.ShowToastMessage( "Create Glossary sub-folders succeed!" );
			// NO NEED to reloading Mapping tables!
			//NeedReloading = false;
		}

		private async void BtnGlossaryDownloadByFileList_Clicked( object sender, EventArgs e )
		{
			Minax.IO.OverwritePolicy policy = Minax.IO.OverwritePolicy.Skip;

			switch( CbGlossaryOverwritePolicy.SelectedIndex ) {
				case 1:
					policy = Minax.IO.OverwritePolicy.ForceOverwriteWithoutAsking;
					break;
				case 2:
					policy = Minax.IO.OverwritePolicy.AlwaysAsking;
					break;
				case 3:
					policy = Minax.IO.OverwritePolicy.FileSizeLarger;
					break;
			}

			ProjectManager.Instance.MappingMonitor?.Stop();

			var rst = await ProjectManager.Instance.FetchFilesByFileListLink(
					CbGlossarySyncFile.SelectedItem.ToString(),
					ProjectManager.Instance.MappingMonitor.BaseProjectPath, policy,
					mCancelTokenSrource, sNetProgrss, this );

			if( rst == false ) {
				await DisplayAlert( "Operation Failed", "Download remote Glossary file(s) by file link failed!", "OK" );
				ProjectManager.Instance.MappingMonitor?.Start();
				return;
			}

			// reload Mapping Tables
			ProjectManager.Instance.MappingMonitor?.ReloadFileList();
			NeedReloading = true;
			if( rst ) {
				CrossToastPopUp.Current.ShowToastMessage( "Download and merge remote Glossary file(s) succeed!" );
			}
			else {
				await DisplayAlert( "Opertion Finished", "Merge remote Glossary file(s) failed!", "OK" );
			}
		}

		private async void BtnGlossarySyncFile_Clicked( object sender, EventArgs e )
		{
			RemoteSyncFileListSettingsPage page = new RemoteSyncFileListSettingsPage( mMainPage );

			await (mMainPage.Detail as NavigationPage)?.PushAsync( new NavigationPage( page ) { Title = "Remote Sync. File List Settings" } );
		}

		private void SwMonitorAutoMergeWhenFileChanged_Toggled( object sender, ToggledEventArgs e )
		{
			ProjectManager.Instance.AutoRemoveMonitoringWhenFileChanged = SwMonitorAutoMergeWhenFileChanged.IsToggled;
		}
	}
}