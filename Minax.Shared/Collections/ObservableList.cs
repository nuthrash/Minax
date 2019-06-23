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
						//where T:class
	{
		public const int ItemsCountMinimum = 16;

		public ObservableList() : base() { }

		public ObservableList( int capacity ): base( capacity ) { }

		public ObservableList( IEnumerable<T> collection ) : base(collection) { }

		/// <summary>
		/// Items count maximum. When items count exceed max., then would auto remove extra items!
		/// </summary>
		public int ItemsCountMaximum {
			get => itemCntMax;
			set {
				if( value < ItemsCountMinimum )
					return;

				SetProperty( ref itemCntMax, value );
				CheckMaxAndRemoveExtra();
			}
		}
		private int itemCntMax = int.MaxValue;

		/// <summary>
		/// Auto remove items from front(true) or end(false) when items count exceed ItemsCountMaximum.
		/// </summary>
		public bool AutoRemoveFromListFront {
			get => autoRemoveFromListFront;
			set => SetProperty( ref autoRemoveFromListFront, value );
		}
		private bool autoRemoveFromListFront = false;

		/// <summary>
		/// Remove first item of list
		/// </summary>
		/// <returns>true means first item had been removed</returns>
		public bool RemoveFirst()
		{
			if( base.list.Count <= 0 )
				return false;

			base.list.RemoveAt( 0 );
			return true;
		}

		/// <summary>
		/// Remove last item of list
		/// </summary>
		/// <returns>true means last item had been removed</returns>
		public bool RemoveLast()
		{
			if( base.list.Count <= 0 )
				return false;

			base.list.RemoveAt( base.list.Count - 1);
			return true;
		}

		/// <summary>
		/// Pop out the first item of list
		/// </summary>
		/// <returns>T object for found item</returns>
		public T PopFirst()
		{
			if( base.list.Count <= 0 )
				return default;

			T first = base.list[0];
			base.list.RemoveAt( 0 );
			return first;
		}

		/// <summary>
		/// Pop out the last item of list
		/// </summary>
		/// <returns>T object for found item</returns>
		public T PopLast()
		{
			if( base.list.Count <= 0 )
				return default;

			T last = base.list[base.list.Count - 1];
			lock( base.SyncRoot )
				base.list.RemoveAt( base.list.Count - 1 );
			return last;
		}

		/// <summary>
		/// Remove items by list argument
		/// </summary>
		/// <param name="items">The items need to remove</param>
		/// <param name="removeAllWhenAllFound">Only remove all items when all items are found in list</param>
		/// <returns>false for at least one item was not found in list</returns>
		public bool RemoveItems( IEnumerable<T> items, bool removeAllWhenAllFound = false )
		{
			bool rst = true;
			int startIdx = -1;

			if( removeAllWhenAllFound ) {
				List<T> l1 = new List<T>();
				int itemCnt = 0;
				foreach( var item in items ) {
					itemCnt++;

					var idx = base.list.IndexOf( item );
					if( idx >= 0 ) {
						if( startIdx < 0 || startIdx > idx )
							startIdx = idx;
						l1.Add( item );
					} else {
						return false;
					}
				}

				if( l1.Count == itemCnt ) {
					lock( base.SyncRoot )
						foreach( var item in items ) {
							base.list.Remove( item );
						}
					rst = true;
				}
			} else {
				foreach( var item in items ) {
					var idx = base.list.IndexOf( item );
					if( idx >= 0 ) {
						if( startIdx < 0 || startIdx > idx )
							startIdx = idx;

						lock( base.SyncRoot )
							base.list.RemoveAt( idx );
					}
					else
						rst = false;
				}
			}

			if( startIdx >= 0 ) {
				//base.OnCollectionChanged( NotifyCollectionChangedAction.Remove, items, startIdx );
				base.OnCollectionChanged( NotifyCollectionChangedAction.Remove, new List<T>( items ) );
				base.OnPropertyChanged( nameof( Count ) );
			}

			return rst;
		}

		public void CheckMaxAndRemoveExtra()
		{
			if( base.list.Count > itemCntMax ) {
				int cnt = base.list.Count - itemCntMax;
				if( AutoRemoveFromListFront ) {
					this.RemoveRange( 0, cnt );
				}
				else {
					this.RemoveRange( base.list.Count - cnt, cnt );
				}
			}
		}

		#region "ObservableCollection like"

		public void Move( int oldIndex, int newIndex )
		{
			if( oldIndex < 0 || oldIndex >= base.list.Count )
				return;
			if( newIndex < 0 || newIndex >= base.list.Count )
				return;

			var oldItem = base.list[oldIndex];
			lock( base.SyncRoot ) {
				base.list.RemoveAt( oldIndex );
				base.list.Insert( newIndex, oldItem );
			}
			base.OnCollectionChanged( NotifyCollectionChangedAction.Move, oldItem, newIndex, oldIndex );
		}

		#endregion

		#region "List<T> like"

		public void AddRange( IEnumerable<T> collection )
		{
			int index = base.list.Count;
			lock( base.SyncRoot )
				base.list.AddRange( collection );

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, new List<T>( collection ) );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public void InsertRange( int index, IEnumerable<T> collection )
		{
			lock( base.SyncRoot )
				base.list.InsertRange( index, collection );

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, new List<T>( collection ), index );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public void RemoveRange( int startIndex, int count )
		{
			var items = new T[count];
			base.list.CopyTo( startIndex, items, 0, count );
			lock( base.SyncRoot )
				base.list.RemoveRange( startIndex, count );
			base.OnCollectionChanged( NotifyCollectionChangedAction.Remove, items, startIndex );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public int RemoveAll( Predicate<T> match )
		{
			var items = base.list.FindAll( match );

			if( items.Count <= 0 )
				return 0;

			var startIdx = base.list.IndexOf( items[0] );
			var rst = -1;
			lock( base.SyncRoot )
				rst = base.list.RemoveAll( match );

			base.OnCollectionChanged( NotifyCollectionChangedAction.Remove, items, startIdx );
			base.OnPropertyChanged( nameof( Count ) );

			return rst;
		}

		#endregion


		#region "IList<T> members"

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
			lock( base.SyncRoot )
				base.list.Add( item );

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, item, base.list.Count - 1 );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public void Clear()
		{
			lock( base.SyncRoot )
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
			lock( base.SyncRoot )
				base.list.Insert( index, item ); // may throw exception

			base.OnCollectionChanged( NotifyCollectionChangedAction.Add, item, index );
			base.OnPropertyChanged( nameof( Count ) );
		}

		public bool Remove( T item )
		{
			int index = base.list.IndexOf( item );
			if( index >= 0 ) {
				lock( base.SyncRoot )
					base.list.Remove( item );
				base.OnCollectionChanged( NotifyCollectionChangedAction.Remove, item, index );
				base.OnPropertyChanged( nameof( Count ) );
				return true;
			}
			return false;
		}

		public void RemoveAt( int index )
		{
			if( index < 0 || index + 1 >= base.list.Count )
				return;

			T item = base.list[index];
			lock( base.SyncRoot )
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
			lock( base.SyncRoot )
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
			if( index < 0 )
				return;

			if( value is T item ) {
				lock( base.SyncRoot ) {
					if( index > list.Count )
						list.Add( item );
					else
						list.Insert( index, item );
				}
			}
		}

		public void Remove( object value )
		{
			if( value is T item && list.Contains(item) ) {
				lock( base.SyncRoot )
					list.Remove( item );
			}
		}

		#endregion

	}
}
