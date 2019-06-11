using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Text;
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

namespace MinaxWebTranslator.Desktop.Views
{
	public partial class QuickTranslationDockingPanel : LayoutAnchorable
	{
		public QuickTranslationDockingPanel( MetroWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();
		}

		private readonly MetroWindow mMainWindow;

		private void MiQuickInputClearAndPaste_Click( object sender, RoutedEventArgs e )
		{

		}

		private void BtnQuickTrans_Click( object sender, RoutedEventArgs e )
		{

		}

		private void BtnQuickClearAndPaste_Click( object sender, RoutedEventArgs e )
		{

		}

		private void BtnQuickXLangCopy_Click( object sender, RoutedEventArgs e )
		{

		}

		private void MiQuickXLangCopyAll_Click( object sender, RoutedEventArgs e )
		{

		}

		private void BtnQuickBaiduCopy_Click( object sender, RoutedEventArgs e )
		{

		}

		private void MiQuickBaiduCopyAll_Click( object sender, RoutedEventArgs e )
		{

		}

		private void BtnQuickYoudaoCopy_Click( object sender, RoutedEventArgs e )
		{

		}

		private void MiQuickYoudaoCopyAll_Click( object sender, RoutedEventArgs e )
		{

		}

		private void BtnQuickGoogleCopy_Click( object sender, RoutedEventArgs e )
		{

		}

		private void MiQuickGoogleCopyAll_Click( object sender, RoutedEventArgs e )
		{

		}

		private void BtnQuickIntOutputCopy_Click( object sender, RoutedEventArgs e )
		{

		}

		private void MiQuickIntOutputCopyAll_Click( object sender, RoutedEventArgs e )
		{

		}
	}
}
