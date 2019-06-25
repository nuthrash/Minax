using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MinaxWebTranslator.Desktop.Models
{
	/// <summary>
	/// Input fields model for MultipleInputsView
	/// </summary>
	internal class InputFieldModel
	{
		public string FieldName { get; set; }

		public Type TypeInfo { get; set; }

		public object Value { get; set; }

		public IValueConverter Converter { get; set; }

		public string Placeholder { get; set; }
	}
}
