using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Web.Formats
{
	public class PapagoFormat1
	{
		[JsonProperty( PropertyName = "errorCode" )]
		public string ErrorCode {
		 get; set;
		}

		[JsonProperty( PropertyName = "errorMessage" )]
		public string ErrorMessage {
			get; set;
		}

		/// <summary>
		/// Source language code
		/// </summary>
		[JsonProperty( PropertyName = "srcLangType" )]
		public string SrcLangType {
			get; set;
		}

		/// <summary>
		/// Translation result language code
		/// </summary>
		[JsonProperty( PropertyName = "tarLangType" )]
		public string TargetLangType {
			get; set;
		}

		/// <summary>
		/// Translated sentence
		/// </summary>
		[JsonProperty( PropertyName = "translatedText" )]
		public string TranslatedText {
			get; set;
		}

		[JsonProperty( PropertyName = "engineType" )]
		public string EngineType {
			get; set;
		}

		[JsonProperty( PropertyName = "pivot" )]
		public object Pivot {
			get; set;
		}

		[JsonProperty( PropertyName = "dict" )]
		public object Dict {
			get; set;
		}

		[JsonProperty( PropertyName = "tarDict" )]
		public object TargetDict {
			get; set;
		}

		[JsonProperty( PropertyName = "delay" )]
		public int Delay {
			get; set;
		}

		[JsonProperty( PropertyName = "delaySmt" )]
		public int DelaySmt {
			get; set;
		}

		[JsonProperty( PropertyName = "tlitSrc" )]
		public TlitSrc TlitSource {
			get; set;
		}

		[JsonProperty( PropertyName = "langDetection" )]
		public LangDetection LanguageDetection {
			get; set;
		}

		public class TlitResult
		{
			[JsonProperty( PropertyName = "token" )]
			public string Token {
				get; set;
			}

			[JsonProperty( PropertyName = "phoneme" )]
			public string Phoneme {
				get; set;
			}
		}

		public class TlitSrc
		{
			[JsonProperty( PropertyName = "message" )]
			public Message Message {
				get; set;
			}
		}

		public class LangDetection
		{
			[JsonProperty( PropertyName = "nbests" )]
			public List<Nbest> Nbests {
				get; set;
			}
		}

		public class Message
		{
			[JsonProperty( PropertyName = "tlitResult" )]
			public List<TlitResult> TlitResult {
				get; set;
			}
		}

		public class Nbest
		{
			[JsonProperty( PropertyName = "lang" )]
			public string Lang {
				get; set;
			}

			[JsonProperty( PropertyName = "prob" )]
			public double Prob {
				get; set;
			}
		}

	}
}
