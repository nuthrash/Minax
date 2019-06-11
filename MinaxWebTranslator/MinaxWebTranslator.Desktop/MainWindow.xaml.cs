using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MinaxWebTranslator.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Layout;

namespace MinaxWebTranslator.Desktop
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			// handy add Docking Components
			mSrcWindow = new Views.SourceDockingPanel( this );
			mTgtWindow = new Views.TargetDockingPanel( this );
			mMappingWindow = new Views.MappingDockingPanel( this );
			mSummaryWindow = new Views.SummaryDockingPanel( this );
			mQuickXlateWindow = new Views.QuickTranslationDockingPanel( this );

			AdlapgMain.Children.Add( new LayoutAnchorablePane( mSrcWindow ) );
			AdlapgMain.Children.Add( new LayoutAnchorablePane( mTgtWindow ) );
			AdlapgRight.Children.Add( mMappingWindow );
			AdlapgRight.Children.Add( mSummaryWindow );
			AdlapgRight.Children.Add( mQuickXlateWindow );

			this.DataContext = new MainWindowViewModel();
		}

		private void MetroWindow_Loaded( object sender, RoutedEventArgs e )
		{

			_PrepareWebBrowser();
			_RestoreAppSettings();
		}

		private void MetroWindow_Unloaded( object sender, RoutedEventArgs e )
		{
			_SaveAppSettings();
		}

		#region "private data/methods/helpers"

		private readonly Views.SourceDockingPanel mSrcWindow = null;
		private readonly Views.TargetDockingPanel mTgtWindow = null;
		private readonly Views.MappingDockingPanel mMappingWindow = null;
		private readonly Views.SummaryDockingPanel mSummaryWindow = null;
		private readonly Views.QuickTranslationDockingPanel mQuickXlateWindow = null;

		private void _PrepareWebBrowser()
		{
			// set WebBrowser's IE version to higher for modern HTML
			var appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
			using( var Key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey( @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true ) ) {
				Key.SetValue( appName, 99999, Microsoft.Win32.RegistryValueKind.DWord );
			}

#if DEBUG
			appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".vhost.exe"; // for visual studio
			using( var Key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey( @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true ) ) {
				Key.SetValue( appName, 99999, Microsoft.Win32.RegistryValueKind.DWord );
			}
#endif
		}

		private void _RestoreAppSettings()
		{
			_RestoreDockingLayout();
		}

		private void _SaveAppSettings()
		{
			_SaveDockingLayout();
		}

		private void _SaveDockingLayout()
		{
			var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );

			// Serialize to Settings
			var base64 = string.Empty;
			using( var ms = new System.IO.MemoryStream() )
			using( var sw = new System.IO.StreamWriter( ms, Encoding.UTF8 ) ) {
				serializer.Serialize( sw );
				base64 = Convert.ToBase64String( ms.ToArray() );
			}
			Properties.Settings.Default.AvalonDockLayout = base64;
			Properties.Settings.Default.Save();
			Properties.Settings.Default.Save();
		}
		private void _RestoreDockingLayout()
		{
			var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.AvalonDockLayout ) ) {
				// Deserialize from Default docking layout file in Resources
				using( var stream = new System.IO.MemoryStream( Properties.Resources.DefaultAvalonDockLayout ) )
					serializer.Deserialize( stream );

				// then, save restored layout to Settings
				var base64 = string.Empty;
				using( var ms = new System.IO.MemoryStream() )
				using( var sw = new System.IO.StreamWriter( ms, Encoding.UTF8 ) ) {
					serializer.Serialize( sw );
					base64 = Convert.ToBase64String( ms.ToArray() );
				}
				Properties.Settings.Default.AvalonDockLayout = base64;
				Properties.Settings.Default.Save();
				Properties.Settings.Default.Save();
			}
			else {
				// Deserialize from Settings resource
				var text = Convert.FromBase64String( Properties.Settings.Default.AvalonDockLayout );
				using( var ms = new System.IO.MemoryStream( text ) )
				using( var sr = new System.IO.StreamReader( ms, Encoding.UTF8 ) ) {
					serializer.Deserialize( sr );
				}
			}
		}


		private void _ShowAutoCloseMessage( string title, string message, long autoCloseInterval = 3000 )
		{
			FoMessage.AutoCloseInterval = autoCloseInterval;
			LblAutoCloseTitle.Content = title;
			TbAutoCloseMessage.Text = message;

			FoMessage.IsOpen = true;
		}

		private void _ShowStatusMessage( string message )
		{
			TbStatusMessage.Text = message;
		}

		#endregion

		#region "private handy event handlers"


		#endregion


		
	}
}
