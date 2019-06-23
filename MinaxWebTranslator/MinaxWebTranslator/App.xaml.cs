using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MinaxWebTranslator.Views;

namespace MinaxWebTranslator
{
    public partial class App : Application
    {
		public string PublicDocumentsPath { get; private set; }

		public App( string publicDocuemntPath = null )
        {
			PublicDocumentsPath = publicDocuemntPath;

			//Load the assembly
			Xamarin.Forms.DataGrid.DataGridComponent.Init();

			InitializeComponent();

			// init. non-GUI data
			ProjectManager.Instance.RestoreListFromSettings();

            MainPage = new MainPage( publicDocuemntPath );
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
			// Handle when your app sleeps

			if( MainPage is MainPage mp ) {
				if( mp.NeedSave )
					mp.SaveAll();
			}
		}

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
