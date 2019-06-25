using System;
using System.Collections.Generic;
using System.Text;

namespace Minax
{
	/// <summary>
	/// ProgressInfo model for IProgress<T>
	/// </summary>
	public class ProgressInfo
	{
		/// <summary>
		/// 0~100 means progress percent from 0% ~ 100%. Others are ErrorCode, minus(< 0) numbers are error,
		/// > 100 are for other purpose.
		/// </summary>
		public int PercentOrErrorCode { get; set; }

		/// <summary>
		/// Human readable message text.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Extra object about error or exception data.
		/// </summary>
		public object InfoObject { get; set; }
	}
}
