using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	public class TencentTranslatorFormat1
	{
		[JsonProperty( PropertyName = "sessionUuid" )]
		public string SessionUuid {
			get; set;
		}

		[JsonProperty( PropertyName = "translate" )]
		public Translate TranslateData {
			get; set;
		}

		[JsonProperty( PropertyName = "dict" )]
		public object Dict {
			get; set;
		}

		[JsonProperty( PropertyName = "suggest" )]
		public object Suggest {
			get; set;
		}

		[JsonProperty( PropertyName = "errCode" )]
		public int ErrorCode {
			get; set;
		}

		[JsonProperty( PropertyName = "errMsg" )]
		public string ErrorMessage {
			get; set;
		}

		public class Translate
		{
			[JsonProperty( PropertyName = "errCode" )]
			public int ErrorCode {
				get; set;
			}

			[JsonProperty( PropertyName = "errMsg" )]
			public string ErrorMessage {
				get; set;
			}

			[JsonProperty( PropertyName = "sessionUuid" )]
			public string SessionUuid {
				get; set;
			}

			[JsonProperty( PropertyName = "source" )]
			public string Source {
				get; set;
			}

			[JsonProperty( PropertyName = "target" )]
			public string Target {
				get; set;
			}

			[JsonProperty( PropertyName = "records" )]
			public Record[] Records {
				get; set;
			}

			[JsonProperty( PropertyName = "full" )]
			public bool Full {
				get; set;
			}

			[JsonProperty( PropertyName = "options" )]
			public Options OptionData {
				get; set;
			}
		}

		public class Options
		{
		}

		public class Record
		{
			[JsonProperty( PropertyName = "sourceText" )]
			public string SourceText {
				get; set;
			}

			[JsonProperty( PropertyName = "targetText" )]
			public string TargetText {
				get; set;
			}

			[JsonProperty( PropertyName = "traceId" )]
			public string TraceId {
				get; set;
			}
		}
	}


	public class TencentTranslatorTokens1
	{
		[JsonProperty( PropertyName = "qtv" )]
		public string Qtv {
			get; set;
		}

		[JsonProperty( PropertyName = "qtk" )]
		public string Qtk {
			get; set;
		}
	}
}
