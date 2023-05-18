using System;
using System.Collections.Generic;

namespace VPE
{
	public class Table
	{
		/// <summary>Provided vlaue was not found.</summary>
		public const ushort NotFound = ushort.MaxValue;
		/// <summary>Index is outside of the bounds.</summary>
		public const ushort Outside = ushort.MaxValue - 1;
		/// <summary>Table is empty.</summary>
		public const ushort Empty = ushort.MaxValue - 2;
		/// <summary>Specified entry in the table is left blank.</summary>
		public const ushort Blank = ushort.MaxValue - 3;
		/// <summary>Where the error codes begin. Meaning values greater (or equal) to this are invalid.</summary>
		public const ushort InvalidArea = ushort.MaxValue - 10;
		/// <summary>Hlavní tabulka hodnot.</summary>
		public List<ushort> MainTable { get; set; } = new (256);
		/// <summary>Jestli je to tabulka párů. Deafultně předpokládám že ne.</summary>
		public bool IsPaired { get; set; } = false;
		/// <summary>Jestli tabulka má pozice (pro rotory). Deafultně předpokládám že ne.</summary>
		public bool HasPozition { get; set; } = false;
		/// <summary>Jestli je vyplněná částečně. Deafultně předpokládám že ne.</summary>
		public bool IsIncomplete { get; set; } = false;
		/// <summary>Pokud relevantní, ukazuje na pozici, kde v tabulce začít. Toto se mění s průběhem en/dekrypce.</summary>
		public ushort Pozition { get; set; }
		/// <summary>Počáteční pozice v tabulce při začátku en/dekrypce.</summary>
		public ushort StartPozition { get; set; }
		/// <summary>Index tabulky, používán k rozeznávání.</summary>
		public uint Idx { get; set; }

		public Table()
		{

		}

		public Table(byte[] set, ref int pozition, ushort limit)
		{
			Idx = BitConverter.ToUInt32(set, pozition);
			pozition += 4;
			SetFlags(set[pozition]);
			pozition++;
			Pozition = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			StartPozition = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			for (ushort j = 0; j < limit; j++)
			{
				MainTable.Add(BitConverter.ToUInt16(set, pozition));
				pozition += 2;
			}
		}
		/// <summary>Dá dohromady booly do 1 bytu.</summary>
		/// <returns>Komprimované flagy.</returns>
		public byte GetFlags()
		{
			byte b0 = IsPaired ? (byte)1 : (byte)0;
			byte b1 = HasPozition ? (byte)2 : (byte)0;
			byte b2 = IsIncomplete ? (byte)4 : (byte)0;
			return (byte)(b0 | b1 | b2);
		}
		/// <summary>Rozbalí flagy z bytu.</summary>
		/// <param name="flags">Komprimované flagy.</param>
		public void SetFlags(byte flags)
		{
			IsPaired = (flags & 0b00000001) == 1;
			HasPozition = (flags & 0b00000010) == 2;
			IsIncomplete = (flags & 0b00000100) == 4;
		}
		/// <summary>Vrátí index hodnoty. Vrací 65535 pokud je tabulka prázdná, 65534 pokud neobsahuje hodnotu.</summary>
		/// <param name="Value">Hodnota.</param>
		/// <returns>Index, případně kód chyby.</returns>
		public ushort FindIndexUsingValue (ushort Value)
		{
			if (MainTable.Count > 0)
			{
				if (MainTable.Contains (Value))
				{
					return (ushort)MainTable.IndexOf(Value);
				}
				else
				{
					return NotFound;
				}
			}
			else
			{
				return Empty;
			}
		}
		/// <summary>Vrací hodnotu na základě indexu. Vrací 65535 pokud je tabulka prázdná, 65534 pokud je index větší než počet prvků v tabulce.</summary>
		/// <param name="Index">Index.</param>
		/// <returns>Hodnota.</returns>
		public ushort FindValueUsingIndex (ushort Index)
		{
			if (MainTable.Count > 0)
			{
				if (MainTable.Count - 1 >= Index)
				{
					return MainTable[Index];
				}
				else
				{
					return Outside;
				}
			}
			else
			{
				return Empty;
			}
		}
		/// <summary>Převede současnou tabulku do listu bytů, bere položky jako 2 byty, tzn. i více jak 256.</summary>
		/// <returns>List bytů reprezentující tuto instanci tabulky.</returns>
		internal List<byte> ToBytes()
		{
			List<byte> result = new();
			result.AddRange(BitConverter.GetBytes(Idx));
			result.Add(GetFlags());
			result.AddRange(BitConverter.GetBytes(Pozition));
			result.AddRange(BitConverter.GetBytes(StartPozition));
			foreach (ushort item in MainTable)
			{
				result.AddRange(BitConverter.GetBytes(item));
			}
			return result;
		}
	}
}