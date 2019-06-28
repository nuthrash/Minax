using System;
using System.Windows;
using System.Windows.Documents;
using Xceed.Wpf.AvalonDock.Layout;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// Dockable panel for Source text
	/// </summary>
	public partial class SourceDockingPanel : LayoutAnchorable
	{
		internal string SourceText {
			get {
				if( string.IsNullOrEmpty( mSourceText ) ) {
					mSourceText = new TextRange( RtbSource.Document.ContentStart, RtbSource.Document.ContentEnd).Text;
				}
				return mSourceText;
			}
			private set => mSourceText = value;
		}

		internal event EventHandler<EventArgs> SourceTextChanged;

		public SourceDockingPanel( MainWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();

			RtbSource.Document.Blocks.Clear();

			// clear mSourceText when GotFocus to get last source text when access SourceText
			RtbSource.GotFocus += ( s1, e1 ) => {
				SourceText = null;
			};
			RtbSource.LostKeyboardFocus += async ( s1, e1 ) => {
				mSourceText = new TextRange( RtbSource.Document.ContentStart, RtbSource.Document.ContentEnd ).Text;
				await MessageHub.SendMessageAsync( this, MessageType.SourceTextChanged, mSourceText );
			};

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
		}

		private readonly MainWindow mMainWindow;
		private string mSourceText;

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{

			switch( type ) {
				case MessageType.XlatingQuick:
				//case MessageType.XlationQuickWithText:
					if( data is bool onOff ) {
						GdSource.IsEnabled = !onOff;
					}
					break;
				case MessageType.XlatingSections:
					if( data is bool onOff2 ) {
						GdSource.IsEnabled = !onOff2;
					}
					break;
			}
		}

		private async void MiSourceCopyAndTranslateSelection_Click( object sender, RoutedEventArgs e )
		{
			if( RtbSource.Selection.IsEmpty )
				return;

			await MessageHub.SendMessageAsync( this, MessageType.XlatingQuickWithText,
												RtbSource.Selection.Text );
		}

		private async void BtnSourceClearAndPaste_Click( object sender, RoutedEventArgs e )
		{
			mSourceText = Clipboard.GetText();
			RtbSource.Document.Blocks.Clear();
			RtbSource.Paste();
			SourceTextChanged?.Invoke( this, null );
			await MessageHub.SendMessageAsync( this, MessageType.SourceTextChanged, mSourceText );
		}

		private async void BtnSourceClear_Click( object sender, RoutedEventArgs e )
		{
			mSourceText = string.Empty;
			RtbSource.Document.Blocks.Clear();
			SourceTextChanged?.Invoke( this, null );
			await MessageHub.SendMessageAsync( this, MessageType.SourceTextChanged, mSourceText );
		}

		private async void BtnSourcePaste_Click( object sender, RoutedEventArgs e )
		{
			mSourceText = Clipboard.GetText();
			RtbSource.Paste();
			SourceTextChanged?.Invoke( this, null );
			await MessageHub.SendMessageAsync( this, MessageType.SourceTextChanged, mSourceText );
		}
	}
}
