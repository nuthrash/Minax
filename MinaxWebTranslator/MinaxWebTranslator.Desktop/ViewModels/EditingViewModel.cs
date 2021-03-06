﻿using MinaxWebTranslator.Desktop.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MinaxWebTranslator.Desktop.ViewModels
{
	/// <summary>
	/// Editing ViewModel with extra editing commands
	/// </summary>
	internal class EditingViewModel : BaseViewModel
	{
		public ICommand CopyAllCmd => AppCommands.CopyAllCmd;
		public ICommand ClearAndPasteCmd => AppCommands.ClearAndPasteCmd;

		public EditingViewModel()
		{

		}

	}
}
