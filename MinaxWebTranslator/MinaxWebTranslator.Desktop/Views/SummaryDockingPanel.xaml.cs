using MahApps.Metro.Controls;
using System.Collections.Generic;
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
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;


namespace MinaxWebTranslator.Desktop.Views
{
	public partial class SummaryDockingPanel : LayoutAnchorable
	{
		public SummaryDockingPanel( MetroWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();
		}

		private readonly MetroWindow mMainWindow;

		private void BtnMappingSummaryToolClearSorting_Click( object sender, RoutedEventArgs e )
		{
			DgMappingSummary.ClearSort();
		}
	}
}
