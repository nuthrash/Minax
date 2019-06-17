using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MinaxWebTranslator.Desktop.Commands
{
	internal static class AppCommands
	{

		/// <summary>
		/// Close Flyout or BaseMetroDialog of MahApps framework
		/// </summary>
		public static ICommand CloseCmd => new SimpleCommand( o => true, ( param ) => {
			if( param is Flyout fo )
				fo.IsOpen = false;
			else if( param is MahApps.Metro.Controls.Dialogs.BaseMetroDialog bmd )
				bmd.RequestCloseAsync();
		} );

		/// <summary>
		/// Copy all text to ClipBoard from TextBox/RichTextBox...
		/// </summary>
		public static ICommand CopyAllCmd => new SimpleCommand(
			( canObj ) => {
				if( canObj is RichTextBox rtb && rtb.Document != null )
					return rtb.Document.Blocks.Count > 0;
				if( canObj is TextBox tb )
					return tb.Text != null;
				return canObj != null;
			},
			( param ) => {
				if( param is System.Windows.Controls.Primitives.TextBoxBase tb ) {
					tb.SelectAll();
					tb.Copy();
				}
			} );

		/// <summary>
		/// Append text to TextBox/RichTextBox from ClipBoard
		/// </summary>
		public static ICommand PasteCmd => new SimpleCommand( o => Clipboard.ContainsText(), (param) => {
			if( param is System.Windows.Controls.Primitives.TextBoxBase tb )
				tb.Paste();
		} );

		/// <summary>
		/// Clear and paste pure text to TextBox/RichTextBox...
		/// </summary>
		public static ICommand ClearAndPasteCmd => new SimpleCommand( o => Clipboard.ContainsText(), ( param ) => {
			if( param is RichTextBox rtb ) {
				rtb.Document.Blocks.Clear();
				rtb.Paste();
			}
			else if( param is TextBox tbox ) {
				tbox.Clear();
				tbox.Paste();
			}
		} );

	}
}
