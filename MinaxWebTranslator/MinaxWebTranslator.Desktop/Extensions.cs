using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Xceed.Wpf.AvalonDock;

namespace MinaxWebTranslator.Desktop
{
	internal static class Extensions
	{
		/// <summary>
		/// Sort a string list with natural number ordering
		/// </summary>
		/// <param name="list">Original unsorted list</param>
		/// <returns>Sorted IEnumerable</returns>
		/// <remarks>https://stackoverflow.com/questions/11052095/how-can-i-sort-a-string-of-text-followed-by-a-number-using-linq/11052176#11052176</remarks>
		public static IEnumerable<string> NumericSort( this IEnumerable<string> list )
		{
			if( list.Count() <= 0 )
				return new string[] { };

			int maxLen = list.Select( s => s.Length ).Max();

			return list.Select( s => new {
				OrgStr = s,
				SortStr = Regex.Replace( s, @"(\d+)|(\D+)", m => m.Value.PadLeft( maxLen, char.IsDigit( m.Value[0] ) ? ' ' : '\xffff' ) )
			} )
			.OrderBy( x => x.SortStr )
			.Select( x => x.OrgStr );
		}

		/// <summary>
		/// Clear DataGrid's sorting to default
		/// </summary>
		/// <param name="grid">Target DatGrid</param>
		/// <remarks>https://stackoverflow.com/questions/13401869/wpf-datagrid-clear-column-sorting</remarks>
		public static void ClearSort( this DataGrid grid )
		{
			ICollectionView view = CollectionViewSource.GetDefaultView( grid.ItemsSource );
			if( view != null ) {
				view.SortDescriptions.Clear();
				foreach( DataGridColumn column in grid.Columns ) {
					column.SortDirection = null;
				}
			}
		}

		/// <summary>
		/// Convert StringCollection content to List<string>
		/// </summary>
		/// <param name="strCollection">Source collection to be converted</param>
		/// <returns>Converted List of string</returns>
		public static List<string> ConvertToList( this StringCollection strCollection )
		{
			List<string> tmpList = new List<string>();
			foreach( var str in strCollection )
				tmpList.Add( str );

			return tmpList;
		}

		/// <summary>
		/// Convert a SecureString to a string
		/// </summary>
		/// <param name="value">Source SecureString</param>
		/// <returns>Converted string</returns>
		// https://stackoverflow.com/questions/818704/how-to-convert-securestring-to-system-string
		public static string ConvertToString( this SecureString value )
		{
			return new System.Net.NetworkCredential( string.Empty, value ).Password;
		}

		/// <summary>
		/// Convert a stirng to a SecureString
		/// </summary>
		/// <param name="value">Source string</param>
		/// <returns>Converted SecureString</returns>
		public static SecureString ConvertToSecureString( this string value )
		{
			SecureString ss = new SecureString();
			foreach( var ch in value )
				ss.AppendChar( ch );

			return ss;
		}

		/// <summary>
		/// Get all text of a RichTextBox
		/// </summary>
		/// <param name="rtb">Source RichTextBox</param>
		/// <returns>All text string of a RichTextBox</returns>
		public static string GetAllText( this RichTextBox rtb )
		{
			return new TextRange( rtb.Document.ContentStart, rtb.Document.ContentEnd ).Text;
		}

		/// <summary>
		/// Get current text line count of a RichTextBox
		/// </summary>
		/// <param name="rtb">Source RichTextBox</param>
		/// <returns>Current text line count of this RichTextBox</returns>
		public static int LineCount( this RichTextBox rtb )
		{
			int lineNumber;
			var lineEnd = rtb.Document.ContentEnd.GetInsertionPosition( LogicalDirection.Backward );
			lineEnd.GetLineStartPosition( int.MinValue, out lineNumber );
			return -lineNumber + 1;
		}

		/// <summary>
		/// Get current text line of cursor of a RichTextBox 
		/// </summary>
		/// <param name="rtb">Source RichTextBox</param>
		/// <returns>Current cursor line number(1-based) of this RichTextBox</returns>
		public static int CurrentLine( this RichTextBox rtb )
		{
			int lineNumber;
			rtb.CaretPosition.GetLineStartPosition( int.MinValue, out lineNumber );
			return -lineNumber + 1;
		}

		/// <summary>
		/// Convert layout data to a string from this DockingManager
		/// </summary>
		/// <param name="dockManager">Source DockingManager</param>
		/// <returns>Converted string of layout data</returns>
		public static string LayoutToString( this DockingManager dockManager )
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter( sb );
			var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			serializer.Serialize( sw );
			return sb.ToString();
		}

		/// <summary>
		/// Convert docking XML string to layout data to this DockingManager
		/// </summary>
		/// <param name="dockManager">Target DockingManager</param>
		/// <param name="layoutXml">Layout XML string</param>
		public static void LayoutFromString( this DockingManager dockManager, string layoutXml )
		{
			StringReader sr = new StringReader( layoutXml );
			var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			serializer.Deserialize( sr );
		}
	}
}
