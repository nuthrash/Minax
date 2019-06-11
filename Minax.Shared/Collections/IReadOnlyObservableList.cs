using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Minax.Collections
{
	/// <summary>
	/// ReadOnly Observable List interface to monitor original notifiable list
	/// </summary>
	/// <typeparam name="T">Item type</typeparam>
	public interface IReadOnlyObservableList<T> : IReadOnlyList<T>, ICollection, INotifyCollectionChanged, INotifyPropertyChanged
	{
		/// <summary>
		/// Get a new unmodifiable observable list
		/// </summary>
		/// <returns>ReadOnlyObservableList collection</returns>
		ReadOnlyObservableList<T> AsReadOnly();
	}
}
