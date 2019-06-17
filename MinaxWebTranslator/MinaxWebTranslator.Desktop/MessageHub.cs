using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MinaxWebTranslator.Desktop
{
	internal static class MessageHub
	{
		public static bool IsEventEnabled { get; set; } = true;

		public static event MessageEventHandler MessageReceived;

		public static async Task<bool> SendMessageAsync( object sender, MessageType type, object data )
		{
			if( IsEventEnabled == false )
				return false;

			if( MessageReceived == null )
				return true;

			var dispatcher = Application.Current.Dispatcher;
			bool result = false;
			if( !dispatcher.CheckAccess() ) {
				await dispatcher.InvokeAsync( () => {
					result = _Invoke( sender, type, data );
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
		/// When existed project opened
		/// </summary>
		ProjectOpened,
		/// <summary>
		/// When current project closed
		/// </summary>
		ProjectClosed,
		/// <summary>
		/// When current project settings/Mapping changed
		/// </summary>
		ProjectChanged,
		/// <summary>
		/// When current project saved
		/// </summary>
		ProjectSaved,
		/// <summary>
		/// When current project renamed
		/// </summary>
		ProjectRenamed,
		/// <summary>
		/// Project updated outside the App
		/// </summary>
		ProjectUpdated, // FileChanged

		/// <summary>
		/// A new Glossary file found
		/// </summary>
		GlossaryNew, // FileChanged
		/// <summary>
		/// Existed Glossary file updated outside the App
		/// </summary>
		GlossaryUpdated, // FileChanged
		/// <summary>
		/// A Glossary file was deleted outside the App
		/// </summary>
		GlossaryDeleted, // FileDeleted
		/// <summary>
		/// A Glossary file was renamed outside the app
		/// </summary>
		GlossaryRenamed, // FileRenamed

		/// <summary>
		/// Status message text to show on StatusBar
		/// </summary>
		StatusMessage,

		/// <summary>
		/// When Source Text changed. (not used in Desktop version)
		/// </summary>
		SourceTextChanged,

		/// <summary>
		/// Current Translator/TranslationService changed
		/// </summary>
		XlatorSelected,
		/// <summary>
		/// Translating sections text mode in On/Off
		/// </summary>
		XlatingSections,
		/// <summary>
		/// Translating Quick/small text mode in On/Off
		/// </summary>
		XlatingQuick,
		/// <summary>
		/// Send translate Quick/small text request with string parameter from client
		/// </summary>
		XlatingQuickWithText,
		/// <summary>
		/// Translating percentage or error code by ProgressInfo
		/// </summary>
		XlatingPercentOrErrorCode,

		/// <summary>
		/// Data/Items source reload to reflect current status
		/// </summary>
		DataReload,
	}
}
