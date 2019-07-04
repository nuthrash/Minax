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

			var srcExcite = ImageSource.FromResource( "MinaxWebTranslator.Resources.Excite.png" );
			var srcCrossLang = ImageSource.FromResource( "MinaxWebTranslator.Resources.CrossLanguage.png" );
			var srcWeblio = ImageSource.FromResource( "MinaxWebTranslator.Resources.WeblioTranslator.png" );
			var srcBaidu = ImageSource.FromResource( "MinaxWebTranslator.Resources.BaiduTranslator.png" );
			var srcYoudao = ImageSource.FromResource( "MinaxWebTranslator.Resources.YoudaoTranslator.png" );
			var srcGoogle = ImageSource.FromResource( "MinaxWebTranslator.Resources.GoogleTranslator.png" );
			var srcMicrosoft = ImageSource.FromResource( "MinaxWebTranslator.Resources.Microsoft.png" );

			mWebXlators = new List<CreditsItemModel> {
				new CreditsItemModel {
					Icon = srcExcite, Title = $"Excite {Languages.Global.Str0Translator} (エキサイト翻訳)",
					Hyperlink = "https://www.excite.co.jp/world/",
				},
				new CreditsItemModel {
					Icon = srcCrossLang, Title = $"CrossLanguage {Languages.Global.Str0Translator} (CROSS-Transer)",
					Hyperlink = "http://cross.transer.com",
				},
				new CreditsItemModel {
					Icon = srcWeblio, Title = $"Weblio {Languages.Global.Str0Translator} (Weblio 翻訳)",
					Hyperlink = "https://translate.weblio.jp/",
				},
				new CreditsItemModel {
					Icon = srcBaidu, Title = $"{Languages.WebXlator.Str0Baidu} {Languages.Global.Str0Translator} (百度翻译)",
					Hyperlink = "https://fanyi.baidu.com",
				},
				new CreditsItemModel {
					Icon = srcYoudao, Title = $"{Languages.WebXlator.Str0Youdao} {Languages.Global.Str0Translator} (有道翻译)",
					Hyperlink = "http://fanyi.youdao.com",
				},
				new CreditsItemModel {
					Icon = srcGoogle, Title = $"Google {Languages.Global.Str0Translator}",
					Hyperlink = "https://translate.google.com/",
				},
				new CreditsItemModel {
					Icon = srcMicrosoft, Title = $"Microsoft/Bing {Languages.Global.Str0Translator}",
					Hyperlink = "https://www.bing.com/translator",
				},
			};

			LvWebXlators.ItemsSource = mWebXlators;
		}

		private List<CreditsItemModel> mWebXlators;
	}
}
