using MinaxWebTranslator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class CreditsSubPage : ContentPage
	{
		public CreditsSubPage()
		{
			InitializeComponent();

			//var srcExcite = ImageSource.FromResource( "MinaxWebTranslator.Resources.Excite.png" );
			//var srcGoogle = ImageSource.FromResource( "MinaxWebTranslator.Resources.GoogleTranslator.png" );

			//mIconCredits = new List<CreditsItemModel> {
			//	new CreditsItemModel { Icon = srcGoogle, Title = "Google Translate Logo (vector version)",
			//			Author = "Google Inc.", License = "Public Domain",
			//			Hyperlink = "https://commons.wikimedia.org/wiki/File:Google_Translate_logo.svg",
			//			Note = "Converted to .png by Minax project."
			//	},

			//	new CreditsItemModel { Icon = srcExcite, Title = "Excite 1 Logo",
			//			Author = "Excite Inc.", License = "Public Domain?",
			//			Hyperlink = "https://freebiesupply.com/logos/excite-1-logo/",
			//			//Hyperlink = "https://worldvectorlogo.com/logo/excite-1",
			//			Note = "Shrinked by Minax project.",
			//	},
			//};

			//DgCreditsIcons.ItemsSource = mIconCredits;


			//mCredits3rdParty = new List<CreditsItemModel> {
			//	new CreditsItemModel {
			//		Title = "Json.NET", Hyperlink = "https://www.newtonsoft.com/json",
			//	},
			//	new CreditsItemModel {
			//		Title = "HtmlAgilityPack", Hyperlink = "https://html-agility-pack.net/",
			//	},
			//	new CreditsItemModel {
			//		Title = "FileHelpers", Hyperlink = "https://www.filehelpers.net/",
			//	},
			//};
			//DgCredits3rdParty.ItemsSource = mCredits3rdParty;
		}

		//private List<CreditsItemModel> mIconCredits;
		private List<CreditsItemModel> mCredits3rdParty;
	}
}