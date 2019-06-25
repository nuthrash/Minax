using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MinaxWebTranslator.Desktop.Models
{
	/// <summary>
	/// Input fields model for MultipleInputDialog
	/// </summary>
	internal class InputFieldModel
	{
		/// <summary>
		/// Column 1, Field Name
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		/// Column 2's Type
		/// </summary>
		public Type TypeInfo { get; set; }

		/// <summary>
		/// Original input value, or result value
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Converter for Enum type
		/// </summary>
		public IValueConverter Converter { get; set; }

		/// <summary>
		/// Placeholder/watermark text for mention this input field's usage
		/// </summary>
		public string Placeholder { get; set; }
	}
}
