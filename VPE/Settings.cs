using System;
using System.Text;
using System.Collections.Generic;
using System.Numerics;
using Common;

namespace VPE
{
	/// <summary>Základní sdílená funkcionalita pro nastavení</summary>
	public class Settings_Base
	{
		/// <summary>Převede list tabulek na pole bytů. 2 bytové položky.</summary>
		/// <param name="tables">List tabulek.</param>
		/// <returns>List bytů představující list tabulek.</returns>
		internal static List<byte> TablesToBytes(List<Table> tables)
		{
			List<byte> result = new();
			foreach (Table table in tables)
			{
				result.AddRange(table.ToBytes());
			}
			return result;
		}
		/// <summary>Dekóduje tabulky z bytů (s 2 bytovými položkami), posouvá pozici v sadě bytů.</summary>
		/// <param name="set">Sada bytů na dekódování.</param>
		/// <param name="pozition">Pozice v sadě.</param>
		/// <param name="tableCount">Počet tabulek.</param>
		/// <param name="limit">Počet položek v tabulce.</param>
		/// <returns>Sada tabulek.</returns>
		internal static List<Table> TablesFromBytes(byte[] set, ref int pozition, int tableCount, ushort limit)
		{
			List<Table> result = new ();
			for (int i = 0; i < tableCount; i++)
			{
				result.Add(TableFromBytes(set, ref pozition, limit));
			}
			return result;
		}

