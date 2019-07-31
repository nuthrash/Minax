using MahApps.Metro.Controls.Dialogs;
using Minax.Web.Translation;
using MinaxWebTranslator.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// Dockable panel for Target text of Translation
	/// </summary>
	public partial class TargetDockingPanel : LayoutAnchorable
	{
		internal string SourceText {
			get => mSourceText;
			set {
				if( mSourceText == value )
					return;

				BtnTargetTranslate.IsEnabled = !string.IsNullOrWhiteSpace( value );
				mSourceText = value;
			}
		}

		internal TranslatorSelector CurrentTranslator { get; private set; }

		public bool IsTranslating {
			get => isXlating;
			private set {
				if( isXlating == value )
					return;
				isXlating = value;
				_ = MessageHub.SendMessageAsync( this, MessageType.XlatingSections, isXlating );
			}
		}

		public bool SyncTargetScroll => CbTargetSyncScroll.IsChecked == true;

		public TargetDockingPanel() : this( Application.Current.MainWindow as MainWindow )
		{

		}
		public TargetDockingPanel( MainWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;

			sTransProgress.ProgressChanged += async ( s1, e1 ) => {
				var value = e1.PercentOrErrorCode;

				if( value >= 0 && value <= 100 ) {
					TbTargetPercent.Text = $"{value}%";
				}
				await MessageHub.SendMessageAsync( this, MessageType.XlatingProgress, e1 );
			};
		}

		private MainWindow mMainWindow;
		private string mSourceText;
		private string mCrytoKey;
		private volatile bool isXlating = false;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		// GUI Progress Indicator
		private static readonly Progress<Minax.ProgressInfo> sTransProgress = new Progress<Minax.ProgressInfo>();

		private SecureString mTmpBaiduAppId, mTmpBaiduSecretKey, mTmpYoudaoAppKey, mTmpYoudaoAppSecret, mTmpGoogleApiKey, mTmpMicrosoftSubKey;

		private async Task<(string AppId, string SecretKey)> _CheckAndAskBaiduCharged()
		{
			string appId = null, secKey = null;

			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorBaiduAppId ) == false &&
				string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorBaiduSecretKey ) == false ) {
				appId = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduAppId, mCrytoKey );
				secKey = Minax.Utils.DecryptFromBas64( Properties.Settings.Default.XlatorBaiduSecretKey, mCrytoKey );
			} else {
				if( mTmpBaiduAppId != null && mTmpBaiduSecretKey != null ) {
					appId = mTmpBaiduAppId.ConvertToString();
					secKey = mTmpBaiduSecretKey.ConvertToString();
				} else {
					var miDialog = new Views.MultipleInputsDialog() {
						MainWindow = mMainWindow,
						Title = string.Format( Languages.WebXlator.Str1FillXlationApiChargedSettings, Languages.WebXlator.Str0Baidu ),
						InputFields = new List<InputFieldModel> {
							new InputFieldModel { FieldName = "APP ID", TypeInfo = typeof(SecureString) },
							new InputFieldModel { FieldName = Languages.WebXlator.Str0SecretKey, TypeInfo = typeof(SecureString) }
						},
					};
					await mMainWindow.ShowMetroDialogAsync( miDialog );
					await miDialog.WaitUntilUnloadedAsync();

					var results = miDialog.Results;
					if( results != null && results.Count >= 2 ) {
						SecureString ssAppId = null, ssSecKey = null;
						foreach( var rst in results ) {
							if( rst.FieldName == "APP ID" && rst.Value is SecureString ss1 )
								ssAppId = ss1;
							else if( rst.FieldName == Languages.WebXlator.Str0SecretKey && rst.Value is SecureString ss2 )
								ssSecKey = ss2;
						}

						if( ssAppId != null && ssSecKey != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( miDialog.TempSaveSecureData ) {
								mTmpBaiduAppId = ssAppId;
								mTmpBaiduSecretKey = ssSecKey;
							}
							appId = ssAppId.ConvertToString();
							secKey = ssSecKey.ConvertToString();
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
					appKey = mTmpYoudaoAppKey.ConvertToString();
					appSecret = mTmpYoudaoAppSecret.ConvertToString();
				}
				else {
					var miDialog = new Views.MultipleInputsDialog() {
						MainWindow = mMainWindow,
						Title = string.Format( Languages.WebXlator.Str1FillXlationApiChargedSettings, Languages.WebXlator.Str0Youdao ),
						InputFields = new List<InputFieldModel> {
							new InputFieldModel { FieldName = Languages.WebXlator.Str0AppKey, TypeInfo = typeof(SecureString) },
							new InputFieldModel { FieldName = Languages.WebXlator.Str0AppSecret, TypeInfo = typeof(SecureString) }
						},
					};
					await mMainWindow.ShowMetroDialogAsync( miDialog );
					await miDialog.WaitUntilUnloadedAsync();

					if( miDialog.Results != null && miDialog.Results.Count >= 2 ) {
						SecureString ssAppKey = null, ssAppSec = null;
						foreach( var rst in miDialog.Results ) {
							if( rst.FieldName == Languages.WebXlator.Str0AppKey && rst.Value is SecureString ss1 )
								ssAppKey = ss1;
							else if( rst.FieldName == Languages.WebXlator.Str0AppSecret && rst.Value is SecureString ss2 )
								ssAppSec = ss2;
						}

						if( ssAppKey != null && ssAppSec != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( miDialog.TempSaveSecureData ) {
								mTmpYoudaoAppKey = ssAppKey;
								mTmpYoudaoAppSecret = ssAppSec;
							}
							appKey = ssAppKey.ConvertToString();
							appSecret = ssAppSec.ConvertToString();
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
					apiKey = mTmpGoogleApiKey.ConvertToString();
				}
				else {
					var miDialog = new Views.MultipleInputsDialog() {
						MainWindow = mMainWindow,
						Title = Languages.WebXlator.Str0FillGoogleApiChargedV2Settings,
						InputFields = new List<InputFieldModel> {
							new InputFieldModel { FieldName = Languages.WebXlator.Str0ApiKey, TypeInfo = typeof(SecureString) },
						},
					};
					await mMainWindow.ShowMetroDialogAsync( miDialog );
					await miDialog.WaitUntilUnloadedAsync();

					if( miDialog.Results != null && miDialog.Results.Count >= 1 ) {
						SecureString ssApiKey = null;
						foreach( var rst in miDialog.Results ) {
							if( rst.FieldName == Languages.WebXlator.Str0ApiKey && rst.Value is SecureString ss1 )
								ssApiKey = ss1;
						}

						if( ssApiKey != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( miDialog.TempSaveSecureData ) {
								mTmpGoogleApiKey = ssApiKey;
							}
							apiKey = ssApiKey.ConvertToString();
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
					subKey = mTmpMicrosoftSubKey.ConvertToString();
				}
				else {
					var miDialog = new Views.MultipleInputsDialog() {
						MainWindow = mMainWindow,
						Title = Languages.WebXlator.Str0FillMicrosoftApiChargedV3Settings,
						InputFields = new List<InputFieldModel> {
							new InputFieldModel { FieldName = "Ocp-Apim-Subscription-Key", TypeInfo = typeof(SecureString) },
						},
					};
					await mMainWindow.ShowMetroDialogAsync( miDialog );
					await miDialog.WaitUntilUnloadedAsync();

					if( miDialog.Results != null && miDialog.Results.Count >= 1 ) {
						SecureString ssSubKey = null;
						foreach( var rst in miDialog.Results ) {
							if( rst.FieldName == "Ocp-Apim-Subscription-Key" && rst.Value is SecureString ss1 )
								ssSubKey = ss1;
						}

						if( ssSubKey != null ) {
							// TempSaveSecureData is not true, then app would ask setting data each time
							if( miDialog.TempSaveSecureData ) {
								mTmpMicrosoftSubKey = ssSubKey;
							}
							subKey = ssSubKey.ConvertToString();
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
					break;


				case MessageType.XlatorSelected:
					if( data is TranslatorSelector translatorSelector ) {
						CurrentTranslator = translatorSelector;
						ImgTargetTranslate.Source = CurrentTranslator.Icon;
					}
					break;

				case MessageType.XlatingQuick:
					if( data is bool onOffQuick ) {
						GdTarget.IsEnabled = !onOffQuick;
					}
					break;
				//case MessageType.XlatingPercentOrErrorCode:
				//	// update Percent string
				//	if( data is int percentOrErrorCode ) {
				//		if( percentOrErrorCode >= 0 && percentOrErrorCode <= 100 ) {
				//			TbTargetPercent.Text = $"{percentOrErrorCode}%";
				//		}
				//	}
				//	break;

				case MessageType.SourceTextChanged:
					if( data is string sourceText ) {
						SourceText = sourceText;
					} else {
						SourceText = null;
					}
					break;
			}
		}

		private async void BtnTargetTranslate_Click( object sender, RoutedEventArgs e )
		{
			if( IsTranslating ) {
				mCancelTokenSrource.Cancel();
				return;
			}

			if( string.IsNullOrWhiteSpace( SourceText ) ) {
				await mMainWindow.ShowMessageAsync( Languages.Global.Str0OperationCancelled, Languages.WebXlator.Str0PlzInputSourceBox );
				return;
			}

			if( CurrentTranslator == null || CurrentTranslator.RemoteType == RemoteType.None )
				return;

			IsTranslating = true;

			if( mCrytoKey == null ) {
				mCrytoKey = Properties.Settings.Default.XlatorCrypto;
			}

			try {

				RtbTarget.IsReadOnly = true;
				MaprTargetTranslate.IsActive = true;
				GdTargetPercent.Visibility = Visibility.Visible;
				TbTargetPercent.Text = "0%";
				PbTarget.Value = 0;
				TbTargetTranslate.Text = Languages.Global.Str0Cancel;

				bool result = false, isCharged = false;

				var remoteType = CurrentTranslator.RemoteType;
				switch( remoteType ) {
					case RemoteType.Excite:
						result = await TranslatorHelpers.XlateExcitePage( WbMain, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Weblio:
						result = await TranslatorHelpers.XlateWeblioPage( WbMain, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.CrossLanguage:
						result = await TranslatorHelpers.XlateCrossLangPage( WbMain, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Baidu:
						result = await TranslatorHelpers.XlateBaiduPage( WbMain, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Youdao:
						result = await TranslatorHelpers.XlateYoudaoPage( WbMain, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Google:
						result = await TranslatorHelpers.XlateGooglePage( WbMain, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Microsoft:
						result = await TranslatorHelpers.XlateMicrosoftPage( WbMain, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;

					case RemoteType.CrossLanguageFree:
					case RemoteType.BaiduFree:
					case RemoteType.YoudaoFree:
					case RemoteType.GoogleFree:
						result = await TranslatorHelpers.XlateApiFree( remoteType, SourceText, RtbTarget,
										mCancelTokenSrource.Token, sTransProgress );
						break;

					// ask APP ID/Secret ... when setting is empty
					case RemoteType.BaiduCharged:
						var tupleBaidu = await _CheckAndAskBaiduCharged();
						if( tupleBaidu.AppId == null || tupleBaidu.SecretKey == null )
							result = false;
						else
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, RtbTarget,
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
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, RtbTarget,
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
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, RtbTarget,
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
							result = await TranslatorHelpers.XlateApiCharged( remoteType, SourceText, RtbTarget,
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
					mMainWindow.ShowAutoCloseMessage( Languages.WebXlator.Str0XlatingResult, Languages.WebXlator.Str0XlatingSucceed );
				else if( isCharged )
					await mMainWindow.ShowMessageAsync( Languages.WebXlator.Str0XlatingFailed, Languages.WebXlator.Str0XlatingFailedFieldMissing );
				else
					await mMainWindow.ShowMessageAsync( Languages.WebXlator.Str0XlatingFailed, Languages.WebXlator.Str0XlatingFailedNetRetry );
			}
			catch( OperationCanceledException oce ) {
				await mMainWindow.ShowMessageAsync( Languages.WebXlator.Str0XlatingCancelled, string.Format( Languages.WebXlator.Str1XlatingAbortedException, oce.Message ) );

			}
			catch( Exception ex ) {
				await mMainWindow.ShowMessageAsync( Languages.WebXlator.Str0XlatingResult, string.Format( Languages.WebXlator.Str1XlatingFailedException, ex.Message ) );
			}

			mCancelTokenSrource = new CancellationTokenSource();

			MaprTargetTranslate.IsActive = false;
			PbTarget.Visibility = Visibility.Hidden;
			GdTargetPercent.Visibility = Visibility.Hidden;
			TbTargetTranslate.Text = Languages.Global.Str0Translate;
			RtbTarget.IsReadOnly = false;
			IsTranslating = false;
		}

		private void CbTargetAutoTop_Click( object sender, RoutedEventArgs e )
		{
			TranslatorHelpers.AutoScrollToTop = CbTargetAutoTop.IsChecked == true;
		}

	}
}
