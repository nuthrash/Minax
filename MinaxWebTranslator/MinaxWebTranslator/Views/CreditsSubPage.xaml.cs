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
			//var srcCrossLang = ImageSource.FromResource( "MinaxWebTranslator.Resources.CrossLanguage.png" );
			var srcMirai = ImageSource.FromResource( "MinaxWebTranslator.Resources.MiraiTranslate.png" );
			var srcWeblio = ImageSource.FromResource( "MinaxWebTranslator.Resources.WeblioTranslator.png" );
			var srcBaidu = ImageSource.FromResource( "MinaxWebTranslator.Resources.BaiduTranslator.png" );
			var srcIciba = ImageSource.FromResource( "MinaxWebTranslator.Resources.IcibaTranslator.png" );
			var srcLingoCloud = ImageSource.FromResource( "MinaxWebTranslator.Resources.LingoCloud.png" );
			var srcYoudao = ImageSource.FromResource( "MinaxWebTranslator.Resources.YoudaoTranslator.png" );
			var srcPapago = ImageSource.FromResource( "MinaxWebTranslator.Resources.NaverPapago.png" );
			var srcGoogle = ImageSource.FromResource( "MinaxWebTranslator.Resources.GoogleTranslator.png" );
			var srcMicrosoft = ImageSource.FromResource( "MinaxWebTranslator.Resources.Microsoft.png" );

			mWebXlators = new List<CreditsItemModel> {
				//new CreditsItemModel {
				//	Icon = srcExcite, Title = $"Excite {Languages.Global.Str0Translator} (エキサイト翻訳)",
				//	Hyperlink = "https://www.excite.co.jp/world/",
				//},
				//new CreditsItemModel {
				//	Icon = srcCrossLang, Title = $"CrossLanguage {Languages.Global.Str0Translator} (CROSS-Transer)",
				//	Hyperlink = "http://cross.transer.com",
				//},
				new CreditsItemModel {
					Icon = srcMirai, Title = $"MiraiTranslate {Languages.Global.Str0Translator} (みらい翻訳)",
					Hyperlink = "https://miraitranslate.com/trial/",
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
					Icon = srcIciba, Title = $"{Languages.WebXlator.Str0Iciba} {Languages.Global.Str0Translator} (爱词霸)",
					Hyperlink = "https://www.iciba.com/fy",
				},
				new CreditsItemModel {
					Icon = srcLingoCloud, Title = $"{Languages.WebXlator.Str0LingoCloud} {Languages.Global.Str0Translator} (彩云小译)",
					Hyperlink = "https://fanyi.caiyunapp.com/",
				},
				new CreditsItemModel {
					Icon = srcYoudao, Title = $"{Languages.WebXlator.Str0Youdao} {Languages.Global.Str0Translator} (有道翻译)",
					Hyperlink = "https://fanyi.youdao.com",
				},
				new CreditsItemModel {
					Icon = srcPapago, Title = $"{Languages.WebXlator.Str0Papago} {Languages.Global.Str0Translator}",
					Hyperlink = "https://papago.naver.com",
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
