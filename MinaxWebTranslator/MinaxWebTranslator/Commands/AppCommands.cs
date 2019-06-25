using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MinaxWebTranslator.Commands
{
	/// <summary>
	/// App internal commands
	/// </summary>
	internal static class AppCommands
	{
		/// <summary>
		/// Clear Entry/Editor content
		/// </summary>
		public static ICommand ClearCmd => new Command(
			(param) => {
				if( param is Entry entry )
					entry.Text = "";
				else if( param is Editor editor )
					editor.Text = "";
			},
			canExecute: o => o != null );

		/// <summary>
		/// Copy all text to ClipBoard from TextBox/RichTextBox...
		/// </summary>
		public static ICommand CopyAllCmd => new Command(
			async ( param ) => {
				if( param is Entry entry ) {
					await Clipboard.SetTextAsync( entry.Text );
				} else if ( param is Editor editor ) {
					await Clipboard.SetTextAsync( editor.Text );
				}

			},
			canExecute: (canObj) => {
				if( canObj is Entry || canObj is Editor )
					return true;
				return false;
			} );

		/// <summary>
		/// Append text to TextBox/RichTextBox from ClipBoard
		/// </summary>
		public static ICommand PasteCmd => new Command(
			async (param) => {
				var text = await Clipboard.GetTextAsync();
				if( param is Entry entry ) {
					entry.Text = entry.Text.Insert( entry.CursorPosition, text );
				}
				else if( param is Editor editor ) {
					editor.Text = editor.Text + text; // append, not insert!!
				}
			},
			canExecute: o => Clipboard.HasText );

		/// <summary>
		/// Clear and paste pure text to TextBox/RichTextBox...
		/// </summary>
		public static ICommand ClearAndPasteCmd => new Command(
			async ( param ) => {
				var text = await Clipboard.GetTextAsync();
				if( param is Entry entry ) {
					entry.Text = text;
				} else if( param is Editor editor ) {
					editor.Text = text;
				}
			},
			canExecute: o => Clipboard.HasText );

		/// <summary>
		/// Open hyperlink by system web browser
		/// </summary>
		public static ICommand OpenWebCmd => new Command(
			canExecute: o => !string.IsNullOrWhiteSpace( o as string ),
			execute: (param) => {
				try {
					var uri = new Uri( param as string );
					Device.OpenUri( uri );
				}
				catch { }
			} );
	}
}
