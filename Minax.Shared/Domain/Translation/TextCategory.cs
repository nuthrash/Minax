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

		Character,
		FamilyName,
		MiddleName,
		NickName,
		AliasName,
		ReincarnatedName,
		OtherName,

		Organization, // PowerRanger, Empire...
		JobPosition, // Manager, BraveMan, PartyRole, nobility...
		Relation,

		Species,
		Animal,
		Monster,

		Material,
		Inventory,

		Food,
		Clothing,
		Housing,
		Transportation,
		Education,
		Entertainment,
		Music,

		HairStyle,
		Shoe,

		Facility,
		Building,
		Location,
		City,

		Action,
		Phenomenon,
		Property,
		Social,

		BodyOrgan,

		Color,

		Weapon,
		Maneuver, // 招式：Punch, Kick, Strike, Dodge
		Magic, // 咒文、魔法 spell
		Alchemy,

		Science, // Physics
		Book, // Novel, SF, Dictionary
		Game, // FPS, Car racing
		Language, // ビビット => 活潑＼生動＼Vivid
		Dialect,
		Translator, // 勇者 => 勇者 (勇者 => 勇敢的人 is not good in this project!!)
		MiscTerms, // SNS, TCP/IP, ラノベ(Light Novel)

		Verbs,
		Noun,
		Adjective,
		Adverb,
		Conjunction, // 連詞
		Determiners, // 限定詞
		Preposition, // 介詞
		Pronouns, // 代詞
	}
}
