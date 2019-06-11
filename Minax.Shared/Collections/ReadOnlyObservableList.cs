using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Minax.Collections
{
	/// <summary>
	/// A duplicated Read Only Observable List 
	/// </summary>
	/// <typeparam name="T">Item type</typeparam>
	public class ReadOnlyObservableList<T> : IReadOnlyObservableList<T>, INotifySuspendable
	{
		/// <summary>
		/// Constructor with another collection for cloning
		/// </summary>
		/// <param name="collection">The original collection to be cloned</param>
		public ReadOnlyObservableList( IEnumerable<T> collection )
		{
			origCollection = collection ?? throw new ArgumentNullException( nameof( collection ) );
			list = new List<T>( origCollection );

			// subscribe all events
			if( origCollection is INotifyPropertyChanged inc ) {
				inc.PropertyChanged -= List_PropertyChanged;
				inc.PropertyChanged += List_PropertyChanged;
			}
			if( origCollection is INotifyCollectionChanged incc ) {
				incc.CollectionChanged -= List_CollectionChanged;
				incc.CollectionChanged += List_CollectionChanged;
			}
		}

		~ReadOnlyObservableList()
		{
			// un-subscribe all events
			if( origCollection is INotifyPropertyChanged inc ) {
				inc.PropertyChanged -= List_PropertyChanged;
			}
			if( origCollection is INotifyCollectionChanged incc ) {
				incc.CollectionChanged -= List_CollectionChanged;
			}
		}

		#region "private ctor/data members/callbacks"

		protected List<T> list = null;
		protected IEnumerable<T> origCollection = null;
		private readonly object syncObj = new object();
		private string groupLongName, groupShortName;
		private bool isNotificationEnabled = true;

		// not allow default ctor public!
		protected ReadOnlyObservableList()
		{
			list = new List<T>();
		}
		protected ReadOnlyObservableList( int capacity )
		{
			list = new List<T>( capacity );
		}

		protected bool SetProperty<Type>( ref Type backingStore, Type value = default(Type),
											[CallerMemberName]string propertyName = "" )
		{
			if( EqualityComparer<Type>.Default.Equals( backingStore, value ) )
				return false;

			backingStore = value;
			if( isNotificationEnabled ) {
				OnPropertyChanged( propertyName );
			}
			return true;
		}

		

		private void List_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			list = new List<T>( origCollection );

			this.PropertyChanged?.Invoke( this, e );
		}

		private void List_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			list = new List<T>( origCollection );

			this.CollectionChanged?.Invoke( this, e );
		}

		#endregion

		#region "Grouped Properties"

		public string GroupedLongName {
			get => groupLongName;
			set => SetProperty( ref groupLongName, value );
		}

		public string GroupedShortName {
			get => groupShortName;
			set => SetProperty( ref groupShortName, value );
		}

		#endregion

		#region "IReadOnlyObservableList<T> members"

		/// <summary>
		/// Get a new unmodifiable observable list
		/// </summary>
		/// <returns>ReadOnlyObservableList collection</returns>
		public ReadOnlyObservableList<T> AsReadOnly() => this;

		#endregion

		#region "INotifyStoppable members"
		public bool IsNotificationEnabled {
			get => isNotificationEnabled;
			set => SetProperty( ref isNotificationEnabled, value );
		}

		#endregion

		#region "IReadOnlyList<T> members"

		public T this[int index] {
			get {
				lock( syncObj )
					return list[index];
			}
		}

		#endregion

		#region "IReadOnlyCollection<T> members"

		public int Count {
			get {
				lock( syncObj )
					return list.Count;
			}
		}

		#endregion

		#region "ICollection members"

		public bool IsSynchronized => true;

		public object SyncRoot => syncObj;

		public void CopyTo( Array array, int index )
		{
			Array.Copy( list.ToArray(), index, array, 0, list.Count - index );
		}

		#endregion

		#region "IEnumerable<T> member"

		public IEnumerator<T> GetEnumerator()
		{
			lock( syncObj )
				return ((ICollection<T>)list).GetEnumerator();
		}

		#endregion

		#region "IEnumerable members"

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock( syncObj )
				return ((IEnumerable)list).GetEnumerator();
		}

		#endregion


		#region "INotifyCollectionChanged and INotifyPropertyChanged members"

		/// <summary>
		/// Collection Changed event
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		/// <summary>
		/// Property Changed event
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnCollectionChanged( NotifyCollectionChangedAction action,
														object changedItem = null, int index = -1 )
		{
			CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( action, changedItem, index ) );
		}

		protected virtual void OnCollectionChanged( NotifyCollectionChangedAction action,
														object newItem = null, object oldItem = null, int index = -1 )
		{
			CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( action, newItem, oldItem, index ) );
		}

		protected virtual void OnCollectionChanged( NotifyCollectionChangedAction action,
														object changedItem, int newIndex, int oldIndex )
		{

			CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( action, changedItem, newIndex, oldIndex ) );
		}

		protected virtual void OnPropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion
	}
}
