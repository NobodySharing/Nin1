using System;
using System.Text;
using System.Collections.Generic;
using System.Numerics;
using Common;
using System.Linq;

namespace VPE
{
	/// <summary>Shared basic funkcionality for keyes classes.</summary>
	public class Key_Base
	{
		/// <summary>Converts tables to byte array.</summary>
		/// <param name="tables">List of tables.</param>
		/// <returns>List of bytes representing list of tables.</returns>
		internal static List<byte> TablesToBytes(List<Table> tables)
		{
			List<byte> result = [];
			foreach (Table table in tables)
			{
				result.AddRange(table.ToBytes());
			}
			return result;
		}
		/// <summary>Decodes the tables from byte array. Shifts the poziton.</summary>
		/// <param name="set">Byte array to decode.</param>
		/// <param name="position">Position in the array.</param>
		/// <param name="tableCount">Tables count.</param>
		/// <param name="limit">Table items count. Usulally the size of the codepage.</param>
		/// <returns>Set of tables.</returns>
		internal static List<Table> TablesFromBytes(byte[] set, ref int position, int tableCount, ushort limit)
		{
			List<Table> result = [];
			for (int i = 0; i < tableCount; i++)
			{
				result.Add(new Table(set, ref position, limit));
			}
			return result;
		}
	}
	/// <summary>Main class for storing and working with key data for the encryption algorithm.</summary>
	public class Key : Key_Base
	{
		/// <summary>All rotors (and their positions). Lowest index is the lowest order. It rotates every char.</summary>
		public List<Table> Rotors { get; private set; } = [];
		/// <summary>All swaps (plugboard).</summary>
		public List<Table> Swaps { get; private set; } = [];
		/// <summary>Single reflector.</summary>
		public Table Reflector { get; set; }
		/// <summary>Table used for scrambling the codepage in the input stage.</summary>
		public Table InputScrambler { get; set; }
		/// <summary>Table used for scrambling the codepage in the output stage.</summary>
		public Table OutputScrambler { get; set; }
		/// <summary>Constant for fixed shift.</summary>
		public uint ConstShift { get; set; }
		/// <summary>Constant for variable shift.</summary>
		public uint VarShift { get; set; }
		/// <summary>Minimal length of space between inserted random chars.</summary>
		public ushort RandCharSpcMin { get; set; }
		/// <summary>Maximal length of space between inserted random chars.</summary>
		public ushort RandCharSpcMax { get; set; }
		/// <summary>Constant A for generator of pseudorandom numbers, equation y = (ax + b) % m. For calculation of how many position to shift the rotor after a char.</summary>
		public PrimeDefinedConstant RotorShiftConstA { get; set; }
		/// <summary>Constant B for generator of pseudorandom numbers, equation y = (ax + b) % m. For calculation of how many position to shift the rotor after a char.</summary>
		public PrimeDefinedConstant RotorShiftConstB { get; set; }
		/// <summary>Constant M for generator of pseudorandom numbers, equation y = (ax + b) % m. For calculation of how many position to shift the rotor after a char.</summary>
		public PrimeDefinedConstant RotorShiftConstM { get; set; }
		/// <summary>Constant for final modulo division of the number for rotor shift, so the shift is not that large.</summary>
		public ulong RotorShiftConstN { get; set; }
		/// <summary>Constant A for generator of pseudorandom numbers, equation y = (ax + b) % m. For calculation of the length of space between inserted random chars.</summary>
		public PrimeDefinedConstant RandCharConstA { get; set; }
		/// <summary>Constant B for generator of pseudorandom numbers, equation y = (ax + b) % m. For calculation of the length of space between inserted random chars.</summary>
		public PrimeDefinedConstant RandCharConstB { get; set; }
		/// <summary>Constant M for generator of pseudorandom numbers, equation y = (ax + b) % m. For calculation of the length of space between inserted random chars.</summary>
		public PrimeDefinedConstant RandCharConstM { get; set; }
		/// <summary>A constant for table creation for character switching. Part of the first pair.</summary>
		public PrimeDefinedConstant SwitchConstA { get; set; }
		/// <summary>B constant for table creation for character switching. Part of the first pair.</summary>
		public PrimeDefinedConstant SwitchConstB { get; set; }
		/// <summary>C constant for table creation for character switching. Part of the second pair.</summary>
		public PrimeDefinedConstant SwitchConstC { get; set; }
		/// <summary>D constant for table creation for character switching. Part of the second pair.</summary>
		public PrimeDefinedConstant SwitchConstD { get; set; }
		/// <summary>Key name.</summary>
		public string Name { get; set; }
		/// <summary>Index of this key.</summary>
		public uint Idx { get; set; }
		/// <summary>Index of selected set of positions (for rotors).</summary>
		public int SelectedPositions { get; set; }
		/// <summary>Gets the index of last position set rotors have. All rotors should have the same.</summary>
		public int GetLastRotorPositionsIdx
		{
			get
			{
				return Rotors[0].Positions.Count - 1;
			}
		}
		/// <summary>Smallest size of this class instance. Approximated.</summary>
		private const int MinSize = 1024;
		public Key ()
		{

		}
		/// <summary>Creates a new instance of this class based on loaded data.</summary>
		/// <param name="file">Data from a file storing this class.</param>
		public Key (byte[] file)
		{
			int position = 0;
			FromBytes (file, ref position);
		}
		/// <summary>Creates a new instance of this class based on loaded data. For mass data loading.</summary>
		/// <param name="file">Data from (part of) a file storing this class.</param>
		/// <param name="position">Position in the file.</param>
		public Key(byte[] file, ref int position)
		{
			FromBytes(file, ref position);
		}
		/// <summary>Duplicates selected position set and set this duplicite position set as active (selected).</summary>
		/// <param name="which">Which position set to duplicate and activate? -1 means the last one, -2 for the active set.</param>
		public void DuplicateAndSetToActivePositions(int which = -2)
		{
			int pozIdx = PositionTranslator(which);
			if (pozIdx == -3)
			{
				return;
			}
			foreach (Table rotor in Rotors)
			{
				rotor.Positions.Add(rotor.Positions[pozIdx]);
			}
			SelectedPositions = GetLastRotorPositionsIdx;
		}
		/// <summary>Adds set of rotor positions.</summary>
		/// <param name="positions">List of positions. Needs to be the same count as rotors count.</param>
		/// <returns>False if error.</returns>
		public bool AddPositions(List<ushort> positions)
		{
			if (positions is null)
			{
				return false;
			}
			if (positions.Count != Rotors.Count)
			{
				return false;
			}
			if (DoesContainPositions(positions))
			{
				return true;
			}
			for (int i = 0; i < positions.Count; i++)
			{
				Rotors[i].Positions.Add((ushort)(positions[i] % Codepage.Limit)); // Just to be sure that I'm in the correct range.
			}
			return true;
		}
		/// <summary>Check, if the supplied position set is already among existing rotor positions.</summary>
		/// <param name="positions">Position set to check.</param>
		/// <returns>Presence of supplied position set among existing rotor positions.</returns>
		private bool DoesContainPositions(List<ushort> positions)
		{
			bool result = true;
			for(int i = 0; i <= GetLastRotorPositionsIdx; i++)
			{
				List<ushort> existing = GetPositions(i);
				result |= existing == positions;
			}
			return result;
		}
		/// <summary>Gets specified position set from all rotors.</summary>
		/// <param name="which">Index of positions to get. -1 means the last one, -2 for the active set.</param>
		/// <returns>Position set.</returns>
		public List<ushort> GetPositions(int which = -2)
		{
			List<ushort> result = [];
			int pozIdx = PositionTranslator(which);
			if (pozIdx == -3)
			{
				return result;
			}
			foreach (Table t in Rotors)
			{
				result.Add(t.Positions[pozIdx]);
			}
			return result;
		}
		/// <summary>Removes specified position set from all rotors.</summary>
		/// <param name="which">Index of positions to remove. -1 means the last one, -2 for the active set.</param>
		/// <returns>If it was successful.</returns>
		public bool RemovePositions(int which = -2)
		{
			int pozIdx = PositionTranslator(which);
			if (pozIdx == -3)
			{
				return false;
			}
			foreach (Table t in Rotors)
			{
				t.Positions.RemoveAt(pozIdx);
			}
			return true;
		}
		/// <summary>Increments positional numbers of all rotors.</summary>
		/// <param name="which">Which set of positional numbers to increment. -1 for the last ones, -2 for the active set.</param>
		public void IncrementPositions(int which = -2)
		{
			int posIdx = PositionTranslator(which);
			if (posIdx <= -3)
			{
				return;
			}
			Rotors[0].Positions[posIdx]++;
			for (int i = 0; i < Rotors.Count - 1; i++) // I don't actually need to go to the last rotor. The last one overflows.
			{
				if (Rotors[i].Positions[posIdx] == (Rotors[i].Notch + 1)) // Check if to increment the next rotor.
				{
					Rotors[i + 1].Positions[posIdx]++;
				}
				if (Rotors[i].Positions[posIdx] >= Codepage.Limit) // Zeroing, but this time without incrementing the next rotor.
				{
					Rotors[i].Positions[posIdx] = 0;
				}
			}
		}
		/// <summary>Increments or decrements rotor positional numbers by specified amount.</summary>
		/// <param name="num">By how much to increment.</param>
		/// <param name="which">Which set of positional numbers to increment. -1 for the last ones, -2 for the active set.</param>
		/// <param name="increment">Increment? (True). False is decrement.</param>
		public void IncrementPositions(BigInteger num, int which = -2, bool increment = true)
		{
			int posIdx = PositionTranslator(which);
			if (posIdx <= -3)
			{
				return;
			}
			ushort[] numConverted = Helpers.BaseConvertor(num, Codepage.Limit);
			if (increment)
			{
				int carry = 0;
				for (int i = 0; i < Rotors.Count; i++)
				{
					int sum = Rotors[i].Positions[posIdx] + numConverted[i] + carry;
					ushort newValue = (ushort)(sum % Codepage.Limit);
					carry = (((Rotors[i].Positions[posIdx] <= Rotors[i].Notch) && (sum >= Rotors[i].Notch)) || newValue > Rotors[i].Notch) ? 1 : 0;
					Rotors[i].Positions[posIdx] = newValue;
				}
			}
			else
			{
				int subCarry = 0;
				for (int i = 0; i < Rotors.Count; i++)
				{
					int sub = Rotors[i].Positions[posIdx] - numConverted[i] - subCarry;

				}
			}
		}
		/// <summary>Increments or decrements custom positional numbers by specified amount.</summary>
		/// <param name="pozs">Custom positional numbers.</param>
		/// <param name="num">By how much to increment.</param>
		/// <param name="increment">Increment? (True). False is decrement.</param>
		public static void IncrementCustomPositions(ref ushort[] pozs, uint num = 1, bool increment = true)
		{
			for (int i = 0; i < pozs.Length; i++)
			{
				num += pozs[i];
				pozs[i] = (ushort)(num % Codepage.Limit);
				num -= pozs[i];
				if (num > 0)
				{
					num /= Codepage.Limit;
				}
				else
				{
					break;
				}
			}
		}
		/// <summary>Gets specified position set as a string from all rotors.</summary>
		/// <param name="which">Index of positions to get. -1 means the last one. -2 for selected ones.</param>
		/// <returns>Position set as a string, comma separated. Empty string if error.</returns>
		public string GetPositionsString(int which = -2)
		{
			int pozIdx = PositionTranslator(which);
			if (pozIdx == -3)
			{
				return "";
			}
			List<ushort> pozs = GetPositions(pozIdx);
			StringBuilder sb = new();
			sb = sb.Append(pozs[^1]);
			for (int i = pozs.Count - 2; i > -1; i--)
			{
				sb.Append(", ");
				sb.Append(pozs[i]);
			}
			return sb.ToString();
		}
		/// <summary>Adds set of rotor positions given as a string.</summary>
		/// <param name="positions">Comma separated string of valid rotor positions containing the same number of positions as the number of rotors.</param>
		/// <returns>False if error.</returns>
		public bool AddPositionsUsingString(string positions)
		{
			if (positions == null)
			{
				return false;
			}
			if (positions.Length == 0)
			{
				return false;
			}
			string[] numsAsTexts = positions.Split(',');
			List<ushort> nums = [];
			foreach (string s in numsAsTexts)
			{
				if (s.Length == 0)
				{
					continue;
				}
				else
				{
					if (ushort.TryParse(s.Trim(), out ushort num))
					{
						if (num >= Codepage.Limit)
						{
							return false;
						}
						else
						{
							nums.Add(num);
						}
					}
					else
					{
						continue;
					}
				}
			}
			if (nums.Count != Rotors.Count)
			{
				return false;
			}
			else
			{
				for (int i = 0; i < Rotors.Count; i++)
				{
					Rotors[i].Positions.Add(nums[i]);
				}
			}
			return true;
		}
		/// <summary>Calculates absolute position.</summary>
		/// <param name="which">Which position set to calculate? -1 means the last one. -2 for selected ones.</param>
		/// <returns>Absolute position, usually really big number. For 50 rotors, it's (up to) 138 digits.</returns>
		public BigInteger GetPositionMagnitude(int which = -2)
		{
			BigInteger result = 1;
			int pozIdx = PositionTranslator(which);
			if (pozIdx == -3)
			{
				return -1;
			}
			for (int i = Rotors.Count - 1, j = 0; i > -1; i--, j++)
			{
				result += Rotors[i].Positions[pozIdx] * BigInteger.Pow(Codepage.Limit, j);
			}
			return result;
		}
		/// <summary>Removes duplicates (relative to selected) in position sets.</summary>
		public void PrunePositions()
		{
			int toRemove = -1;
			for (int i = 0; i <= GetLastRotorPositionsIdx; i++)
			{
				if (i == SelectedPositions)
				{
					continue;
				}
				for (int j = 0; j < Rotors.Count; j++)
				{
					if (Rotors[j].Positions[i] == Rotors[j].Positions[SelectedPositions])
					{
						toRemove = i;
					}
					else
					{
						toRemove = -1;
						break;
					}
				}
			}
			if (toRemove >= 0)
			{
				foreach (Table rotor in Rotors)
				{
					rotor.Positions.RemoveAt(toRemove);
				}
				SelectedPositions--;
			}
		}
		/// <summary>Translates special position set number to actual one.</summary>
		/// <param name="which">Position set number to translate.</param>
		/// <returns>-3 if error, otherwise non-negative number of position set.</returns>
		private int PositionTranslator (int which)
		{
			int pozIdx;
			if (which < -2)
			{
				return -3;
			}
			else if (which == -2)
			{
				pozIdx = SelectedPositions;
			}
			else if (which == -1)
			{
				pozIdx = Rotors[0].Positions.Count - 1;
			}
			else
			{
				pozIdx = which;
			}
			return pozIdx;
		}
		/// <summary>Converts this instance to byte array.</summary>
		public byte[] ToBytes ()
		{
			List<byte> result = new(65536);
			result.AddRange(Helpers.GetBytes(Codepage.Limit));
			result.AddRange(Helpers.GetBytes(Name.Length));
			result.AddRange(Encoding.UTF32.GetBytes(Name));
			result.AddRange(Helpers.GetBytes(Idx));
			result.AddRange(Helpers.GetBytes(Rotors.Count));
			result.AddRange(TablesToBytes(Rotors));
			result.AddRange(Helpers.GetBytes(Swaps.Count));
			result.AddRange(TablesToBytes(Swaps));
			result.AddRange(Reflector.ToBytes());
			result.AddRange(InputScrambler.ToBytes());
			result.AddRange(OutputScrambler.ToBytes());
			result.AddRange(Helpers.GetBytes(ConstShift));
			result.AddRange(Helpers.GetBytes(VarShift));
			result.AddRange(Helpers.GetBytes(RandCharSpcMin));
			result.AddRange(Helpers.GetBytes(RandCharSpcMax));
			result.AddRange(Helpers.GetBytes(RotorShiftConstN));
			result.AddRange(RotorShiftConstA.ToBytes());
			result.AddRange(RotorShiftConstB.ToBytes());
			result.AddRange(RotorShiftConstM.ToBytes());
			result.AddRange(RandCharConstA.ToBytes());
			result.AddRange(RandCharConstB.ToBytes());
			result.AddRange(RandCharConstM.ToBytes());
			result.AddRange(SwitchConstA.ToBytes());
			result.AddRange(SwitchConstB.ToBytes());
			result.AddRange(SwitchConstC.ToBytes());
			result.AddRange(SwitchConstD.ToBytes());
			return [.. result];
		}
		/// <summary>Converts byte array to instance of this class.</summary>
		/// <param name="file">Byte array.</param>
		/// <param name="position">Starting position in array.</param>
		private void FromBytes(byte[] file, ref int position)
		{
			if (file == null)
			{
				return;
			}
			if (file.Length < MinSize)
			{
				return;
			}
			ushort limit = Helpers.ToUInt16(file, position);
			position += 2;
			int nameLength = Helpers.ToInt32(file, position);
			position += 4;
			Name = Encoding.UTF32.GetString(file, position, nameLength * 4);
			position += nameLength * 4;
			Idx = Helpers.ToUInt32(file, position);
			position += 4;
			int tables = Helpers.ToInt32(file, position);
			position += 4;
			Rotors.AddRange(TablesFromBytes(file, ref position, tables, limit));
			tables = Helpers.ToInt32(file, position);
			position += 4;
			Swaps.AddRange(TablesFromBytes(file, ref position, tables, limit));
			Reflector = new Table (file, ref position, limit);
			InputScrambler = new Table(file, ref position, limit);
			OutputScrambler = new Table(file, ref position, limit);
			ConstShift = Helpers.ToUInt32(file, position);
			position += 4;
			VarShift = Helpers.ToUInt32(file, position);
			position += 4;
			RandCharSpcMin = Helpers.ToUInt16(file, position);
			position += 2;
			RandCharSpcMax = Helpers.ToUInt16(file, position);
			position += 2;
			RotorShiftConstN = Helpers.ToUInt64(file, position);
			position += 8;
			const int PDCsCount = 10;
			PrimeDefinedConstant[] consts = new PrimeDefinedConstant[PDCsCount];
			for (int i = 0; i < PDCsCount; i++)
			{
				consts[i] = new();
				consts[i].FromBytes(file, ref position);
			}
			RotorShiftConstA = consts[0];
			RotorShiftConstB = consts[1];
			RotorShiftConstM = consts[2];
			RandCharConstA = consts[3];
			RandCharConstB = consts[4];
			RandCharConstM = consts[5];
			SwitchConstA = consts[6];
			SwitchConstB = consts[7];
			SwitchConstC = consts[8];
			SwitchConstD = consts[9];
		}
	}
	/// <summary>Defines a very large constant. Using it's factorization./summary>
	public class PrimeDefinedConstant
	{
		/// <summary>Maximum count of primes and exponens. Arbitrarly set, yes.</summary>
		private const int Size = 8;
		/// <summary>List of indexes of prime numbers. Up to 78 497 ONLY!</summary>
		public int[] PrimeIdxs = new int[Size];
		/// <summary>List of corresponging powers, to which a prime with the same index should be raised.</summary>
		public byte[] Exponents = new byte[Size];
		public PrimeDefinedConstant()
		{
			for (int i = 0; i < Size; i++)
			{
				PrimeIdxs[i] = -1;
			}
		}
		/// <summary>Computes the constant represented by given primes and their powers.</summary>
		/// <param name="toAdd">How much to add to the result.</param>
		/// <returns>Really big number. At least that is the idea. With all digits.</returns>
		public BigInteger ComputeConstant(long toAdd = 0)
		{
			BigInteger result = new(1);
			for (int i = 0; i < Size; i++)
			{
				if (PrimeIdxs[i] >= PrimeList.First1Digit && PrimeIdxs[i] <= PrimeList.Last6Digit)
				{
					result *= BigInteger.Pow((BigInteger)PrimeList.Primes[PrimeIdxs[i]], Exponents[i]);
				}
			}
			return result + toAdd;
		}
		/// <summary>Converts to a resulting number as a string.</summary>
		/// <returns>Number as a string.</returns>
		public override string ToString()
		{
			return ComputeConstant().ToString();
		}
		/// <summary>Converts this instance to byte array.</summary>
		/// <returns>Byte array representing this instance.</returns>
		public byte[] ToBytes()
		{
			List<byte> result = [];
			for (int i = 0; i < Size; i++)
			{
				result.AddRange(Helpers.GetBytes(PrimeIdxs[i]));
				result.Add(Exponents[i]);
			}
			return [.. result];
		}
		/// <summary>Fills this instance with decoded data.</summary>
		/// <param name="bytes">Set of bytes containing data for this class.</param>
		/// <param name="poz">Position in the data to start with.</param>
		public void FromBytes(byte[] bytes, ref int poz)
		{
			for (int i = 0; i < Size; i++)
			{
				PrimeIdxs[i] = Helpers.ToInt32(bytes, poz);
				poz += 4;
				Exponents[i] = bytes[poz];
				poz++;
			}
		}
	}
	/// <summary>Stores sets of tables.</summary>
	public class TableLibrary : Key_Base
	{
		/// <summary>All rotors.</summary>
		public List<Table> Rotors { get; private set; } = [];
		/// <summary>All reflectors.</summary>
		public List<Table> Reflectors { get; private set; } = [];
		/// <summary>All swaps (plugboards).</summary>
		public List<Table> Swaps { get; private set; } = [];
		/// <summary>All scramblers.</summary>
		public List<Table> Scramblers { get; private set; } = [];
		/// <summary>Path to this instance on disk.</summary>
		public string PathToThis { get; set; }
		/// <summary>Creates an empty instance of Table library class.</summary>
		public TableLibrary ()
		{

		}
		/// <summary>Creates an instance of this class by reading data from a file.</summary>
		/// <param name="file">File (as byte array) with data for this class.</param>
		public TableLibrary(byte[] file)
		{
			if (file == null)
			{
				return;
			}
			if (file.Length == 0)
			{
				return;
			}
			int position = 0;
			ushort lim = Helpers.ToUInt16(file, position);
			position += 2;
			int tableCount = Helpers.ToInt32(file, position);
			position += 4;
			Rotors.AddRange(TablesFromBytes(file, ref position, tableCount, lim));
			tableCount = Helpers.ToInt32(file, position);
			position += 4;
			Reflectors.AddRange(TablesFromBytes(file, ref position, tableCount, lim));
			tableCount = Helpers.ToInt32(file, position);
			position += 4;
			Swaps.AddRange(TablesFromBytes(file, ref position, tableCount, lim));
			tableCount = Helpers.ToInt32(file, position);
			position += 4;
			Scramblers.AddRange(TablesFromBytes(file, ref position, tableCount, lim));
		}
		/// <summary>Converts this instance to byte array.</summary>
		/// <returns>Byte array representing this instance.</returns>
		public byte[] ToBytes()
		{
			List<byte> result = new(65536);
			result.AddRange(Helpers.GetBytes(Codepage.Limit));
			result.AddRange(Helpers.GetBytes(Rotors.Count));
			result.AddRange(TablesToBytes(Rotors));
			result.AddRange(Helpers.GetBytes(Reflectors.Count));
			result.AddRange(TablesToBytes(Reflectors));
			result.AddRange(Helpers.GetBytes(Swaps.Count));
			result.AddRange(TablesToBytes(Swaps));
			result.AddRange(Helpers.GetBytes(Scramblers.Count));
			result.AddRange(TablesToBytes(Scramblers));
			return [.. result];
		}
		/// <summary>Gets the IDs of all tables in a selection.</summary>
		/// <param name="what">Which tables to get IDs from?</param>
		/// <returns>List of IDs.</returns>
		public static List<string> GetIDs(List<Table> what)
		{
			List<string> result = [];
			foreach (Table table in what)
			{
				result.Add(table.Idx.ToString());
			}
			return result;
		}
		/// <summary>Merges 2 TLs together by appending new one to this one.</summary>
		/// <param name="newTables">TL to add.</param>
		public void Merge(TableLibrary newTables)
		{
			if (newTables is null)
			{
				return;
			}
			foreach(Table rotor in newTables.Rotors)
			{
				rotor.Idx = (uint)Rotors.Count;
				Rotors.Add(rotor);
			}
			foreach (Table swap in newTables.Swaps)
			{
				swap.Idx = (uint)Swaps.Count;
				Rotors.Add(swap);
			}
			foreach (Table reflector in newTables.Reflectors)
			{
				reflector.Idx = (uint)Reflectors.Count;
				Rotors.Add(reflector);
			}
			foreach (Table scrambler in newTables.Scramblers)
			{
				scrambler.Idx = (uint)Scramblers.Count;
				Scramblers.Add(scrambler);
			}
		}
		/// <summary>Creates Settings class based on tables selection.</summary>
		/// <param name="rotors">Which rotors to use?</param>
		/// <param name="swaps">Which swaps to use?</param>
		/// <param name="refl">Which reflector to use?</param>
		/// <param name="scramblers">Which scramblers to use? Set of 2 nums exactly.</param>
		/// <returns>Settings class with all the tables filled. The rest is not!</returns>
		public Key Select (List<ushort> rotors, List<ushort> swaps, int refl, ushort[] scramblers)
		{
			Key s = new()
			{
				Reflector = Reflectors[refl],
				InputScrambler = Scramblers[scramblers[0]],
				OutputScrambler = Scramblers[scramblers[1]],
			};
			foreach (ushort i in rotors)
			{
				s.Rotors.Add(Rotors[i]);
			}
			foreach (ushort i in swaps)
			{
				s.Swaps.Add(Swaps[i]);
			}
			return s;
		}
	}
	/// <summary>Settings library, for storing a set of settings.</summary>
	public class KeyesLibrary : Key_Base
	{
		/// <summary>Settings library.</summary>
		public List<Key> Library { get; private set; } = [];
		/// <summary>Index of the last active settings.</summary>
		public int LastActive { get; set; }
		/// <summary>Path to this instance on disk.</summary>
		public string PathToThis { get; set; }
		/// <summary>Creates a new empty settings library.</summary>
		public KeyesLibrary()
		{

		}
		/// <summary>Crates a settings library from a file.</summary>
		public KeyesLibrary(byte[] file)
		{
			int position = 0;
			int count = Helpers.ToInt32(file, position);
			position += 4;
			for (int i = 0; i < count; i++)
			{
				Key s = new(file, ref position);
				Library.Add(s);
			}
			LastActive = Helpers.ToInt32(file, position);
		}
		/// <summary>Reindexes all the settings contained here.</summary>
		public void ReIndexSetts ()
		{
			for (int i = 0; i < Library.Count; i++)
			{
				Library[i].Idx = Convert.ToUInt32(i);
			}
		}
		/// <summary>Converts this instance to a byte array.</summary>
		public byte[] ToBytes()
		{
			List<byte> result = new(524288);
			result.AddRange(Helpers.GetBytes(Library.Count));
			foreach (Key s in Library)
			{
				result.AddRange(s.ToBytes());
			}
			result.AddRange(Helpers.GetBytes(LastActive));
			return [.. result];
		}
	}
}