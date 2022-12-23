using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Minax.Web.Formats
{
	public class BaiduTranslatorFormat1
	{
		// ERROR JSON ==> {"error":998,"msg":"fail"}
		[JsonProperty( PropertyName = "error" )]
		public int Error { get; set; }
		[JsonProperty( PropertyName = "msg" )]
		public string Message { get; set; }

		// RESULT JSON ==> {"from":"jp","to":"cht","domain":"all","type":2,"status":0,"data":[{"dst":"\u4f0a\u85a9\u62c9\u65af\u738b\u570b","prefixWrap":0,"src":"\u30a4\u30b5\u30e9\u30b9\u738b\u56fd","relation":[],"result":[[0,"\u4f0a\u85a9\u62c9\u65af\u738b\u570b",["0|18"],[],["0|18"],["0|18"]]]}]}
		[JsonProperty( PropertyName = "from" )]
		public string From { get; set; }
		[JsonProperty( PropertyName = "to" )]
		public string To { get; set; }
		[JsonProperty( PropertyName = "domain" )]
		public string Domain { get; set; }
		[JsonProperty( PropertyName = "type" )]
		public int Type { get; set; }
		[JsonProperty( PropertyName = "status" )]
		public int Status { get; set; }
		[JsonProperty( PropertyName = "data" )]
		public List<DataFormat> Data { get; set; }

		public class DataFormat
		{
			[JsonProperty( PropertyName = "dst" )]
			public string Destination { get; set; }
			[JsonProperty( PropertyName = "src" )]
			public string Source { get; set; }
			[JsonProperty( PropertyName = "prefixWrap" )]
			public int PrefixWrap { get; set; }

			[JsonProperty( PropertyName = "relation" )]
			public List<string> Relation { get; set; }

			[JsonProperty( PropertyName = "result" )]
			[JsonIgnore]
			public JObject Result { get; set; }
		}
	}

	public class BaiduTranslatorFormat2
	{
		// ERROR JSON ==> {"error":998,"msg":"fail"}
		[JsonProperty( PropertyName = "error" )]
		public int Error { get; set; }
		[JsonProperty( PropertyName = "msg" )]
		public string Message { get; set; }

		[JsonProperty( PropertyName = "logid" )]
		public long LogId { get; set; }

		[JsonProperty( PropertyName = "trans_result" )]
		public TransResultData TransResult { get; set; }

		[JsonProperty( PropertyName = "dict_result" )]
		public List<object> DictResult { get; set; }

		[JsonProperty( PropertyName = "liju_result" )]
		public object LijuResult { get; set; }

		public class TransResultData
		{
			[JsonProperty( PropertyName = "from" )]
			public string From { get; set; }

			[JsonProperty( PropertyName = "to" )]
			public string To { get; set; }

			[JsonProperty( PropertyName = "domain" )]
			public string Domain { get; set; }

			[JsonProperty( PropertyName = "type" )]
			public int Type { get; set; }

			[JsonProperty( PropertyName = "status" )]
			public int Status { get; set; }

			[JsonProperty( PropertyName = "data" )]
			public List<DataFormat> Data { get; set; }
		}

		public class DataFormat
		{
			[JsonProperty( PropertyName = "dst" )]
			public string Destination { get; set; }

			[JsonProperty( PropertyName = "prefixWrap" )]
			public int PrefixWrap { get; set; }

			[JsonProperty( PropertyName = "src" )]
			public string Source { get; set; }

			[JsonProperty( PropertyName = "relation" )]
			public List<object> Relation { get; set; }

			[JsonProperty( PropertyName = "result" )]
			public List<List<object>> Result { get; set; }
		}
	}

	public class BaiduTranslatorMobileResponse1
	{

		[JsonProperty( PropertyName = "errno" )]
		public int Errno {
			get; set;
		}

		[JsonProperty( PropertyName = "from" )]
		public string From {
			get; set;
		}

		[JsonProperty( PropertyName = "to" )]
		public string To {
			get; set;
		}

		[JsonProperty( PropertyName = "trans" )]
		public List<TranData> Trans {
			get; set;
		}

		[JsonProperty( PropertyName = "dict" )]
		public List<object> Dict {
			get; set;
		}

		[JsonProperty( PropertyName = "keywords" )]
		public List<object> Keywords {
			get; set;
		}

		public class TranData
		{
			// unicode string \u4e0d\u6703\u5427 不會吧
			[JsonProperty( PropertyName = "dst" )]
			public string Dst {
				get; set;
			}

			[JsonProperty( PropertyName = "prefixWrap" )]
			public int PrefixWrap {
				get; set;
			}

			// [ 0, "不會吧", […], [], […], […] ]
			[JsonProperty( PropertyName = "result" )]
			public List<List<object>> Result {
				get; set;
			}

			// unicode string \u307e\u3055\u304b まさか
			[JsonProperty( PropertyName = "src" )]
			public string Src {
				get; set;
			}
		}
	}

	// http://api.fanyi.baidu.com/api/trans/product/apidoc#joinFile
	public class BaiduTranslationRequestV1
	{
		[JsonProperty( PropertyName = "q" )]
		public string Query { get; set; }

		[JsonProperty( PropertyName = "from" )]
		public string SourceLanguage { get; set; } = "auto";

		[JsonProperty( PropertyName = "to" )]
		public string TargetLanguage { get; set; }

		[JsonProperty( PropertyName = "appid" )]
		public string AppId { get; set; } // int???

		[JsonProperty( PropertyName = "salt" )]
		public string Salt { get; set; } // int ???

		[JsonProperty( PropertyName = "sign" )]
		public string Md5Sign { get; set; }
	}

	public class BaiduTranslationResponseV1
	{
		[JsonProperty( PropertyName = "error_code" )]
		public string ErrorCode { set; get; }

		[JsonProperty( PropertyName = "error_msg" )]
		public string ErrorMessage { set; get; }

		[JsonProperty( PropertyName = "from" )]
		public string SourceLanguage { get; set; }

		[JsonProperty( PropertyName = "to" )]
		public string TargetLanguage { get; set; }

		[JsonProperty( PropertyName = "trans_result" )]
		public List<TranslateContent> TranslatedResult { set; get; }

		public class TranslateContent
		{
			[JsonProperty( PropertyName = "src" )]
			public string Source { set; get; }

			[JsonProperty( PropertyName = "dst" )]
			public string Destination { set; get; }
		}
	}

}
