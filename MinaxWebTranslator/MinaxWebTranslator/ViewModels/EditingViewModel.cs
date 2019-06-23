using MinaxWebTranslator.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MinaxWebTranslator.ViewModels
{
	/// <summary>
	/// Editing ViewModel with extra editing commands
	/// </summary>
	internal class EditingViewModel : BaseViewModel
	{
		public ICommand ClearCmd => AppCommands.ClearCmd;
		public ICommand PasteCmd => AppCommands.PasteCmd;
		public ICommand CopyAllCmd => AppCommands.CopyAllCmd;
		public ICommand ClearAndPasteCmd => AppCommands.ClearAndPasteCmd;

		public EditingViewModel()
		{

		}
	}
}
