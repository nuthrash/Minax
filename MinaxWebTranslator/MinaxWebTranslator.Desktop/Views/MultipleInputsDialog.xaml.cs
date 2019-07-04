using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MinaxWebTranslator.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// Multiple inputs custom dialog for MahApps CustomDialog
	/// </summary>
	public partial class MultipleInputsDialog : CustomDialog
	{
		internal MetroWindow MainWindow { get; set; }

		/// <summary>
		/// InputFields count max
		/// </summary>
		internal int InputCountMax { get; set; } = 20;

		/// <summary>
		/// Temporary save SecureString for this session
		/// </summary>
		internal bool TempSaveSecureData => MatsTempSave.IsChecked == true;

		/// <summary>
		/// Input fields models
		/// </summary>
		internal IList<InputFieldModel> InputFields { get; set; }

		/// <summary>
		/// Results of setting
		/// </summary>
		internal IList<InputFieldModel> Results { get; private set; } = new List<InputFieldModel>();


		public MultipleInputsDialog()
		{
			InitializeComponent();
		}

		private void CustomDialog_Loaded( object sender, RoutedEventArgs e )
		{
			if( InputFields == null || InputFields.Count <= 0 ) {
				return;
			}

			MatsTempSave.Visibility = Visibility.Collapsed;

			// manually build each field
			GdInputs.RowDefinitions.Clear();
			for( int i = 0; i < InputFields.Count; ++i ) {
				var field = InputFields[i];
				if( field.TypeInfo == null ) {
					if( field.Value == null )
						continue;
					field.TypeInfo = field.Value.GetType();
				}

				RowDefinition rd = new RowDefinition() { Height = new GridLength( 1.0, GridUnitType.Auto) };
				GdInputs.RowDefinitions.Add( rd );

				// Column 0: Field Name
				Label lbl = new Label() {
					Content = field.FieldName,
					HorizontalContentAlignment = HorizontalAlignment.Right,
					VerticalContentAlignment = VerticalAlignment.Center,
				};
				GdInputs.Children.Add( lbl );
				Grid.SetColumn( lbl, 0 );
				Grid.SetRow( lbl, i );


				// Column 1: Coresponding Type Control
				UIElement col2Element = null;
				if( field.TypeInfo.IsEnum ) {
					ComboBox cb = new ComboBox();
					var values = Enum.GetValues( field.TypeInfo );
					var itemsSourceBinding = new Binding { Source = values, Converter = field.Converter };
					cb.SetBinding( ComboBox.ItemsSourceProperty, itemsSourceBinding );

					if( field.Value != null ) {
						try {
							object tmpEnum = Enum.Parse( field.TypeInfo, field.Value.ToString() );
							if( tmpEnum != null && tmpEnum.ToString() == field.Value.ToString() ) {
								var tmpEnumStr = tmpEnum.ToString();
								int idx = 0;
								foreach( var v in values ) {
									if( v.ToString() == tmpEnumStr ) {
										cb.SelectedIndex = idx;
										break;
									}
									idx++;
								}
							}
						}
						catch { }
					}

					if( cb.SelectedIndex < 0 )
						cb.SelectedIndex = 0;

					col2Element = cb;
				}
				else if( field.TypeInfo == typeof(bool) ) {
					CheckBox cb = new CheckBox() {
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Center,
					};

					if( field.Value != null && field.Value is bool onOff )
						cb.IsChecked = onOff == true;

					col2Element = cb;
				}
				else if( field.TypeInfo == typeof(int) ||
							field.TypeInfo == typeof(long) ) {
					NumericUpDown nud = new NumericUpDown() {
						NumericInputMode = NumericInput.Numbers,
						HorizontalAlignment = HorizontalAlignment.Left,
						HorizontalContentAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Center,
						VerticalContentAlignment = VerticalAlignment.Center,
					};

					if( field.TypeInfo == typeof(int) ) {
						nud.Maximum = int.MaxValue;
						nud.Minimum = int.MinValue;
					} else {
						nud.Maximum = long.MaxValue;
						nud.Minimum = long.MinValue;
					}

					if( field.Value != null ) {
						if( field.Value is int intValue )
							nud.Value = intValue;
						else if( field.Value is long longValue )
							nud.Value = longValue;
					}

					col2Element = nud;
				}
				else if( field.TypeInfo == typeof(float) ||
							field.TypeInfo == typeof(double) ) {
					NumericUpDown nud = new NumericUpDown() {
						NumericInputMode = NumericInput.Decimal,
						HorizontalAlignment = HorizontalAlignment.Left,
						HorizontalContentAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Center,
						VerticalContentAlignment = VerticalAlignment.Center,
					};

					if( field.TypeInfo == typeof(float) ) {
						nud.Maximum = float.MaxValue;
						nud.Minimum = float.MinValue;
					} else {
						nud.Maximum = double.MaxValue;
						nud.Minimum = double.MinValue;
					}

					if( field.Value != null ) {
						if( field.Value is float fValue )
							nud.Value = fValue;
						else if( field.Value is double dValue )
							nud.Value = dValue;
					}

					col2Element = nud;
				} else if( field.TypeInfo == typeof(SecureString) ) {
					PasswordBox pb = new PasswordBox() {
						HorizontalContentAlignment = HorizontalAlignment.Left,
						VerticalContentAlignment = VerticalAlignment.Center,
					};

					MatsTempSave.Visibility = Visibility.Visible;
					if( field.Value is SecureString ss ) {
						TextBoxHelper.SetWatermark( pb, string.Format( Languages.Global.Str1CurrentSecureStringLength, ss.Length) );
						TextBoxHelper.SetUseFloatingWatermark( pb, true );
						pb.Password = ss.ConvertToString();
					}
					col2Element = pb;
				}
				else {
					TextBox tb = new TextBox() {
						HorizontalContentAlignment = HorizontalAlignment.Left,
						VerticalContentAlignment = VerticalAlignment.Center,
					};

					if( field.Value != null )
						tb.Text = field.Value.ToString();

					if( string.IsNullOrWhiteSpace( field.Placeholder ) == false ) {
						TextBoxHelper.SetWatermark( tb, field.Placeholder );
						TextBoxHelper.SetUseFloatingWatermark( tb, true );
					}

					col2Element = tb;
				}


				if( col2Element != null ) {
					GdInputs.Children.Add( col2Element );
					Grid.SetColumn( col2Element, 1 );
					Grid.SetRow( col2Element, i );
				}

				if( i >= InputCountMax - 1 )
					break;
			}

		}

		private async void BtnOk_Click( object sender, RoutedEventArgs e )
		{
			if( InputFields == null || InputFields.Count <= 0 ||
				GdInputs.RowDefinitions.Count != InputFields.Count )
				return;

			object[] outValues = new object[InputFields.Count];
			foreach( UIElement ch in GdInputs.Children ) {
				if( ch is Label )
					continue;

				var row = Grid.GetRow( ch );
				if( row < 0 || row >= outValues.Length )
					continue;

				if( ch is TextBox tb ) {
					outValues[row] = tb.Text;
				} else if( ch is PasswordBox pb ) {
					outValues[row] = pb.SecurePassword;
				} else if( ch is CheckBox checkBox ) {
					outValues[row] = checkBox.IsChecked == true;
				} else if( ch is ComboBox cb ) {
					if( cb.SelectedIndex < 0 )
						cb.SelectedIndex = 0;

					outValues[row] = InputFields[row].Value;
					if( InputFields[row].TypeInfo != null && InputFields[row].TypeInfo.IsEnum ) {
						int idx = 0;
						foreach( var val in Enum.GetValues( InputFields[row].TypeInfo ) ) {
							if( idx++ == cb.SelectedIndex ) {
								outValues[row] = val;
								break;
							}
						}
					}
				} else if( ch is NumericUpDown nud ) {
					var field = InputFields[row];
					if( field.TypeInfo == typeof( int ) )
						outValues[row] = (int)nud.Value;
					else if( field.TypeInfo == typeof( long ) )
						outValues[row] = (long)nud.Value;
					else if( field.TypeInfo == typeof( float ) )
						outValues[row] = (float)nud.Value;
					else
						outValues[row] = nud.Value;
				} else {
					outValues[row] = string.Empty;
				}
			}

			Results.Clear();
			for( int i = 0; i < InputFields.Count; ++i ) {
				var field = InputFields[i];
				Results.Add( new InputFieldModel {
					FieldName = field.FieldName,
					TypeInfo = field.TypeInfo,
					Value = outValues[i],
				} );
			}

			if( MainWindow != null )
				await MainWindow.HideMetroDialogAsync( this );
			else
				this.OnClose();
		}

		private async void ButtonCancel_Click( object sender, RoutedEventArgs e )
		{
			Results.Clear();

			if( MainWindow != null )
				await MainWindow.HideMetroDialogAsync( this );
			else
				this.OnClose();
		}
	}
}
