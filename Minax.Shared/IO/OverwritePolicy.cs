using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.IO
{
	/// <summary>
	/// Overwrite Policy for download/save file to same file name
	/// </summary>
	public enum OverwritePolicy
	{
		/// <summary>
		/// Skip without asking
		/// </summary>
		Skip,
		/// <summary>
		/// Force overwrite without asking
		/// </summary>
		ForceOverwriteWithoutAsking,
		/// <summary>
		/// Always asking when found same file name
		/// </summary>
		AlwaysAsking,
		/// <summary>
		/// Overwrite only when target file date is older
		/// </summary>
		FileDateNew,
		/// <summary>
		/// Overwrite only when target file size is smaller
		/// </summary>
		FileSizeLarger,
	}
}
