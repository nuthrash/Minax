using MinaxWebTranslator.Commands;
using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace MinaxWebTranslator.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
		public ICommand OpenWebCmd => AppCommands.OpenWebCmd;

        public AboutViewModel()
        {
            Title = Languages.Global.Str0About;
        }
    }
}
