using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MinaxWebTranslator
{
	/// <summary>
	/// App internal Message Hub
	/// </summary>
	internal static class MessageHub
	{
		/// <summary>
		/// Is MessageReceived event enabled
		/// </summary>
		public static bool IsEventEnabled { get; set; } = true;

		/// <summary>
		/// MessageReceived event for receiving message from others
		/// </summary>
		public static event MessageEventHandler MessageReceived;

		public static async Task<bool> SendMessageAsync( object sender, MessageType type, object data )
		{
			if( IsEventEnabled == false )
				return false;

			if( MessageReceived == null )
				return true;

			bool result = false;
			if( !MainThread.IsMainThread ) {
				await Task.Run( () => {
					MainThread.BeginInvokeOnMainThread( () => {
						result = _Invoke( sender, type, data );
					} );
				} );				
			} else {
				return _Invoke( sender, type, data );
			}
			
			return result;
		}

		private static bool _Invoke( object sender, MessageType type, object data )
		{
			foreach( var subscriber in MessageReceived.GetInvocationList() ) {
				try {
					subscriber.DynamicInvoke( sender, type, data );
				}
				catch {
					return false;
				}
			}
			return true;
		}
	}


	internal delegate void MessageEventHandler( object sender, MessageType type, object data );

	internal enum MessageType
	{
		None = 0,

		/// <summary>
		/// MainWindow app closing
		/// </summary>
		AppClosing,
		/// <summary>
		/// MainWindow app closed
		/// </summary>
		AppClosed,

		/// <summary>
		/// GUI view component loaded
		/// </summary>
		ViewLoaded,
		/// <summary>
		/// GUI view component unloaded
		/// </summary>
		ViewUnloaded,

		/// <summary>
		/// When new ProjectModel and TranslationProject object created
		/// </summary>
		ProjectCreated,
		/// <summary>
		/// When existed project opened, data is a ProjectModel
		/// </summary>
		ProjectOpened,
		/// <summary>
		/// When current project closed, data is a closed ProjectModel
		/// </summary>
		ProjectClosed,
		/// <summary>
		/// When current project settings/Mapping changed, data is a ProjectModel
		/// </summary>
		ProjectChanged,
		/// <summary>
		/// When current project saved, data is a ProjectModel
		/// </summary>
		ProjectSaved,
		/// <summary>
		/// When current project renamed, data is a MappingMonitor.MappingEventArgs
		/// </summary>
		ProjectRenamed,
		/// <summary>
		/// Project updated outside the App, data is a MappingMonitor.MappingEventArgs
		/// </summary>
		ProjectUpdated, // FileChanged

		/// <summary>
		/// A new Glossary file found,  data is a MappingMonitor.MappingEventArgs
		/// </summary>
		GlossaryNew, // FileChanged
		/// <summary>
		/// Existed Glossary file updated outside the App, data is a MappingMonitor.MappingEventArgs
		/// </summary>
		GlossaryUpdated, // FileChanged
		/// <summary>
		/// A Glossary file was deleted outside the App, data is a MappingMonitor.MappingEventArgs
		/// </summary>
		GlossaryDeleted, // FileDeleted
		/// <summary>
		/// A Glossary file was renamed outside the app, data is a MappingMonitor.MappingEventArgs
		/// </summary>
		GlossaryRenamed, // FileRenamed

		/// <summary>
		/// Status message text to show on StatusBar, data is a string
		/// </summary>
		StatusMessage,

		/// <summary>
		/// When Source Text changed, data is a string
		/// </summary>
		SourceTextChanged,

		/// <summary>
		/// Current Translator/TranslationService changed, data is a TranslatorSelector
		/// </summary>
		XlatorSelected,
		/// <summary>
		/// Translating sections text mode in On/Off, data is a bool
		/// </summary>
		XlatingSections,
		/// <summary>
		/// Translating Quick/small text mode in On/Off, data is a bool
		/// </summary>
		XlatingQuick,
		/// <summary>
		/// Send translate Quick/small text request with string parameter from client, data is a string
		/// </summary>
		XlatingQuickWithText,
		/// <summary>
		/// Translating percentage or error code by ProgressInfo, data is a Minax.ProgressInfo
		/// </summary>
		XlatingProgress,

		/// <summary>
		/// Data/Items source reload to reflect current status, data is a ProjectModel
		/// </summary>
		DataReload,

		/// <summary>
		/// Networking operation percentage or error code, data is a Minax.ProgressInfo
		/// </summary>
		NetProgress,

		/// <summary>
		/// Navigating of Menu page/module (not used in Desktop version), data is a MenuItemType
		/// </summary>
		MenuNavigate,
	}
}
