using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	public class IcibaTranslatorFormat1
	{
		[JsonProperty( PropertyName = "error_code" )]
		public int ErrorCode {
			get; set;
		}

		[JsonProperty( PropertyName = "message" )]
		public string Message {
			get; set;
		}


		[JsonProperty( PropertyName = "status" )]
		public int Status {
			get; set;
		}

		[JsonProperty( PropertyName = "content" )]
		public Content ContentData {
			get; set;
		}

		public class Content
		{
			[JsonProperty( PropertyName = "from" )]
			public string From {
				get; set;
			}

			[JsonProperty( PropertyName = "to" )]
			public string To {
				get; set;
			}

			[JsonProperty( PropertyName = "vendor" )]
			public string Vendor {
				get; set;
			}

			[JsonProperty( PropertyName = "out" )]
			public string Out {
				get; set;
			}

			[JsonProperty( PropertyName = "reqid" )]
			public string Reqid {
				get; set;
			}

			[JsonProperty( PropertyName = "version" )]
			public string Version {
				get; set;
			}

			[JsonProperty( PropertyName = "ciba_use" )]
			public string CibaUse {
				get; set;
			}

			[JsonProperty( PropertyName = "ciba_out" )]
			public string CibaOut {
				get; set;
			}

			[JsonProperty( PropertyName = "err_no" )]
			public int ErrorNo {
				get; set;
			}

			[JsonProperty( PropertyName = "ttsLan" )]
			public int TtsLan {
				get; set;
			}

			[JsonProperty( PropertyName = "ttsLanFrom" )]
			public int TtsLanFrom {
				get; set;
			}
		}
	}
}
