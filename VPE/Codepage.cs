using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Documents;

namespace VPE
{
	public static class Codepage
	{
		/// <summary>Array of allowed chars, the codepage. The basis of everything. Char count MUST be divisible by 2, for paired tables. Currently 720.</summary>
		public static readonly string[] CharSet =
		[
			" ", // Ordinary space
			// Uppercase latin letters, with diacritics.
			"A", "Á", "Ä", "À", "Å", "Ā", "Ã", "Â", "Ă", "Ą", "Ǻ", "Æ", "Ǽ", "B", "C", "Ć", "Č", "Ç", "Ċ", "D", "Ď", "Đ", "Ð", "ʤ", "E", "Ė", "É", "Ě", "Ë", "Ē", "Ê", "È", "Ę", "Ȅ", "Ə", "F", "G", "Ģ", "Ğ", "Ĝ", "Ġ", "H", "Ĥ", "Ħ", "I", "Í", "Ï", "Ī", "Ì", "İ", "Į", "Ĩ", "Î", "Ȉ", "Ĳ", "J", "K", "Ķ", "L", "Ł", "Ļ", "Ľ", "Ĺ", "Ŀ", "M", "N", "Ň", "Ñ", "Ņ", "Ŋ", "Ń", "ǋ", "O", "Ó", "Ö", "Ō", "Õ", "Ô", "Ò", "Ő", "Ø", "Ǿ", "Œ", "P", "Q", "R", "Ř", "Ŗ", "Ŕ", "S", "Š", "Ş", "Ș", "Ś", "ẞ", "T", "Ť", "Ţ", "Ț", "Ŧ", "Þ", "U", "Ú", "Ů", "Ü", "Ū", "Ù", "Ű", "Û", "Ų", "V", "W", "Ŵ", "Ẅ", "Ƿ", "X", "Y", "Ý", "Ÿ", "Ŷ", "Z", "Ż", "Ź", "Ž", "Ƶ",
			// Lowercase latin letters, with diacritics.
			"a", "á", "ä", "à", "å", "ā", "ã", "â", "ă", "ą", "ǻ", "æ", "ǽ", "b", "c", "ć", "č", "ç", "ċ", "d", "ď", "đ", "ð", "ʣ", "e", "ė", "é", "ě", "ë", "ē", "ê", "è", "ę", "ȅ", "ə", "f", "g", "ģ", "ğ", "ĝ", "ġ", "h", "ĥ", "ħ", "i", "í", "ï", "ī", "ì", "ı", "į", "ĩ", "î", "ȉ", "ĳ", "j", "k", "ķ", "l", "ł", "ļ", "ľ", "ĺ", "ŀ", "m", "n", "ň", "ñ", "ņ", "ŋ", "ń", "ǌ", "o", "ó", "ö", "ō", "õ", "ô", "ò", "ő", "ø", "ǿ", "œ", "p", "q", "r", "ř", "ŗ", "ŕ", "s", "š", "ş", "ș", "ś", "ß", "t", "ť", "ţ", "ț", "ŧ", "þ", "u", "ú", "ů", "ü", "ū", "ù", "ű", "û", "ų", "v", "w", "ŵ", "ẅ", "ƿ", "x", "y", "ý", "ÿ", "ŷ", "z", "ż", "ź", "ž", "ƶ",
			// Uppercase Greek letters, with diacritics.
			"Α", "Ά", "Β", "Γ", "Δ", "Ε", "Έ", "Ζ", "Η", "Ή", "Θ", "Ι", "Ί", "Ϊ", "Κ", "Λ", "Μ", "Ν", "Ξ", "Ο", "Ό", "Π", "Ρ", "Σ", "Τ", "Υ", "Ύ", "Ϋ", "Φ", "Χ", "Ψ", "Ω", "Ώ",
			// Lowercase Greek letters, with diacritics.
			"α", "ά", "β", "γ", "δ", "ε", "έ", "ζ", "η", "ή", "θ", "ι", "ί", "ϊ", "ΐ", "κ", "λ", "μ", "ν", "ξ", "ο", "ό", "π", "ρ", "σ", "ς", "τ", "υ", "ύ", "ϋ", "ΰ", "φ", "χ", "ψ", "ω", "ώ",
			// Cyrilic, uppercase, all (I hope) variants.
			"А", "Ӑ", "Ӓ", "Б", "В", "Г", "Ѓ", "Ґ", "Д", "Ђ", "Е", "Ё", "Э", "Ж", "Ӂ", "З", "И", "Й", "Ӣ", "Ӥ", "К", "Л", "М", "Н", "О", "Ӧ", "П", "Р", "С", "Т", "У", "Ӱ", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Ю", "Я", "Ә", "Ғ", "Қ", "Ң", "Ө", "Ұ", "Ү", "Һ", "І", "Ј", "Љ", "Њ", "Ћ", "Џ",
			// Cyrilic, lowercase, all (I hope) variants.
			"а", "ӑ", "ӓ", "б", "в", "г", "ѓ", "ґ", "д", "ђ", "е", "ё", "э", "ж", "ӂ", "з", "и", "й", "ӣ", "ӥ", "к", "л", "м", "н", "о", "ӧ", "п", "р", "с", "т", "у", "ӱ", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "ю", "я", "ә", "ғ", "қ", "ң", "ө", "ұ", "ү", "һ", "і", "ј", "љ", "њ", "ћ", "џ",
			// Georgian. Upper or lowercase, IDK. Maybe not even the full set.
			"ა", "ბ", "გ", "დ", "ე", "ვ", "ზ", "თ", "ი", "კ", "ლ", "მ", "ნ", "ო", "პ", "ჟ", "რ", "ს", "ტ", "უ", "ფ", "ქ", "ღ", "ყ", "შ", "ჩ", "ც", "ძ", "წ", "ჭ", "ხ", "ჯ", "ჰ",
			// Armenian, upercase
			"Ա", "Բ", "Գ", "Դ", "Ե", "Զ", "Է", "Ը", "Թ", "Ժ", "Ի", "Լ", "Խ", "Ծ", "Կ", "Հ", "Ձ", "Ղ", "Ճ", "Մ", "Յ", "Ն", "Շ", "Ո", "Չ", "Պ", "Ջ", "Ռ", "Ս", "Վ", "Տ", "Ր", "Ց", "Ւ", "Փ", "Ք", "Օ", "Ֆ",
			// Armenian, lowercase
			"ա", "բ", "գ", "դ", "ե", "զ", "է", "ը", "թ", "ժ", "ի", "լ", "խ", "ծ", "կ", "հ", "ձ", "ղ", "ճ", "մ", "յ", "ն", "շ", "ո", "չ", "պ", "ջ", "ռ", "ս", "վ", "տ", "ր", "ց", "ւ", "փ", "ք", "օ", "ֆ",
			// Digits, different variants.
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "⁰", "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹", "₀", "₁", "₂", "₃", "₄", "₅", "₆", "₇", "₈", "₉", "⓪", "①", "②", "③", "④", "⑤", "⑥", "⑦", "⑧", "⑨", "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ", "Ⅹ", "Ⅺ", "Ⅻ", "Ⅼ", "Ⅽ", "Ⅾ", "Ⅿ",
			// Interpunction, parenthesis, symbols, currency symbols.
			",", ".", "-", "?", "¿", "!", "¡", "_", ":", ";", "(", ")", "[", "]", "{", "}", "<", ">", "«", "»", "‹", "›", "∈", "∉", "∫", "⌈", "⌉", "⌊", "⌋", "–", "—", "…", "„", "“", "”", "/", "\\", "|", "\'", "\"", "%", "‰", "=", "+", "*", "^", "@", "#", "§", "€", "¥", "₹", "₽", "₩", "₿", "₦", "₲", "$", "£", "₣", "₱", "¢", "°", "÷", "×", "©", "®", "™", "≠", "≈", "←", "↑", "→", "↓", "⭦", "⭧", "⭨", "⭩", "↔", "↕", "•", "○", "√", "∞", "≤", "≥", "±", "~", "‼", "⁈", "⁇", "¶", "✓", "✕", "☢", "☣", "∛", "∜", "¦", "□", "○", "△", "∀", "∃", "∐", "Ǯ", "ǯ", "†",
			"’", // Apostophe symbol. Different from minute symbol and '.
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
		];
		private static readonly ushort LimitV = Convert.ToUInt16(CharSet.Length);
		/// <summary>Gets the number of chars in the codepage. Currently 720, which is highly composite number. Chosen by design.</summary>
		public static ushort Limit
		{
			get
			{
				return LimitV;
			}
		}
		/// <summary>Charset used for basse-128 encoding. Aimed at maximum compatibility for different sites.</summary>
		private static readonly char[] Base128 =
		[
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'Á', 'Ä', 'Æ', 'B', 'C', 'Č', 'D', 'Ď', 'E', 'É', 'Ě', 'Ë', 'F', 'G', 'Ğ', 'H', 'I', 'Í', 'J', 'K', 'L', 'Ł', 'Ľ', 'M', 'N', 'Ň', 'Ñ', 'O', 'Ó', 'Ö', 'P', 'Q', 'R', 'Ř', 'S', 'Š', 'T', 'Ť', 'U', 'Ú', 'Ů', 'Ü', 'V', 'W', 'X', 'Y', 'Ý', 'Z', 'Ž',
			'a', 'á', 'ä', 'æ', 'b', 'c', 'č', 'd', 'ď', 'e', 'é', 'ě', 'ë', 'f', 'g', 'ğ', 'h', 'i', 'í', 'j', 'k', 'l', 'ł', 'ľ', 'm', 'n', 'ň', 'ñ', 'o', 'ó', 'ö', 'p', 'q', 'r', 'ř', 's', 'š', 't', 'ť', 'u', 'ú', 'ů', 'ü', 'v', 'w', 'x', 'y', 'ý', 'z', 'ž',
			',', '.', '-', '?', '!', '_', ':', ';', '(', ')', '~', '°', '=', '+', '«', '»', '^', '€'
		];
		/// <summary>Specifies the encrypted type. Either as characters, or base-128, or in binary, either without or with dashes.</summary>
		public enum Encoding
		{
			Text = 0,
			Base64,
			Base128,
			Base128WithSpaces,
			Binary,
			BinaryWithDashes
		}
		/// <summary>Encodes a message to specified encoding, text output only.</summary>
		/// <param name="message">Message to encode, as list of ushorts.</param>
		/// <param name="encoding">Which encoding?</param>
		/// <returns>Text representation of encoded message.</returns>
		public static string Encode (List<ushort> message, Encoding encoding)
		{
			string result = "";
			switch (encoding)
			{
				case Encoding.Text:
					result = ConvertToString(message);
					break;
				case Encoding.Base64:
					byte error = TightlyPackToBytes(message, out byte[] encoded, false);
					if (error == 0)
					{
						result = Convert.ToBase64String(encoded);
					}
					break;
				case Encoding.Base128:
					error = TightlyPackToBytes(message, out encoded, true);
					if (error == 0)
					{
						result = ConvertToBase128Text(encoded, false);
					}
					break;
				case Encoding.Base128WithSpaces:
					error = TightlyPackToBytes(message, out encoded, true);
					if (error == 0)
					{
						result = ConvertToBase128Text(encoded, true);
					}
					break;
				case Encoding.Binary:
					error = TightlyPackToBytes(message, out encoded, false);
					if (error == 0)
					{
						result = BitConverter.ToString(encoded).Replace("-", "");
					}
					break;
				case Encoding.BinaryWithDashes:
					error = TightlyPackToBytes(message, out encoded, false);
					if (error == 0)
					{
						result = BitConverter.ToString(encoded);
					}
					break;
			}
			return result;
		}
		/// <summary>Encodes a message to a byte array, for writing as a file.</summary>
		/// <param name="message">Message to encode, as list of ushorts.</param>
		/// <returns>Message as a byte array.</returns>
		public static byte[] Encode(List<ushort> message)
		{
			byte error = TightlyPackToBytes(message, out byte[] encoded, false);
			if (error == 0)
			{
				return encoded;
			}
			return [];
		}
		/// <summary>Decodes a message in specified encoding, text input only.</summary>
		/// <param name="message">Message to encode, as a text.</param>
		/// <param name="encoding">Which encoding?</param>
		/// <returns>List of ushorts representing decoded message.</returns>
		public static List<ushort> Decode(string message, Encoding encoding)
		{
			List<ushort> result = [];
			switch (encoding)
			{
				case Encoding.Text:
					result = ConvertToNums(message);
					break;
				case Encoding.Base64:
					byte[] data = Convert.FromBase64String(message);
					byte error = UnpackTightlyPackedBytes(data, out List<ushort> temp, false);
					if (error == 0)
					{
						result = temp;
					}
					break;
				case Encoding.Base128:
					data = ConvertFromBase128Text(message);
					error = UnpackTightlyPackedBytes(data, out temp, true);
					if (error == 0)
					{
						result = temp;
					}
					break;
				case Encoding.Base128WithSpaces:
					message = message.Replace(" ", "");
					data = ConvertFromBase128Text(message);
					error = UnpackTightlyPackedBytes(data, out temp, true);
					if (error == 0)
					{
						result = temp;
					}
					break;
				case Encoding.Binary:
					data = Convert.FromHexString(message);
					error = UnpackTightlyPackedBytes(data, out temp, false);
					if (error == 0)
					{
						result = temp;
					}
					break;
				case Encoding.BinaryWithDashes:
					data = Convert.FromHexString(message.Replace("-", ""));
					error = UnpackTightlyPackedBytes(data, out temp, false);
					if (error == 0)
					{
						result = temp;
					}
					break;
			}
			return result;
		}
		/// <summary>Decodes a message in specified encoding, binary input only.</summary>
		/// <param name="binaryFile">Message as a byte array.</param>
		/// <returns>List of ushorts representing decoded message.</returns>
		public static List<ushort> Decode(byte[] binaryFile)
		{
			byte error = UnpackTightlyPackedBytes(binaryFile, out List<ushort> decoded, false);
			if (error == 0)
			{
				return decoded;
			}
			return [];
		}
		/// <summary>Converts text message to set of numbers using the codepage.</summary>
		/// <param name="text">Text message.</param>
		/// <returns>Numbers representing a message.</returns>
		private static List<ushort> ConvertToNums(string text)
		{
			List<ushort> result = new(text.Length);
			for (int i = 0; i < text.Length; i++)
			{
				char Ch = text[i];
				char N = i == (text.Length - 1) ? '\0' : text[i + 1];
				if (Ch == '\r')
				{
					result.Add((ushort)Array.IndexOf(CharSet, "\r\n"));
					i = (N == '\n') || (N == '\0') ? i + 1 : i;
					continue;
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
		private static string ConvertToString(List<ushort> nums)
		{
			StringBuilder result = new();
			foreach (ushort num in nums)
			{
				if (num < Limit)
				{
					result.Append(CharSet[num]);
				}
				else
				{ // ToDo: Throw some kind of error when invalid char is present.
					result.Append('✳'); // Placeholder for unknown char.
				}
			}
			return result.ToString();
		}
		/// <summary>Tightly packs a set of numbers. No unused bits will remain.</summary>
		/// <param name="nums">Numbers to pack.</param>
		/// <param name="result">Tightly packed numbers, in bytes. Or base-128 numbers.</param>
		/// <param name="base128">Encode to base-128 numbers?</param>
		/// <returns>Error code. 0 = OK, 1 = nums is null, 2 = nums is empty, 3 = codepage has less than 32 chars (too little).</returns>
		private static byte TightlyPackToBytes(List<ushort> nums, out byte[] result, bool base128)
		{
			if (nums is null)
			{
				result = [];
				return 1;
			}
			if (nums.Count == 0)
			{
				result = [];
				return 2;
			}
			double dataBitLength = Math.Log(Limit, 2);
			double flooredLength = Math.Floor(dataBitLength);
			if (flooredLength < 6)
			{ // This is a limit for minimum codepage size (64 chars at least), but this should not be triggered, since the check for custom codepage should be done somewhere else.
				result = [];
				return 3;
			}
			List<uint> converted = [];
			byte flags = 0; // Set of flags providing context info: 7 – 2: unused, 1: is the last number within Limit (non-combined)? 0: are all the numbers combined (except the last, that has special flag)?
			if ((dataBitLength - flooredLength) < 0.5)
			{ // If I need less than N,5 bits (and more than N,0), then I combine 2 numbers together.
				for (int i = 0; i < (nums.Count / 2 - 1); i++)
				{
					converted.Add((uint)nums[i * 2] * Limit + nums[i * 2 + 1]);
				}
				if ((converted.Count & 1) == 1)
				{ // If there is an odd number of original ushort, the last one is treated specially.
					converted.Add(nums.Last());
					flags |= 0x02;
				}
				flags |= 0x01;
			}
			else
			{
				converted = nums.ConvertAll(a => (uint)a);
			}
			result = base128 ? PackTo7BitBytes(converted, dataBitLength, flags) : PackTo8BitBytes(converted, dataBitLength, flags);
			return 0;
		}
		/// <summary>Unpacks tightly packed bytes into usable data (message).</summary>
		/// <param name="data">Data to unpack.</param>
		/// <param name="result">Unpacked data (message).</param>
		/// <param name="base128">Decode as base-128 numbers?</param>
		/// <returns>Error code.</returns>
		private static byte UnpackTightlyPackedBytes(byte[] data, out List<ushort> result, bool base128)
		{
			if (data is null)
			{
				result = [];
				return 1;
			}
			if (data.Length < 3)
			{ // Must be at least 3 bytes. 2 for length and 1 for data. Usually should be much longer than that.
				result = [];
				return 2;
			}
			ushort dataMax = Helpers.ToUInt16(data, 0);
			if (dataMax != Limit)
			{ // For now, I don't know how to handle different sizes of codepage. It should be the same. If not, nothing will work.
				result = [];
				return 3;
			}
			double dataBitLength = Math.Log(dataMax, 2);
			double flooredLength = Math.Floor(dataBitLength);
			if (flooredLength < 6)
			{ // This is a limit for minimum codepage size (64 chars at least), but this should not be triggered, since the check for custom codepage should be done somewhere else.
				result = [];
				return 4;
			}
			Span<byte> pureData = new(data, 2, data.Length - 2);
			List<uint> decoded = base128 ? UnpackFrom7BitBytes(pureData, dataBitLength, flooredLength) : UnpackFrom8BitBytes(pureData, dataBitLength, flooredLength);
			if((dataBitLength - flooredLength) < 0.5)
			{
				result = [];
				foreach (uint duo in decoded)
				{
					ushort b = (ushort)(duo % Limit);
					ushort a = (ushort)((duo - b) / Limit);
					result.AddRange([a, b]);
				}
			}
			else
			{
				result = decoded.ConvertAll(a => (ushort)a);
			}
			return 0;
		}
		/// <summary>Packs prepared data to 8bit bytes, for universal use as byte array.</summary>
		/// <param name="data">Prepared data to pack.</param>
		/// <param name="dataBitLength">What is the bitlength of non-combined data elements (ushorts)?</param>
		/// <param name="flags">Set of flags providing context info: 7 – 2: unused, 1: is the last number within Limit (non-combined)? 0: are all the numbers combined (except the last, that has special flag)?</param>
		/// <returns>Array of 8bit bytes, for binary output.</returns>
		private static byte[] PackTo8BitBytes(List<uint> data, double dataBitLength, byte flags)
		{
			const byte bitsInByte = 8;
			const byte fullByte = 0xFF;
			byte realDataBitLength = (flags & 0x01) == 1 ? (byte)Math.Ceiling(dataBitLength * 2) : (byte)Math.Ceiling(dataBitLength);
			byte[] result = new byte[(int)Math.Ceiling((double)(realDataBitLength * data.Count) / bitsInByte)];
			Helpers.GetBytes(Limit).CopyTo(result, 0);
			uint position = 2;
			if (realDataBitLength < bitsInByte)
			{ // 6, 7.
				foreach (uint number in data)
				{

				}
			}
			else if (realDataBitLength % bitsInByte == 0)
			{ // 8, 16, but never 24 or 32.
				if (realDataBitLength == bitsInByte)
				{ // 8.
					data.ConvertAll(new Converter<uint, byte>(UintToSingleByte)).CopyTo(result, (int)position);
					data.ConvertAll(a => (byte)a).CopyTo(result, (int)position);
				}
				else
				{ // 16.
					foreach (uint number in data)
					{
						byte[] bytes = Helpers.GetBytes(number);
						result[position] = bytes[2];
						position++;
						result[position] = bytes[3];
						position++;
					}
				}
			}
			else
			{ // 9 - 15, 17, 19, …, 31. Always odd.
				foreach (uint number in data)
				{

				}
			}
			return result;
		}
		/// <summary>Packs data to bytes 7 bits long, for conversion to base-128.</summary>
		/// <param name="data">Data to pack.</param>
		/// <param name="dataBitLength">What is the bitlength of non-combined data elements (ushorts)?</param>
		/// <param name="flags">Set of flags providing context info: 7 – 2: unused, 1: is the last number within Limit (non-combined)? 0: are all the numbers combined (except the last, that has special flag)?</param>
		/// <returns>Array of 7bit bytes, for base-128 conversion.</returns>
		private static byte[] PackTo7BitBytes(List<uint> data, double dataBitLength, byte flags)
		{
			const byte bitsInByte = 7;
			const byte fullByte = 0x7F;
			byte realDataBitLength = (flags & 0x01) == 1 ? (byte)Math.Ceiling(dataBitLength * 2) : (byte)Math.Ceiling(dataBitLength);
			byte[] result = new byte[(int)Math.Ceiling((double)(realDataBitLength * data.Count) / bitsInByte)];
			result[0] = (byte)((byte)(Limit >> 9) & fullByte);
			result[1] = (byte)((byte)(Limit >> 2) & fullByte);
			result[2] = (byte)((byte)(Limit << 5) & fullByte);
			uint position = 2;
			if (realDataBitLength < bitsInByte)
			{ // 6.
				foreach (uint number in data)
				{

				}
			}
			else if (realDataBitLength == bitsInByte)
			{ // 7.
				data.ConvertAll(new Converter<uint, byte>(UintToSingleByte)).CopyTo(result, (int)position);
			}
			else
			{ // 8 - 17, 19, 21, …, 31. Always odd.
				foreach (uint number in data)
				{

				}
			}
			return result;
		}
		/// <summary>Unpacks data from 8bit bytes to a list of ushorts, which represent symbols.</summary>
		/// <param name="data">8bit bytes to unpack.</param>
		/// <param name="dataBitLength">What is the bitlength of non-combined data elements (ushorts)?</param>
		/// <param name="flooredLength">Just floored version of dataBitLength.</param>
		/// <returns>List of ushorts, which represent symbols in codepage.</returns>
		private static List<uint> UnpackFrom8BitBytes(Span<byte> data, double dataBitLength, double flooredLength)
		{
			List<uint> result = [];
			const int bitsInByte = 8;
			const int fullByte = 0xFF;
			byte realDataBitLength = (dataBitLength - flooredLength) < 0.5 ? (byte)Math.Ceiling(dataBitLength * 2) : (byte)Math.Ceiling(dataBitLength);
			if ((dataBitLength - flooredLength) < 0.5)
			{

			}
			return result;
		}
		/// <summary>Unpacks data from 7bit bytes to a list of ushorts, which represent symbols.</summary>
		/// <param name="data">7bit bytes to unpack.</param>
		/// <param name="dataBitLength">What is the bitlength of non-combined data elements (ushorts)?</param>
		/// <param name="flooredLength">Just floored version of dataBitLength.</param>
		/// <returns>List of ushorts, which represent symbols in codepage.</returns>
		private static List<uint> UnpackFrom7BitBytes(Span<byte> data, double dataBitLength, double flooredLength)
		{
			List<uint> result = [];
			const int bitsInByte = 7;
			const int fullByte = 0x7F;
			byte realDataBitLength = (dataBitLength - flooredLength) < 0.5 ? (byte)Math.Ceiling(dataBitLength * 2) : (byte)Math.Ceiling(dataBitLength);
			if ((dataBitLength - flooredLength) < 0.5)
			{

			}
			return result;
		}
		/// <summary>Encodes 7 bit numbers to their representing chars in base-128.</summary>
		/// <param name="data">7 bit numbers to encode. If number is greater than 127, it will be skipped.</param>
		/// <param name="spaces">Add a space every 8th char?</param>
		/// <returns>Text encoding of the 7 bit numbers.</returns>
		private static string ConvertToBase128Text(byte[] data, bool spaces)
		{
			StringBuilder sb = new();
			if (spaces)
			{
				for (int i = 0; i < data.Length; i++)
				{
					if (data[i] < 128)
					{
						sb.Append(Base128[data[i]]);
					}
					if ((i % 8 == 0) && (i != 0))
					{
						sb.Append(' ');
					}
				}
			}
			else
			{
				for (int i = 0; i < data.Length; i++)
				{
					if (data[i] < 128)
					{
						sb.Append(Base128[data[i]]);
					}
				}
			}
			return sb.ToString();
		}
		/// <summary>Decodes base-128 chars to 7 bit numbers.</summary>
		/// <param name="data">Text representing base-128 encoding.</param>
		/// <returns>Decoded 7 bit numbers.</returns>
		private static byte[] ConvertFromBase128Text(string data)
		{
			byte[] result = new byte[data.Length];
			for (int i = 0; i < result.Length; i++)
			{
				if (data[i] == ' ')
				{
					continue;
				}
				else
				{
					int idx = Array.IndexOf(Base128, data[i]);
					if (idx >= 0 && idx <= 127)
					{
						result[i] = (byte)idx;
					}
				}
			}
			return result;
		}
		/// <summary>Wrapper, calculates message entropy from…well…message, as letters.</summary>
		/// <param name="message">Message as a string.</param>
		/// <returns>Array of 3 numbers: Entropy (normalized), number of unique chars, message length.</returns>
		public static double[] CalculateMessageEntropy(string message)
		{
			return CalculateMessageEntropy(ConvertToNums(message));
		}
		/// <summary>Calculates message entropy from numerical representation of the message.</summary>
		/// <param name="message">Message as numerical representation.</param>
		/// <returns>Array of 3 numbers: Entropy (normalized), number of unique chars, message length.</returns>
		public static double[] CalculateMessageEntropy(List<ushort> message)
		{
			IEnumerable<KeyValuePair<ushort, int>> counts = message.GroupBy(a => a).Select(group => new KeyValuePair<ushort, int>(group.Key, group.Count())).OrderBy(item => item.Key);
			double entropy = 0;
			foreach(KeyValuePair<ushort, int> pair in counts)
			{
				double ratio = (double)pair.Value / message.Count;
				entropy += ratio * Math.Log2(ratio);
			}
			entropy *= -1;
			return [entropy / Math.Log2(Limit), counts.Count(), message.Count];
		}
		/// <summary>Just a conversion wrapper. It only takes into account the least significant byte, which it returns.</summary>
		/// <param name="value">Uint to convert.</param>
		/// <returns>Value as a byte.</returns>
		public static byte UintToSingleByte(uint value)
		{
			return (byte)(value & 0x000000FF);
		}
	}
}