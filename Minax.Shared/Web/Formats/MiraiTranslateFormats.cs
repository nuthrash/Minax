using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	public class MiraiTranslateFormat1
	{
		public const string StatusSuccess = "success";

		[JsonProperty( PropertyName = "status" )]
		public string Status {
			get; set;
		}

		[JsonProperty( PropertyName = "error_msg" )]
		public string ErrorMessage {
			get; set;
		}

		[JsonProperty( PropertyName = "outputs" )]
		public List<OutputResult> Outputs {
			get; set;
		}

		public class OutputResult
		{
			[JsonProperty( PropertyName = "output" )]
			public List<Output> Results {
				get; set;
			}
		}

		public class Output
		{
			// composited translate text with all sentences
			[JsonProperty( PropertyName = "translation" )]
			public string Translation {
				get; set;
			}

			[JsonProperty( PropertyName = "sentences" )]
			public List<Sentence> Sentences {
				get; set;
			}
		}

		public class Sentence
		{
			[JsonProperty( PropertyName = "original" )]
			public string Original {
				get; set;
			}

			[JsonProperty( PropertyName = "originalPosition" )]
			public int OriginalPosition {
				get; set;
			}

			[JsonProperty( PropertyName = "originalLength" )]
			public int OriginalLength {
				get; set;
			}

			// single translate text of this sentence
			[JsonProperty( PropertyName = "translation" )]
			public string Translation {
				get; set;
			}

			[JsonProperty( PropertyName = "translationPosition" )]
			public int TranslationPosition {
				get; set;
			}

			[JsonProperty( PropertyName = "translationLength" )]
			public int TranslationLength {
				get; set;
			}

			[JsonProperty( PropertyName = "type" )]
			public string Type {
				get; set;
			}

			[JsonProperty( PropertyName = "target" )]
			public string Target {
				get; set;
			}

			[JsonProperty( PropertyName = "prefix" )]
			public string Prefix {
				get; set;
			}

			[JsonProperty( PropertyName = "boundary" )]
			public List<int> Boundary {
				get; set;
			}

			[JsonProperty( PropertyName = "originalDelimiter" )]
			public string OriginalDelimiter {
				get; set;
			}

			[JsonProperty( PropertyName = "delimiter" )]
			public string Delimiter {
				get; set;
			}

			[JsonProperty( PropertyName = "words" )]
			public List<object> Words {
				get; set;
			}

			[JsonProperty( PropertyName = "sentences" )]
			public List<object> Sentences {
				get; set;
			}
		}

	
	}
}
