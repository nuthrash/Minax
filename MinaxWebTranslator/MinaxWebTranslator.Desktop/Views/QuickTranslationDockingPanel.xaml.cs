using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using AvalonDock.Layout;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// Dockable panel for quick translation with multiple translators
	/// </summary>
	public partial class QuickTranslationDockingPanel : LayoutAnchorable
	{
		internal bool IsTranslating {
			get => isTranslating;
			private set {
				if( isTranslating == value )
					return;
				isTranslating = value;
				_ = MessageHub.SendMessageAsync( this, MessageType.XlatingQuick, isTranslating );
			}
		}

		internal string CurrentInput => mCurrentInput;

		internal int TranslatingMaxWordCount { get; private set; } = Properties.Settings.Default.QuickTranslationWordMax;

		public QuickTranslationDockingPanel() : this( Application.Current.MainWindow as MainWindow )
		{
		}
		public QuickTranslationDockingPanel( MainWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();

			// NO WAY can do this!!! nor MVVM arch. pattern!!
			//this.Content = new ViewModels.EditingViewModel();
			GdQuick.DataContext = mEditingVM = new ViewModels.EditingViewModel() {
				// ClearAndPasteCmd and CopyAllCmd cannot work normally under AvalonDock's LayoutAnchorable!!
				NonEmptyMaxPlaceholder = string.Format( Languages.WebXlator.Str2InputTextCountFractions, mCurrentInput.Length, TranslatingMaxWordCount ),
			};

			//RtbQuickXLangOutput.DataContext = mXlangVM = new ViewModels.BaseViewModel();
			RtbQuickBaiduOutput.DataContext = mBaiduVM = new ViewModels.BaseViewModel();
			RtbQuickYoudaoOutput.DataContext = mYoudaoVM = new ViewModels.BaseViewModel();
			RtbQuickGoogleOutput.DataContext = mGoogleVM = new ViewModels.BaseViewModel();

			mRtbs = new List<RichTextBox> {
				RtbQuickInput,
				//RtbQuickXLangOutput,
				RtbQuickBaiduOutput,
				RtbQuickYoudaoOutput,
				RtbQuickGoogleOutput,
			};

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;

			sProgressXlang.ProgressChanged += async ( s1, e1 ) => {
				if( e1.PercentOrErrorCode < 0 && string.IsNullOrWhiteSpace(e1.Message) == false ) {
					mXlangVM.DataErrorPlaceholder = e1.Message;
				}

				await MessageHub.SendMessageAsync( this, MessageType.XlatingProgress, e1 );
			};
			sProgressBaidu.ProgressChanged += async ( s1, e1 ) => {
				if( e1.PercentOrErrorCode < 0 && string.IsNullOrWhiteSpace( e1.Message ) == false ) {
					mBaiduVM.DataErrorPlaceholder = e1.Message;
				}

				await MessageHub.SendMessageAsync( this, MessageType.XlatingProgress, e1 );
			};
			sProgressYoudao.ProgressChanged += async ( s1, e1 ) => {
				if( e1.PercentOrErrorCode < 0 && string.IsNullOrWhiteSpace( e1.Message ) == false ) {
					mYoudaoVM.DataErrorPlaceholder = e1.Message;
				}

				await MessageHub.SendMessageAsync( this, MessageType.XlatingProgress, e1 );
			};
			sProgressGoogle.ProgressChanged += async ( s1, e1 ) => {
				if( e1.PercentOrErrorCode < 0 && string.IsNullOrWhiteSpace( e1.Message ) == false ) {
					mGoogleVM.DataErrorPlaceholder = e1.Message;
				}

				await MessageHub.SendMessageAsync( this, MessageType.XlatingProgress, e1 );
			};

			RtbQuickInput.TextChanged += ( s1, e1 ) => {
				var text = new TextRange( RtbQuickInput.Document.ContentStart, RtbQuickInput.Document.ContentEnd ).Text;
				mCurrentInput = text;
				mEditingVM.NonEmptyMaxPlaceholder = string.Format( Languages.WebXlator.Str2InputTextCountFractions, mCurrentInput.Length, TranslatingMaxWordCount );
				if( text.Length > TranslatingMaxWordCount ) {
					if( mDeferredWorker != null )
						mDeferredWorker.Dispose();
					mDeferredWorker = new Timer( (sub) => {
						this.Dispatcher.Invoke( () => {
							RtbQuickInput.Document.Blocks.Clear();
							mCurrentInput = sub as string;
							RtbQuickInput.AppendText( mCurrentInput );
							if( mDeferredWorker != null )
								mDeferredWorker.Dispose();
							mDeferredWorker = null;
							mEditingVM.NonEmptyMaxPlaceholder = string.Format( Languages.WebXlator.Str2InputTextCountFractions, mCurrentInput.Length, TranslatingMaxWordCount );
							BtnQuickTrans.IsEnabled = true;
						} );
					}, text.Substring(0, TranslatingMaxWordCount - 2), 200, Timeout.Infinite );
					// Hmm, RichTextBox seems has 2 extra characters!!!

					BtnQuickTrans.IsEnabled = false;

					e1.Handled = true;
					return;
				} else {
					BtnQuickTrans.IsEnabled = !string.IsNullOrWhiteSpace(mCurrentInput);
				}
			};
			RtbQuickInput.PreviewKeyDown += ( s1, e1 ) => {
				if( e1.Key == Key.Back || e1.Key == Key.Delete )
					return;

				var text = new TextRange(RtbQuickInput.Document.ContentStart, RtbQuickInput.Document.ContentEnd).Text;
				if( text.Length > TranslatingMaxWordCount ) {
					e1.Handled = true;
					return;
				}
			};
		}

		internal bool QuickTranslate( string sourceText )
		{
			if( string.IsNullOrWhiteSpace( sourceText ) )
				return false;

			if( sourceText.Length > TranslatingMaxWordCount )
				sourceText = sourceText.Substring( 0, TranslatingMaxWordCount );

			_FillAndTriggerXlate( sourceText );

			return true;
		}


		private readonly MainWindow mMainWindow; // = Application.Current.MainWindow as MainWindow;
		private readonly ViewModels.EditingViewModel mEditingVM;
		private readonly ViewModels.BaseViewModel mXlangVM, mBaiduVM, mYoudaoVM, mGoogleVM;
		private readonly List<RichTextBox> mRtbs;
		private volatile bool isTranslating = false;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		private Timer mDeferredWorker = null;
		private string mCurrentInput = "";

		// GUI Progress Indicator
		private static readonly Progress<Minax.ProgressInfo> sProgressXlang = new Progress<Minax.ProgressInfo>(),
					sProgressBaidu = new Progress<Minax.ProgressInfo>(),
					sProgressYoudao = new Progress<Minax.ProgressInfo>(),
					sProgressGoogle = new Progress<Minax.ProgressInfo>();


		private void _FillAndTriggerXlate( string textQuick )
		{
			RtbQuickInput.Document.Blocks.Clear();
			RtbQuickInput.AppendText( textQuick );
			AdlaQuickTranslation.IsActive = true; // focus to this DockingPanel
			AdlaQuickTranslation.IsSelected = true; // focus to this DockingPanel
			AdlaQuickTranslation.Show();
			RtbQuickInput.Focus();
			BtnQuickTrans.RaiseEvent( new RoutedEventArgs( System.Windows.Controls.Button.ClickEvent ) );
		}

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			switch( type ) {
				case MessageType.AppClosed:
				case MessageType.AppClosing:
					if( mCancelTokenSrource.IsCancellationRequested == false )
						mCancelTokenSrource.Cancel();
					break;

				case MessageType.XlatingSections:
					if( data is bool xlatingTarget ) {
						GdQuick.IsEnabled = !xlatingTarget;
					}
					break;

				case MessageType.XlatingQuickWithText:
					if( data is string textQuick && string.IsNullOrWhiteSpace( textQuick ) == false ) {
						_FillAndTriggerXlate( textQuick );
					}
					break;
			}
		}

		private async void BtnQuickTrans_Click( object sender, RoutedEventArgs e )
		{
			// when translating
			if( isTranslating ) {
				mCancelTokenSrource.Cancel();
				return;
			}

			var sourceText = new TextRange( RtbQuickInput.Document.ContentStart, RtbQuickInput.Document.ContentEnd).Text;

			// when not translating
			if( string.IsNullOrWhiteSpace( sourceText ) ) {
				await mMainWindow.ShowMessageAsync( Languages.Global.Str0OperationCancelled,
									Languages.WebXlator.Str0QuickXlationPlzInputText );
				return;
			}

			if( sourceText.Length > TranslatingMaxWordCount ) {
				await mMainWindow.ShowMessageAsync( Languages.Global.Str0OperationWarning,
									string.Format(Languages.WebXlator.Str1QuiclXlationInputCountMax, TranslatingMaxWordCount ) );
				return;
			}

			//if( CbQuickXLang.IsChecked != true && CbQuickBaidu.IsChecked != true &&
			if( CbQuickBaidu.IsChecked != true &&
				CbQuickYoudao.IsChecked != true && CbQuickGoogle.IsChecked != true ) {
				await mMainWindow.ShowMessageAsync( Languages.Global.Str0OperationWarning, Languages.WebXlator.Str0PlzSelectXlator );
				return;
			}

			this.IsTranslating = true;
			TbQuickTrans.Text = Languages.Global.Str0Cancel;
			MaprQuickTrans.Visibility = Visibility.Visible;
			BtnQuickClearAndPaste.IsEnabled = false;
			foreach( var rtb in mRtbs ) {
				rtb.IsEnabled = false;
			}

			List<Task> tasks = new List<Task>();

			//if( CbQuickXLang.IsChecked == true ) {
			//	mXlangVM.DataErrorPlaceholder = "";
			//	tasks.Add( TranslatorHelpers.XlateApiFree( RemoteType.CrossLanguageFree, sourceText, RtbQuickXLangOutput,
			//									mCancelTokenSrource.Token, sProgressXlang ) );
			//}
			if( CbQuickBaidu.IsChecked == true ) {
				mBaiduVM.DataErrorPlaceholder = "";
				tasks.Add( TranslatorHelpers.XlateApiFree( RemoteType.BaiduFree, sourceText, RtbQuickBaiduOutput,
												mCancelTokenSrource.Token, sProgressBaidu ) );
			}
			if( CbQuickYoudao.IsChecked == true ) {
				mYoudaoVM.DataErrorPlaceholder = "";
				tasks.Add( TranslatorHelpers.XlateApiFree( RemoteType.YoudaoFree, sourceText, RtbQuickYoudaoOutput,
												mCancelTokenSrource.Token, sProgressYoudao ) );
			}
			if( CbQuickGoogle.IsChecked == true ) {
				mGoogleVM.DataErrorPlaceholder = "";
				tasks.Add( TranslatorHelpers.XlateApiFree( RemoteType.GoogleFree, sourceText, RtbQuickGoogleOutput,
												mCancelTokenSrource.Token, sProgressGoogle ) );
			}

			await Task.WhenAll( tasks );
			await Task.Delay( 500 );

			RtbQuickIntOutput.Document.Blocks.Clear();
			mCancelTokenSrource = new CancellationTokenSource();

			//if( CbQuickXLang.IsChecked == true ) {
			//	RtbQuickIntOutput.AppendBlocks( RtbQuickXLangOutput );
			//}
			if( CbQuickBaidu.IsChecked == true ) {
				RtbQuickIntOutput.AppendBlocks( RtbQuickBaiduOutput );
			}
			if( CbQuickYoudao.IsChecked == true ) {
				RtbQuickIntOutput.CheckAndAppendFirstLineWhitespaces( sourceText );
				RtbQuickIntOutput.AppendBlocks( RtbQuickYoudaoOutput );
			}
			if( CbQuickGoogle.IsChecked == true ) {
				RtbQuickIntOutput.CheckAndAppendFirstLineWhitespaces( sourceText );
				RtbQuickIntOutput.AppendBlocks( RtbQuickGoogleOutput );
			}

			BtnQuickClearAndPaste.IsEnabled = true;
			MaprQuickTrans.Visibility = Visibility.Collapsed;
			TbQuickTrans.Text = Languages.Global.Str0Translate;
			foreach( var rtb in mRtbs ) {
				rtb.IsEnabled = true;
			}
			this.IsTranslating = false;
		}

		private void BtnQuickClearAndPaste_Click( object sender, RoutedEventArgs e )
		{
			RtbQuickInput.Document.Blocks.Clear();
			RtbQuickInput.Paste();
		}

		private void BtnQuickXLangCopy_Click( object sender, RoutedEventArgs e )
		{
			//RtbQuickXLangOutput.SelectAll();
			//RtbQuickXLangOutput.Copy();
		}

		private void BtnQuickBaiduCopy_Click( object sender, RoutedEventArgs e )
		{
			RtbQuickBaiduOutput.SelectAll();
			RtbQuickBaiduOutput.Copy();
		}

		private void BtnQuickYoudaoCopy_Click( object sender, RoutedEventArgs e )
		{
			RtbQuickYoudaoOutput.SelectAll();
			RtbQuickYoudaoOutput.Copy();
		}

		private void BtnQuickGoogleCopy_Click( object sender, RoutedEventArgs e )
		{
			RtbQuickGoogleOutput.SelectAll();
			RtbQuickGoogleOutput.Copy();
		}

		private void BtnQuickIntOutputCopy_Click( object sender, RoutedEventArgs e )
		{
			RtbQuickIntOutput.SelectAll();
			RtbQuickIntOutput.Copy();
		}
	}
}
