using Minax.Collections;
using MinaxWebTranslator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class RecentProjectsPage : ContentPage
	{
		internal ReadOnlyObservableList<ProjectModel> RecentProjects {
			get => mProjects;
			private set {
				mProjects = value;
				LvRecentProjects.ItemsSource = mProjects;
			}
		}

		internal bool IsRemeberRecent {
			get => SwRemeberRecent.IsToggled;
			set => SwRemeberRecent.IsToggled = value;
		}

		internal double RecentProjectCountMax {
			get => SpRecentMax.Value;
			set => SpRecentMax.Value = value;
		}

		internal ProjectModel SelectedProject {
			get => mSelectedProject;
			set {
				if( mSelectedProject == value )
					return;

				if( value == null ) {
					LvRecentProjects.ItemsSource = null;
					LvRecentProjects.SelectedItem = null;
					LvRecentProjects.ItemsSource = mProjects;
					mSelectedProject = null;
					return;
				}

				if( mProjects == null || mProjects.Contains( value ) == false )
					return;
				mSelectedProject = value;
				LvRecentProjects.SelectedItem = value;
				_ = MessageHub.SendMessageAsync( this, MessageType.MenuNavigate, MenuItemType.RecentProjectSelected );
			}
		}

		public RecentProjectsPage( MainPage mainPage )
		{
			mMainPage = mainPage;

			InitializeComponent();

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;

			LvRecentProjects.ItemSelected += ( s1, e1 ) => {
				this.SelectedProject = e1.SelectedItem as ProjectModel;
			};
		}
		~RecentProjectsPage()
		{
			MessageHub.MessageReceived -= MsgHub_MessageRecevied;

			Properties.Settings.Default.RemeberRecentProjects = IsRemeberRecent;
			Properties.Settings.Default.RemeberRecentProjectMax = (int)SpRecentMax.Value;
		}

		private MainPage mMainPage;
		private ReadOnlyObservableList<ProjectModel> mProjects = new ObservableList<ProjectModel>();
		private ProjectModel mSelectedProject;

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			switch( type ) {
				case MessageType.AppClosed:
				case MessageType.AppClosing:
					LvRecentProjects.ItemsSource = null;
					break;

				case MessageType.ProjectOpened:
					break;

				case MessageType.ProjectClosed:
					if( data is ProjectModel closedProj && closedProj == mSelectedProject )
						SelectedProject = null;
					break;
			}
		}

		private void ContentPage_Appearing( object sender, EventArgs e )
		{
			IsRemeberRecent = Properties.Settings.Default.RemeberRecentProjects;

			int max = Properties.Settings.Default.RemeberRecentProjectMax;
			if( max < SpRecentMax.Minimum || max > SpRecentMax.Maximum )
				SpRecentMax.Value = SpRecentMax.Minimum;
			else
				SpRecentMax.Value = max;

			if( IsRemeberRecent ) {
				RecentProjects = ProjectManager.Instance.RecentProjects;
				LvRecentProjects.SelectedItem = mSelectedProject;
			}
		}

		private void SwRemeberRecent_Toggled( object sender, ToggledEventArgs e )
		{
			if( e.Value == true ) {
				SpRecentMax.IsEnabled = true;
				BtnRecentProjClearAll.IsEnabled = true;
				LvRecentProjects.IsEnabled = true;
				RecentProjects = ProjectManager.Instance.RecentProjects;
			}
			else {
				SpRecentMax.IsEnabled = false;
				BtnRecentProjClearAll.IsEnabled = false;
				LvRecentProjects.IsEnabled = false;
			}
			Properties.Settings.Default.RemeberRecentProjects = e.Value;
		}

		private void SpRecentMax_ValueChanged( object sender, ValueChangedEventArgs e )
		{
			EdRecentMax.Text = e.NewValue.ToString();
			Properties.Settings.Default.RemeberRecentProjectMax = (int)e.NewValue;
		}

		private async void BtnRecentProjClearAll_Clicked( object sender, EventArgs e )
		{
			var rst = await DisplayAlert( Languages.Global.Str0DeleteConfirm,
								Languages.ProjectGlossary.Str0ClearExistedRecentProjectListAsk,
								Languages.Global.Str0Yes, Languages.Global.Str0No );
			if( rst == true )
				ProjectManager.Instance.ClearRecentProjects();
		}
	}
}
