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
	public partial class WaitingView : ContentView
	{
		public string Message {
			get => LblMessage.Text;
			set => LblMessage.Text = value;
		}

		public WaitingView()
		{
			InitializeComponent();
		}
	}
}
