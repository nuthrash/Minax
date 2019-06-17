using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	public class GoogleTranslatorFormat1
	{
		[JsonProperty( PropertyName = "sentences" )]
		public List<SentenceEntry> Sentences { get; set; } = new List<SentenceEntry>();

		[JsonProperty( PropertyName = "src" )]
		public string Source { get; set; } // like "ja"

		[JsonProperty( PropertyName = "confidence" )]
		public int Confidence { get; set; }

		[JsonProperty( PropertyName = "ld_result" )]
		public LdResultData LdResult { get; set; }

		public class LdResultData
		{
			[JsonProperty( PropertyName = "srclangs" )]
			public List<string> SourceLanguages { get; set; }

			[JsonProperty( PropertyName = "srclangs_confidences" )]
			public List<int> SourceLanguagesConfidences { get; set; }

			[JsonProperty( PropertyName = "extended_srclangs" )]
			public List<string> ExtendedSourceLanguages { get; set; }
		}

		public class SentenceEntry
		{
			[JsonProperty( PropertyName = "trans" )]
			public string Translated { get; set; }

			[JsonProperty( PropertyName = "orig" )]
			public string Original { get; set; }

			[JsonProperty( PropertyName = "backend" )]
			public int Backend { get; set; }
		}

	}


	public class GoogleCloudTranslationRequestV2
	{
		/// <summary>
		/// A valid API key to handle requests for this API.
		/// </summary>
		/// <remarks>If you are using OAuth 2.0 service account credentials (recommended), do not supply this parameter. </remarks>
		[JsonProperty( PropertyName = "key" )]
		public string ApiKey { get; set; }

		/// <summary>
		/// Required The input text to translate. Repeat this parameter to perform translation operations on multiple text inputs. 
		/// </summary>
		[JsonProperty( PropertyName = "q" )]
		public List<string> Query { get; set; }

		/// <summary>
		/// Required The language to use for translation of the input text, set to one of the language codes listed in Language Support. 
		/// </summary>
		[JsonProperty( PropertyName = "target" )]
		public string TargetLanguage { get; set; }

		/// <summary>
		/// The language of the source text, set to one of the language codes listed in Language Support.
		/// If the source language is not specified, the API will attempt to detect the source language automatically and return it within the response. 
		/// </summary>
		[JsonProperty( PropertyName = "source" )]
		public string SourceLanguage { get; set; }

		/// <summary>
		/// The format of the source text, in either HTML or plain-text (default).
		/// A value of html indicates HTML and a value of text indicates plain-text. 
		/// </summary>
		[JsonProperty( PropertyName = "format" )]
		public string Format { get; set; } = "text";

		/// <summary>
		/// The translation model. Can be either base to use the Phrase-Based Machine Translation (PBMT) model,
		/// or nmt to use the Neural Machine Translation (NMT) model. If omitted, then nmt is used.
		///	If the model is nmt, and the requested language translation pair is not supported for the NMT model,
		///	then the request is translated using the base model.
		/// </summary>
		[JsonProperty( PropertyName = "model" )]
		public string Model { get; set; } = "nmt"; // or base

	}

	public class GoogleCloudTranslationResponseV2
	{
		[JsonProperty( PropertyName = "data" )]
		public DataContent Data { get; set; }

		public class DataContent
		{
			[JsonProperty( PropertyName = "translations" )]
			public List<Translation> Translations { get; set; }
		}

		public class Translation
		{
			[JsonProperty( PropertyName = "translatedText" )]
			public string TranslatedText { get; set; }

			[JsonProperty( PropertyName = "detectedSourceLanguage" )]
			public string DetectedSourceLanguage { get; set; }

			[JsonProperty( PropertyName = "model" )]
			public string Model { get; set; }
		}
	}
}
