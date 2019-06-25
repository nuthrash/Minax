using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Controls
{
	/// <summary>
	/// Custom ViewCell for Image, Text, and Detail
	/// </summary>
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class ImageHyperlinkCell : ViewCell
	{
		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create( "ImageSource", typeof( ImageSource ), typeof( ImageHyperlinkCell ), default( ImageSource ) );
		public static readonly BindableProperty ImageNavigateUriProperty = BindableProperty.Create( "ImageNavigateUri", typeof( string ), typeof( ImageHyperlinkCell ), default( string ) );
		public static readonly BindableProperty ImageWidthProperty = BindableProperty.Create( "ImageWidth", typeof( double ), typeof( ImageHyperlinkCell ), default( double ) );
		public static readonly BindableProperty TextProperty = BindableProperty.Create( "Text", typeof( string ), typeof( ImageHyperlinkCell ), default( string ) );
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create( "TextColor", typeof( Color ), typeof( ImageHyperlinkCell ), Color.Black );
		public static readonly BindableProperty TextNavigateUriProperty = BindableProperty.Create( "TextNavigateUri", typeof( string ), typeof( ImageHyperlinkCell ), default( string ) );
		public static readonly BindableProperty DetailProperty = BindableProperty.Create( "Detail", typeof( string ), typeof( ImageHyperlinkCell ), default( string ) );
		public static readonly BindableProperty DetailColorProperty = BindableProperty.Create( "DetailColor", typeof( Color ), typeof( ImageHyperlinkCell ), Color.Gray );
		public static readonly BindableProperty DetailNavigateUriProperty = BindableProperty.Create( "DetailNavigateUri", typeof( string ), typeof( ImageHyperlinkCell ), default( string ) );

		/// <summary>
		/// Image part's ImageSource
		/// </summary>
		public ImageSource ImageSource {
			get => (ImageSource)GetValue( ImageSourceProperty );
			set => SetValue( ImageSourceProperty, value );
		}
		/// <summary>
		/// Image part's Navigate URI
		/// </summary>
		public string ImageNavigateUri {
			get => (string)GetValue( ImageNavigateUriProperty );
			set => SetValue( ImageNavigateUriProperty, value );
		}
		/// <summary>
		/// Image part's Image width
		/// </summary>
		public double ImageWidth {
			get => (double)GetValue( ImageWidthProperty );
			set => SetValue( ImageWidthProperty, value );
		}

		/// <summary>
		/// Text part's text
		/// </summary>
		public string Text {
			get => (string)GetValue( TextProperty );
			set => SetValue( TextProperty, value );
		}
		/// <summary>
		/// Text part's text color
		/// </summary>
		public Color TextColor {
			get => (Color)GetValue( TextColorProperty );
			set => SetValue( TextColorProperty, value );
		}
		/// <summary>
		/// Text part's Navigate URI
		/// </summary>
		public string TextNavigateUri {
			get => (string)GetValue( TextNavigateUriProperty );
			set => SetValue( TextNavigateUriProperty, value );
		}

		/// <summary>
		/// Detail part's text
		/// </summary>
		public string Detail {
			get => (string)GetValue( DetailProperty );
			set => SetValue( DetailProperty, value );
		}
		/// <summary>
		/// Detail part's text color
		/// </summary>
		public Color DetailColor {
			get => (Color)GetValue( DetailColorProperty );
			set => SetValue( DetailColorProperty, value );
		}
		/// <summary>
		/// Detail part's Navigate URI
		/// </summary>
		public string DetailNavigateUri {
			get => (string)GetValue( DetailNavigateUriProperty );
			set => SetValue( DetailNavigateUriProperty, value );
		}

		public Color HyperlinkColor => Color.Blue;
		public ICommand OpenWebCommand => Commands.AppCommands.OpenWebCmd;

		public ImageHyperlinkCell()
		{
			InitializeComponent();

			this.BindingContext = this;
		}

		private void ViewCell_Appearing( object sender, EventArgs e )
		{
			// Text NavigateUri
			var spanText = new Span();
			if( string.IsNullOrWhiteSpace( TextNavigateUri ) ) {
				spanText.TextColor = this.TextColor;
				spanText.TextDecorations = TextDecorations.None;
				spanText.FontAttributes = FontAttributes.Bold;
			}
			else {
				spanText.TextColor = HyperlinkColor;
				spanText.TextDecorations = TextDecorations.Underline;
				spanText.FontAttributes = FontAttributes.None;
			}
			spanText.Text = this.Text;
			spanText.GestureRecognizers.Add( new TapGestureRecognizer() {
				Command = this.OpenWebCommand,
				CommandParameter = TextNavigateUri,
			} );
			List<Span> textSpans = new List<Span>() { spanText };
			var textFStr = new FormattedString();
			foreach( var span in textSpans )
				textFStr.Spans.Add( span );
			LblText.FormattedText = textFStr;


			// Detail NavigateUri
			var spanDetail = new Span();
			if( string.IsNullOrWhiteSpace( DetailNavigateUri ) ) {
				spanDetail.TextColor = DetailColor;
				//spanDetail.TextColor = Color.Black;
				spanDetail.TextDecorations = TextDecorations.None;
				//spanDetail.FontAttributes = FontAttributes.None;
			}
			else {
				spanDetail.TextColor = HyperlinkColor;
				spanDetail.TextDecorations = TextDecorations.Underline;
				//spanDetail.FontAttributes = FontAttributes.None;
			}

			spanDetail.Text = this.Detail;
			spanDetail.GestureRecognizers.Add( new TapGestureRecognizer() {
				Command = this.OpenWebCommand,
				CommandParameter = DetailNavigateUri,
			} );
			List<Span> detailSpans = new List<Span>() { spanDetail };
			var detailFStr = new FormattedString();
			foreach( var span in detailSpans )
				detailFStr.Spans.Add( span );
			LblDetail.FormattedText = detailFStr;
		}
	}
}