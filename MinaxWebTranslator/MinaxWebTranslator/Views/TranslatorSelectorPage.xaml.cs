using Minax.Collections;
using Minax.Web.Translation;
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
	public partial class TranslatorSelectorPage : ContentPage
	{
		internal TranslatorSelector SelectedTranslator {
			get => mSelectedTranslator;
			set {
				mSelectedTranslator = sSelectorList[0];
				if( value != null ) {
					foreach( var ts in sSelectorList ) {
						if( ts.RemoteType == value.RemoteType ) {
							mSelectedTranslator = value;
							break;
						}
					}
				}
				LvXlatorSelector.SelectedItem = mSelectedTranslator;
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
				mSelectedTranslator = sSelectorList[0];
				foreach( var ts in sSelectorList ) {
					if( ts.RemoteType == value ) {
						mSelectedTranslator = ts;
						break;
					}
				}
				LvXlatorSelector.SelectedItem = mSelectedTranslator;
				NeedReloading = false;
			}

		}

		internal bool NeedReloading { get; private set; }

		internal ReadOnlyObservableList<TranslatorSelector> SupportedSelectors => sSelectorList;

		public TranslatorSelectorPage()
		{
			InitializeComponent();

			// clear Items
			PkMicrosoftServerRegion.Items.Clear();

			// prepare charged API Grid list
			mChargedApiGrids = new List<Grid> { GdBaiduApi, GdYoudaoApi, GdGoogleApi, GdMicrosoftApi };

			_CheckAndCreateSelectors();
			LvXlatorSelector.ItemsSource = sSelectorList;
			this.SelectedTranslatorType = Properties.Settings.Default.RemoteXlatorType;
		}

		internal void ReloadSettings()
		{
			// prepare app settings
			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorCrypto ) ) {
				Properties.Settings.Default.XlatorCrypto = (DateTime.Now.Ticks + (new Random().Next( 99999, 99999999 ))).ToString();
			}
			var cryptoKey = mCrypto = Properties.Settings.Default.XlatorCrypto;

			EtXlatorBaiduAppId.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduAppId, cryptoKey );
			EtXlatorBaiduSecretKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduSecretKey, cryptoKey );
			EtXlatorYoudaoAppKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorYoudaoAppKey, cryptoKey );
			EtXlatorYoudaoSecretKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorYoudaoAppSecret, cryptoKey );
			EtXlatorGoogleApiKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorGoogleApiKey, cryptoKey );
			EtXlatorMicrosoftSubKey.Text = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorMicrosoftSubKey, cryptoKey );
			EtXlatorMicrosoftSubRegion.Text = Properties.Settings.Default.XlatorMicrosoftSubRegion;

			var msServer = Properties.Settings.Default.XlatorMicrosoftServer;
			for( int i = 0; i < PkMicrosoftServerRegion.Items.Count; ++i ) {
				var server = PkMicrosoftServerRegion.Items[i];
				if( server.Contains( msServer ) ) {
					PkMicrosoftServerRegion.SelectedIndex = i;
					break;
				}
			}
			if( PkMicrosoftServerRegion.SelectedIndex < 0 )
				PkMicrosoftServerRegion.SelectedIndex = 0;
		}

		private TranslatorSelector mSelectedTranslator = null, mPrevSelected = null;
		private static ObservableList<TranslatorSelector> sSelectorList = null;
		private List<Grid> mChargedApiGrids = null;
		private string mCrypto = null;

		private void _CheckAndCreateSelectors()
		{
			if( sSelectorList != null )
				return;

			// prepare TranslatorSelector models
			var srcExcite = ImageSource.FromResource( "MinaxWebTranslator.Resources.Excite.png" );
			//var srcCrossLang = ImageSource.FromResource( "MinaxWebTranslator.Resources.CrossLanguage.png" );
			var srcWeblio = ImageSource.FromResource( "MinaxWebTranslator.Resources.WeblioTranslator.png" );
			var srcBaidu = ImageSource.FromResource( "MinaxWebTranslator.Resources.BaiduTranslator.png" );
			var srcYoudao = ImageSource.FromResource( "MinaxWebTranslator.Resources.YoudaoTranslator.png" );
			var srcGoogle = ImageSource.FromResource( "MinaxWebTranslator.Resources.GoogleTranslator.png" );
			var srcMicrosoft = ImageSource.FromResource( "MinaxWebTranslator.Resources.Microsoft.png" );

			var list = new ObservableList<TranslatorSelector> {
				new TranslatorSelector { RemoteType = RemoteType.Excite, Header="Excite", Checked = true, Icon = srcExcite, Description = Languages.WebXlator.Str0ExciteXlatorJapan },
				//new TranslatorSelector { RemoteType = RemoteType.CrossLanguageFree, Header="CROSS-Transer", Checked = false, Icon = srcCrossLang, Description = Languages.WebXlator.Str0XTranserXlatorJapan },
				new TranslatorSelector { RemoteType = RemoteType.Weblio, Header="Weblio", Checked = false, Icon = srcWeblio, Description = Languages.WebXlator.Str0WeblioXlatorJapan },
				//new TranslatorSelector { RemoteType = RemoteType.BaiduFree, Header="Baidu", Checked = false, Icon = srcBaidu, Description = Languages.WebXlator.Str0BaiduXlatorChina },
				new TranslatorSelector { RemoteType = RemoteType.YoudaoFree, Header=Languages.WebXlator.Str0Youdao, Checked = false, Icon = srcYoudao, Description = Languages.WebXlator.Str0YoudaoXlatorChina },
				new TranslatorSelector { RemoteType = RemoteType.GoogleFree, Header="Google", Checked = false, Icon = srcGoogle, Description = Languages.WebXlator.Str0GoogleXlatorAmerica },

				new TranslatorSelector { IsSeparator = true },

				new TranslatorSelector { RemoteType = RemoteType.BaiduCharged, Header=$"{Languages.WebXlator.Str0Baidu} {Languages.WebXlator.Str0XlationApi}", Checked = false, Icon = srcBaidu, Description = Languages.WebXlator.Str0BaiduXlationChargedChina },
				new TranslatorSelector { RemoteType = RemoteType.YoudaoCharged, Header=$"{Languages.WebXlator.Str0Youdao} {Languages.WebXlator.Str0XlationApi}", Checked = false, Icon = srcYoudao, Description = Languages.WebXlator.Str0YoudaoXlationChargedChina },
				new TranslatorSelector { RemoteType = RemoteType.GoogleCharged, Header=$"Google {Languages.WebXlator.Str0XlationApi}", Checked = false, Icon = srcGoogle, Description = Languages.WebXlator.Str0GoogleXlationChargedAmerica },
				new TranslatorSelector { RemoteType = RemoteType.MicrosoftCharged, Header=$"Microsoft {Languages.WebXlator.Str0XlationApi}", Checked = false, Icon = srcMicrosoft, Description = Languages.WebXlator.Str0MicrosoftXlationChargedAmerica },

			};

			sSelectorList = list;
		}

		private void PkMicrosoftServerRegion_SelectedIndexChanged( object sender, EventArgs e )
		{
			var server = "api.cognitive.microsofttranslator.com";
			switch( PkMicrosoftServerRegion.SelectedIndex ) {
				case 1:
					server = "api-nam.cognitive.microsofttranslator.com";
					break;
				case 2:
					server = "api-eur.cognitive.microsofttranslator.com";
					break;
				case 3:
					server = "api-apc.cognitive.microsofttranslator.com";
					break;
			}
			Properties.Settings.Default.XlatorMicrosoftServer = server;
		}

		private void ContentPage_Appearing( object sender, EventArgs e )
		{
			PkMicrosoftServerRegion.SelectedIndexChanged -= PkMicrosoftServerRegion_SelectedIndexChanged;

			// prepare i10n server region list
			PkMicrosoftServerRegion.ItemsSource = new List<string> {
				$"{Languages.Global.Str0Global} (api.cognitive.microsofttranslator.com)",
				$"{Languages.Global.Str0NorthAmerica} (api-nam.cognitive.microsofttranslator.com)",
				$"{Languages.Global.Str0Europe} (api-eur.cognitive.microsofttranslator.com)",
				$"{Languages.Global.Str0AsiaPacific} (api-apc.cognitive.microsofttranslator.com)",
			};

			// prepare app settings
			ReloadSettings();

			PkMicrosoftServerRegion.SelectedIndexChanged += PkMicrosoftServerRegion_SelectedIndexChanged;
		}

		private async void ContentPage_Disappearing( object sender, EventArgs e )
		{
			PkMicrosoftServerRegion.SelectedIndexChanged -= PkMicrosoftServerRegion_SelectedIndexChanged;

			await MessageHub.SendMessageAsync( this, MessageType.MenuNavigate, MenuItemType.TranslatorSelectorClosed );
		}

		private void EtBaiduAppId_Completed( object sender, EventArgs e )
		{
			Properties.Settings.Default.XlatorBaiduAppId = Minax.Utils.EncryptToBase64( (sender as Entry).Text, mCrypto );
		}

		private void EtBaiduSecretKey_Completed( object sender, EventArgs e )
		{
			Properties.Settings.Default.XlatorBaiduSecretKey = Minax.Utils.EncryptToBase64( (sender as Entry).Text, mCrypto );
		}

		private void EtYoudaoAppKey_Completed( object sender, EventArgs e )
		{
			Properties.Settings.Default.XlatorYoudaoAppKey = Minax.Utils.EncryptToBase64( (sender as Entry).Text, mCrypto );
		}

		private void EtYoudaoSecretKey_Completed( object sender, EventArgs e )
		{
			Properties.Settings.Default.XlatorYoudaoAppSecret = Minax.Utils.EncryptToBase64( (sender as Entry).Text, mCrypto );
		}

		private void EtGoogleApiKey_Completed( object sender, EventArgs e )
		{
			Properties.Settings.Default.XlatorGoogleApiKey = Minax.Utils.EncryptToBase64( (sender as Entry).Text, mCrypto );
		}

		private void EtMicrosoftSubKey_Completed( object sender, EventArgs e )
		{
			Properties.Settings.Default.XlatorMicrosoftSubKey = Minax.Utils.EncryptToBase64( (sender as Entry).Text, mCrypto );
		}

		private void EtMicrosoftSubRegion_Completed( object sender, EventArgs e )
		{
			Properties.Settings.Default.XlatorMicrosoftSubRegion = (sender as Entry).Text;
		}

		private void LvXlatorSelector_ItemSelected( object sender, SelectedItemChangedEventArgs e )
		{
			LblWarningCharged.IsVisible = false;
			var model = e.SelectedItem as TranslatorSelector;
			if( model == null || model.IsSeparator ) {
				return;
			}

			if( mPrevSelected != model )
				NeedReloading = true;

			LblCurrent.Text = model.Header;
			mSelectedTranslator = model;
			mPrevSelected = model;
			Properties.Settings.Default.RemoteXlatorType = model.RemoteType;

			foreach( var gd in mChargedApiGrids ) {
				gd.IsVisible = false;
			}

			Grid chargedGrid = null;
			switch( model.RemoteType ) {
				case RemoteType.BaiduCharged:
					chargedGrid = GdBaiduApi;
					break;
				case RemoteType.YoudaoCharged:
					chargedGrid = GdYoudaoApi;
					break;
				case RemoteType.GoogleCharged:
					chargedGrid = GdGoogleApi;
					break;
				case RemoteType.MicrosoftCharged:
					chargedGrid = GdMicrosoftApi;
					break;
			}

			if( chargedGrid != null ) {
				chargedGrid.IsVisible = true;
				LblWarningCharged.IsVisible = true;
			}

		}

		private void LvXlatorSelector_ItemTapped( object sender, ItemTappedEventArgs e )
		{
			// don't do anything if we just de-selected the row.
			if( e.Item == null )
				return;

			// Optionally pause a bit to allow the preselect hint.
			Task.Delay( 300 );

			// Deselect the separator
			if( sender is ListView lv && lv.SelectedItem is TranslatorSelector model &&
				model.IsSeparator ) {
				lv.SelectedItem = mPrevSelected;
			}
		}
	}
}
