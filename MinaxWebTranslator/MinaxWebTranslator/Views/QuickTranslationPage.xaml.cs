using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class QuickTranslationPage : ContentPage
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

		public QuickTranslationPage()
		{
			InitializeComponent();

			EtQuickInput.MaxLength = TranslatingMaxWordCount;

			this.BindingContext = mEditingVM = new ViewModels.EditingViewModel() {
				NonEmptyMaxPlaceholder = string.Format( Languages.WebXlator.Str2InputTextCountFractions, mCurrentInput.Length, TranslatingMaxWordCount ),
			};

			EtQuickXLangOutput.BindingContext = mXlangVM = new ViewModels.BaseViewModel();
			EtQuickYoudaoOutput.BindingContext = mYoudaoVM = new ViewModels.BaseViewModel();
			EtQuickGoogleOutput.BindingContext = mGoogleVM = new ViewModels.BaseViewModel();

			mEntries = new List<Entry> {
				EtQuickInput,
				EtQuickXLangOutput,
				EtQuickYoudaoOutput,
				EtQuickGoogleOutput,
			};

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;

			sProgressXlang.ProgressChanged += async ( s1, e1 ) => {
				if( e1.PercentOrErrorCode < 0 && string.IsNullOrWhiteSpace( e1.Message ) == false ) {
					mXlangVM.DataErrorPlaceholder = e1.Message;
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


			EtQuickInput.TextChanged += ( s1, e1 ) => {
				var text = EtQuickInput.Text;
				if( string.IsNullOrEmpty( text ) )
					text = "";

				mCurrentInput = text; // DO NOT let mCurrentInput become empty
				mEditingVM.NonEmptyMaxPlaceholder = string.Format( Languages.WebXlator.Str2InputTextCountFractions, mCurrentInput.Length, TranslatingMaxWordCount );
				if( text.Length > TranslatingMaxWordCount ) {
					if( mDeferredWorker != null )
						mDeferredWorker.Dispose();
					mDeferredWorker = new Timer( ( sub ) => {
						MainThread.BeginInvokeOnMainThread( () => {
							mCurrentInput = sub as string;
							EtQuickInput.Text = mCurrentInput;
							if( mDeferredWorker != null )
								mDeferredWorker.Dispose();
							mDeferredWorker = null;
							mEditingVM.NonEmptyMaxPlaceholder = string.Format( Languages.WebXlator.Str2InputTextCountFractions, mCurrentInput.Length, TranslatingMaxWordCount );
							BtnQuickTrans.IsEnabled = true;
						} );
					}, text.Substring( 0, TranslatingMaxWordCount ), 200, Timeout.Infinite );

					BtnQuickTrans.IsEnabled = false;
				}
				else {
					BtnQuickTrans.IsEnabled = !string.IsNullOrWhiteSpace( mCurrentInput );
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

		private bool isTranslating;
		private string mCurrentInput = "";
		private readonly ViewModels.EditingViewModel mEditingVM;
		private readonly ViewModels.BaseViewModel mXlangVM, mYoudaoVM, mGoogleVM;
		private readonly List<Entry> mEntries;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		private Timer mDeferredWorker = null;

		private static readonly Progress<Minax.ProgressInfo> sProgressXlang = new Progress<Minax.ProgressInfo>(),
					sProgressYoudao = new Progress<Minax.ProgressInfo>(),
					sProgressGoogle = new Progress<Minax.ProgressInfo>();

		private void _FillAndTriggerXlate( string textQuick )
		{
			EtQuickInput.Text = textQuick;
			this.Focus();
			(BtnQuickTrans as IButtonController)?.SendClicked();
		}

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			switch( type ) {
				case MessageType.AppClosed:
				case MessageType.AppClosing:
					if( IsTranslating && mCancelTokenSrource.IsCancellationRequested == false )
						mCancelTokenSrource.Cancel();
					//CurrentProject = null;
					break;

				case MessageType.ProjectOpened:
					break;

				case MessageType.ProjectClosed:
					if( IsTranslating && mCancelTokenSrource.IsCancellationRequested == false )
						mCancelTokenSrource.Cancel();
					mCancelTokenSrource = new CancellationTokenSource();
					break;

				case MessageType.XlatingSections:
					if( data is bool xlatingTarget ) {
						SlQuick.IsEnabled = !xlatingTarget;
					}
					break;

				case MessageType.XlatingQuickWithText:
					if( data is string textQuick && string.IsNullOrWhiteSpace( textQuick ) == false ) {
						_FillAndTriggerXlate( textQuick );
					}
					break;
			}
		}

		private async void BtnQuickTrans_Clicked( object sender, EventArgs e )
		{
			// when translating
			if( IsTranslating ) {
				mCancelTokenSrource.Cancel();
				return;
			}

			var sourceText = EtQuickInput.Text;
			if( string.IsNullOrWhiteSpace( sourceText ) ) {
				await DisplayAlert( Languages.Global.Str0OperationCancelled,
						Languages.WebXlator.Str0QuickXlationPlzInputText, Languages.Global.Str0Ok );
				return;
			}

			// when not translating
			if( sourceText.Length > TranslatingMaxWordCount ) {
				await DisplayAlert( Languages.Global.Str0OperationWarning,
						string.Format( Languages.WebXlator.Str1QuiclXlationInputCountMax, TranslatingMaxWordCount ), Languages.Global.Str0Ok );
				return;
			}

			if( CbQuickXLang.IsChecked != true &&
				CbQuickYoudao.IsChecked != true &&
				CbQuickGoogle.IsChecked != true ) {
				await DisplayAlert( Languages.Global.Str0OperationWarning,
						Languages.WebXlator.Str0PlzSelectXlator, Languages.Global.Str0Ok );
				return;
			}

			List<Task> tasks = new List<Task>();

			this.IsTranslating = true;
			BtnQuickTrans.Text = Languages.Global.Str0Cancel;
			foreach( var rtb in mEntries ) {
				rtb.IsEnabled = false;
			}

			TranslatorHelpers.MarkMappedTextWithHtmlBoldTag = false;

			double preferSize = EtQuickInput.Height;
			if( CbQuickXLang.IsChecked == true ) {
				tasks.Add( TranslatorHelpers.XlateApiFree( RemoteType.CrossLanguageFree, sourceText,
							EtQuickXLangOutput, mCancelTokenSrource.Token, sProgressXlang ) );
				AiBusyCrossTranser.HeightRequest = AiBusyCrossTranser.WidthRequest = preferSize;
				AiBusyCrossTranser.IsRunning = true;
			}
			if( CbQuickYoudao.IsChecked == true ) {
				tasks.Add( TranslatorHelpers.XlateApiFree( RemoteType.YoudaoFree, sourceText,
							EtQuickYoudaoOutput, mCancelTokenSrource.Token, sProgressYoudao ) );
				AiBusyYoudao.HeightRequest = AiBusyYoudao.WidthRequest = preferSize;
				AiBusyYoudao.IsRunning = true;
			}
			if( CbQuickGoogle.IsChecked == true ) {
				tasks.Add( TranslatorHelpers.XlateApiFree( RemoteType.GoogleFree, sourceText,
							EtQuickGoogleOutput, mCancelTokenSrource.Token, sProgressGoogle ) );
				AiBusyGoogle.HeightRequest = AiBusyGoogle.WidthRequest = preferSize;
				AiBusyGoogle.IsRunning = true;
			}

			await Task.WhenAll( tasks );
			await Task.Delay( 500 );

			EdIntOutput.Text = null;
			mCancelTokenSrource = new CancellationTokenSource();

			if( CbQuickXLang.IsChecked == true ) {
				EdIntOutput.Text += EtQuickXLangOutput.Text;
			}
			if( CbQuickYoudao.IsChecked == true ) {
				EdIntOutput.Text += EtQuickYoudaoOutput.Text;
			}
			if( CbQuickGoogle.IsChecked == true ) {
				EdIntOutput.Text += EtQuickGoogleOutput.Text;
			}

			AiBusyCrossTranser.IsRunning = false;
			AiBusyYoudao.IsRunning = false;
			AiBusyGoogle.IsRunning = false;

			BtnQuickTrans.Text = Languages.Global.Str0Translate;
			foreach( var et in mEntries ) {
				et.IsEnabled = true;
			}
			IsTranslating = false;
		}
	}
}
