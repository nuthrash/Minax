using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Translation
{
	/// <summary>
	/// The translated result by remote Translator/Translation service with section by section
	/// </summary>
	public class YieldResult
	{
		/// <summary>
		/// Original section text which is divided by MaxWord
		/// </summary>
		public string OriginalSection { get; set; }
		/// <summary>
		/// Translated section text
		/// </summary>
		public string TranslatedSection { get; set; }
		/// <summary>
		/// 0~100 means progress percent from 0% ~ 100%. Others are ErrorCode, minus(< 0) numbers are errors,
		/// > 100 are for other purpose. If encounter <code>yield break;</code> would not return any value,
		/// so you shall keep last success percent to track yield status.
		/// </summary>
		public int PercentOrErrorCode { get; set; }

	}
}
