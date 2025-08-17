using System;
using Common;
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
		/// <summary>Does this table has positions? Used by rotors only. No by default.</summary>
		public bool HasPosition { get; set; } = false;
		/// <summary>Is this table filled only partially (unfilled are marked as Blank). False by default.</summary>
		public bool IsIncomplete { get; set; } = false;
		/// <summary>If relevant, this is the list of positions, it is updated with each en/decryption. 0 is starting, last is the last used.</summary>
		public List<ushort> Positions { get; set; } = [];
		/// <summary>On what position of this table are we shifting the next table by 1? This is last, still allowed position, like num. 9.</summary>
		public ushort Notch { get; set; } = (ushort)(Codepage.Limit - 1);
		/// <summary>Index of this table, for searchability in a large pool of tables.</summary>
		public uint Idx { get; set; }
		/// <summary>Creation of an empty table.</summary>
		public Table()
		{

		}
		/// <summary>Builds an instance of this class from byte array.</summary>
		/// <param name="set">byte array containing data for creating an instance of this class.</param>
		/// <param name="position">Position, where data begins.</param>
		/// <param name="limit">How many entries are in the main table.</param>
		public Table(byte[] set, ref int position, ushort limit)
		{
			Idx = Helpers.ToUInt32(set, position);
			position += 4;
			SetFlags(set[position]);
			position++;
			Notch = Helpers.ToUInt16(set, position);
			position += 2;
			if (HasPosition)
			{
				int count = Helpers.ToInt32(set, position);
				position += 4;
				Positions = new List<ushort>(count);
				for (int i = 0; i < count; i++)
				{
					Positions.Add(Helpers.ToUInt16(set, position));
					position += 2;
				}
			}
			for (ushort j = 0; j < limit; j++)
			{
				MainTable.Add(Helpers.ToUInt16(set, position));
				position += 2;
			}
		}
		/// <summary>Composes byte from flags.</summary>
		/// <returns>Byte representing flags.</returns>
		public byte GetFlags()
		{
			byte b0 = IsPaired ? (byte)1 : (byte)0;
			byte b1 = HasPosition ? (byte)2 : (byte)0;
			byte b2 = IsIncomplete ? (byte)4 : (byte)0;
			return (byte)(b0 | b1 | b2);
		}
		/// <summary>Unpacks flags from byte.</summary>
		/// <param name="flags">Flag representing byte.</param>
		public void SetFlags(byte flags)
		{
			IsPaired = (flags & 0b00000001) == 1;
			HasPosition = (flags & 0b00000010) == 2;
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
		/// <returns>List of bytes representing this instance.</returns>
		internal List<byte> ToBytes()
		{
			List<byte> result = [.. Helpers.GetBytes(Idx), GetFlags(), .. Helpers.GetBytes(Notch)];
			if (HasPosition)
			{
				result.AddRange(Helpers.GetBytes(Positions.Count));
				foreach (ushort poz in Positions)
				{
					result.AddRange(Helpers.GetBytes(poz));
				}
			}
			foreach (ushort item in MainTable)
			{
				result.AddRange(Helpers.GetBytes(item));
			}
			return result;
		}
	}
}