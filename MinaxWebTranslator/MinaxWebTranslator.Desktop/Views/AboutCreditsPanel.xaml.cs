using MinaxWebTranslator.Desktop.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MinaxWebTranslator.Desktop.Views
{
	public partial class AboutCreditsPanel : UserControl
	{
		public AboutCreditsPanel( MainWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();
		}

		private MainWindow mMainWindow;
		private List<WebTranslatorModel> mWebXlators;
		private List<IconCreditsModel> mIconCredits;

		private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
		{
			Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
			e.Handled = true;
		}

		private void DataGridHyperlink_Click( object sender, RoutedEventArgs e )
		{
			Hyperlink link = (Hyperlink)e.OriginalSource;
			Process.Start( link.NavigateUri.AbsoluteUri );
		}

		private void Hyperlink_MouseLeftButtonDown( object sender, MouseEventArgs e )
		{
			var hyperlink = (Hyperlink)sender;
			Process.Start( hyperlink.NavigateUri.ToString() );
		}

		private void SvCredits_PreviewMouseWheel( object sender, System.Windows.Input.MouseWheelEventArgs e )
		{
			SvCredits.ScrollToVerticalOffset( SvCredits.VerticalOffset - (double)e.Delta );
		}

		private void AboutCreditsPanel_Loaded( object sender, RoutedEventArgs e )
		{
			var conv = new ImageSourceConverter();

			var srcExcite = conv.ConvertFromString( "pack://application:,,,/Resources/Excite.png" ) as ImageSource;
			var srcCrossLang = conv.ConvertFromString( "pack://application:,,,/Resources/CrossLanguage.png" ) as ImageSource;
			var srcWeblio = conv.ConvertFromString( "pack://application:,,,/Resources/WeblioTranslator.png" ) as ImageSource;
			var srcBaidu = conv.ConvertFromString( "pack://application:,,,/Resources/BaiduTranslator.png" ) as ImageSource;
			var srcYoudao = conv.ConvertFromString( "pack://application:,,,/Resources/YoudaoTranslator.png" ) as ImageSource;
			var srcGoogle = conv.ConvertFromString( "pack://application:,,,/Resources/GoogleTranslator.png" ) as ImageSource;
			var srcMicrosoft = conv.ConvertFromString( "pack://application:,,,/Resources/Microsoft.png" ) as ImageSource;

			mWebXlators = new List<WebTranslatorModel> {
				new WebTranslatorModel { Title = "Excite Translator (エキサイト翻訳)", Hyperlink = "https://www.excite.co.jp/world/", Icon = srcExcite },
				new WebTranslatorModel { Title = "CrossLanguage Translator (CROSS-Transer)", Hyperlink = "http://cross.transer.com", Icon = srcCrossLang },
				new WebTranslatorModel { Title = "Weblio Translator (Weblio 翻訳)", Hyperlink = "https://translate.weblio.jp/", Icon = srcWeblio },
				new WebTranslatorModel { Title = "Baidu Translator (百度翻译)", Hyperlink = "https://fanyi.baidu.com", Icon = srcBaidu },
				new WebTranslatorModel { Title = "Youdao Translator (有道翻译)", Hyperlink = "http://fanyi.youdao.com", Icon = srcYoudao },
				new WebTranslatorModel { Title = "Google Translator", Hyperlink = "https://translate.google.com/", Icon = srcGoogle },
				new WebTranslatorModel { Title = "Microsoft/Bing Translator", Hyperlink = "https://www.bing.com/translator", Icon = srcMicrosoft },
			};
			LvWebXlators.ItemsSource = mWebXlators;


			mIconCredits = new List<IconCreditsModel> {
				new IconCreditsModel { Icon = srcGoogle, Title = "Google Translate Logo (vector version)",
						Author = "Google Inc.", License = "Public Domain",
						Hyperlink = "https://commons.wikimedia.org/wiki/File:Google_Translate_logo.svg",
						Note = "Converted to .png by Minax project."
				},

				new IconCreditsModel { Icon = srcExcite, Title = "Excite 1 Logo",
						Author = "Excite Inc.", License = "Public Domain?",
						Hyperlink = "https://freebiesupply.com/logos/excite-1-logo/",
						//Hyperlink = "https://worldvectorlogo.com/logo/excite-1",
						Note = "Shrinked by Minax project.",
				},
			};

			DgCreditsIcons.Items.Clear();
			DgCreditsIcons.ItemsSource = mIconCredits;
		}
	}
}
