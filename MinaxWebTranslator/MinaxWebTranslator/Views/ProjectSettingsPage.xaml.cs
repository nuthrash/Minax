using Minax.Domain.Translation;
using MinaxWebTranslator.Models;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.AlertDialogModal;
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
				if( mProject == null || mProject.Project == null )
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

				EtProjectDesc.Text = mProject.Project.Description;
				EtProjectRemoteSite.Text = mProject.Project.RemoteSite;
			}

		}

		public string CurrentProjectName => EtProjectName.Text;

		public SupportedSourceLanguage CurrentSourceLanguage { get; private set; }
		public SupportedTargetLanguage CurrentTargetLanguage { get; private set; }

		public string CurrentProjectDescription => EtProjectDesc.Text;
		public string CurrentProjectRemoteSite => EtProjectRemoteSite.Text;

		internal bool NeedReloading { get; set; }

		public ProjectSettingsPage( MainPage mainPage )
		{
			mMainPage = mainPage;

			InitializeComponent();

			PkSourceLang.Items.Clear();
			PkTargetLang.Items.Clear();
			CbGlossaryOverwritePolicy.Items.Clear();
			CbGlossarySyncFile.Items.Clear();


			PkSourceLang.ItemsSource = new List<string> {
				Languages.WebXlator.Str0LangJapanese,
				Languages.WebXlator.Str0LangEnglish,
			};
			PkTargetLang.ItemsSource = new List<string> {
				Languages.WebXlator.Str0LangChineseTraditional,
				Languages.WebXlator.Str0LangEnglish,
			};
			CbGlossaryOverwritePolicy.ItemsSource = new List<string> {
				Languages.ProjectGlossary.Str0Skip,
				Languages.ProjectGlossary.Str0ForceOverwriteWithoutAsking,
				Languages.ProjectGlossary.Str0AlwaysAsking,
				Languages.ProjectGlossary.Str0BiggerFileSize,
			};

			CbGlossaryOverwritePolicy.SelectedIndex = 0;

			mNetProgress.ProgressChanged += async ( s1, e1 ) => {
				if( e1.PercentOrErrorCode >= 0 && e1.PercentOrErrorCode <= 100 ) {
					MainThread.BeginInvokeOnMainThread( async () => {
						// show download progress message...
						if( e1.PercentOrErrorCode == 100 && mNetWaitDlg != null ) {
							await mNetWaitDlg.Dismiss();
							return;
						}

						if( mNetWaitView != null )
							mNetWaitView.Message = string.Format(Languages.ProjectGlossary.Str1PlzWaitOperationFinished, e1.PercentOrErrorCode );
					} );
				}
				await MessageHub.SendMessageAsync( this, MessageType.NetProgress, e1 );
			};
		}

		private MainPage mMainPage;
		private ProjectModel mProject;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		private readonly Progress<Minax.ProgressInfo> mNetProgress = new Progress<Minax.ProgressInfo>();
		private AlertDialogPage mNetWaitDlg;
		private Views.WaitingView mNetWaitView;

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
			if( mProject == null || mProject.Project == null || PkSourceLang.ItemsSource == null )
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
			if( mProject == null || mProject.Project == null || PkTargetLang.ItemsSource == null )
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
			CrossToastPopUp.Current.ShowToastMessage( Languages.ProjectGlossary.Str0CreateGlossaryFolderSucceed );
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

			// show waiting dialog...
			if( mNetWaitView == null )
				mNetWaitView = new WaitingView { Message = string.Format( Languages.ProjectGlossary.Str1PlzWaitOperationFinished, 0 ) };

			if( mNetWaitDlg != null ) {
				await mNetWaitDlg.Dismiss();
			} else {
				mNetWaitView.Message = string.Format( Languages.ProjectGlossary.Str1PlzWaitOperationFinished, 0 );
				mNetWaitDlg = new AlertDialogBuilder()
					.SetView( mNetWaitView )
					.SetPositiveButton( Languages.Global.Str0Cancel, async () => {
						if( mCancelTokenSrource.IsCancellationRequested == false )
							mCancelTokenSrource.Cancel();
						await mNetWaitDlg.Dismiss();
						mCancelTokenSrource = new CancellationTokenSource();
					} )
					.Build();
			}
			await mNetWaitDlg.Show( this );

			var rst = await ProjectManager.Instance.FetchFilesByFileListLink(
					CbGlossarySyncFile.SelectedItem.ToString(),
					ProjectManager.Instance.MappingMonitor.BaseProjectPath, policy,
					mCancelTokenSrource, mNetProgress, this );

			if( rst == false ) {
				await DisplayAlert( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0DownloadRemoteGlossaryFilesByLinkFailed, Languages.Global.Str0Ok );
				ProjectManager.Instance.MappingMonitor?.Start();
				return;
			}

			// reload Mapping Tables
			ProjectManager.Instance.MappingMonitor?.ReloadFileList();
			NeedReloading = true;
			if( rst ) {
				CrossToastPopUp.Current.ShowToastMessage( Languages.ProjectGlossary.Str0DownloadAndMergeRemoteGlossaryFilesSucceed );
			}
			else {
				await DisplayAlert( Languages.Global.Str0OperationFinished, Languages.ProjectGlossary.Str0MergeGlossaryFilesFailed, Languages.Global.Str0Ok );
			}
		}

		private async void BtnGlossarySyncFile_Clicked( object sender, EventArgs e )
		{
			RemoteSyncFileListSettingsPage page = new RemoteSyncFileListSettingsPage( mMainPage );

			await (mMainPage.Detail as NavigationPage)?.PushAsync( new NavigationPage( page ) { Title = Languages.ProjectGlossary.Str0RemoteSyncFileListSettings } );
		}

		private void SwMonitorAutoMergeWhenFileChanged_Toggled( object sender, ToggledEventArgs e )
		{
			ProjectManager.Instance.AutoRemoveMonitoringWhenFileChanged = SwMonitorAutoMergeWhenFileChanged.IsToggled;
		}
	}
}
