using MinaxWebTranslator.Models;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	/// <summary>
	/// Main Detail page
	/// </summary>
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class DetailPage : TabbedPage
	{
		internal ProjectModel CurrentProject { get; private set; }

		internal TranslatorSelector CurrentTranslator { get; set; }


		public DetailPage()
		{
			InitializeComponent();

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
		}

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			switch( type ) {
				case MessageType.AppClosed:
				case MessageType.AppClosing:
					CurrentTranslator = null;
					break;

				case MessageType.ProjectOpened:
					if( data is ProjectModel openedPM ) {
						CurrentProject = openedPM;
						this.Title = $"{CurrentProject.ProjectName} - Minax Web Translator";
					}
					break;
				case MessageType.ProjectClosed:
					CurrentProject = null;
					this.Title = "Minax Web Translator";
					break;
				case MessageType.ProjectChanged:
					if( data is ProjectModel ) {
						this.Title = $"{CurrentProject.ProjectName}* - Minax Web Translator";
					}
					break;
				case MessageType.ProjectSaved:
					if( CurrentProject != null )
						this.Title = $"{CurrentProject.ProjectName} - Minax Web Translator";
					break;

				case MessageType.XlatorSelected:
					if( data is TranslatorSelector ts ) {
						CurrentTranslator = ts;
						//if( ts != null && ts.Icon != null )
						//	BtnTargetTranslate.ImageSource = ts.Icon;
					}
					break;
			}
		}

	}
}
