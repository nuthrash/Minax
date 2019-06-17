using System;
using System.Collections.Generic;
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

		public static string GetAllText( this RichTextBox rtb )
		{
			var tr = new TextRange( rtb.Document.ContentStart, rtb.Document.ContentEnd );
			return tr.Text;
		}

		// https://stackoverflow.com/questions/818704/how-to-convert-securestring-to-system-string
		public static string ConvertToString( this SecureString value )
		{
			return new System.Net.NetworkCredential( string.Empty, value ).Password;
		}

		public static SecureString ConvertToSecureString( this string value )
		{
			SecureString ss = new SecureString();
			foreach( var ch in value )
				ss.AppendChar( ch );

			return ss;
		}

		public static string LayoutToString( this DockingManager dockManager )
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter( sb );
			var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			serializer.Serialize( sw );
			return sb.ToString();
		}

		public static void LayoutFromString( this DockingManager dockManager, string layoutXml )
		{
			StringReader sr = new StringReader( layoutXml );
			var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			serializer.Deserialize( sr );
		}
	}
}
