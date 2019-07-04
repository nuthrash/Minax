using MinaxWebTranslator.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// About/Credits panel
	/// </summary>
	public partial class AboutCreditsPanel : UserControl
	{
		public AboutCreditsPanel( MainWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();

			RtbDescription.FontFamily = this.FontFamily;
		}

		private MainWindow mMainWindow;
		private List<CreditsItemModel> mWebXlators;
		private List<CreditsItemModel> mIconCredits;

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
			// load AppAbout.html with {AppVersion} replaced text to RichTextBox
			var appVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			string aboutStr = Languages.Global.AppAbout.Replace( "{AppVersion}", appVer.ToString() );

			var xamlStr = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml( aboutStr, false );
			StringBuilder sb = new StringBuilder( xamlStr );
			// adjust Table visual effects
			sb.Replace( "<Table>", "<Table TextAlignment=\"Justify\"><Table.Columns><TableColumn Width=\"Auto\"/><TableColumn Width=\"Auto\"/><TableColumn Width=\"Auto\"/></Table.Columns>" );
			sb.Replace( "<TableRow><TableCell ", "<TableRow><TableCell TextAlignment=\"Right\" " );
			sb.Replace( "BorderThickness=\"1,1,1,1\"", "BorderThickness=\"0,0,0,0\"" );
			sb.Replace( "</TableCell><TableCell ", "</TableCell><TableCell ColumnSpan=\"4\" " );
			var range = new TextRange( RtbDescription.Document.ContentStart, RtbDescription.Document.ContentEnd );
			using( var ms = new MemoryStream( Encoding.UTF8.GetBytes( sb.ToString() ) ) )
				range.Load( ms, DataFormats.Xaml );

			var conv = new ImageSourceConverter();

			var srcExcite = conv.ConvertFromString( "pack://application:,,,/Resources/Excite.png" ) as ImageSource;
			var srcCrossLang = conv.ConvertFromString( "pack://application:,,,/Resources/CrossLanguage.png" ) as ImageSource;
			var srcWeblio = conv.ConvertFromString( "pack://application:,,,/Resources/WeblioTranslator.png" ) as ImageSource;
			var srcBaidu = conv.ConvertFromString( "pack://application:,,,/Resources/BaiduTranslator.png" ) as ImageSource;
			var srcYoudao = conv.ConvertFromString( "pack://application:,,,/Resources/YoudaoTranslator.png" ) as ImageSource;
			var srcGoogle = conv.ConvertFromString( "pack://application:,,,/Resources/GoogleTranslator.png" ) as ImageSource;
			var srcMicrosoft = conv.ConvertFromString( "pack://application:,,,/Resources/Microsoft.png" ) as ImageSource;

			mWebXlators = new List<CreditsItemModel> {
				new CreditsItemModel { Title = $"Excite {Languages.Global.Str0Translator} (エキサイト翻訳)", Hyperlink = "https://www.excite.co.jp/world/", Icon = srcExcite },
				new CreditsItemModel { Title = $"CrossLanguage {Languages.Global.Str0Translator} (CROSS-Transer)", Hyperlink = "http://cross.transer.com", Icon = srcCrossLang },
				new CreditsItemModel { Title = $"Weblio {Languages.Global.Str0Translator} (Weblio 翻訳)", Hyperlink = "https://translate.weblio.jp/", Icon = srcWeblio },
				new CreditsItemModel { Title = $"{Languages.WebXlator.Str0Baidu} {Languages.Global.Str0Translator} (百度翻译)", Hyperlink = "https://fanyi.baidu.com", Icon = srcBaidu },
				new CreditsItemModel { Title = $"{Languages.WebXlator.Str0Youdao} {Languages.Global.Str0Translator} (有道翻译)", Hyperlink = "http://fanyi.youdao.com", Icon = srcYoudao },
				new CreditsItemModel { Title = $"Google {Languages.Global.Str0Translator}", Hyperlink = "https://translate.google.com/", Icon = srcGoogle },
				new CreditsItemModel { Title = $"Microsoft/Bing {Languages.Global.Str0Translator}", Hyperlink = "https://www.bing.com/translator", Icon = srcMicrosoft },
			};
			LvWebXlators.ItemsSource = mWebXlators;


			mIconCredits = new List<CreditsItemModel> {
				new CreditsItemModel { Icon = srcGoogle, Title = "Google Translate Logo (vector version)",
						Author = "Google Inc.", License = Languages.Global.Str0PublicDomain,
						Hyperlink = "https://commons.wikimedia.org/wiki/File:Google_Translate_logo.svg",
						Note = Languages.WebXlator.Str0Converted2PngByMinaxProject
				},

				new CreditsItemModel { Icon = srcExcite, Title = "Excite 1 Logo",
						Author = "Excite Inc.", License = Languages.Global.Str0PublicDomain,
						Hyperlink = "https://freebiesupply.com/logos/excite-1-logo/",
						//Hyperlink = "https://worldvectorlogo.com/logo/excite-1",
						Note = Languages.WebXlator.Str0ShrinkedByMinaxProject,
				},
			};

			DgCreditsIcons.Items.Clear();
			DgCreditsIcons.ItemsSource = mIconCredits;
		}
	}
}
