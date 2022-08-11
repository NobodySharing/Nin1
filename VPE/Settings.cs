using System;
using System.Collections.Generic;

namespace VPE
{
	/// <summary>Základní sdílená funkcionalita pro tabulky.</summary>
	public class Settings_Base
	{
		internal static List<byte> Single_TablesToBytes(List<Table> tables)
		{
			List<byte> result = new();
			foreach (Table table in tables)
			{
				result.AddRange(Single_TableToBytes(table));
			}
			return result;
		}

		internal static List<byte> Double_TablesToBytes(List<Table> tables)
		{
			List<byte> result = new();
			foreach (Table table in tables)
			{
				result.AddRange(Double_TableToBytes(table));
			}
			return result;
		}

		internal static List<byte> Single_TableToBytes(Table t)
		{
			List<byte> result = new()
			{
				t.GetFlags(),
				(byte)t.Pozition
			};
			result.AddRange(BitConverter.GetBytes(t.Idx));
			foreach (ushort item in t.MainTable)
			{
				result.Add((byte)item);
			}
			return result;
		}

		internal static List<byte> Double_TableToBytes(Table t)
		{
			List<byte> result = new()
			{
				t.GetFlags()
			};
			result.AddRange(BitConverter.GetBytes(t.Pozition));
			result.AddRange(BitConverter.GetBytes(t.Idx));
			foreach (ushort item in t.MainTable)
			{
				result.AddRange(BitConverter.GetBytes(item));
			}
			return result;
		}

		internal static List<Table> Single_TablesFromBytes(byte[] set, ref int pozition, int tableCount, ushort lim)
		{
			List<Table> result = new ();
			for (int i = 0; i < tableCount; i++)
			{
				result.Add(Single_TableFromBytes(set, ref pozition, lim));
			}
			return result;
		}

		internal static List<Table> Double_TablesFromBytes(byte[] set, ref int pozition, int tableCount, ushort lim)
		{
			List<Table> result = new ();
			for (int i = 0; i < tableCount; i++)
			{
				result.Add(Double_TableFromBytes(set, ref pozition, lim));
			}
			return result;
		}

		internal static Table Single_TableFromBytes(byte[] set, ref int pozition, ushort lim)
		{
			Table t = new();
			t.SetFlags(set[pozition]);
			pozition++;
			t.Pozition = set[pozition];
			pozition++;
			t.Idx = BitConverter.ToUInt16(set, pozition);
			pozition += 4;
			for (ushort j = 0; j < lim; j++)
			{
				t.MainTable.Add(set[pozition]);
				pozition++;
			}
			return t;
		}

		internal static Table Double_TableFromBytes(byte[] set, ref int pozition, ushort lim)
		{
			Table t = new();
			t.SetFlags(set[pozition]);
			pozition++;
			t.Pozition = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			t.Idx = BitConverter.ToUInt16(set, pozition);
			pozition += 4;
			for (ushort j = 0; j < lim; j++)
			{
				t.MainTable.Add(BitConverter.ToUInt16(set, pozition));
				pozition += 2;
			}
			return t;
		}

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
		public List<Table> Rotors { get; private set; } = new();
		public List<Table> Swaps { get; private set; } = new();
		public Table Reflector { get; set; }
		public ushort ConstShift { get; set; }
		public ushort VarShift { get; set; }
		public ushort RandCharSpcMin { get; set; }
		public ushort RandCharSpcMax { get; set; }
		public decimal RandCharConstA { get; set; }
		public decimal RandCharConstB { get; set; }
		public decimal RandCharConstM { get; set; }

