using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	public class CrossLanguageFormat1
	{

		[JsonProperty( PropertyName = "t" )]
		public List<TranslationResult> Results { get; set; }

		[JsonProperty( PropertyName = "isDicRegist" )]
		public bool IsDicRegist { get; set; }


		public class TranslationResult
		{
			[JsonProperty( PropertyName = "text" )]
			public string OriginalText { get; set; }

			[JsonProperty( PropertyName = "equiv" )]
			public EquivData Equiv { get; set; }

			[JsonProperty( PropertyName = "html" )]
			public HtmlResult Html { get; set; }
		}

		public class HtmlResult
		{
			[JsonProperty( PropertyName = "org" )]
			public string Org { get; set; }

			[JsonProperty( PropertyName = "txn" )]
			public string Txn { get; set; }

			[JsonProperty( PropertyName = "nmt" )]
			public string Nmt { get; set; }
		}

		public class EquivData
		{
			[JsonProperty( PropertyName = "org" )]
			public List<List<object>> Org { get; set; }

			[JsonProperty( PropertyName = "txn" )]
			public List<List<object>> Txn { get; set; }
		}
	}
}
