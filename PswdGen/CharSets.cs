using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PswdGen
{
	internal class CharSets
	{
		public static readonly char[] UpperCaseLetters =
			{
			'A',
			'B',
			'C',
			'D',
			'E',
			'F',
			'G',
			'H',
			'I',
			'J',
			'K',
			'L',
			'M',
			'N',
			'O',
			'P',
			'Q',
			'R',
			'S',
			'T',
			'U',
			'V',
			'W',
			'X',
			'Y',
			'Z',
			};
		public static readonly char[] LowerCaseLetters =
			{
			'a',
			'b',
			'c',
			'd',
			'e',
			'f',
			'g',
			'h',
			'i',
			'j',
			'k',
			'l',
			'm',
			'n',
			'o',
			'p',
			'q',
			'r',
			's',
			't',
			'u',
			'v',
			'w',
			'x',
			'y',
			'z',
			};
		public static readonly char[] SafeChars =
		{
			',',
			'.',
			'-',
			'?',
			'(',
			')',
			'[',
			']',
			'{',
			'}',
			':',
			';',
			'<',
			'>',
			'_',
		};
		public static readonly char[] UnsafeChars =
		{
			'+',
			'%',
			'&',
			'/',
			'\\',
			'!',
			'\'',
			'\"',
			'*',
			'@',
			'#',
			'$',
			'*',
			'|',
			'^',
		};
	}
}