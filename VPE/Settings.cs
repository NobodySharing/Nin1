using System;
using System.Text;
using System.Collections.Generic;
using System.Numerics;
using Common;
using System.Linq;

namespace VPE
{
	/// <summary>Shared basic funkcionality for settings classes.</summary>
	public class Settings_Base
	{
		/// <summary>Converts tables to byte array.</summary>
		/// <param name="tables">List of tables.</param>
		/// <returns>List of bytes representing list of tables.</returns>
		internal static List<byte> TablesToBytes(List<Table> tables)
		{
			List<byte> result = new();
			foreach (Table table in tables)
			{
				result.AddRange(table.ToBytes());
			}
			return result;
		}
		/// <summary>Decodes the tables from byte array. Shifts the poziton.</summary>
		/// <param name="set">Byte array to decode.</param>
		/// <param name="pozition">Pozition in the array.</param>
		/// <param name="tableCount">Tables count.</param>
		/// <param name="limit">Table items count. Usulally the size of the codepage.</param>
		/// <returns>Set of tables.</returns>
		internal static List<Table> TablesFromBytes(byte[] set, ref int pozition, int tableCount, ushort limit)
		{
			List<Table> result = new ();
			for (int i = 0; i < tableCount; i++)
			{
				result.Add(new Table (set, ref pozition, limit));
			}
			return result;
		}
	}
	/// <summary>Main class for storing and working with settings data for the encryption algorithm.</summary>
	public class Settings : Settings_Base
	{
		/// <summary>All rotors (and their pozitons). Lowest index is the lowest order. It rotates every char.</summary>
		public List<Table> Rotors { get; private set; } = new();
		/// <summary>All swaps (plugboard).</summary>
		public List<Table> Swaps { get; private set; } = new();
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
		/// <summary>Constant A for generator of random numbers, equation y = (ax + b) % m. For calculation of the length of space between inserted random chars.</summary>
		public PrimeDefinedConstant RandCharConstA { get; set; }
		/// <summary>Constant B for generator of random numbers, equation y = (ax + b) % m. For calculation of the length of space between inserted random chars.</summary>
		public PrimeDefinedConstant RandCharConstB { get; set; }
		/// <summary>Constant M for generator of random numbers, equation y = (ax + b) % m. For calculation of the length of space between inserted random chars.</summary>
		public PrimeDefinedConstant RandCharConstM { get; set; }
		/// <summary>A constant for table creation for character switching. Part of the first pair.</summary>
		public PrimeDefinedConstant SwitchConstA { get; set; }
		/// <summary>B constant for table creation for character switching. Part of the first pair.</summary>
		public PrimeDefinedConstant SwitchConstB { get; set; }
		/// <summary>C constant for table creation for character switching. Part of the second pair.</summary>
		public PrimeDefinedConstant SwitchConstC { get; set; }
		/// <summary>D constant for table creation for character switching. Part of the second pair.</summary>
		public PrimeDefinedConstant SwitchConstD { get; set; }
		/// <summary>Settings name.</summary>
		public string Name { get; set; }
		/// <summary>Index of this settings.</summary>
		public uint Idx { get; set; }
		/// <summary>Index of selected set of pozitions (for rotors).</summary>
		public int SelectedPozitions { get; set; }
		/// <summary>Gets the index of last pozition set rotors have. All rotors should have the same.</summary>
		public int GetLastRotorPozitionsIdx
		{
			get
			{
				return Rotors[0].Pozitions.Count - 1;
			}
		}
		/// <summary>Smallest size of this class instance. Approximated.</summary>
		private const int MinSize = 1024;
		public Settings ()
		{

		}
		/// <summary>Creates a new instance of this class based on loaded data.</summary>
		/// <param name="file">Data from a file storing this class.</param>
		public Settings (byte[] file)
		{
			int pozition = 0;
			FromBytes (file, ref pozition);
		}
		/// <summary>Creates a new instance of this class based on loaded data. For mass data loading.</summary>
		/// <param name="file">Data from (part of) a file storing this class.</param>
		/// <param name="pozition">Pozition in the file.</param>
		public Settings(byte[] file, ref int pozition)
		{
			FromBytes(file, ref pozition);
		}
		/// <summary>Duplicates selected pozition set and set this duplicite pozition set as active (selected).</summary>
		/// <param name="which">Which pozition set to duplicate and activate? -1 means the last one, -2 for the active set.</param>
		public void DuplicateAndSetToActivePozitions(int which = -2)
		{
			int pozIdx;
			if (which < -2)
			{
				return;
			}
			else if (which == -2)
			{
				pozIdx = SelectedPozitions;
			}
			else if (which == -1)
			{
				pozIdx = GetLastRotorPozitionsIdx;
			}
			else
			{
				pozIdx = which;
			}
			foreach (Table rotor in Rotors)
			{
				rotor.Pozitions.Add(rotor.Pozitions[pozIdx]);
			}
			SelectedPozitions = GetLastRotorPozitionsIdx;
		}
		/// <summary>Adds set of rotor pozitions.</summary>
		/// <param name="pozitions">List of pozitions. Needs to be the same count as rotors count.</param>
		/// <returns>False if error.</returns>
		public bool AddPozitions(List<ushort> pozitions)
		{
			if (pozitions is null)
			{
				return false;
			}
			if (pozitions.Count != Rotors.Count)
			{
				return false;
			}
			if (DoesContainPozitions(pozitions))
			{
				return true;
			}
			for (int i = 0; i < pozitions.Count; i++)
			{
				Rotors[i].Pozitions.Add((ushort)(pozitions[i] % Codepage.Limit)); // Just to be sure that I'm in the correct range.
			}
			return true;
		}
		/// <summary>Check, if the supplied pozition set is already among existing rotor pozitions.</summary>
		/// <param name="pozitions">Pozition set to check.</param>
		/// <returns>Presence of supplied pozition set among existing rotor pozitions.</returns>
		private bool DoesContainPozitions(List<ushort> pozitions)
		{
			bool result = true;
			for(int i = 0; i <= GetLastRotorPozitionsIdx; i++)
			{
				List<ushort> existing = GetPozitions(i);
				result |= existing == pozitions;
			}
			return result;
		}
		/// <summary>Gets specified pozition set from all rotors.</summary>
		/// <param name="which">Index of pozitions to get. -1 means the last one, -2 for the active set.</param>
		/// <returns>Pozition set.</returns>
		public List<ushort> GetPozitions(int which = -2)
		{
			int pozIdx;
			List<ushort> result = new();
			if (which < -2)
			{
				return result;
			}
			else if (which == -2)
			{
				pozIdx = SelectedPozitions;
			}
			else if (which == -1)
			{
				pozIdx = GetLastRotorPozitionsIdx;
			}
			else
			{
				pozIdx = which;
			}
			foreach (Table t in Rotors)
			{
				result.Add(t.Pozitions[pozIdx]);
			}
			return result;
		}
		/// <summary>Removes specified pozition set from all rotors.</summary>
		/// <param name="which">Index of pozitions to remove. -1 means the last one, -2 for the active set.</param>
		/// <returns>If it was successful.</returns>
		public bool RemovePozitions(int which = -2)
		{
			int pozIdx;
			if (which < -2)
			{
				return false;
			}
			else if (which == -2)
			{
				pozIdx = SelectedPozitions;
			}
			else if (which == -1)
			{
				pozIdx = GetLastRotorPozitionsIdx;
			}
			else
			{
				pozIdx = which;
			}
			foreach (Table t in Rotors)
			{
				t.Pozitions.RemoveAt(pozIdx);
			}
			return true;
		}
		/// <summary>Increments last pozitional numbers of all rotors.</summary>
		/// <param name="which">Which set of pozitional numbers to increment. -1 for the last ones, -2 for the active set.</param>
		public void IncrementPozitions(int which = -2)
		{
			int pozIdx;
			if (which < -2)
			{
				return;
			}
			else if (which == -2)
			{
				pozIdx = SelectedPozitions;
			}
			else if (which == -1)
			{
				pozIdx = Rotors[0].Pozitions.Count - 1;
			}
			else
			{
				pozIdx = which;
			}
			Rotors[0].Pozitions[pozIdx]++;
			for (int i = 0; i < Rotors.Count; i++)
			{
				if (Rotors[i].Pozitions[pozIdx] >= Codepage.Limit)
				{
					Rotors[i].Pozitions[pozIdx] = 0;
					if (i < (Rotors.Count - 1))
					{
						Rotors[i + 1].Pozitions[pozIdx]++;
					}
				}
				else
				{
					break;
				}
			}
		}
		/// <summary>Increments last rotor pozitional numbers by specified amount.</summary>
		/// <param name="num">By how much to increment.</param>
		/// <param name="which">Which set of pozitional numbers to increment. -1 for the last ones, -2 for the active set.</param>
		public void IncrementPozitions(uint num, int which = -2)
		{
			int pozIdx;
			if (which < -2)
			{
				return;
			}
			else if (which == -2)
			{
				pozIdx = SelectedPozitions;
			}
			else if (which == -1)
			{
				pozIdx = Rotors[0].Pozitions.Count - 1;
			}
			else
			{
				pozIdx = which;
			}
			foreach (Table t in Rotors)
			{
				num += t.Pozitions[pozIdx];
				t.Pozitions[pozIdx] = (ushort)(num % Codepage.Limit);
				num -= t.Pozitions[pozIdx];
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
		/// <summary>Increments custom pozitional numbers by specified amount.</summary>
		/// <param name="pozs">Custom pozitional numbers.</param>
		/// <param name="num">By how much to increment.</param>
		public static void IncrementCustomPozitions(ref ushort[] pozs, uint num = 1)
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
		/// <summary>Gets specified pozition set as a string from all rotors.</summary>
		/// <param name="which">Index of pozitions to get. -1 means the last one. -2 for selected ones.</param>
		/// <returns>Pozition set as a string, comma separated. Empty string if error.</returns>
		public string GetPozitionsString(int which = -2)
		{
			int pozIdx;
			if (which < -2)
			{
				return "";
			}
			else if (which == -2)
			{
				pozIdx = SelectedPozitions;
			}
			else if (which == -1)
			{
				pozIdx = GetLastRotorPozitionsIdx;
			}
			else
			{
				pozIdx = which;
			}
			List<ushort> pozs = GetPozitions(pozIdx);
			StringBuilder sb = new();
			sb = sb.Append(pozs[^1]);
			for (int i = pozs.Count - 2; i > -1; i--)
			{
				sb.Append(", ");
				sb.Append(pozs[i]);
			}
			return sb.ToString();
		}
		/// <summary>Adds set of rotor pozitions given as a string.</summary>
		/// <param name="pozs">Comma separated string of valid rotor pozitions containing the same number of pozitions as the number of rotors.</param>
		/// <returns>False if error.</returns>
		public bool AddPozitionsUsingString(string pozs)
		{
			if (pozs == null)
			{
				return false;
			}
			if (pozs.Length == 0)
			{
				return false;
			}
			string[] numsAsTexts = pozs.Split(',');
			List<ushort> nums = new();
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
					Rotors[i].Pozitions.Add(nums[i]);
				}
			}
			return true;
		}
		/// <summary>Converts this instance to byte array.</summary>
		public byte[] ToBytes ()
		{
			List<byte> result = new(65536);
			result.AddRange(BitConverter.GetBytes(Codepage.Limit));
			result.AddRange(BitConverter.GetBytes(Name.Length));
			result.AddRange(Encoding.UTF32.GetBytes(Name));
			result.AddRange(BitConverter.GetBytes(Idx));
			result.AddRange(BitConverter.GetBytes(Rotors.Count));
			result.AddRange(TablesToBytes(Rotors));
			result.AddRange(BitConverter.GetBytes(Swaps.Count));
			result.AddRange(TablesToBytes(Swaps));
			result.AddRange(Reflector.ToBytes());
			result.AddRange(InputScrambler.ToBytes());
			result.AddRange(OutputScrambler.ToBytes());
			result.AddRange(BitConverter.GetBytes(ConstShift));
			result.AddRange(BitConverter.GetBytes(VarShift));
			result.AddRange(BitConverter.GetBytes(RandCharSpcMin));
			result.AddRange(BitConverter.GetBytes(RandCharSpcMax));
			result.AddRange(RandCharConstA.ToBytes());
			result.AddRange(RandCharConstB.ToBytes());
			result.AddRange(RandCharConstM.ToBytes());
			result.AddRange(SwitchConstA.ToBytes());
			result.AddRange(SwitchConstB.ToBytes());
			result.AddRange(SwitchConstC.ToBytes());
			result.AddRange(SwitchConstD.ToBytes());
			return result.ToArray();
		}
		/// <summary>Converts byte array to instance of this class.</summary>
		/// <param name="file">Byte array.</param>
		/// <param name="pozition">Starting pozition in array.</param>
		private void FromBytes(byte[] file, ref int pozition)
		{
			if (file == null)
			{
				return;
			}
			if (file.Length < MinSize)
			{
				return;
			}
			ushort limit = BitConverter.ToUInt16(file, pozition);
			pozition += 2;
			int nameLength = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Name = Encoding.UTF32.GetString(file, pozition, nameLength * 4);
			pozition += nameLength * 4;
			Idx = BitConverter.ToUInt32(file, pozition);
			pozition += 4;
			int tables = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Rotors.AddRange(TablesFromBytes(file, ref pozition, tables, limit));
			tables = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Swaps.AddRange(TablesFromBytes(file, ref pozition, tables, limit));
			Reflector = new Table (file, ref pozition, limit);
			InputScrambler = new Table(file, ref pozition, limit);
			OutputScrambler = new Table(file, ref pozition, limit);
			ConstShift = BitConverter.ToUInt32(file, pozition);
			pozition += 4;
			VarShift = BitConverter.ToUInt32(file, pozition);
			pozition += 4;
			RandCharSpcMin = BitConverter.ToUInt16(file, pozition);
			pozition += 2;
			RandCharSpcMax = BitConverter.ToUInt16(file, pozition);
			pozition += 2;
			PrimeDefinedConstant[] consts = new PrimeDefinedConstant[7];
			for (int i = 0; i < 7; i++)
			{
				consts[i] = new();
				consts[i].FromBytes(file, ref pozition);
			}
			RandCharConstA = consts[0];
			RandCharConstB = consts[1];
			RandCharConstM = consts[2];
			SwitchConstA = consts[3];
			SwitchConstB = consts[4];
			SwitchConstC = consts[5];
			SwitchConstD = consts[6];
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
			List<byte> result = new();
			for (int i = 0; i < Size; i++)
			{
				result.AddRange(BitConverter.GetBytes(PrimeIdxs[i]));
				result.Add(Exponents[i]);
			}
			return result.ToArray();
		}
		/// <summary>Fills this instance with decoded data.</summary>
		/// <param name="bytes">Set of bytes containing data for this class.</param>
		/// <param name="poz">Pozition in the data to start with.</param>
		public void FromBytes(byte[] bytes, ref int poz)
		{
			for (int i = 0; i < Size; i++)
			{
				PrimeIdxs[i] = BitConverter.ToInt32(bytes, poz);
				poz += 4;
				Exponents[i] = bytes[poz];
				poz++;
			}
		}
	}
	/// <summary>Stores sets of tables.</summary>
	public class TableLibrary : Settings_Base
	{
		/// <summary>All rotors.</summary>
		public List<Table> Rotors { get; private set; } = new();
		/// <summary>All reflectors.</summary>
		public List<Table> Reflectors { get; private set; } = new();
		/// <summary>All swaps (plugboards).</summary>
		public List<Table> Swaps { get; private set; } = new();
		/// <summary>All scramblers.</summary>
		public List<Table> Scramblers { get; private set; } = new();
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
			int pozition = 0;
			ushort lim = BitConverter.ToUInt16(file, pozition);
			pozition += 2;
			int tableCount = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Rotors.AddRange(TablesFromBytes(file, ref pozition, tableCount, lim));
			tableCount = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Reflectors.AddRange(TablesFromBytes(file, ref pozition, tableCount, lim));
			tableCount = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Swaps.AddRange(TablesFromBytes(file, ref pozition, tableCount, lim));
		}
		/// <summary>Converts this instance to byte array.</summary>
		/// <returns>Byte array representing this instance.</returns>
		public byte[] ToBytes()
		{
			List<byte> result = new(65536);
			result.AddRange(BitConverter.GetBytes(Codepage.Limit));
			result.AddRange(BitConverter.GetBytes(Rotors.Count));
			result.AddRange(TablesToBytes(Rotors));
			result.AddRange(BitConverter.GetBytes(Reflectors.Count));
			result.AddRange(TablesToBytes(Reflectors));
			result.AddRange(BitConverter.GetBytes(Swaps.Count));
			result.AddRange(TablesToBytes(Swaps));
			result.AddRange(BitConverter.GetBytes(Scramblers.Count));
			result.AddRange(TablesToBytes(Scramblers));
			return result.ToArray();
		}
		/// <summary>Gets the IDs of all tables in a selection.</summary>
		/// <param name="what">Which tables to get IDs from?</param>
		/// <returns>List of IDs.</returns>
		public static List<string> GetIDs(List<Table> what)
		{
			List<string> result = new();
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
		public Settings Select (List<ushort> rotors, List<ushort> swaps, int refl, ushort[] scramblers)
		{
			Settings s = new()
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
	public class SettingsLibrary : Settings_Base
	{
		/// <summary>Settings library.</summary>
		public List<Settings> Library { get; private set; } = new();
		/// <summary>Creates a new empty settings library.</summary>
		public SettingsLibrary()
		{

		}
		/// <summary>Crates a settings library from a file.</summary>
		public SettingsLibrary(byte[] file)
		{
			int pozition = 0;
			int count = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			for (int i = 0; i < count; i++)
			{
				Settings s = new(file, ref pozition);
				Library.Add(s);
			}
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
			List<byte> result = new(262144);
			result.AddRange(BitConverter.GetBytes(Library.Count));
			foreach (Settings s in Library)
			{
				result.AddRange(s.ToBytes());
			}
			return result.ToArray();
		}
	}
}