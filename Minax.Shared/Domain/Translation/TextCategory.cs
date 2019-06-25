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
}
