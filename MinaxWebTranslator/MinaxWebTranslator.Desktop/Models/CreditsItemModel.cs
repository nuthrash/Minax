using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MinaxWebTranslator.Desktop.Models
{
	/// <summary>
	/// About/Credits item model
	/// </summary>
	internal class CreditsItemModel
	{
		public string Title { get; set; }

		public string Hyperlink { get; set; }

		public string Author { get; set; }

		public ImageSource Icon { get; set; }

		public string License { get; set; }
		
		public string Note { get; set; }
	}
}
