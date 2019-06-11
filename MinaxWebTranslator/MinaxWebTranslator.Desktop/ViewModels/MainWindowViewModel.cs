using MahApps.Metro.Controls;
using MinaxWebTranslator.Desktop.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MinaxWebTranslator.Desktop.ViewModels
{
	internal class MainWindowViewModel : BaseViewModel
	{
		public ICommand CloseFlyoutCmd { get; }
		public ICommand CloseCmd { get; }

		public bool CanCloseFlyout {
			get => this.canCloseFlyout;
			set => this.SetProperty( ref this.canCloseFlyout, value );
		}
		private bool canCloseFlyout = true;

		public MainWindowViewModel()
		{

			this.CloseFlyoutCmd = new SimpleCommand( o => this.CanCloseFlyout, x => ((Flyout)x).IsOpen = false );

			this.CloseCmd = new SimpleCommand( o => true, (param) => {
				if( param is Flyout fo )
					fo.IsOpen = false;
				else if( param is MahApps.Metro.Controls.Dialogs.BaseMetroDialog bmd )
					bmd.RequestCloseAsync();
			} );
		}
	}
}
