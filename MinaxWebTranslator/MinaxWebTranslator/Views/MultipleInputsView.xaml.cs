using MinaxWebTranslator.Desktop.Models;
using Plugin.InputKit.Shared.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	/// <summary>
	/// Multiple inputs custom ContentView for AlertDialogBuilder
	/// </summary>
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class MultipleInputsView : ContentView
	{
		/// <summary>
		/// InputFields count max
		/// </summary>
		internal int InputCountMax { get; set; } = 20;

		/// <summary>
		/// Temporary save SecureString for this session
		/// </summary>
		internal bool TempSaveSecureData => CbTempSave.IsChecked;

		/// <summary>
		/// Input fields models
		/// </summary>
		internal IList<InputFieldModel> InputFields { get; set; }

		public MultipleInputsView()
		{
			InitializeComponent();
		}

		internal void BuildInputs( IList<InputFieldModel> inputList )
		{
			if( inputList == null )
				return;

			this.InputFields = inputList;
			this.BuildInputs();
		}

		internal void BuildInputs()
		{
			if( InputFields == null || InputFields.Count <= 0 ) {
				return;
			}

			CbTempSave.IsVisible = false;

			// manually build each field
			GdInputs.RowDefinitions.Clear();
			for( int i = 0; i < InputFields.Count; ++i ) {
				var field = InputFields[i];
				if( field.TypeInfo == null ) {
					if( field.Value == null )
						continue;
					field.TypeInfo = field.Value.GetType();
				}

				RowDefinition rd = new RowDefinition() { Height = new GridLength( 1.0, GridUnitType.Auto ) };
				GdInputs.RowDefinitions.Add( rd );

				// Column 0: Field Name
				Label lbl = new Label() {
					Text = field.FieldName,
					HorizontalOptions = LayoutOptions.End,
					VerticalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.End,
					VerticalTextAlignment = TextAlignment.Center,
				};
				GdInputs.Children.Add( lbl );
				Grid.SetColumn( lbl, 0 );
				Grid.SetRow( lbl, i );


				// Column 1: Corresponding Type Control; Column 2: Stepper when numerice type
				var hOptions = LayoutOptions.Fill;
				var vOptions = LayoutOptions.Center;
				View col2Element = null;
				if( field.TypeInfo.IsEnum ) {
					Picker cb = new Picker() {
						HorizontalOptions = hOptions,
						VerticalOptions = vOptions,
					};

					var values = Enum.GetValues( field.TypeInfo );
					var itemsSourceBinding = new Binding { Source = values, Converter = field.Converter };
					cb.SetBinding( Picker.ItemsSourceProperty, itemsSourceBinding );

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
				else if( field.TypeInfo == typeof( bool ) ) {
					CheckBox cb = new CheckBox() {
						HorizontalOptions = hOptions,
						VerticalOptions = vOptions,
						Type = CheckBox.CheckType.Check,
					};

					if( field.Value != null && field.Value is bool onOff )
						cb.IsChecked = onOff == true;

					col2Element = cb;
				}
				else if( field.TypeInfo == typeof( double ) ||
							field.TypeInfo == typeof( int ) ||
							field.TypeInfo == typeof( float ) ||
							field.TypeInfo == typeof( long ) ) {
					Stepper nud = new Stepper() {
						HorizontalOptions = hOptions,
						VerticalOptions = vOptions,
					};
					Label lblValue = new Label() {
						Text = "0",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center,
					};
					nud.ValueChanged += (s1, e1) => {
						if( nud.Increment == 0.01 ) {
							lblValue.Text = e1.NewValue.ToString( "0.##" );
						} else 
						lblValue.Text = e1.NewValue.ToString();
					};

					if( field.TypeInfo == typeof( double ) ) {
						nud.Maximum = double.MaxValue;
						nud.Minimum = double.MinValue;
						nud.Increment = 0.01;
					} else if( field.TypeInfo == typeof( int ) ) {
						nud.Maximum = int.MaxValue;
						nud.Minimum = int.MinValue;
						nud.Increment = 1;
					}
					else if(field.TypeInfo == typeof(float) ) {
						nud.Maximum = float.MaxValue;
						nud.Minimum = float.MinValue;
						nud.Increment = 0.01;
					}
					else {
						nud.Maximum = long.MaxValue;
						nud.Minimum = long.MinValue;
						nud.Increment = 1;
					}

					if( field.Value != null ) {
						if( field.Value is int intValue )
							nud.Value = intValue;
						else if( field.Value is long longValue )
							nud.Value = longValue;
						else if( field.Value is float floatValue )
							nud.Value = floatValue;
						else
							nud.Value = (double)field.Value;
					}

					// put Stepper to Column3
					GdInputs.Children.Add( nud );
					Grid.SetColumn( nud, 2 );
					Grid.SetRow( nud, i );

					//col2Element = nud;
					col2Element = lblValue;
				}
				else if( field.TypeInfo == typeof( SecureString ) ) {
					Entry pb = new Entry() {
						Keyboard = Keyboard.Plain,
						IsPassword = true,
						IsSpellCheckEnabled = false,
						HorizontalOptions = hOptions,
						VerticalOptions = vOptions,
						HorizontalTextAlignment = TextAlignment.Start,
					};

					// show TempSavexxxx only when input contains SecureString!!
					CbTempSave.IsVisible = true;
					if( field.Value is SecureString ss ) {
						pb.Placeholder = $"Current text length is {ss.Length}";
						pb.Text = Minax.Utils.ConvertToString( ss );
					}
					col2Element = pb;
				}
				else {
					Entry tb = new Entry() {
						HorizontalOptions = hOptions,
						VerticalOptions = vOptions,
						HorizontalTextAlignment = TextAlignment.Start,
					};

					if( field.Value != null )
						tb.Text = field.Value.ToString();

					if( string.IsNullOrWhiteSpace( field.Placeholder ) == false ) {
						tb.Placeholder = field.Placeholder;
					}

					col2Element = tb;
				}

				if( col2Element != null ) {
					GdInputs.Children.Add( col2Element );
					Grid.SetColumn( col2Element, 1 );
					Grid.SetRow( col2Element, i );
					if( col2Element is Label == false ) {
						Grid.SetColumnSpan( col2Element, 2 );
					}
				}

				if( i >= InputCountMax - 1 )
					break;
			}
		}

		internal IList<InputFieldModel> GetResults()
		{
			List<InputFieldModel> results = new List<InputFieldModel>();

			if( InputFields == null || InputFields.Count <= 0 ||
				GdInputs.RowDefinitions.Count != InputFields.Count )
				return results;

			object[] outValues = new object[InputFields.Count];
			foreach( View ch in GdInputs.Children ) {
				if( ch is Label )
					continue;

				var row = Grid.GetRow( ch );
				if( row < 0 || row >= outValues.Length )
					continue;

				if( ch is Entry tb ) {
					if( tb.IsPassword && InputFields[row].TypeInfo == typeof( SecureString ) )
						outValues[row] = Minax.Utils.ConvertToSecureString( tb.Text );
					else
						outValues[row] = tb.Text;
				}
				else if( ch is CheckBox checkBox ) {
					outValues[row] = checkBox.IsChecked == true;
				}
				else if( ch is Picker cb ) {
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
				}
				else if( ch is Stepper nud ) {
					var field = InputFields[row];
					if( field.TypeInfo == typeof( int ) )
						outValues[row] = (int)nud.Value;
					else if( field.TypeInfo == typeof( long ) )
						outValues[row] = (long)nud.Value;
					else if( field.TypeInfo == typeof( float ) )
						outValues[row] = (float)nud.Value;
					else
						outValues[row] = nud.Value;
				}
				else {
					outValues[row] = string.Empty;
				}
			}

			for( int i = 0; i < InputFields.Count; ++i ) {
				var field = InputFields[i];
				results.Add( new InputFieldModel {
					FieldName = field.FieldName,
					TypeInfo = field.TypeInfo,
					Value = outValues[i],
				} );
			}

			return results;
		}

	}
}