		internal static Table TableFromBytes(byte[] set, ref int pozition, ushort limit)
		{
			Table t = new()
			{
				Idx = BitConverter.ToUInt32(set, pozition)
			};
			pozition += 4;
			t.SetFlags(set[pozition]);
			pozition++;
			t.Pozition = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			t.StartPozition = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			for (ushort j = 0; j < limit; j++)
			{
				t.MainTable.Add(BitConverter.ToUInt16(set, pozition));
				pozition += 2;
			}
			return t;
		}
		/// <summary>Enkóduje decimal do pole 16 bytů.</summary>
		/// <param name="number">Číslo.</param>
		/// <returns>Pole 16 bytů.</returns>
		internal static byte[] DecimalToBytes (decimal number)
		{
			byte[] result = new byte[16];
			int[] temp = decimal.GetBits(number);
			int i = 0;
			foreach (int t in temp)
			{
				byte[] b = BitConverter.GetBytes(t);
				for (byte j = 0; j < 4; j++)
				{
					result[i++] = b[j];
				}
			}
			return result;
		}
		/// <summary>Dekóduje decimal číslo ze sady bytů.</summary>
		/// <param name="set">Sada bytů.</param>
		/// <param name="pozition">Pozice, kde začít.</param>
		/// <returns>Decimal číslo.</returns>
		internal static decimal DecimalFromBytes(byte[] set, ref int pozition)
		{
			int[] temp = new int[4];
			for (int i = 0; i < 4; i++)
			{
				temp[i] = BitConverter.ToInt32(set, pozition);
				pozition += 4;
			}
			return new decimal(temp);
		}
	}
	public class Settings : Settings_Base
	{
		/// <summary>Všechny rotory.</summary>
		public List<Table> Rotors { get; private set; } = new();
		/// <summary>Všechny swapy (plugboard).</summary>
		public List<Table> Swaps { get; private set; } = new();
		/// <summary>Jediný reflektor.</summary>
		public Table Reflector { get; set; }
		/// <summary>Konstanta pro pevný posun.</summary>
		public uint ConstShift { get; set; }
		/// <summary>Konstanta pro proměnný posun.</summary>
		public uint VarShift { get; set; }
		/// <summary>Minimální velikost mezery mezi vloženými náhodnými znaky.</summary>
		public ushort RandCharSpcMin { get; set; }
		/// <summary>Maximální velikost mezery mezi vloženými náhodnými znaky.</summary>
		public ushort RandCharSpcMax { get; set; }
		/// <summary>Konstanta A generátoru náhodných čísel rovnice y = (ax + b) % m. Pro výpočet délky mezery mezi přidanými náhodnými znaky.</summary>
		public PrimeDefinedConstant RandCharConstA { get; set; }
		/// <summary>Konstanta B generátoru náhodných čísel rovnice y = (ax + b) % m. Pro výpočet délky mezery mezi přidanými náhodnými znaky.</summary>
		public PrimeDefinedConstant RandCharConstB { get; set; }
		/// <summary>Konstanta M generátoru náhodných čísel rovnice y = (ax + b) % m. Pro výpočet délky mezery mezi přidanými náhodnými znaky.</summary>
		public PrimeDefinedConstant RandCharConstM { get; set; }
		/// <summary>A constant for table creation for character switching. Part of the first pair.</summary>
		public PrimeDefinedConstant SwitchConstA { get; set; }
		/// <summary>B constant for table creation for character switching. Part of the first pair.</summary>
		public PrimeDefinedConstant SwitchConstB { get; set; }
		/// <summary>C constant for table creation for character switching. Part of the second pair.</summary>
		public PrimeDefinedConstant SwitchConstC { get; set; }
		/// <summary>D constant for table creation for character switching. Part of the second pair.</summary>
		public PrimeDefinedConstant SwitchConstD { get; set; }
		/// <summary>Jméno nastavení.</summary>
		public string Name { get; set; }
		/// <summary>Index nastavení.</summary>
		public uint Idx { get; set; }
		/// <summary>Smallest size of this class instance. (2 034)</summary>
		private const int MinSize = 182 * 2 + 182 * 2 + 182 * 4 + 4 + 2 + 2 + 2 + 80 + 80 + 80 + 80 + 80 + 80 + 80 + 4 + 4;
		public Settings ()
		{

		}
		/// <summary>Vytvoří novou instanci třídy na základě načtených dat.</summary>
		/// <param name="file">Data ze souboru, co ukládá tuto třídu.</param>
		public Settings (byte[] file)
		{
			int pozition = 0;
			FromBytes (file, ref pozition);
		}
		/// <summary>Vytvoří novou instanci třídy na základě načtených dat. Pro načtení mnoha instancí třídy.</summary>
		/// <param name="file">Data ze souboru, co ukládá tuto třídu.</param>
		/// <param name="pozition">Pozice v souboru.</param>
		public Settings(byte[] file, ref int pozition)
		{
			FromBytes(file, ref pozition);
		}
		/// <summary>Inkrementuje čísla pozic rotorů. Nejnižší index značí nejvyšší řád.</summary>
		public void IncrementPozitions()
		{
			int Last = Rotors.Count - 1;
			Rotors[Last].Pozition++;
			for (int i = Last; i >= 0; i--)
			{
				if (Rotors[i].Pozition >= Codepage.Limit)
				{
					Rotors[i].Pozition = 0;
					if (i > 0)
					{
						Rotors[i - 1].Pozition++;
					}
				}
			}
		}
		/// <summary>Ahtualizuje startovací pozice na současné pozice.</summary>
		public void UpdateStartPozitions()
		{
			foreach (Table t in Rotors)
			{
				t.StartPozition = t.Pozition;
			}
		}
		/// <summary>Aktualizuje pozice rotorů na specifikované.</summary>
		/// <param name="pozitions">Seznam pozic. Měl by být stejný jak počet rotorů.</param>
		/// <param name="includeStart">Zahrnout i startovní pozice?</param>
		/// <returns>False pokud počet pozic je jiný než počet rotorů.</returns>
		public bool ChangePozitions(List<ushort> pozitions, bool includeStart = false)
		{
			if (pozitions is null)
			{
				return false;
			}
			if (pozitions.Count != Rotors.Count)
			{
				return false;
			}
			if (includeStart)
			{
				for (int i = 0; i < pozitions.Count; i++)
				{
					Rotors[i].Pozition = Rotors[i].StartPozition = pozitions[i];
				}
			}
			else
			{
				for (int i = 0; i < pozitions.Count; i++)
				{
					Rotors[i].Pozition = pozitions[i];
				}
			}
			return true;
		}
		/// <summary>Převede celou instanci třídy na pole bytů.</summary>
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
		/// <summary>Dekóduje bytové pole na instanci této třídy.</summary>
		/// <param name="file">Bytové pole.</param>
		/// <param name="pozition">Pozice v souboru.</param>
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
			int rotors = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Rotors.AddRange(TablesFromBytes(file, ref pozition, rotors, limit));
			rotors = BitConverter.ToInt32(file, pozition);
			pozition += 4;
			Swaps.AddRange(TablesFromBytes(file, ref pozition, rotors, limit));
			Reflector = TableFromBytes(file, ref pozition, limit);
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
		public byte[] Powers = new byte[Size];
		public PrimeDefinedConstant()
		{
			for (int i = 0; i < Size; i++)
			{
				PrimeIdxs[i] = -1;
			}
		}
		/// <summary>Computes the constant represented by given primes and their powers.</summary>
		/// <returns>Really big number. At least that is the idea. With all digits.</returns>
		public BigInteger ComputeConstant()
		{
			BigInteger result = new(1);
			for (int i = 0; i < Size; i++)
			{
				if (PrimeIdxs[i] >= PrimeList.First1Digit || PrimeIdxs[i] <= PrimeList.Last6Digit)
				{
					result *= BigInteger.Pow((BigInteger)PrimeList.Primes[PrimeIdxs[i]], Powers[i]);
				}
			}
			return result;
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
				result.Add(Powers[i]);
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
				Powers[i] = bytes[poz];
				poz++;
			}
		}
	}
	/// <summary>Ukládá množiny tabulek.</summary>
	public class TableLibrary : Settings_Base
	{
		public List<Table> Swaps { get; private set; } = new();
		public List<Table> Rotors { get; private set; } = new();
		public List<Table> Reflectors { get; private set; } = new();

		public TableLibrary ()
		{

		}

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
			int pozition = 1;
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

		public byte[] ToBytes()
		{
			List<byte> result = new(65536);
			bool size = Codepage.Limit <= 256;
			result.Add(size ? (byte)0 : (byte)1); // Pseudoverzovací číslo: pokud tam je maximálně 256 znaků, budu ukládat čísla z tabulek jako byte, pokud jich je víc, bude to jako ushort.
			result.AddRange(BitConverter.GetBytes(Codepage.Limit));
			result.AddRange(BitConverter.GetBytes(Rotors.Count));
			result.AddRange(TablesToBytes(Rotors));
			result.AddRange(BitConverter.GetBytes(Reflectors.Count));
			result.AddRange(TablesToBytes(Reflectors));
			result.AddRange(BitConverter.GetBytes(Swaps.Count));
			result.AddRange(TablesToBytes(Swaps));
			return result.ToArray();
		}

		public void Merge(TableLibrary settingsStorage)
		{
			Swaps.AddRange(settingsStorage?.Swaps);
			Rotors.AddRange(settingsStorage?.Rotors);
			Reflectors.AddRange(settingsStorage?.Reflectors);
		}

		public Settings Select (List<ushort> rotors, List<ushort> swaps, int refl)
		{
			Settings s = new()
			{
				Reflector = Reflectors[refl],
			};
			foreach (ushort i in rotors)
			{
				s.Rotors.Add(Rotors[i]);
			}
			foreach (ushort i in swaps)
			{
				s.Swaps.Add(Rotors[i]);
			}
			return s;
		}
	}
	/// <summary>Knihovna nastavení. Slouží k uchovávání množiny nastavení.</summary>
	public class SettingsLibrary : Settings_Base
	{
		public List<Settings> Library { get; private set; } = new();

		public SettingsLibrary()
		{

		}

		public SettingsLibrary(byte[] file)
		{
			int pozition = 0;
			while (pozition <= file.Length)
			{
				Settings s = new (file, ref pozition);
				Library.Add(s);
			}
		}
		/// <summary>Znovu zindexuje všechny nastavení.</summary>
		public void ReIndexSetts ()
		{
			for (int i = 0; i < Library.Count; i++)
			{
				Library[i].Idx = Convert.ToUInt32(i);
			}
		}
		/// <summary>Převede celou instanci třídy na bytové pole.</summary>
		public byte[] ToBytes()
		{
			List<byte> result = new(262144);
			foreach (Settings s in Library)
			{
				result.AddRange(s.ToBytes());
			}
			return result.ToArray();
		}
	}
}