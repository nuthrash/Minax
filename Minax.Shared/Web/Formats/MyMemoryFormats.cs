using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{

	/// <summary>
	/// MyMemory 
	/// </summary>
	/// <remarks>https://api.mymemory.translated.net/get?q=Hello%20World&langpair=en|zh-TW</remarks>
	public class MyMemoryGetFormat1
	{
		[JsonProperty( PropertyName = "responseData" )]
		public ResponseDataContent ResponseData { get; set; }

		[JsonProperty( PropertyName = "quotaFinished" )]
		public bool QuotaFinished { get; set; }

		[JsonProperty( PropertyName = "mtLangSupported" )]
		public object MtLangSupported { get; set; }

		[JsonProperty( PropertyName = "responseDetails" )]
		public string ResponseDetails { get; set; }

		[JsonProperty( PropertyName = "responseStatus" )]
		public int ResponseStatus { get; set; }

		[JsonProperty( PropertyName = "responderId" )]
		public string ResponderId { get; set; }

		[JsonProperty( PropertyName = "exception_code" )]
		public object Exception_code { get; set; }

		[JsonProperty( PropertyName = "matches" )]
		public List<MatchData> Matches { get; set; }

		public class ResponseDataContent
		{
			[JsonProperty( PropertyName = "translatedText" )]
			public string TranslatedText { get; set; }

			[JsonProperty( PropertyName = "match" )]
			public int Match { get; set; }
		}


		public class MatchData
		{
			[JsonProperty( PropertyName = "id" )]
			public string Id { get; set; }

			[JsonProperty( PropertyName = "segment" )]
			public string Segment { get; set; }

			[JsonProperty( PropertyName = "translation" )]
			public string Translation { get; set; }

			[JsonProperty( PropertyName = "quality" )]
			public string Quality { get; set; }

			[JsonProperty( PropertyName = "reference" )]
			public object Reference { get; set; }

			[JsonProperty( PropertyName = "usage-count" )]
			public int UsageCount { get; set; }

			[JsonProperty( PropertyName = "subject" )]
			public string Subject { get; set; }

			[JsonProperty( PropertyName = "created-by" )]
			public string CreatedBy { get; set; }

			[JsonProperty( PropertyName = "last-updated-by" )]
			public string LastUpdatedBy { get; set; }

			[JsonProperty( PropertyName = "create-date" )]
			public string CreateDate { get; set; }

			[JsonProperty( PropertyName = "last-update-date" )]
			public string LastUpdateDate { get; set; }

			[JsonProperty( PropertyName = "match" )]
			public int Match { get; set; }
		}
	}
}