		public Settings ()
		{

		}
		/// <summary>Vytvoří novou instanci třídy na základě načtených dat.</summary>
		/// <param name="file">Data ze souboru, co ukládá tuto třídu.</param>
		public Settings (byte[] file)
		{
			if (file == null)
			{
				return;
			}
			if (file.Length == 0)
			{
				return;
			}
			Rotors = new List<Table> ();
			Swaps = new List<Table> ();
			if (file[0] == 1)
			{
				Double_ByteDecode(file);
			}
			else
			{
				Single_ByteDecode(file);
			}
		}
		/// <summary>Inkrementuje čísla pozic. Nejnižší index značí nejvyšší řád.</summary>
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
		/// <summary>Převede celou instanci třídy na pole bytů.</summary>
		public byte[] ToBytes ()
		{
			List<byte> result = new(65536);
			bool size = Codepage.Limit <= 256;
			result.Add(size ? (byte)0 : (byte)1); // Pseudoverzovací číslo: pokud tam je maximálně 256 znaků, budu ukládat čísla z tabulek jako byte, pokud jich je víc, bude to jako ushort.
			if (size)
			{
				Double_ByteConv(ref result);
			}
			else
			{
				Single_ByteConv(ref result);
			}
			return result.ToArray();
		}
		/// <summary>Převede většinu třídy na pole bytů, počítá s počtem znaků méně jak 256, čísla jsou ukládány jako 1 byt.</summary>
		/// <param name="set">Sada bytů, kam budu přidávat a kterou budu měnit.</param>
		private void Single_ByteConv(ref List<byte> set)
		{
			List<byte[]> common = Common_ByteConv();
			set.Add((byte)Codepage.Limit);
			set.AddRange(common[0]);
			set.AddRange(Single_TablesToBytes(Rotors));
			set.AddRange(Single_TableToBytes(Reflector));
			set.AddRange(common[1]);
			set.AddRange(Single_TablesToBytes(Swaps));
			set.Add((byte)ConstShift);
			set.Add((byte)VarShift);
			set.AddRange(common[2]);
		}
		/// <summary>Převede většinu třídy na pole bytů, počítá s počtem znaků více jak 255, čísla jsou ukládány jako 2 byty.</summary>
		/// <param name="set">Sada bytů, kam budu přidávat a kterou budu měnit.</param>
		private void Double_ByteConv(ref List<byte> set)
		{
			List<byte[]> common = Common_ByteConv();
			set.AddRange(BitConverter.GetBytes(Codepage.Limit));
			set.AddRange(common[0]);
			set.AddRange(Double_TablesToBytes(Rotors));
			set.AddRange(Double_TableToBytes(Reflector));
			set.AddRange(common[1]);
			set.AddRange(Double_TablesToBytes(Swaps));
			set.AddRange(BitConverter.GetBytes(ConstShift));
			set.AddRange(BitConverter.GetBytes(VarShift));
			set.AddRange(common[2]);
		}
		/// <summary>Společná část převodu na pole bytů.</summary>
		/// <returns>3 části: 0: počet rotorů, 1: počet swapů, 2: zbytek na konci souboru.</returns>
		private List<byte[]> Common_ByteConv()
		{
			List<byte[]> result = new()
			{
				BitConverter.GetBytes(Rotors.Count),
				BitConverter.GetBytes(Swaps.Count),
			};
			List<byte> temp = new();
			temp.AddRange(BitConverter.GetBytes(RandCharSpcMin));
			temp.AddRange(BitConverter.GetBytes(RandCharSpcMax));
			temp.AddRange(DecimalToBytes(RandCharConstA));
			temp.AddRange(DecimalToBytes(RandCharConstB));
			temp.AddRange(DecimalToBytes(RandCharConstM));
			result.Add(temp.ToArray());
			return result;
		}
		/// <summary>Přečte informace z bytového pole a uloží je do současné instance třídy. Počítá s počtem znaků menším než 256.</summary>
		/// <param name="set">Zdrojové bytové pole.</param>
		private void Single_ByteDecode(byte[] set)
		{
			int pozition = 1;
			ushort lim = set[pozition];
			pozition++;
			int tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Rotors.AddRange(Single_TablesFromBytes(set, ref pozition, tableCount, lim));
			Reflector = Single_TableFromBytes(set, ref pozition, lim);
			tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Swaps.AddRange(Single_TablesFromBytes(set, ref pozition, tableCount, lim));
			ConstShift = set[pozition];
			pozition++;
			VarShift = set[pozition];
			pozition++;
			Common_ByteDecode(set, pozition);
		}
		/// <summary>Přečte informace z bytového pole a uloží je do současné instance třídy. Počítá s počtem znaků větším než 255.</summary>
		/// <param name="set">Zdrojové bytové pole.</param>
		private void Double_ByteDecode(byte[] set)
		{
			int pozition = 1;
			ushort lim = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			int tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Rotors.AddRange(Double_TablesFromBytes(set, ref pozition, tableCount, lim));
			Reflector = Double_TableFromBytes(set, ref pozition, lim);
			tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Swaps.AddRange(Double_TablesFromBytes(set, ref pozition, tableCount, lim));
			ConstShift = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			VarShift = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			Common_ByteDecode(set, pozition);
		}
		/// <summary>Společná funkcionalita pro čtení informací z bytového pole. Obsluhuje konec souboru.</summary>
		/// <param name="set">Zdrojové bytové pole.</param>
		/// <param name="pozition">Pozice, kde začíná část obsluhovaná touto metodou.</param>
		private void Common_ByteDecode (byte[] set, int pozition)
		{
			RandCharSpcMin = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			RandCharSpcMax = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			RandCharConstA = DecimalFromBytes(set, ref pozition);
			RandCharConstB = DecimalFromBytes(set, ref pozition);
			RandCharConstM = DecimalFromBytes(set, ref pozition);
		}
	}
	/// <summary>Ukládá množiny tabulek.</summary>
	public class SettingsStorage : Settings_Base
	{
		public List<Table> Swaps { get; private set; } = new();
		public List<Table> Rotors { get; private set; } = new();
		public List<Table> Reflectors { get; private set; } = new();

