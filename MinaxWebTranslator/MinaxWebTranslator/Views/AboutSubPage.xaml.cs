using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class AboutSubPage : ContentPage
    {
        public AboutSubPage()
        {
            InitializeComponent();

			//FormattableString formattableString;
			//FormattedString fs = new FormattedString();
			//fs.LoadFromXaml( "<Span Text=\"Telefone: \" FontAttributes=\"Bold\"/><Span Text=\" &#10;12413122\" ForegroundColor=\"Red\" />" );
			//LblAboutApp.FormattedText = fs;
		}
    }
}