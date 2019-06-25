using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Collections
{
	/// <summary>
	/// Suspendable interface for INotifyXXXXXXX series
	/// </summary>
	public interface INotifySuspendable
	{
		/// <summary>
		/// Is notification event raisable
		/// </summary>
		bool IsNotificationEnabled { get; set; }
	}
}