		public SettingsStorage ()
		{

		}

		public SettingsStorage(byte[] file)
		{
			if (file == null)
			{
				return;
			}
			if (file.Length == 0)
			{
				return;
			}
			if (file[0] == 1)
			{
				Double_ByteDecode(file);
			}
			else
			{
				Single_ByteDecode(file);
			}
		}

		public byte[] ToBytes()
		{
			List<byte> result = new(65536);
			bool size = Codepage.Limit <= 256;
			result.Add(size ? (byte)0 : (byte)1); // Pseudoverzovací číslo: pokud tam je maximálně 256 znaků, budu ukládat čísla z tabulek jako byte, pokud jich je víc, bude to jako ushort.
			if (size)
			{
				Double_ByteConv(ref result);
			}
			else
			{
				Single_ByteConv(ref result);
			}
			return result.ToArray();
		}

		private void Single_ByteConv(ref List<byte> set)
		{
			set.Add((byte)Codepage.Limit);
			set.AddRange(BitConverter.GetBytes(Rotors.Count));
			set.AddRange(Single_TablesToBytes(Rotors));
			set.AddRange(BitConverter.GetBytes(Reflectors.Count));
			set.AddRange(Single_TablesToBytes(Reflectors));
			set.AddRange(BitConverter.GetBytes(Swaps.Count));
			set.AddRange(Single_TablesToBytes(Swaps));
		}

		private void Double_ByteConv(ref List<byte> set)
		{
			set.AddRange(BitConverter.GetBytes(Codepage.Limit));
			set.AddRange(BitConverter.GetBytes(Rotors.Count));
			set.AddRange(Double_TablesToBytes(Rotors));
			set.AddRange(BitConverter.GetBytes(Reflectors.Count));
			set.AddRange(Double_TablesToBytes(Reflectors));
			set.AddRange(BitConverter.GetBytes(Swaps.Count));
			set.AddRange(Double_TablesToBytes(Swaps));
		}

		private void Single_ByteDecode(byte[] set)
		{
			int pozition = 1;
			ushort lim = set[pozition];
			pozition++;
			int tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Rotors.AddRange(Single_TablesFromBytes(set, ref pozition, tableCount, lim));
			tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Reflectors.AddRange(Single_TablesFromBytes(set, ref pozition, tableCount, lim));
			tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Swaps.AddRange(Single_TablesFromBytes(set, ref pozition, tableCount, lim));
		}

		private void Double_ByteDecode(byte[] set)
		{
			int pozition = 1;
			ushort lim = BitConverter.ToUInt16(set, pozition);
			pozition += 2;
			int tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Rotors.AddRange(Double_TablesFromBytes(set, ref pozition, tableCount, lim));
			tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Reflectors.AddRange(Double_TablesFromBytes(set, ref pozition, tableCount, lim));
			tableCount = BitConverter.ToInt32(set, pozition);
			pozition += 4;
			Swaps.AddRange(Double_TablesFromBytes(set, ref pozition, tableCount, lim));
		}

		public void Merge(SettingsStorage settingsStorage)
		{
			Swaps.AddRange(settingsStorage?.Swaps);
			Rotors.AddRange(settingsStorage?.Rotors);
			Reflectors.AddRange(settingsStorage?.Reflectors);
		}

		public Settings Select (List<ushort> tables, List<ushort> swaps, int refl)
		{
			Settings s = new()
			{
				Reflector = Reflectors[refl],
			};
			foreach (ushort i in tables)
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
}