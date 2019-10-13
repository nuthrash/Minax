using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MinaxWebTranslator.Desktop
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += ( s1, e1 ) => {
				System.Diagnostics.Trace.WriteLine( $"{DateTime.Now.ToString( "yyyy/MM/dd HH:mm:ss.fff" )} caught unhanlded exception {e1.ExceptionObject}." );
			};

			TaskScheduler.UnobservedTaskException += ( s1, e1 ) => {
				e1.SetObserved();

				System.Diagnostics.Trace.WriteLine( $"{DateTime.Now.ToString( "yyyy/MM/dd HH:mm:ss.fff" )} caught unobserveded task exception {e1.Exception}." );
			};
		}

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			// make all DataGrids in this app have a single click to edit 
			EventManager.RegisterClassHandler( typeof( System.Windows.Controls.DataGrid ),
				System.Windows.Controls.DataGrid.PreviewMouseLeftButtonDownEvent,
				new RoutedEventHandler( EventHelper.DataGridPreviewMouseLeftButtonDownEvent ) );

		}

	}
}
