using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class SourcePage : ContentPage
	{
		internal string SourceText {
			get {
				if( string.IsNullOrEmpty( mSourceText ) ) {
					mSourceText = EdMain.Text;
				}
				return mSourceText;
			}
			private set {
				if( mSourceText == value )
					return;
				mSourceText = value;
				SourceTextChanged?.Invoke( this, null );
			}
		}

		internal event EventHandler<EventArgs> SourceTextChanged;

		public SourcePage()
		{
			InitializeComponent();

			EdMain.Text = "";
			//this.BindingContext = new ViewModels.EditingViewModel();

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
		}

		private string mSourceText;

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{

			switch( type ) {
				case MessageType.XlatingQuick:
					//case MessageType.XlationQuickWithText:
					if( data is bool onOff ) {
						SlSource.IsEnabled = !onOff;
					}
					break;
				case MessageType.XlatingSections:
					if( data is bool onOff2 ) {
						SlSource.IsEnabled = !onOff2;
					}
					break;
			}
		}

		private async void ContentPage_Disappearing( object sender, EventArgs e )
		{
			SourceText = EdMain.Text;
			await MessageHub.SendMessageAsync( this, MessageType.SourceTextChanged, SourceText );
		}

		private async void BtnClearAndPaste_Clicked( object sender, EventArgs e )
		{
			EdMain.Text = SourceText = await Clipboard.GetTextAsync();
		}

		private void BtnClear_Clicked( object sender, EventArgs e )
		{
			EdMain.Text = SourceText = "";
		}

		private async void BtnAppend_Clicked( object sender, EventArgs e )
		{
			EdMain.Text += await Clipboard.GetTextAsync();
			SourceText = EdMain.Text;
		}
	}
}
