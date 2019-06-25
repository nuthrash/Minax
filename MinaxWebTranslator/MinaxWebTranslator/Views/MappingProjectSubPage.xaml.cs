using Minax.Collections;
using Minax.Domain.Translation;
using MinaxWebTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.DataGrid;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class MappingProjectSubPage : ContentPage
	{
		internal ProjectModel CurrentProject {
			get => mProject;
			private set {
				if( mProject == value )
					return;

				if( mProject != null )
					mProject.PropertyChanged -= ProjectModel_PropertyChanged;
				mProject = value;
				if( mProject != null ) {
					ReloadMapping();
					GdMain.IsEnabled = true;
				} else {
					GdMain.IsEnabled = false;
				}

				
			}
		}

		public MappingProjectSubPage()
		{
			InitializeComponent();

			// replace DgMapping with new one...
			// seems something wrong with Xamarin.Forms 4.0...
			// 1. highlight row sometimes wrong
			// 2. cannot Binding with Converter "TcL10nItemsConverter"
			DgMapping.ActiveRowColor = Color.FromHex( "#8899AA" );
			DgMapping.HeaderBackground = Color.FromHex( "#E0E6F8" );


			Label noData = new Label();
			noData.Text = "NO DATA";
			noData.FontAttributes = FontAttributes.Bold;
			noData.FontSize = Device.GetNamedSize( NamedSize.Large, typeof( Label ) );
			noData.HorizontalOptions = LayoutOptions.CenterAndExpand;
			noData.VerticalOptions = LayoutOptions.CenterAndExpand;
			noData.Margin = new Thickness( 20 );
			noData.TextColor = Color.DarkOrange;

			var newDg = new DataGrid( ListViewCachingStrategy.RetainElement );
			newDg.ActiveRowColor = DgMapping.ActiveRowColor;
			newDg.HeaderBackground = DgMapping.HeaderBackground;
			newDg.HeaderLabelStyle = DgMapping.HeaderLabelStyle;
			newDg.NoDataView = noData;
			newDg.RowsBackgroundColorPalette = DgMapping.RowsBackgroundColorPalette;
			newDg.SelectionEnabled = DgMapping.SelectionEnabled;
			newDg.RowHeight = DgMapping.RowHeight;
			newDg.HeaderHeight = DgMapping.HeaderHeight;

			newDg.Columns.AddRange( DgMapping.Columns );

			// useless
			//if( newDg.Resources == null )
			//	newDg.Resources = new ResourceDictionary();
			//newDg.Resources.Add( "TcL10nItemsConverter", new Converters.TextCategoryL10nItemsConverter() );

			GdMain.Children.Remove( DgMapping );
			GdMain.Children.Add( newDg, 0, 1 );

			DgMapping = newDg;
			DgMapping.ItemsSource = new ObservableList<MappingMonitor.MappingModel>();
			DgMapping.ItemSelected -= DgMapping_ItemSelected;
			DgMapping.ItemSelected += DgMapping_ItemSelected;

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
		}

		private List<MappingMonitor.MappingModel> mData = null;
		private ProjectModel mProject;
		private bool mProjChanged;
		private TranslatorSelector mCurrentXlator;
		private ObservableList<MappingMonitor.MappingModel> mList = null;
		private NewMappingPage mNewMappingPage = null;

		private async void _SetProjChanged()
		{
			mProjChanged = true;
			await MessageHub.SendMessageAsync( this, MessageType.ProjectChanged, mProject );
		}

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			if( sender == this )
				return;

			switch( type ) {
				case MessageType.ProjectOpened:
					if( data is ProjectModel pm ) {
						CurrentProject = pm;
					}
					else {
						CurrentProject = ProjectManager.Instance.CurrentProject;
					}
					break;

				case MessageType.AppClosed:
				case MessageType.AppClosing:
				case MessageType.ProjectClosed:
					CurrentProject = null;
					break;

				case MessageType.ProjectSaved:
					mProjChanged = false;
					break;

				case MessageType.ProjectChanged:
					if( data is ProjectModel projModel ) {
						CurrentProject = projModel;
					}
					break;
				case MessageType.DataReload:
					if( data is ProjectModel reloadModel ) {
						CurrentProject = reloadModel;
					}
					break;

				// File Changed/Deleted/Updated
				case MessageType.ProjectRenamed:
				case MessageType.ProjectUpdated:
					
					break;

				case MessageType.XlatingQuick:
				case MessageType.XlatingSections:
					if( data is bool onOffXlating ) {
						GdMain.IsEnabled = !onOffXlating;
					}
					break;
			}
		}


		private void ProjectModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			var projObj = sender as ProjectModel;
			if( projObj == null )
				return;

			switch( e.PropertyName ) {
				case nameof( ProjectModel.ProjectName ):
					if( this.Parent is NavigationPage parentNav && parentNav.RootPage == this )
						parentNav.Title = mProject.ProjectName;
					break;
			}
		}

		internal void ReloadMapping()
		{
			BtnMappingProjToolAdd.IsEnabled = false;

			var parentNav = this.Parent as NavigationPage;

			DgMapping.SelectedItem = null;
			DgMapping.ItemsSource = new ObservableList<MappingMonitor.MappingModel>(); // force DataGrid become empty!
			mList = null;

			this.Title = "<Project Name>";
			if( parentNav != null && parentNav.RootPage == this )
				parentNav.Title = this.Title;
			if( mProject == null ) {
				DgMapping.BatchCommit();
				return;
			}


			mProject.PropertyChanged -= ProjectModel_PropertyChanged;
			mProject.PropertyChanged += ProjectModel_PropertyChanged;

			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon == null )
				return;

			var coll = mon.GetMappingCollection( mProject.FullPathFileName );
			if( coll == null )
				return;
			DgMapping.ItemsSource = coll;
			mList = coll;

			this.Title = mProject.ProjectName;
			if( parentNav != null && parentNav.RootPage == this )
				parentNav.Title = this.Title;

			BtnMappingProjToolAdd.IsEnabled = true;
		}

		private void DgMapping_ItemSelected( object sender, SelectedItemChangedEventArgs e )
		{
			if( e.SelectedItem == null ) {
				BtnMappingProjToolDelete.IsEnabled = false;
				BtnMappingProjToolMoveDown.IsEnabled = false;
				BtnMappingProjToolMoveUp.IsEnabled = false;
				BtnMappingProjToolEdit.IsEnabled = false;
				return;
			}

			BtnMappingProjToolDelete.IsEnabled = true;
			BtnMappingProjToolMoveDown.IsEnabled = true;
			BtnMappingProjToolMoveUp.IsEnabled = true;
			BtnMappingProjToolEdit.IsEnabled = true;
		}

		private void BtnMappingProjToolDelete_Clicked( object sender, EventArgs e )
		{
			var model = DgMapping.SelectedItem as MappingMonitor.MappingModel;
			if( model == null || mList == null || mList.Contains( model ) == false )
				return;

			mList.Remove( model );
			_SetProjChanged();
		}

		private void BtnMappingProjToolMoveUp_Clicked( object sender, EventArgs e )
		{
			var model = DgMapping.SelectedItem as MappingMonitor.MappingModel;
			if( model == null || mList == null )
				return;

			int oldIdx = mList.IndexOf( model );
			if( oldIdx <= 0 || mList.Count <= 1 )
				return;

			mList.Move( oldIdx, oldIdx - 1 );
			_SetProjChanged();
		}

		private void BtnMappingProjToolMoveDown_Clicked( object sender, EventArgs e )
		{
			var model = DgMapping.SelectedItem as MappingMonitor.MappingModel;
			if( model == null || mList == null )
				return;

			int oldIdx = mList.IndexOf( model );
			if( oldIdx < 0 || oldIdx >= mList.Count - 1 )
				return;

			mList.Move( oldIdx, oldIdx + 1 );
			_SetProjChanged();
		}

		private async void BtnMappingProjToolEdit_Clicked( object sender, EventArgs e )
		{
			var model = DgMapping.SelectedItem as MappingMonitor.MappingModel;
			if( model == null || mList == null || mList.Contains( model ) == false )
				return;

			if( mNewMappingPage == null )
				mNewMappingPage = new NewMappingPage() { Model = model };
			mNewMappingPage.Model = model;

			await Navigation.PushModalAsync( new NavigationPage( mNewMappingPage ) );
		}

		private async void BtnMappingProjToolAdd_Clicked( object sender, EventArgs e )
		{
			if( mList == null )
				return;

			if( mNewMappingPage == null )
				mNewMappingPage = new NewMappingPage() { };

			mNewMappingPage.Model = new MappingMonitor.MappingModel() { ProjectBasedFileName = mProject.FileName };
			mNewMappingPage.TargetList = mList;

			await Navigation.PushModalAsync( new NavigationPage( mNewMappingPage ) );
		}
	}
}
