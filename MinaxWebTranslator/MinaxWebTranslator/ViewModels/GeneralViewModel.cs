using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MinaxWebTranslator.ViewModels
{
	/// <summary>
	/// Gerneral GUI related ViewModel
	/// </summary>
	public class GeneralViewModel : BaseViewModel
	{

		public double TextWidthRequest {
			get => textWidthReq;
			set => SetProperty( ref textWidthReq, value );
		}
		private double textWidthReq;

		public GridLength TextColumnGridLength {
			get => textColumnGridLength;
			set => SetProperty( ref textColumnGridLength, value );
		}
		private GridLength textColumnGridLength;


	}
}
