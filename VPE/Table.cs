using System;
using System.Collections.Generic;
using System.Linq;

namespace VPE
{
	public class Table
	{
		/// <summary>Provided value was not found.</summary>
		public const ushort NotFound = ushort.MaxValue;
		/// <summary>Index is outside of the bounds.</summary>
		public const ushort Outside = ushort.MaxValue - 1;
		/// <summary>Table is empty.</summary>
		public const ushort Empty = ushort.MaxValue - 2;
		/// <summary>Specified entry in the table is left blank.</summary>
		public const ushort Blank = ushort.MaxValue - 3;
		/// <summary>Where the error codes begin. Meaning values greater (or equal) to this are invalid.</summary>
		public const ushort InvalidArea = ushort.MaxValue - 10;
		/// <summary>Main table of values.</summary>
		public List<ushort> MainTable { get; set; } = new (Codepage.Limit);
		/// <summary>Is this a table of pairs? No by default.</summary>
		public bool IsPaired { get; set; } = false;
		/// <summary>Does this table has pozitions? Used by rotors only. No by default.</summary>
		public bool HasPozition { get; set; } = false;
		/// <summary>Is this table filled only partially (unfilled are marked as Blank). No by default.</summary>
		public bool IsIncomplete { get; set; } = false;
		/// <summary>If relevant, this is the active pozition, changes during en/decryption.</summary>
		public ushort Pozition { get; set; }
		/// <summary>If relevant, this is the starting pozition, changes AFTER en/decryption.</summary>
		public ushort StartPozition { get; set; }
		/// <summary>Index of this table, for searchability in a large pool of tables.</summary>
		public uint Idx { get; set; }

		public Table()
		{

		}
		/// <summary>Builds an instance of this class from byte array.</summary>
		/// <param name="set">byte array containing data for creating an instance of this class.</param>
		/// <param name="pozition">Pozition, where data begins.</param>
		/// <param name="limit">How many entries are in the main table.</param>
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
		/// <summary>Clones this instance of table.</summary>
		/// <returns>Clone, by value, not reference, completely independent object.</returns>
		public Table Clone()
		{
			Table table = new()
			{
				IsPaired = IsPaired,
				HasPozition = HasPozition,
				IsIncomplete = IsIncomplete,
				Pozition = Pozition,
				StartPozition = StartPozition,
				Idx = Idx,
				MainTable = MainTable.ToList(), // IDK.
			};
			return table;
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
		/// <summary></summary>
		/// <param name="Value"></param>
		/// <returns></returns>
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
		/// <summary></summary>
		/// <param name="Index"></param>
		/// <returns></returns>
		public ushort FindValueUsingIndex (ushort Index)
		{
			if (MainTable.Count > 0)
			{
				if (MainTable.Count > Index)
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