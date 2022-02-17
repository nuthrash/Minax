using Minax.Collections;
using Minax.Web.Translation;
using MinaxWebTranslator.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// Translator selector panel for selecting and configuring translator
	/// </summary>
	public partial class TranslatorSelectorPanel : UserControl
	{
		internal TranslatorSelector SelectedTranslator {
			get => mSelectedTranslator;
			set {
				var xlator = sSelectorList[0];
				if( value != null ) {
					foreach( var ts in sSelectorList ) {
						if( ts.RemoteType == value.RemoteType ) {
							xlator = value;
							break;
						}
					}
				}
				LvXlatorSelector.SelectedItem = xlator;
				NeedReloading = false;
			}
		}

		internal RemoteType SelectedTranslatorType {
			get {
				if( mSelectedTranslator == null )
					return RemoteType.None;
				return mSelectedTranslator.RemoteType;
			}
			set {
				var xlator = sSelectorList[0];
				foreach( var ts in sSelectorList ) {
					if( ts.RemoteType == value ) {
						xlator = ts;
						break;
					}
				}
				LvXlatorSelector.SelectedItem = xlator;
				NeedReloading = false;
			}
		}

		internal bool NeedReloading { get; private set; }

		internal ReadOnlyObservableList<TranslatorSelector> SupportedSelectors => sSelectorList;

		public TranslatorSelectorPanel()
		{
			InitializeComponent();

			// prepare charged API Grid list
			mChargedApiGrids = new List<Grid> { GdBaiduApi, GdYoudaoApi, GdGoogleApi, GdMicrosoftApi };

			_CheckAndCreateSelectors();

			LvXlatorSelector.ItemsSource = sSelectorList;

			RemoteType remoteType = RemoteType.Excite;
			Enum.TryParse( Properties.Settings.Default.RemoteXlatorType, out remoteType );
			this.SelectedTranslatorType = remoteType;

			// resize ListView's MaxHeight to let it show ScrollBar!!
			this.SizeChanged += ( s1, e1 ) => {
				_CalculateListViewMaxHeight();
			};
		}

		internal void ReloadSettings()
		{
			// prepare app settings
			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorCrypto ) ) {
				Properties.Settings.Default.XlatorCrypto = (DateTime.Now.Ticks + (new Random().Next( 99999, 99999999 ))).ToString();
			}
			var cryptoKey = mCrypto = Properties.Settings.Default.XlatorCrypto;

			if( string.IsNullOrEmpty( Properties.Settings.Default.XlatorBaiduAppId ) == false ) {
				TbXlatorBaiduAppId.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduAppId, cryptoKey );
			}
			if( string.IsNullOrEmpty( Properties.Settings.Default.XlatorBaiduSecretKey ) == false ) {
				TbXlatorBaiduSecretKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduSecretKey, cryptoKey );
			}

			if( string.IsNullOrEmpty( Properties.Settings.Default.XlatorYoudaoAppKey ) == false ) {
				TbXlatorYoudaoAppKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorYoudaoAppKey, cryptoKey );
			}
			if( string.IsNullOrEmpty( Properties.Settings.Default.XlatorYoudaoAppSecret ) == false ) {
				TbXlatorYoudaoAppSecret.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorYoudaoAppSecret, cryptoKey );
			}

			if( string.IsNullOrEmpty( Properties.Settings.Default.XlatorGoogleApiKey ) == false ) {
				TbXlatorGoogleApiKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorGoogleApiKey, cryptoKey );
			}

			TbXlatorMicrosoftSubRegion.Text = Properties.Settings.Default.XlatorMicrosoftSubRegion;
			if( string.IsNullOrEmpty( Properties.Settings.Default.XlatorMicrosoftSubKey ) == false ) {
				TbXlatorMicrosoftSubKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorMicrosoftSubKey, cryptoKey );
			}
			if( string.IsNullOrEmpty( Properties.Settings.Default.XlatorMicrosoftServer ) == false ) {
				var serv = Properties.Settings.Default.XlatorMicrosoftServer;
				for( int i = 0; i < CbXlatorMicrosoftServer.Items.Count; ++i ) {
					if( serv == (CbXlatorMicrosoftServer.Items[i] as ComboBoxItem).ToolTip as string ) {
						CbXlatorMicrosoftServer.SelectedIndex = i;
						break;
					}
				}
			}
		}

		private TranslatorSelector mSelectedTranslator;
		private static ObservableList<TranslatorSelector> sSelectorList = null;
		private List<Grid> mChargedApiGrids = null;
		private string mCrypto = null;
		private Timer mDeferredWorker = null;

		private static void _CheckAndCreateSelectors()
		{
			if( sSelectorList != null )
				return;

			var conv = new ImageSourceConverter();
			// prepare TranslatorSelector models
			var srcExcite = conv.ConvertFromString( "pack://application:,,,/Resources/Excite.png" ) as ImageSource;
			var srcCrossLang = conv.ConvertFromString( "pack://application:,,,/Resources/CrossLanguage.png" ) as ImageSource;
			var srcWeblio = conv.ConvertFromString( "pack://application:,,,/Resources/WeblioTranslator.png" ) as ImageSource;
			var srcBaidu = conv.ConvertFromString( "pack://application:,,,/Resources/BaiduTranslator.png" ) as ImageSource;
			var srcYoudao = conv.ConvertFromString( "pack://application:,,,/Resources/YoudaoTranslator.png" ) as ImageSource;
			var srcGoogle = conv.ConvertFromString( "pack://application:,,,/Resources/GoogleTranslator.png" ) as ImageSource;
			var srcMicrosoft = conv.ConvertFromString( "pack://application:,,,/Resources/Microsoft.png" ) as ImageSource;

			var list = new ObservableList<TranslatorSelector>() {
					new TranslatorSelector { RemoteType = RemoteType.Excite, Header = "Excite", Checked = true, Icon = srcExcite, Description = Languages.WebXlator.Str0ExciteXlatorJapan },
					new TranslatorSelector { RemoteType = RemoteType.CrossLanguage, Header = "CrossLanguage", Checked = false, Icon = srcCrossLang, Description = Languages.WebXlator.Str0XTranserXlatorJapan },
					new TranslatorSelector { RemoteType = RemoteType.Weblio, Header = "Weblio", Checked = false, Icon = srcWeblio, Description = Languages.WebXlator.Str0WeblioXlatorJapan },
					// new TranslatorSelector { RemoteType = RemoteType.Baidu, Header = Languages.WebXlator.Str0Baidu, Checked = false, Icon = srcBaidu, Description = Languages.WebXlator.Str0BaiduXlatorChina },
					new TranslatorSelector { RemoteType = RemoteType.Youdao, Header = Languages.WebXlator.Str0Youdao, Checked = false, Icon = srcYoudao, Description = Languages.WebXlator.Str0YoudaoXlatorChina },
					// new TranslatorSelector { RemoteType = RemoteType.Google, Header = "Google", Checked = false, Icon = srcGoogle, Description = Languages.WebXlator.Str0GoogleXlatorAmerica },
					// new TranslatorSelector { RemoteType = RemoteType.Microsoft, Header = "Microsoft", Checked = false, Icon = srcMicrosoft, Description = Languages.WebXlator.Str0MicrosoftXlatorAmerica },

					//new TranslatorSelector { SeparatorVisibility = Visibility.Visible, Header = "Free Web APIs" },
					new TranslatorSelector { SeparatorVisibility = Visibility.Visible, },
					
					new TranslatorSelector { RemoteType = RemoteType.CrossLanguageFree, Header = string.Format( Languages.WebXlator.Str1XlationApiFree, "CrossLanguage" ), Checked = false,
							Icon = srcCrossLang, Description = Languages.WebXlator.Str0XTranserXlationJapan },
					new TranslatorSelector { RemoteType = RemoteType.BaiduFree, Header = string.Format( Languages.WebXlator.Str1XlationApiFree, Languages.WebXlator.Str0Baidu ), Checked = false,
							Icon = srcBaidu, Description = Languages.WebXlator.Str0BaiduXlationChina },
					new TranslatorSelector { RemoteType = RemoteType.YoudaoFree, Header = string.Format( Languages.WebXlator.Str1XlationApiFree, Languages.WebXlator.Str0Youdao ), Checked = false,
							Icon = srcYoudao, Description = Languages.WebXlator.Str0YoudaoXlationChina },
					new TranslatorSelector { RemoteType = RemoteType.GoogleFree, Header = string.Format( Languages.WebXlator.Str1XlationApiFree, "Google" ), Checked = false,
							Icon = srcGoogle, Description = Languages.WebXlator.Str0GoogleXlationAmerica },
					//new TranslatorSelector { RemoteType = RemoteType.MicrosoftFree, Header = "Microsoft API (Free)", Checked = false, Icon = srcMicrosoft, Description = Languages.WebXlator.Str0MicrosoftXlationAmerica },

					//new TranslatorSelector { SeparatorVisibility = Visibility.Visible, Header = "Charged Web APIs" },
					new TranslatorSelector { SeparatorVisibility = Visibility.Visible, },

					new TranslatorSelector { RemoteType = RemoteType.BaiduCharged, Header=string.Format( Languages.WebXlator.Str1XlationApiCharged, Languages.WebXlator.Str0Baidu ), Checked = false,
							Icon = srcBaidu, Description = Languages.WebXlator.Str0BaiduXlationChargedChina },
					new TranslatorSelector { RemoteType = RemoteType.YoudaoCharged, Header=string.Format( Languages.WebXlator.Str1XlationApiCharged, Languages.WebXlator.Str0Youdao ), Checked = false,
							Icon = srcYoudao, Description = Languages.WebXlator.Str0YoudaoXlationChargedChina },
					new TranslatorSelector { RemoteType = RemoteType.GoogleCharged, Header=string.Format( Languages.WebXlator.Str1XlationApiCharged, "Google" ), Checked = false,
							Icon = srcGoogle, Description = Languages.WebXlator.Str0GoogleXlationChargedAmerica },
					new TranslatorSelector { RemoteType = RemoteType.MicrosoftCharged, Header=string.Format( Languages.WebXlator.Str1XlationApiCharged, "Microsoft" ), Checked = false,
							Icon = srcMicrosoft, Description = Languages.WebXlator.Str0MicrosoftXlationChargedAmerica },
				};

			sSelectorList = list;

		}

		private void _CalculateListViewMaxHeight()
		{
			Point relativePoint = LvXlatorSelector.TransformToAncestor( SpMain )
										.Transform( new Point( 0, 0 ) );
			LvXlatorSelector.MaxHeight = this.ActualHeight - relativePoint.Y;
		}


		private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
		{
			Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
			e.Handled = true;
		}

		private void CbXlatorMicrosoftServer_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			int idx = 0;
			if( CbXlatorMicrosoftServer.SelectedIndex > 0 )
				idx = CbXlatorMicrosoftServer.SelectedIndex;
			Properties.Settings.Default.XlatorMicrosoftServer = (CbXlatorMicrosoftServer.Items[idx] as ComboBoxItem).ToolTip.ToString();
			Properties.Settings.Default.Save();
		}

		private void TranslatorSelectorPanel_Loaded( object sender, RoutedEventArgs e )
		{
			ReloadSettings();

			CbXlatorMicrosoftServer.SelectionChanged -= CbXlatorMicrosoftServer_SelectionChanged;
			CbXlatorMicrosoftServer.SelectionChanged += CbXlatorMicrosoftServer_SelectionChanged;
		}

		private void LvXlatorSelector_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			LblWarningCharged.Visibility = Visibility.Collapsed;

			var model = LvXlatorSelector.SelectedItem as TranslatorSelector;
			if( model == null || model.SeparatorVisibility == Visibility.Visible )
				return;

			if( mSelectedTranslator == model )
				return;

			NeedReloading = true;

			LblCurrent.Content = model.Header;
			mSelectedTranslator = model;
			Properties.Settings.Default.RemoteXlatorType = model.RemoteType.ToString();
			Properties.Settings.Default.Save();

			// defer to update UI
			if( mDeferredWorker != null )
				mDeferredWorker.Dispose();
			mDeferredWorker = new Timer( (sub) => {
				Dispatcher.Invoke( async () => {
					foreach( var gd in mChargedApiGrids ) {
						gd.Visibility = Visibility.Collapsed;
					}

					Grid gridCharged = null;
					switch( mSelectedTranslator.RemoteType ) {
						case RemoteType.BaiduCharged:
							gridCharged = GdBaiduApi;
							break;
						case RemoteType.YoudaoCharged:
							gridCharged = GdYoudaoApi;
							break;
						case RemoteType.GoogleCharged:
							gridCharged = GdGoogleApi;
							break;
						case RemoteType.MicrosoftCharged:
							gridCharged = GdMicrosoftApi;
							break;
					}

					if( gridCharged != null ) {
						gridCharged.Visibility = Visibility.Visible;
						LblWarningCharged.Visibility = Visibility.Visible;
					}

					await Task.Delay( 100 );
					_CalculateListViewMaxHeight();
					LvXlatorSelector.ScrollIntoView( mSelectedTranslator );


					if( mDeferredWorker != null )
						mDeferredWorker.Dispose();
					mDeferredWorker = null;
				} );
			}, null, 350, Timeout.Infinite);
		}

		private void TbXlatorBaiduAppId_TextChanged( object sender, TextChangedEventArgs e )
		{
			Properties.Settings.Default.XlatorBaiduAppId = Minax.Utils.EncryptToBase64( TbXlatorBaiduAppId.Text, mCrypto );
			Properties.Settings.Default.Save();
		}

		private void TbXlatorBaiduSecretKey_TextChanged( object sender, TextChangedEventArgs e )
		{
			Properties.Settings.Default.XlatorBaiduSecretKey = Minax.Utils.EncryptToBase64( TbXlatorBaiduSecretKey.Text, mCrypto );
			Properties.Settings.Default.Save();
		}

		private void TbXlatorYoudaoAppKey_TextChanged( object sender, TextChangedEventArgs e )
		{
			Properties.Settings.Default.XlatorYoudaoAppKey = Minax.Utils.EncryptToBase64( TbXlatorYoudaoAppKey.Text, mCrypto );
			Properties.Settings.Default.Save();
		}

		private void TbXlatorYoudaoAppSecret_TextChanged( object sender, TextChangedEventArgs e )
		{
			Properties.Settings.Default.XlatorYoudaoAppSecret = Minax.Utils.EncryptToBase64( TbXlatorYoudaoAppSecret.Text, mCrypto );
			Properties.Settings.Default.Save();
		}

		private void TbXlatorGoogleApiKey_TextChanged( object sender, TextChangedEventArgs e )
		{
			Properties.Settings.Default.XlatorGoogleApiKey = Minax.Utils.EncryptToBase64( TbXlatorGoogleApiKey.Text, mCrypto );
			Properties.Settings.Default.Save();
		}

		private void TbXlatorMicrosoftSubKey_TextChanged( object sender, TextChangedEventArgs e )
		{
			Properties.Settings.Default.XlatorMicrosoftSubKey = Minax.Utils.EncryptToBase64( TbXlatorMicrosoftSubKey.Text, mCrypto );
			Properties.Settings.Default.Save();
		}

		private void TbXlatorMicrosoftSubRegion_TextChanged( object sender, TextChangedEventArgs e )
		{
			Properties.Settings.Default.XlatorMicrosoftSubRegion = TbXlatorMicrosoftSubRegion.Text;
			Properties.Settings.Default.Save();
		}
	}
}
