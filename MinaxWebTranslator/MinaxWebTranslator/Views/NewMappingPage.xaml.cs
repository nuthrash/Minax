﻿using Minax.Collections;
using Minax.Domain.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	/// <summary>
	/// New/Editing Mapping item editing page
	/// </summary>
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class NewMappingPage : ContentPage
	{
		public MappingMonitor.MappingModel Model { get; set; }
		public MappingMonitor.MappingModel EditingModel { get; private set; }

		public ObservableList<MappingMonitor.MappingModel> TargetList { get; set; }

		public NewMappingPage()
		{
			InitializeComponent();

			if( EditingModel == null ) {
				EditingModel = new MappingMonitor.MappingModel();
			}

			BindingContext = this;

			// prepare Picker
			PkCategory.ItemsSource = Minax.Utils.GetAllTextCategoryL10nStrings().ToList();
		}

		private void ContentPage_Appearing( object sender, EventArgs e )
		{
			if( EditingModel == null ) {
				EditingModel = new MappingMonitor.MappingModel();
			}
			BindingContext = this;

			if( Model != null ) {

				if( Model.Category == null ) {
					PkCategory.SelectedIndex = 0;
				}
				else {
					int enumIdx = (int)Model.Category;
					if( enumIdx >= 0 && enumIdx < PkCategory.ItemsSource.Count )
						PkCategory.SelectedIndex = enumIdx;
					else
						PkCategory.SelectedIndex = 0;
				}
				EditingModel.OriginalText = Model.OriginalText;
				EditingModel.MappingText = Model.MappingText;
				EditingModel.Description = Model.Description;
				EditingModel.Comment = Model.Comment;
			}
		}

		private async void TbiCancel_Clicked( object sender, EventArgs e )
		{
			await Navigation.PopModalAsync();
		}

		private async void TbiSave_Clicked( object sender, EventArgs e )
		{
			if( string.IsNullOrWhiteSpace( EditingModel.OriginalText ) ) {
				await DisplayAlert( "Invalid Field", "Original Text cannot be empty or full of white spaces!", "OK" );
				return;
			}

			if( Model != null ) {
				Model.OriginalText = EditingModel.OriginalText;
				Model.MappingText = EditingModel.MappingText;
				Model.Description = EditingModel.Description;
				Model.Comment = EditingModel.Comment;

				int idx = PkCategory.SelectedIndex;
				Model.Category = (TextCategory)idx;

				if( TargetList != null && TargetList.Contains( Model ) == false )
					TargetList.Add( Model );
			}

			await Navigation.PopModalAsync();
		}
	}
}