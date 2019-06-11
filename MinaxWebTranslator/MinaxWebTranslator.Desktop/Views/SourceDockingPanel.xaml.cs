using MahApps.Metro.Controls;
using System;
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
	public partial class SourceDockingPanel : LayoutAnchorable
	{
		internal WebBrowser Browser => WbMain;
		internal LayoutAnchorable AdlaQuickTranslation { get; set; }
		internal RichTextBox RtbQuickInput { get; set; }
		internal Button BtnQuickTrans { get; set; }

		internal RichTextBox RtbTarget { get; set; }
		internal bool SyncTargetScroll { get; set; }

		public SourceDockingPanel( MetroWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();
		}

		private readonly MetroWindow mMainWindow;

		private volatile bool _IsScrolling = false;
		private ScrollViewer svAfter = null, svBefore = null;

		private async void RtbSource_ScrollChanged( object sender, ScrollChangedEventArgs e )
		{
			if( _IsScrolling || SyncTargetScroll != true )
				return;
			if( e.VerticalChange == 0 && e.HorizontalChange == 0 ) { return; }

			_IsScrolling = true;

			var rtbToSync = (sender == RtbTarget) ? RtbTarget : RtbSource;

			if( svAfter == null && sender == RtbTarget )
				svAfter = e.OriginalSource as ScrollViewer;
			if( svBefore == null && sender == RtbSource )
				svBefore = e.OriginalSource as ScrollViewer;

			var sv = e.OriginalSource as ScrollViewer;

			if( sv.ScrollableHeight <= 0.0 )
				goto exit;

			var percentV = e.VerticalOffset / sv.ScrollableHeight;

			if( sender == RtbTarget ) {
				if( svBefore != null )
					svBefore.ScrollToVerticalOffset( percentV * svBefore.ScrollableHeight );
				else
					RtbSource.ScrollToVerticalOffset( percentV * RtbSource.ViewportHeight );
			}
			else {
				if( svAfter != null )
					svAfter.ScrollToVerticalOffset( percentV * svAfter.ScrollableHeight );
				else
					RtbTarget.ScrollToVerticalOffset( percentV * RtbTarget.ViewportHeight );
			}

			await Task.Delay( 100 );
			rtbToSync.InvalidateVisual();

		exit:
			_IsScrolling = false;
		}

		private void MiSourceCopyAndTranslateSelection_Click( object sender, RoutedEventArgs e )
		{
			if( RtbSource.Selection.IsEmpty )
				return;

			if( RtbQuickInput != null ) {
				RtbQuickInput.Document.Blocks.Clear();
				var text = new TextRange( RtbSource.Selection.Start, RtbSource.Selection.End ).Text;
				RtbQuickInput.AppendText( text );
				Clipboard.SetText( text );
			}

			if( AdlaQuickTranslation != null )
				AdlaQuickTranslation.IsActive = true;
			BtnQuickTrans?.RaiseEvent( new RoutedEventArgs( System.Windows.Controls.Button.ClickEvent ) );
		}

		private void BtnSourceClearAndPaste_Click( object sender, RoutedEventArgs e )
		{
			RtbSource.Document.Blocks.Clear();
			RtbSource.Paste();
		}

		private void BtnSourceClear_Click( object sender, RoutedEventArgs e )
		{
			RtbSource.Document.Blocks.Clear();
		}

		private void BtnSourcePaste_Click( object sender, RoutedEventArgs e )
		{
			RtbSource.Paste();
		}
	}
}
