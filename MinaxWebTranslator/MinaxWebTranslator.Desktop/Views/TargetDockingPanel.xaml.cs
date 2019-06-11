using MahApps.Metro.Controls;
using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace MinaxWebTranslator.Desktop.Views
{
	public partial class TargetDockingPanel : LayoutAnchorable
	{

		public bool IsTranslating { get; private set; }

		public event PropertyChangedEventHandler StatusChanged;

		public TargetDockingPanel( MetroWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();
		}

		private MetroWindow mMainWindow;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();

		protected void OnStatusChanged( [CallerMemberName] string propertyName = "" )
		{
			StatusChanged?.Invoke( this, new PropertyChangedEventArgs(propertyName) );
		}

		private async void BtnTargetTranslate_Click( object sender, RoutedEventArgs e )
		{

			/*
			if( TbTargetTranslate.Text == Languages.Global.Str0Cancel ) {
				//TbTargetTranslate.Text = Languages.Global.Str0Translate;
				mCancelTokenSrource.Cancel();
				return;
			}

			try {
				RtbTarget.IsReadOnly = true;
				//MaprTargetTranslate.Visibility = Visibility.Visible;
				MaprTargetTranslate.IsActive = true;
				//PbTarget.Visibility = Visibility.Visible;
				GdTargetPercent.Visibility = Visibility.Visible;
				TbStatusMessage.Text = "";
				TbTargetPercent.Text = "0%";
				PbTarget.Value = 0;
				TbTargetTranslate.Text = Languages.Global.Str0Cancel;

				bool result = false;
				switch( mCurrentRemoteTranslator ) {
					case RemoteType.Excite:
						result = await TranslatorHelpers.XlateExcitePage( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Weblio:
						result = await TranslatorHelpers.XlateWeblioPage( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.CrossLanguage:
						result = await TranslatorHelpers.XlateCrossLangPage( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Baidu:
						result = await TranslatorHelpers.XlateBaiduPage( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Youdao:
						result = await TranslatorHelpers.XlateYoudaoPage( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Google:
						result = await TranslatorHelpers.XlateGooglePage( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						//result = await TranslatorHelpers.XlateGooglePageFile( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						break;
					case RemoteType.Bing:
						result = await TranslatorHelpers.XlateBingPage( WbMain, RtbSource, RtbTarget, mCancelTokenSrource.Token, sTransProgress );
						break;

					case RemoteType.CrossLanguageFree:
					case RemoteType.BaiduFree:
					case RemoteType.YoudaoFree:
					case RemoteType.GoogleFree:
						result = await TranslatorHelpers.XlateApiFree( mCurrentRemoteTranslator, RtbSource, RtbTarget,
															mCancelTokenSrource.Token, sTransProgress );
						break;

					case RemoteType.BaiduCharged:
						result = await TranslatorHelpers.XlateApiCharged( mCurrentRemoteTranslator, RtbSource, RtbTarget,
															mCancelTokenSrource.Token, sTransProgress,
															TbXlatorBaiduAppId.Text, TbXlatorBaiduSecretKey.Text, null, null );
						break;
					case RemoteType.YoudaoCharged:
						result = await TranslatorHelpers.XlateApiCharged( mCurrentRemoteTranslator, RtbSource, RtbTarget,
															mCancelTokenSrource.Token, sTransProgress,
															TbXlatorYoudaoAppKey.Text, TbXlatorYoudaoAppSecret.Text, null, null );
						break;

					case RemoteType.GoogleCharged:
						result = await TranslatorHelpers.XlateApiCharged( mCurrentRemoteTranslator, RtbSource, RtbTarget,
															mCancelTokenSrource.Token, sTransProgress,
															TbXlatorGoogleApiKey.Text, null, null, null );
						break;

					case RemoteType.BingCharged:
						result = await TranslatorHelpers.XlateApiCharged( mCurrentRemoteTranslator, RtbSource, RtbTarget,
															mCancelTokenSrource.Token, sTransProgress,
															TbXlatorMicrosoftSubKey.Text, null,
															(string)CbXlatorMicrosoftServer.ToolTip, TbXlatorMicrosoftSubRegion.Text );
						break;
				}

				await this.ShowMessageAsync( "Translating Result", result ? "Translation succeded!" : "Translation failed!" );
			}
			catch( OperationCanceledException oce ) {
				await mMainWindow.ShowMessageAsync( "Translating Cancelled", "Translation aborted! Exception: " + oce.Message );

			}
			catch( Exception ex ) {
				await mMainWindow.ShowMessageAsync( "Translating Result", "Translation failed! Exception: " + ex.Message );
			}
			//ICommand ccd = new Command();

			mCancelTokenSrource = new CancellationTokenSource();

			//MaprTargetTranslate.Visibility = Visibility.Collapsed;
			MaprTargetTranslate.IsActive = false;
			PbTarget.Visibility = Visibility.Hidden;
			GdTargetPercent.Visibility = Visibility.Hidden;
			TbTargetTranslate.Text = Languages.Global.Str0Translate;
			RtbTarget.IsReadOnly = false;

			// https://github.com/MahApps/MahApps.Metro/issues/1710  Auto Close flyout animation...
			//MahApps.Metro.Controls.Flyout.IsOpenProperty
			//MahApps.Metro.Controls.Dialogs.MessageDialog.IsVisibleProperty
			//MahApps.Metro.Controls.Dialogs.MessageDialog msgDlg = MessageDialog.


			*/
		}

		private void CbTargetAutoTop_Click( object sender, RoutedEventArgs e )
		{
			TranslatorHelpers.AutoScrollToTop = CbTargetAutoTop.IsChecked == true;
		}

		private void RtbTarget_ScrollChanged( object sender, ScrollChangedEventArgs e )
		{

		}
	}
}
