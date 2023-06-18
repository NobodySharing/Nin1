using System;
using System.Collections.Generic;
using System.Linq;

namespace VPE
{
	/// <summary>Class for table functions. Can be rotor, swap or reflector. Has all the required data.</summary>
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
		/// <summary>If relevant, this is the list of pozitions, it is updated with each en/decryption. 0 is starting, last is the last used.</summary>
		public List<ushort> Pozitions { get; set; }
		/// <summary>Index of this table, for searchability in a large pool of tables.</summary>
		public uint Idx { get; set; }
		/// <summary>Creation of an empty table.</summary>
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
			if (HasPozition)
			{
				int count = BitConverter.ToInt32(set, pozition);
				pozition += 4;
				Pozitions = new List<ushort>(count);
				for (int i = 0; i < count; i++)
				{
					Pozitions.Add(BitConverter.ToUInt16(set, pozition));
					pozition += 2;
				}
			}
			for (ushort j = 0; j < limit; j++)
			{
				MainTable.Add(BitConverter.ToUInt16(set, pozition));
				pozition += 2;
			}
		}
		/// <summary>Composes byte from flags.</summary>
		/// <returns>Flag representing byte.</returns>
		public byte GetFlags()
		{
			byte b0 = IsPaired ? (byte)1 : (byte)0;
			byte b1 = HasPozition ? (byte)2 : (byte)0;
			byte b2 = IsIncomplete ? (byte)4 : (byte)0;
			return (byte)(b0 | b1 | b2);
		}
		/// <summary>Unpacks flags from byte.</summary>
		/// <param name="flags">Flag representing byte.</param>
		public void SetFlags(byte flags)
		{
			IsPaired = (flags & 0b00000001) == 1;
			HasPozition = (flags & 0b00000010) == 2;
			IsIncomplete = (flags & 0b00000100) == 4;
		}
		/// <summary>Finds index using given value.</summary>
		/// <param name="Value">Which value to find?</param>
		/// <returns>Index or error code.</returns>
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
		/// <summary>Finds value using given index.</summary>
		/// <param name="Index">Which index to find?</param>
		/// <returns>Value or error code.</returns>
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
		/// <summary>Converts current instance of this class to list of bytes.</summary>
		/// <returns>List of bytes reprezenting this instance.</returns>
		internal List<byte> ToBytes()
		{
			List<byte> result = new();
			result.AddRange(BitConverter.GetBytes(Idx));
			result.Add(GetFlags());
			if (HasPozition)
			{
				result.AddRange(BitConverter.GetBytes(Pozitions.Count));
				foreach (ushort poz in Pozitions)
				{
					result.AddRange(BitConverter.GetBytes(poz));
				}
			}
			foreach (ushort item in MainTable)
			{
				result.AddRange(BitConverter.GetBytes(item));
			}
			return result;
		}
	}
}