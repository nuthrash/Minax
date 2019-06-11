using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Minax.Collections
{
	/// <summary>
	/// ObservableList for whole new collection or observable collection
	/// </summary>
	/// <typeparam name="T">Item type</typeparam>
	public class ObservableList<T> : ReadOnlyObservableList<T>, IList<T>, IList
	{
		public ObservableList() : base() { }

		public ObservableList( int capacity ): base( capacity ) { }

		public ObservableList( IEnumerable<T> collection ) : base(collection) { }


		#region "ObservableCollection"

		public void Move( int oldIndex, int newIndex )
		{
			if( oldIndex < 0 || oldIndex > base.list.Count )
				return;
			if( newIndex < 0 || newIndex > base.list.Count )
				return;

			var oldItem = base.list[oldIndex];
			base.list.RemoveAt( oldIndex );
			base.list.Insert( newIndex, oldItem );
			base.OnCollectionChanged( NotifyCollectionChangedAction.Move, oldItem, newIndex, oldIndex );
		}

		#endregion

		#region "List<T> members"

		public void AddRange( IEnumerable<T> collection )
		{
			int index = base.list.Count - 1;
			base.list.AddRange( collection );

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, collection, index );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public void InsertRange( int index, IEnumerable<T> collection )
		{
			base.list.InsertRange( index, collection );

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, collection, index );
			base.OnPropertyChanged( nameof( Count ) );
		}

		#endregion


		#region "IList<T>"

		public new T this[int index] {
			get => base.list[index];
			set {
				T oldValue = base.list[index];
				if( oldValue.Equals( value ) == false ) {
					base.list[index] = value;
					base.OnCollectionChanged( NotifyCollectionChangedAction.Replace, value, oldValue, index );
				}
			}
		}

		public bool IsReadOnly => false;

		public void Add( T item )
		{
			base.list.Add( item );

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, item, base.list.Count - 1 );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public void Clear()
		{
			base.list.Clear();

			base.OnCollectionChanged( NotifyCollectionChangedAction.Reset, null, -1 );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public bool Contains( T item )
		{
			return base.list.Contains( item );
		}

		public void CopyTo( T[] array, int arrayIndex )
		{
			base.list.CopyTo( array, arrayIndex );
		}

		public int IndexOf( T item )
		{
			return base.list.IndexOf( item );
		}

		public void Insert( int index, T item )
		{
			base.list.Insert( index, item ); // may throw exception

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, item, index );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public bool Remove( T item )
		{
			int index = base.list.IndexOf( item );
			if( index >= 0 ) {
				base.list.Remove( item );
				base.OnCollectionChanged( NotifyCollectionChangedAction.Remove, item, index );
				base.OnPropertyChanged( nameof( Count ) );
				return true;
			}
			return false;
		}

		public void RemoveAt( int index )
		{
			T item = base.list[index];
			base.list.RemoveAt( index );
			base.OnCollectionChanged( NotifyCollectionChangedAction.Remove, item, index );
			base.OnPropertyChanged( nameof( Count ) );
		}

		#endregion

		#region "IList members"

		public bool IsFixedSize => false;

		object IList.this[int index] {
			get => base.list[index];
			set {
				T oldValue = base.list[index];
				if( oldValue.Equals( value ) == false && value is T ) {
					base.list[index] = (T)value;
					base.OnCollectionChanged( NotifyCollectionChangedAction.Replace, value, oldValue, index );
				}
			}
		}


		public int Add( object value )
		{
			if( value is T == false )
				return -1;

			var item = (T)value;
			base.list.Add( item );

			int idx = base.list.Count - 1;
			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, item, idx );
			base.OnPropertyChanged( nameof( Count ) );
			return idx;
		}

		public bool Contains( object value )
		{
			if( value is T item ) {
				return list.Contains( item );
			}
			return false;
		}

		public int IndexOf( object value )
		{
			if( value is T item ) {
				return list.IndexOf( item );
			}
			return -1;
		}

		public void Insert( int index, object value )
		{
			if( value is T item ) {
				list.Insert( index, item );
			}
		}

		public void Remove( object value )
		{
			if( value is T item ) {
				list.Remove( item );
			}
		}

		#endregion

	}
}
