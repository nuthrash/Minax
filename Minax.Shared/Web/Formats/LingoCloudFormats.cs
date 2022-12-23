using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	public class LingoCloudXlateRequest1
	{
		[JsonProperty( "source" )]
		public List<string> Source {
			get; set;
		}

		[JsonProperty( "trans_type" )]
		public string TransType {
			get; set;
		}

		[JsonProperty( "request_id" )]
		public string RequestId {
			get; set;
		}

		[JsonProperty( "media" )]
		public string Media {
			get; set;
		}

		[JsonProperty( "os_type" )]
		public string OsType {
			get; set;
		}

		[JsonProperty( "dict" )]
		public bool Dict {
			get; set;
		}

		[JsonProperty( "cached" )]
		public bool Cached {
			get; set;
		}

		[JsonProperty( "replaced" )]
		public bool Replaced {
			get; set;
		}

		[JsonProperty( "detect" )]
		public bool Detect {
			get; set;
		}

		[JsonProperty( "browser_id" )]
		public string BrowserId {
			get; set;
		}
	}

	public class LingoCloudXlateResponse1
	{
		[JsonProperty( "rc" )]
		public int Rc {
			get; set;
		}

		[JsonProperty( "target" )]
		public List<string> Target {
			get; set;
		}

		[JsonProperty( "confidence" )]
		public double Confidence {
			get; set;
		}

		[JsonProperty( "src_tgt" )]
		public SrcTgt SourceTarget {
			get; set;
		}

		[JsonProperty( "isdict" )]
		public int IsDict {
			get; set;
		}


		public class SrcTgt
		{
		}
	}

	public class LingoCloudJwtRequest1
	{
		[JsonProperty( PropertyName = "browser_id" )]
		public string BrowserId {
			get; set;
		}
	}

	public class LingoCloudJwtResponse1
	{
		[JsonProperty( PropertyName = "rc" )]
		public int Rc {
			get; set;
		}

		[JsonProperty( PropertyName = "jwt" )]
		public string Jwt {
			get; set;
		}
	}
}
