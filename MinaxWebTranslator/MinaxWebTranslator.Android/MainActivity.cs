using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;

namespace MinaxWebTranslator.Droid
{
    [Activity(Label = "MinaxWebTranslator", Icon = "@drawable/translator", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
			//TabLayoutResource = Resource.Layout.Tabbar;
			//ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			Rg.Plugins.Popup.Popup.Init( this, savedInstanceState );
			Plugin.InputKit.Platforms.Droid.Config.Init( this, savedInstanceState );

			if( Android.Support.V4.Content.ContextCompat.CheckSelfPermission( this, Manifest.Permission.WriteExternalStorage ) != (int)Permission.Granted ) {
				Android.Support.V4.App.ActivityCompat.RequestPermissions( this, new string[] { Manifest.Permission.WriteExternalStorage }, 0 );
			}

			if( Android.Support.V4.Content.ContextCompat.CheckSelfPermission( this, Manifest.Permission.ReadExternalStorage ) != (int)Permission.Granted ) {
				Android.Support.V4.App.ActivityCompat.RequestPermissions( this, new string[] { Manifest.Permission.ReadExternalStorage }, 0 );
			}


			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
			LoadApplication( new App( Android.OS.Environment.GetExternalStoragePublicDirectory( Android.OS.Environment.DirectoryDocuments ).AbsolutePath ) );
		}
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

		public override void OnBackPressed()
		{
			if( Rg.Plugins.Popup.Popup.SendBackPressed( base.OnBackPressed ) ) {
				// Do something if there are some pages in the `PopupStack`
			}
			else {
				// Do something if there are not any pages in the `PopupStack`
			}
		}
	}
}
