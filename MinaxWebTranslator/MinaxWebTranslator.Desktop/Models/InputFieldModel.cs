using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MinaxWebTranslator.Desktop.Models
{
	internal class InputFieldModel
	{
		public string FieldName { get; set; }

		public Type TypeInfo { get; set; }

		public object Value { get; set; }

		public IValueConverter Converter { get; set; }

		public string PlaceHolder { get; set; }
	}
}
