using Minax;
using Minax.Web.Translation;
using MinaxWebTranslator.Desktop.Models;
using MinaxWebTranslator.Models;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.AlertDialogModal;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class TargetPage : ContentPage
	{
		internal TranslatorSelector CurrentTranslator { get; set; }

		internal string SourceText {
			get => mSourceText;
			set {
				if( mSourceText == value )
					return;

				BtnTargetTranslate.IsEnabled = !string.IsNullOrWhiteSpace( value );
				mSourceText = value;
			}
		}

		internal bool IsTranslating {
			get => isTranslating;
			private set {
				if( isTranslating == value )
					return;
				isTranslating = value;
				_ = MessageHub.SendMessageAsync( this, MessageType.XlatingSections, isTranslating );
			}
		}

		public TargetPage()
		{
			InitializeComponent();

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;

			sTransProgress.ProgressChanged += async (s1, e1) => {
				var value = e1.PercentOrErrorCode;

				if( value >= 0 && value <= 100 ) {
					// update progress percent
					LblTargetPercent.Text = $"{e1.PercentOrErrorCode}%";
				}
				await MessageHub.SendMessageAsync( this, MessageType.XlatingProgress, e1 );
			};
		}

		private bool isTranslating = false;
		private HtmlAgilityPack.HtmlDocument mHtmlDoc = new HtmlAgilityPack.HtmlDocument();
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		private static readonly Progress<ProgressInfo> sTransProgress = new Progress<ProgressInfo>();

		private string mSourceText, mCrytoKey;
		private SecureString mTmpBaiduAppId, mTmpBaiduSecretKey, mTmpYoudaoAppKey, mTmpYoudaoAppSecret, mTmpGoogleApiKey, mTmpMicrosoftSubKey;

		private async Task<(IList<InputFieldModel>, bool)> _ShowInputDialog( string title, List<InputFieldModel> input )
		{
			var miView = new Views.MultipleInputsView() {
				InputFields = input,
			};
			miView.BuildInputs();

			IList<InputFieldModel> results = null;
			var sema = new SemaphoreSlim( 0, 1 );
			AlertDialogPage manualDialog = null;
			manualDialog = new AlertDialogBuilder()
				.SetCustomTitle( new Label {
					Text = title,
					FontAttributes = FontAttributes.Bold,
				} )
				.SetView( miView )
				.SetPositiveButton( Languages.Global.Str0Ok, async () => {
					results = miView.GetResults();
					await manualDialog.Dismiss();
					sema.Release();
				} )
				.Build();

			await manualDialog.Show( this );
			await sema.WaitAsync();

			return (results, miView.TempSaveSecureData);
		}


		private async Task<(string AppId, string SecretKey)> _CheckAndAskBaiduCharged()
		{
			string appId = null, secKey = null;

			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorBaiduAppId ) == false &&
				string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorBaiduSecretKey ) == false ) {
				appId = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduAppId, mCrytoKey );
				secKey = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduSecretKey, mCrytoKey );
			}
			else {
				if( mTmpBaiduAppId != null && mTmpBaiduSecretKey != null ) {
					appId = Minax.Utils.ConvertToString( mTmpBaiduAppId );
					secKey = Minax.Utils.ConvertToString( mTmpBaiduSecretKey );
				}
				else {
					var inputs = new List<InputFieldModel> {
							new InputFieldModel { FieldName = Languages.WebXlator.Str0AppId, TypeInfo = typeof(SecureString) },
							new InputFieldModel { FieldName = Languages.WebXlator.Str0SecretKey, TypeInfo = typeof(SecureString) }
						};

					var resultTuple = await _ShowInputDialog( string.Format( Languages.WebXlator.Str1FillXlationApiChargedSettings, Languages.WebXlator.Str0Baidu), inputs );
					var results = resultTuple.Item1;
					if( results != null && results.Count >= 2 ) {
						SecureString ssAppId = null, ssSecKey = null;
						foreach( var rst in results ) {
							if( rst.FieldName == Languages.WebXlator.Str0AppId && rst.Value is SecureString ss1 )
								ssAppId = ss1;
							else if( rst.FieldName == Languages.WebXlator.Str0SecretKey && rst.Value is SecureString ss2 )
								ssSecKey = ss2;
						}

						if( ssAppId != null && ssSecKey != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( resultTuple.Item2 ) {
								mTmpBaiduAppId = ssAppId;
								mTmpBaiduSecretKey = ssSecKey;
							}
							appId = Minax.Utils.ConvertToString( ssAppId );
							secKey = Minax.Utils.ConvertToString( ssSecKey );
						}
					}
				}
			}

			return (appId, secKey);
		}

		private async Task<(string AppKey, string AppSecret)> _CheckAndAskYoudaoCharged()
		{
			string appKey = null, appSecret = null;

			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorYoudaoAppKey ) == false &&
				string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorYoudaoAppSecret ) == false ) {
				appKey = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorYoudaoAppKey, mCrytoKey );
				appSecret = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorYoudaoAppSecret, mCrytoKey );
			}
			else {
				if( mTmpYoudaoAppKey != null && mTmpYoudaoAppSecret != null ) {
					appKey = Minax.Utils.ConvertToString( mTmpYoudaoAppKey );
					appSecret = Minax.Utils.ConvertToString( mTmpYoudaoAppSecret );
				}
				else {
					var inputs = new List<InputFieldModel> {
							new InputFieldModel { FieldName = Languages.WebXlator.Str0AppKey, TypeInfo = typeof(SecureString) },
							new InputFieldModel { FieldName = Languages.WebXlator.Str0AppSecret, TypeInfo = typeof(SecureString) }
						};

					var resultTuple = await _ShowInputDialog( string.Format( Languages.WebXlator.Str1FillXlationApiChargedSettings, Languages.WebXlator.Str0Youdao ), inputs );
					var results = resultTuple.Item1;
					if( results != null && results.Count >= 2 ) {
						SecureString ssAppKey = null, ssAppSecret = null;
						foreach( var rst in results ) {
							if( rst.FieldName == Languages.WebXlator.Str0AppKey && rst.Value is SecureString ss1 )
								ssAppKey = ss1;
							else if( rst.FieldName == Languages.WebXlator.Str0AppSecret && rst.Value is SecureString ss2 )
								ssAppSecret = ss2;
						}

						if( ssAppKey != null && ssAppSecret != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( resultTuple.Item2 ) {
								mTmpYoudaoAppKey = ssAppKey;
								mTmpYoudaoAppSecret = ssAppSecret;
							}
							appKey = Minax.Utils.ConvertToString( ssAppKey );
							appSecret = Minax.Utils.ConvertToString( ssAppSecret );
						}
					}
				}
			}

			return (appKey, appSecret);
		}

		private async Task<string> _CheckAndAskGoogleCharged()
		{
			string apiKey = null;

			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorGoogleApiKey ) == false ) {
				apiKey = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorGoogleApiKey, mCrytoKey );
			}
			else {
				if( mTmpGoogleApiKey != null ) {
					apiKey = Minax.Utils.ConvertToString( mTmpGoogleApiKey );
				}
				else {
					var inputs = new List<InputFieldModel> {
						new InputFieldModel { FieldName = Languages.WebXlator.Str0ApiKey, TypeInfo = typeof(SecureString) },
					};

					var resultTuple = await _ShowInputDialog( Languages.WebXlator.Str0FillGoogleApiChargedV2Settings, inputs );
					var results = resultTuple.Item1;
					if( results != null && results.Count >= 1 ) {
						SecureString ssApiKey = null;
						foreach( var rst in results ) {
							if( rst.FieldName == Languages.WebXlator.Str0ApiKey && rst.Value is SecureString ss1 )
								ssApiKey = ss1;
						}

						if( ssApiKey != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( resultTuple.Item2 ) {
								mTmpGoogleApiKey = ssApiKey;
							}
							apiKey = Minax.Utils.ConvertToString( ssApiKey );
						}
					}
				}
			}

			return apiKey;
		}

		private async Task<string> _CheckAndAskMicrosoftCharged()
		{
			string subKey = null;

			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorMicrosoftSubKey ) == false ) {
				subKey = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorMicrosoftSubKey, mCrytoKey );
			}
			else {
				if( mTmpMicrosoftSubKey != null ) {
					subKey = Minax.Utils.ConvertToString( mTmpMicrosoftSubKey );
				}
				else {
					var inputs = new List<InputFieldModel> {
						//new InputFieldModel { FieldName = Languages.WebXlator.Str0SubscriptionKey, TypeInfo = typeof(SecureString) },
						new InputFieldModel { FieldName = "Ocp-Apim-Subscription-Key", TypeInfo = typeof(SecureString) },
					};

					var resultTuple = await _ShowInputDialog( Languages.WebXlator.Str0FillMicrosoftApiChargedV3Settings, inputs );
					var results = resultTuple.Item1;
					if( results != null && results.Count >= 1 ) {
						SecureString ssApiKey = null;
						foreach( var rst in results ) {
							//if( rst.FieldName == Languages.WebXlator.Str0SubscriptionKey && rst.Value is SecureString ss1 )
							if( rst.FieldName == "Ocp-Apim-Subscription-Key" && rst.Value is SecureString ss1 )
								ssApiKey = ss1;
						}

						if( ssApiKey != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( resultTuple.Item2 ) {
								mTmpMicrosoftSubKey = ssApiKey;
							}
							subKey = Minax.Utils.ConvertToString( ssApiKey );
						}
					}
				}
			}

			return subKey;
		}


		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			if( sender == this )
				return;

			switch( type ) {
				case MessageType.AppClosed:
				case MessageType.AppClosing:
					if( mCancelTokenSrource.IsCancellationRequested == false )
						mCancelTokenSrource.Cancel();
					CurrentTranslator = null;
					break;

				case MessageType.SourceTextChanged:
					if( data is string srcText )
						SourceText = srcText;
					else
						SourceText = null;
					break;

				case MessageType.XlatorSelected:
					if( data is TranslatorSelector ts ) {
						CurrentTranslator = ts;
					}
					break;

				case MessageType.XlatingQuick:
					if( data is bool xlatingTarget ) {
						SlTranslate.IsEnabled = !xlatingTarget;
					}
					break;

			}
		}


		private async void BtnTargetTranslate_Clicked( object sender, EventArgs e )
		{
			if( string.IsNullOrWhiteSpace( SourceText ) ) {
				await DisplayAlert( Languages.Global.Str0OperationWarning,
						Languages.WebXlator.Str0PlzInputSourceBox, Languages.Global.Str0Ok );
				return;
			}

			if( IsTranslating ) {
				mCancelTokenSrource.Cancel();
				return;
			}

			if( CurrentTranslator == null || CurrentTranslator.RemoteType == RemoteType.None )
				return;

			try {
				IsTranslating = true;
				if( mCrytoKey == null ) {
					mCrytoKey = Properties.Settings.Default.XlatorCrypto;
				}

				AiBusy.IsRunning = true;

				LblTargetPercent.Text = "0%";
				LblTargetPercent.IsVisible = true;
				BtnTargetTranslate.Text = Languages.Global.Str0Cancel;

				TranslatorHelpers.MarkMappedTextWithHtmlBoldTag = true;

				var remoteType = CurrentTranslator.RemoteType;
				var cryptoKey = mCrytoKey;

				bool result = false, isCharged = false;
				switch( remoteType ) {
					case RemoteType.Excite:
						result = await TranslatorHelpers.XlateExcitePage( SourceText, WvTarget, mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Weblio:
						result = await TranslatorHelpers.XlateWeblioPage( SourceText, WvTarget, mCancelTokenSrource.Token, sTransProgress );
						break;

					case RemoteType.CrossLanguageFree:
					case RemoteType.MiraiTranslateFree:
					case RemoteType.BaiduFree:
					case RemoteType.IcibaFree:
					case RemoteType.LingoCloudFree:
					case RemoteType.YoudaoFree:
					case RemoteType.PapagoFree:
					case RemoteType.GoogleFree:
						result = await TranslatorHelpers.XlateApiFree( remoteType, SourceText, WvTarget,
															mCancelTokenSrource.Token, sTransProgress );
						break;

					case RemoteType.BaiduCharged:
						var tupleBaidu = await _CheckAndAskBaiduCharged();
						if( tupleBaidu.AppId == null || tupleBaidu.SecretKey == null )
							result = false;
						else
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, WvTarget,
										mCancelTokenSrource.Token, sTransProgress,
										tupleBaidu.AppId, tupleBaidu.SecretKey, null, null );

						// maybe input wrong character...so clear data for next input
						if( result != true ) {
							mTmpBaiduAppId = null;
							mTmpBaiduSecretKey = null;
						}

						isCharged = true;
						break;
					case RemoteType.YoudaoCharged:
						var tupleYoudao = await _CheckAndAskYoudaoCharged();
						if( tupleYoudao.AppKey == null || tupleYoudao.AppSecret == null )
							result = false;
						else
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, WvTarget,
										mCancelTokenSrource.Token, sTransProgress,
										tupleYoudao.AppKey, tupleYoudao.AppSecret, null, null );

						// maybe input wrong character...so clear data for next input
						if( result != true ) {
							mTmpYoudaoAppKey = null;
							mTmpYoudaoAppSecret = null;
						}
						isCharged = true;
						break;

					case RemoteType.GoogleCharged:
						var apiKeyGoogle = await _CheckAndAskGoogleCharged();
						if( apiKeyGoogle == null )
							result = false;
						else
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, WvTarget,
										mCancelTokenSrource.Token, sTransProgress,
										apiKeyGoogle, null, null, null );
						if( result != true ) {
							mTmpGoogleApiKey = null;
						}
						isCharged = true;
						break;

					case RemoteType.MicrosoftCharged:
						var subKeyMs = await _CheckAndAskMicrosoftCharged();
						if( subKeyMs == null )
							result = false;
						else
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, WvTarget,
										mCancelTokenSrource.Token, sTransProgress,
										subKeyMs, null,
										Properties.Settings.Default.XlatorMicrosoftServer,
										Properties.Settings.Default.XlatorMicrosoftSubRegion );
						if( result != true ) {
							mTmpMicrosoftSubKey = null;
						}
						isCharged = true;
						break;
				}

				if( result )
					CrossToastPopUp.Current.ShowToastMessage( Languages.WebXlator.Str0XlatingSucceed );
				else if( isCharged )
					await DisplayAlert( Languages.WebXlator.Str0XlatingFailed,
								Languages.WebXlator.Str0XlatingFailedFieldMissing,
								Languages.Global.Str0Ok );
				else
					await DisplayAlert( Languages.WebXlator.Str0XlatingFailed,
								Languages.WebXlator.Str0XlatingFailedNetRetry,
								Languages.Global.Str0Ok );
			}
			catch( OperationCanceledException oce ) {
				await DisplayAlert( Languages.WebXlator.Str0XlatingCancelled,
						string.Format( Languages.WebXlator.Str1XlatingAbortedException, oce.Message), Languages.Global.Str0Ok );
			}
			catch( Exception ex ) {
				await DisplayAlert( Languages.WebXlator.Str0XlatingFailed,
						string.Format( Languages.WebXlator.Str1XlatingFailedException, ex.Message ), Languages.Global.Str0Ok );
			}

			mCancelTokenSrource = new CancellationTokenSource();

			AiBusy.IsRunning = false;
			LblTargetPercent.IsVisible = false;
			BtnTargetTranslate.Text = Languages.Global.Str0Translate;
			IsTranslating = false;
		}

		private void SwAutoTop_Toggled( object sender, ToggledEventArgs e )
		{
			TranslatorHelpers.AutoScrollToTop = SwAutoTop.IsToggled;
		}

		private async void BtnCopyAll_Clicked( object sender, EventArgs e )
		{
			var html = WvTarget.Source as HtmlWebViewSource;

			if( html == null || string.IsNullOrWhiteSpace( html.Html ) )
				return;

			mHtmlDoc.LoadHtml( html.Html );
			await Clipboard.SetTextAsync( mHtmlDoc.DocumentNode.InnerText );
		}
	}
}
