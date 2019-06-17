using Newtonsoft.Json;
using System.Collections.Generic;

namespace Minax.Web.Formats
{
	/// <summary>
	/// JSON format for Fanhuaji (繁化姬 https://zhconvert.org/ ) online Chinese characters converter and i10n API
	/// </summary>
	public class ZhConvertFormat1
	{
		[JsonProperty( PropertyName = "code" )]
		public int Code { get; set; }
		[JsonProperty( PropertyName = "execTime" )]
		public double ExecTime { get; set; }
		[JsonProperty( PropertyName = "msg" )]
		public string Message { get; set; }

		[JsonProperty( PropertyName = "revisions" )]
		public Revision Revisions { get; set; }
		[JsonProperty( PropertyName = "data" )]
		public DataFormat1 Data { get; set; }

		public class Revision
		{
			[JsonProperty( PropertyName = "msg" )]
			public string Message { get; set; }
			[JsonProperty( PropertyName = "build" )]
			public string Build { get; set; }
			[JsonProperty( PropertyName = "time" )]
			public long Time { get; set; }
		}

		public class DataFormat1
		{
			[JsonProperty( PropertyName = "converter" )]
			public string Converter { get; set; }
			[JsonProperty( PropertyName = "text" )]
			public string Text { get; set; }
			[JsonProperty( PropertyName = "diff" )]
			public string Diff { get; set; }
			[JsonProperty( PropertyName = "textFormat" )]
			public string TextFormat { get; set; }

			[JsonProperty( PropertyName = "usedModules" )]
			public List<string> UsedModules { get; set; }
			[JsonProperty( PropertyName = "jpTextStyles" )]
			public List<string> JpTextStyles { get; set; }
		}
	}
}
