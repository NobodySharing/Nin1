using System;
using System.Collections.Generic;

namespace VPE
{
	public class Table
	{
		/// <summary>Hlavní tabulka hodnot.</summary>
		public List<ushort> MainTable { get; set; } = new (256);
		/// <summary>Jestli je to tabulka párů. Deafultně předpokládám že ne.</summary>
		public bool IsPaired { get; set; } = false;
		/// <summary>Jestli tabulka má pozice (pro rotory). Deafultně předpokládám že ne.</summary>
		public bool HasPozition { get; set; } = false;
		/// <summary>Pokud relevantní, ukazuje na pozici, kde v tabulce začít. Toto se mění s průběhem en/dekrypce.</summary>
		public ushort Pozition { get; set; }
		/// <summary>Počáteční pozice v tabulce při začátku en/dekrypce.</summary>
		public ushort StartPozition { get; set; }
		/// <summary>Index tabulky, používán k rozeznávání.</summary>
		public uint Idx { get; set; }
		/// <summary>Dá dohromady booly do 1 bytu.</summary>
		/// <returns>Komprimované flagy.</returns>
		public byte GetFlags()
		{
			int b0 = IsPaired ? 1 : 0;
			int b1 = HasPozition ? 2 : 0;
			return (byte)(b0 | b1);
		}
		/// <summary>Rozbalí flagy z bytu.</summary>
		/// <param name="flags">Komprimované flagy.</param>
		public void SetFlags(byte flags)
		{
			IsPaired = (flags & 0b00000001) == 1;
			HasPozition = (flags & 0b00000010) == 2;
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
					return ushort.MaxValue - 1;
				}
			}
			else
			{
				return ushort.MaxValue;
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
					return ushort.MaxValue - 1;
				}
			}
			else
			{
				return ushort.MaxValue;
			}
		}
		/// <summary>Převede současnou tabulku do listu bytů, bere položky jako 1 byt, tzn. méně jak 256.</summary>
		/// <returns>List bytů reprezentující tuto instanci tabulky.</returns>
		internal List<byte> Single_ToBytes()
		{
			List<byte> result = new();
			result.AddRange(BitConverter.GetBytes(Idx));
			result.Add(GetFlags());
			result.Add((byte)Pozition);
			result.Add((byte)StartPozition);
			foreach (ushort item in MainTable)
			{
				result.Add((byte)item);
			}
			return result;
		}
		/// <summary>Převede současnou tabulku do listu bytů, bere položky jako 2 byty, tzn. i více jak 256.</summary>
		/// <returns>List bytů reprezentující tuto instanci tabulky.</returns>
		internal List<byte> Double_ToBytes()
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