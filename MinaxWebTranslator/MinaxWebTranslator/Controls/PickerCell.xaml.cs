using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Controls
{
	/// <summary>
	/// Picker style ViewCell
	/// </summary>
	[XamlCompilation( XamlCompilationOptions.Compile )]
	[ContentProperty( "Items" )]
	public partial class PickerCell : ViewCell
	{
		public static readonly BindableProperty LabelProperty = BindableProperty.Create( "Label", typeof( string ), typeof( PickerCell ), default( string ) );

		public static readonly BindableProperty TitleProperty = BindableProperty.Create( "Title", typeof( string ), typeof( PickerCell ), default( string ) );

		public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create( "SelectedValue", typeof( string ), typeof( PickerCell ), null,
								BindingMode.TwoWay, propertyChanged: ( sender, oldValue, newValue ) => {
									PickerCell pickerCell = (PickerCell)sender;
									if( String.IsNullOrEmpty( (string)newValue ) ) {
										pickerCell.picker.SelectedIndex = -1;
									}
									else {
										pickerCell.picker.SelectedIndex =
													  pickerCell.Items.IndexOf( (string)newValue );
									}

								} );

		public PickerCell()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Label column
		/// </summary>
		public string Label {
			get { return (string)GetValue( LabelProperty ); }
			set { SetValue( LabelProperty, value ); }
		}

		/// <summary>
		/// Picker's title
		/// </summary>
		public string Title {
			get { return (string)GetValue( TitleProperty ); }
			set { SetValue( TitleProperty, value ); }
		}

		/// <summary>
		/// Selected value
		/// </summary>
		public string SelectedValue {
			get { return (string)GetValue( SelectedValueProperty ); }
			set { SetValue( SelectedValueProperty, value ); }
		}

		/// <summary>
		/// Picker's Items
		/// </summary>
		public IList<string> Items {
			get { return picker.Items; }
		}

		void OnPickerSelectedIndexChanged( object sender, EventArgs args )
		{
			if( picker.SelectedIndex == -1 ) {
				SelectedValue = null;
			}
			else {
				SelectedValue = Items[picker.SelectedIndex];
			}
		}
	}
}