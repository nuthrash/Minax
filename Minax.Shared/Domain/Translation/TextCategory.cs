using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Domain.Translation
{
	/// <summary>
	/// Mapping text category
	/// </summary>
	public enum TextCategory
	{
		Undefined = 0,

		/// <summary>
		/// Forename, given name, or first name
		/// </summary>
		FirstName,
		/// <summary>
		/// Surname, last name, or family name
		/// </summary>
		FamilyName,
		MiddleName,
		NickName,
		AliasName,
		/// <summary>
		/// Before or after reincarnated name
		/// </summary>
		ReincarnatedName,
		/// <summary>
		/// Other name for un-classified name
		/// </summary>
		OtherName,

		Character,
		Machine,
		/// <summary>
		/// Ship or craft like starship, aircraft, etc.
		/// </summary>
		Ship,
		Car,

		/// <summary>
		/// Organization like PowerRanger, government, xxx empire
		/// </summary>
		Organization,
		/// <summary>
		/// Job position like manager, BraveMan, party role, nobility
		/// </summary>
		JobPosition,
		Relation,

		Species,
		//Race,
		Animal,
		Monster,

		Material,
		Inventory,
		Object,

		Food,
		Clothing,
		Housing,
		Transportation,
		Education,
		Entertainment,
		Music,

		HairStyle,
		/// <summary>
		/// Accessory like ring, ear ring, etc.
		/// </summary>
		Accessory,
		Shoe,

		Facility,
		Building,
		Location,
		City,

		Action,
		/// <summary>
		/// Phenomenon like gravity, weather, etc.
		/// </summary>
		Phenomenon,
		/// <summary>
		/// Attribute of something
		/// </summary>
		Property,
		/// <summary>
		/// Social related terms
		/// </summary>
		Social,

		BodyOrgan,

		Color,

		Weapon,
		/// <summary>
		/// Skill for fighting, like punch, kick, strike, dodge, etc.
		/// </summary>
		Maneuver,
		/// <summary>
		/// Skill for magic, mana, and conjuration
		/// </summary>
		Magic,
		Alchemy,

		/// <summary>
		/// Knowledge of Physics, Natural, etc.
		/// </summary>
		Science,
		/// <summary>
		/// Book category like novel, SF, dictionary
		/// </summary>
		Book,
		/// <summary>
		/// Game category like FPS, car racing, etc.
		/// </summary>
		Game,
		/// <summary>
		/// Source language related
		/// </summary>
		Language,
		/// <summary>
		/// Dialect about regional language
		/// </summary>
		Dialect,
		/// <summary>
		/// Translator-related mapping to avoid mis-translating
		/// </summary>
		/// <example>勇者 => 勇者 (勇者 => 勇敢的人 is not good when translated by some Translators!!)</example>
		Translator,
		/// <summary>
		/// Other terms, like SNS, TCP/IP, ラノベ(Light Novel)
		/// </summary>
		MiscTerms,

		Verbs,
		Noun,
		Adjective,
		Adverb,
		Conjunction,
		Determiners,
		Preposition,
		Pronouns,
	}

	public static class TextCategoryExtensions
	{
		public static string ToL10nString( this TextCategory category )
		{
			switch( category ) {
				case TextCategory.FirstName:
					return Languages.TextCategory.Str0FirstName;
				case TextCategory.FamilyName:
					return Languages.TextCategory.Str0FamilyName;
				case TextCategory.MiddleName:
					return Languages.TextCategory.Str0MiddleName;
				case TextCategory.NickName:
					return Languages.TextCategory.Str0NickName;
				case TextCategory.AliasName:
					return Languages.TextCategory.Str0AliasName;
				case TextCategory.ReincarnatedName:
					return Languages.TextCategory.Str0ReincarnatedName;
				case TextCategory.OtherName:
					return Languages.TextCategory.Str0OtherName;

				case TextCategory.Character:
					return Languages.TextCategory.Str0Character;
				case TextCategory.Machine:
					return Languages.TextCategory.Str0Machine;
				case TextCategory.Ship:
					return Languages.TextCategory.Str0Ship;
				case TextCategory.Car:
					return Languages.TextCategory.Str0Car;

				case TextCategory.Organization:
					return Languages.TextCategory.Str0Organization;
				case TextCategory.JobPosition:
					return Languages.TextCategory.Str0JobPosition;
				case TextCategory.Relation:
					return Languages.TextCategory.Str0Relation;

				case TextCategory.Species:
					return Languages.TextCategory.Str0Species;
				//case TextCategory.Race:
				//return Languages.TextCategory.Str0Race;
				case TextCategory.Animal:
					return Languages.TextCategory.Str0Animal;
				case TextCategory.Monster:
					return Languages.TextCategory.Str0Monster;

				case TextCategory.Material:
					return Languages.TextCategory.Str0Material;
				case TextCategory.Inventory:
					return Languages.TextCategory.Str0Inventory;
				case TextCategory.Object:
					return Languages.TextCategory.Str0Object;

				case TextCategory.Food:
					return Languages.TextCategory.Str0Food;
				case TextCategory.Clothing:
					return Languages.TextCategory.Str0Clothing;
				case TextCategory.Housing:
					return Languages.TextCategory.Str0Housing;
				case TextCategory.Transportation:
					return Languages.TextCategory.Str0Transportation;
				case TextCategory.Education:
					return Languages.TextCategory.Str0Education;
				case TextCategory.Entertainment:
					return Languages.TextCategory.Str0Entertainment;
				case TextCategory.Music:
					return Languages.TextCategory.Str0Music;

				case TextCategory.HairStyle:
					return Languages.TextCategory.Str0HairStyle;
				case TextCategory.Accessory:
					return Languages.TextCategory.Str0Accessory;
				case TextCategory.Shoe:
					return Languages.TextCategory.Str0Shoe;

				case TextCategory.Facility:
					return Languages.TextCategory.Str0Facility;
				case TextCategory.Building:
					return Languages.TextCategory.Str0Building;
				case TextCategory.Location:
					return Languages.TextCategory.Str0Location;
				case TextCategory.City:
					return Languages.TextCategory.Str0City;

				case TextCategory.Action:
					return Languages.TextCategory.Str0Action;
				case TextCategory.Phenomenon:
					return Languages.TextCategory.Str0Phenomenon;
				case TextCategory.Property:
					return Languages.TextCategory.Str0Property;
				case TextCategory.Social:
					return Languages.TextCategory.Str0Social;

				case TextCategory.BodyOrgan:
					return Languages.TextCategory.Str0BodyOrgan;

				case TextCategory.Color:
					return Languages.TextCategory.Str0Color;

				case TextCategory.Weapon:
					return Languages.TextCategory.Str0Weapon;
				case TextCategory.Maneuver:
					return Languages.TextCategory.Str0Maneuver;
				case TextCategory.Magic:
					return Languages.TextCategory.Str0Magic;
				case TextCategory.Alchemy:
					return Languages.TextCategory.Str0Alchemy;

				case TextCategory.Science:
					return Languages.TextCategory.Str0Science;
				case TextCategory.Book:
					return Languages.TextCategory.Str0Book;
				case TextCategory.Game:
					return Languages.TextCategory.Str0Game;
				case TextCategory.Language:
					return Languages.TextCategory.Str0Language;
				case TextCategory.Dialect:
					return Languages.TextCategory.Str0Dialect;
				case TextCategory.Translator:
					return Languages.TextCategory.Str0Translator;
				case TextCategory.MiscTerms:
					return Languages.TextCategory.Str0MiscTerms;

				case TextCategory.Verbs:
					return Languages.TextCategory.Str0Verbs;
				case TextCategory.Noun:
					return Languages.TextCategory.Str0Noun;
				case TextCategory.Adjective:
					return Languages.TextCategory.Str0Adjective;
				case TextCategory.Adverb:
					return Languages.TextCategory.Str0Adverb;
				case TextCategory.Conjunction:
					return Languages.TextCategory.Str0Conjunction;
				case TextCategory.Determiners:
					return Languages.TextCategory.Str0Determiners;
				case TextCategory.Preposition:
					return Languages.TextCategory.Str0Preposition;
				case TextCategory.Pronouns:
					return Languages.TextCategory.Str0Pronouns;


				case TextCategory.Undefined:
				default:
					return Languages.TextCategory.Str0Undefined;
			}
		}

	}
}
