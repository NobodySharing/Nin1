using System;
using System.Collections.Generic;
using System.Text;

namespace VPE
{
	public static class Codepage
	{
		/// <summary>Array of allowed chars, the codepage. The basis of everything. Char count MUST be divisible by 2, for paired tables.</summary>
		public static readonly string[] CharSet =
		{
			" ", // Ordinary space
			"A", // Uppercase latin letters, with diacritics.
			"Á",
			"Ä",
			"Ȁ",
			"Å",
			"Ā",
			"Ã",
			"Â",
			"Ă",
			"Æ",
			"B",
			"C",
			"Ć",
			"Č",
			"Ç",
			"D",
			"Ď",
			"Đ",
			"E",
			"Ė",
			"É",
			"Ě",
			"Ë",
			"Ȅ",
			"Ē",
			"Ê",
			"È",
			"Ę",
			"F",
			"G",
			"Ģ",
			"Ğ",
			"Ĝ",
			"Ġ",
			"H",
			"Ĥ",
			"Ħ",
			"I",
			"Í",
			"Ï",
			"Ī",
			"Ì",
			"İ",
			"Į",
			"Ĩ",
			"Ĳ",
			"J",
			"K",
			"Ķ",
			"L",
			"Ł",
			"Ļ",
			"M",
			"N",
			"Ň",
			"Ñ",
			"Ņ",
			"Ŋ",
			"O",
			"Ó",
			"Ö",
			"Ō",
			"Õ",
			"Ô",
			"Ò",
			"Ő",
			"Ø",
			"Œ",
			"P",
			"Q",
			"R",
			"Ř",
			"Ŗ",
			"S",
			"Š",
			"Ş",
			"Ș",
			"ẞ",
			"T",
			"Ť",
			"Ţ",
			"Ț",
			"Þ",
			"U",
			"Ú",
			"Ů",
			"Ü",
			"Ū",
			"Ù",
			"Ű",
			"V",
			"W",
			"Ŵ",
			"X",
			"Y",
			"Ý",
			"Ÿ",
			"Z",
			"Ż",
			"Ź",
			"Ž",
			"a", // Lowercase latin letters, with diacritics.
			"á",
			"ä",
			"ȁ",
			"å",
			"ā",
			"ã",
			"â",
			"ă",
			"æ",
			"b",
			"c",
			"ć",
			"č",
			"ç",
			"d",
			"ď",
			"đ",
			"e",
			"ė",
			"é",
			"ě",
			"ë",
			"ȅ",
			"ē",
			"ê",
			"è",
			"ę",
			"f",
			"g",
			"ģ",
			"ğ",
			"ĝ",
			"ġ",
			"h",
			"ĥ",
			"ħ",
			"i",
			"í",
			"ï",
			"ī",
			"ì",
			"ı",
			"į",
			"ĩ",
			"ĳ",
			"j",
			"k",
			"ķ",
			"l",
			"ł",
			"ļ",
			"m",
			"n",
			"ň",
			"ñ",
			"ņ",
			"ŋ",
			"o",
			"ó",
			"ö",
			"ō",
			"õ",
			"ô",
			"ò",
			"ő",
			"ø",
			"œ",
			"p",
			"q",
			"r",
			"ř",
			"ŗ",
			"s",
			"š",
			"ş",
			"ș",
			"ß",
			"t",
			"ť",
			"ţ",
			"ț",
			"þ",
			"u",
			"ú",
			"ů",
			"ü",
			"ū",
			"ù",
			"ű",
			"v",
			"w",
			"ŵ",
			"x",
			"y",
			"ý",
			"ÿ",
			"z",
			"ż",
			"ź",
			"ž",
			"Α", // Uppercase Greek letters, with diacritics.
			"Ά",
			"Β",
			"Γ",
			"Δ",
			"Ε",
			"Έ",
			"Ζ",
			"Η",
			"Ή",
			"Θ",
			"Ι",
			"Ί",
			"Ϊ",
			"Κ",
			"Λ",
			"Μ",
			"Ν",
			"Ξ",
			"Ο",
			"Ό",
			"Π",
			"Ρ",
			"Σ",
			"Τ",
			"Υ",
			"Ύ",
			"Ϋ",
			"Φ",
			"Χ",
			"Ψ",
			"Ω",
			"Ώ",
			"α", // Lowercase Greek letters, with diacritics.
			"ά",
			"β",
			"γ",
			"δ",
			"ε",
			"έ",
			"ζ",
			"η",
			"ή",
			"θ",
			"ι",
			"ί",
			"ϊ",
			"ΐ",
			"κ",
			"λ",
			"μ",
			"ν",
			"ξ",
			"ο",
			"ό",
			"π",
			"ρ",
			"σ",
			"ς",
			"τ",
			"υ",
			"ύ",
			"ϋ",
			"ΰ",
			"φ",
			"χ",
			"ψ",
			"ω",
			"ώ",
			"А", // Cyrilic, uppercase, all (I hope) variants.
			"Б",
			"В",
			"Г",
			"Ѓ",
			"Ґ",
			"Д",
			"Ђ",
			"Е",
			"Ё",
			"Э",
			"Ж",
			"Ӂ",
			"З",
			"И",
			"Й",
			"Ӣ",
			"Ӥ",
			"К",
			"Л",
			"М",
			"Н",
			"О",
			"П",
			"Р",
			"С",
			"Т",
			"У",
			"Ф",
			"Х",
			"Ц",
			"Ч",
			"Ш",
			"Щ",
			"Ъ",
			"Ы",
			"Ь",
			"Ю",
			"Я",
			"Ә",
			"Ғ",
			"Қ",
			"Ң",
			"Ө",
			"Ұ",
			"Ү",
			"Һ",
			"І",
			"Ј",
			"Љ",
			"Њ",
			"Ћ",
			"Џ",
			"а", // Cyrilic, lowercase, all (I hope) variants.
			"б",
			"в",
			"г",
			"ѓ",
			"ґ",
			"д",
			"ђ",
			"е",
			"ё",
			"э",
			"ж",
			"ӂ",
			"з",
			"и",
			"й",
			"ӣ",
			"ӥ",
			"к",
			"л",
			"м",
			"н",
			"о",
			"п",
			"р",
			"с",
			"т",
			"у",
			"ф",
			"х",
			"ц",
			"ч",
			"ш",
			"щ",
			"ъ",
			"ы",
			"ь",
			"ю",
			"я",
			"ә",
			"ғ",
			"қ",
			"ң",
			"ө",
			"ұ",
			"ү",
			"һ",
			"і",
			"ј",
			"љ",
			"њ",
			"ћ",
			"џ",
			"ა", // Georgian. Upper or lowercase, IDK. Maybe not even the full set.
			"ბ",
			"გ",
			"დ",
			"ე",
			"ვ",
			"ზ",
			"თ",
			"ი",
			"კ",
			"ლ",
			"მ",
			"ნ",
			"ო",
			"პ",
			"ჟ",
			"რ",
			"ს",
			"ტ",
			"უ",
			"ფ",
			"ქ",
			"ღ",
			"ყ",
			"შ",
			"ჩ",
			"ც",
			"ძ",
			"წ",
			"ჭ",
			"ხ",
			"ჯ",
			"ჰ",
			"0", // Digits.
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9",
			"⁰", // Sup-digits.
			"¹",
			"²",
			"³",
			"⁴",
			"⁵",
			"⁶",
			"⁷",
			"⁸",
			"⁹",
			"₀", // Sub-digits.
			"₁",
			"₂",
			"₃",
			"₄",
			"₅",
			"₆",
			"₇",
			"₈",
			"₉",
			"٠", // Arabic-Indic digits
			"١",
			"٢",
			"٣",
			"٤",
			"٥",
			"٦",
			"٧",
			"٨",
			"٩",
			"Ⅰ", // Roman numeral symbols.
			"Ⅱ",
			"Ⅲ",
			"Ⅳ",
			"Ⅴ",
			"Ⅵ",
			"Ⅶ",
			"Ⅷ",
			"Ⅸ",
			"Ⅹ",
			"Ⅺ",
			"Ⅻ",
			"Ⅼ",
			"Ⅽ",
			"Ⅾ",
			"Ⅿ",
			",", // Interpunction, parenthesis, symbols, currency symbols.
			".",
			"-",
			"?",
			"¿",
			"!",
			"¡",
			"_",
			":",
			";",
			"(",
			")",
			"[",
			"]",
			"{",
			"}",
			"<",
			">",
			"«",
			"»",
			"‹",
			"›",
			"↤",
			"↥",
			"↦",
			"↧",
			"∈",
			"∉",
			"∫",
			"⌈",
			"⌉",
			"⌊",
			"⌋",
			"–",
			"—",
			"…",
			"„",
			"“",
			"”",
			"/",
			"\\",
			"|",
			"\'",
			"\"",
			"%",
			"‰",
			"=",
			"+",
			"*",
			"^",
			"@",
			"#",
			"§",
			"€",
			"¥",
			"₹",
			"₽",
			"₩",
			"₿",
			"₦",
			"₲",
			"$",
			"£",
			"¢",
			"°",
			"÷",
			"×",
			"©",
			"®",
			"™",
			"≠",
			"≈",
			"⊂",
			"⊃",
			"⊆",
			"⊇",
			"←",
			"↑",
			"→",
			"↓",
			"↔",
			"↕",
			"•",
			"○",
			"√",
			"∞",
			"≤",
			"≥",
			"±",
			"~",
			"‼",
			"¶",
			"№",
			"✓",
			"✕",
			"⁈",
			"⁇",
			"′", // Minute symbol.
			"″", // Second symbol.
			"Ω", // Ohm sign, different from Greek letter.
			"µ", // Micro sign, different from Greek letter.
			"∆", // Absolute difference, different from Greek letter.
			"∂", // Relative difference, different from Greek letter.
			"∑", // Summation symbol, different from Greek letter.
			"∏", // Product symbol, different from Greek letter.
			" ", // Non-breaking space.
			" ", // Half-space.
			"\t", // Tab char.
			"\r\n", // New line. Done like this by design.
		};
		private static readonly ushort LimitV = Convert.ToUInt16(CharSet.Length);
		/// <summary>Converts text message to set of numbers using the codepage.</summary>
		/// <param name="text">Text message.</param>
		/// <returns>Numbers representing a message.</returns>
		public static List<ushort> ConvertToNums(string text)
		{
			List<ushort> result = new(text.Length);
			for (int i = 0; i < text.Length; i++)
			{
				char Ch = text[i];
				char N = i == (text.Length - 1) ? '\0' : text[i + 1];
				if (Ch == '\r')
				{
					if ((N == '\n') || (N == '\0'))
					{
						result.Add((ushort)Array.IndexOf(CharSet, "\r\n"));
						i++;
						continue;
					}
					else
					{
						result.Add((ushort)Array.IndexOf(CharSet, "\r\n"));
						continue;
					}
				}
				else if (Ch == '\n')
				{
					result.Add((ushort)Array.IndexOf(CharSet, "\r\n"));
					continue;
				}
				int index = Array.IndexOf(CharSet, Convert.ToString(Ch));
				if (index >= 0)
				{
					result.Add((ushort)index);
				}
				else
				{
					continue;
				}
			}
			return result;
		}
		/// <summary>Converts set of numbers to text message using the codepage.</summary>
		/// <param name="nums">Numbers representing a message.</param>
		/// <returns>Text message.</returns>
		public static string ConvertToString(List<ushort> nums)
		{
			StringBuilder result = new();
			foreach (ushort num in nums)
			{
				if (num < Limit)
				{
					result.Append(CharSet[num]);
				}
				else
				{
					result.Append('✳'); // Placeholder for unknown char.
				}
			}
			return result.ToString();
		}
		/// <summary>Converts the message to string containing the chars as their numerical representation.</summary>
		/// <param name="message">Meesage.</param>
		/// <returns>Comma separated chars as numbers.</returns>
		public static string ConvertToNumeric(string message)
		{
			StringBuilder sb = new();
			List<ushort> nums = ConvertToNums(message);
			sb.Append(nums[0]);
			for (int i = 1; i < nums.Count; i++)
			{
				sb.Append(", ");
				sb.Append(nums[i]);
			}
			return sb.ToString();
		}
		/// <summary>Converts the string containing the chars as their numerical representation to the message as string.</summary>
		/// <param name="message">Comma separated chars as numbers.</param>
		/// <returns>Meesage.</returns>
		public static string ConvertFromNumeric(string message)
		{
			string[] chars = message.Split(',');
			StringBuilder sb = new();
			foreach (string ch in chars)
			{
				if (ushort.TryParse(ch.Trim(), out ushort num))
				{
					if (num >= 0 && num < Limit)
					{
						sb.Append(CharSet[num]);
					}
				}
			}
			return sb.ToString();
		}
		/// <summary>Gets the number of chars in the codepage. Currently 576.</summary>
		public static ushort Limit
		{
			get
			{
				return LimitV;
			}
		}
	}
}