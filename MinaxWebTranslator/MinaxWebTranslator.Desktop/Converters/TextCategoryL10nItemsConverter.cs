using Minax.Domain.Translation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace MinaxWebTranslator.Desktop.Converters
{
	/// <summary>
	/// Mapping text category converter
	/// </summary>
	internal class TextCategoryL10nItemsConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if( value is TextCategory ) {
				return Minax.Utils.GetTextCategoryL10nString( (TextCategory)value );
			}
			if( value is IEnumerable<TextCategory> ) {
				return Minax.Utils.GetAllTextCategoryL10nStrings();
			}

			throw new NotImplementedException();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if( value is string str ) {
				return Minax.Utils.GetTextCategoryL10nValue( str );
			}
			if( value is IEnumerable<string> valueList ) {
				var back = new List<TextCategory>();
				foreach( var desc in valueList ) {
					back.Add( Minax.Utils.GetTextCategoryL10nValue( desc ) );
				}
				return back;
			}

			throw new NotImplementedException();
		}
	}
}
