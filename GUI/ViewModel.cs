using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

using VPE;
using Factorizator;
using NeueDT;
using DTcalc;

namespace GUI
{
	public class VPE_VM
	{
		#region Variables
		private Generators Generator = new(Codepage.Limit, DateTime.Now.Ticks);
		public SettingsLibrary SL = new();
		public TableLibrary TL = new();
		public Settings ActiveSett;
		public Crypto C;

		internal C_VPE_Sett DataFromGUI_Sett = new();
		internal List<C_UC_Rotor> DataFromGUI_Sett_Rotors = new();
		internal C_VPE_SettSel DataFromGUI_SettSel = new();

		private const string Invalid_File = "N/A";
		private const string VPESL_filter = "VPE Settings Library files (*.vpesl)|*.vpesl";
		private const string VPETL_filter = "VPE Table Library files (*.vpetl)|*.vpetl";
		private const string VPES_filter = "VPE Settings files (*.vpes)|*.vpes";
		private const string TXT_filter = "Text files (*.txt)|*.txt";
		public ushort RotorCNTR = 0, ReflCNTR = 0, SwapCNTR = 0;
		public List<uint> Rotors = new();
		public List<ushort> Refls = new();
		public List<ushort> Swaps = new();
		#endregion

		#region Public methods
		/// <summary>Vygeneruje rotory.</summary>
		/// <param name="count">Kolik rotorů?</param>
		/// <returns>Seznam indexů vygenerovaných.</returns>
		public void GenerateRotors(uint count = 20)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			for (uint i = 0; i < count; i++)
			{
				TL.Rotors.Add(Generator.GenerateTable(RotorCNTR));
				Rotors.Add(RotorCNTR);
				RotorCNTR++;
			}
		}

		public void GenerateReflector(uint count = 5)
		{
			for (uint i = 0; i < count; i++)
			{
				Generator.UpdateSeed(DateTime.Now.Ticks);
				TL.Reflectors.Add(Generator.GeneratePairs(ReflCNTR));
				ReflCNTR++;
			}
		}

		public void GenerateSwaps(uint count = 10)
		{
			for (uint i = 0; i < count; i++)
			{
				Generator.UpdateSeed(DateTime.Now.Ticks);
				TL.Swaps.Add(Generator.GeneratePairsWithSkips(SwapCNTR));
				SwapCNTR++;
			}
		}
		/// <summary>Vygeneruje kompletní nastavení.</summary>
		public void GenerateComplete()
		{
			ActiveSett = Generator.GenerateSetts();
			SL.Library.Add(ActiveSett);
			TL.Rotors.AddRange(ActiveSett.Rotors);
			TL.Reflectors.Add(ActiveSett.Reflector);
			TL.Swaps.AddRange(ActiveSett.Swaps);
		}

		public void SelectRotors(List<ushort> tables, List<ushort> swaps, ushort reflector)
		{
			ActiveSett = TL.Select(tables, swaps, reflector);
		}

		public void LoadTables()
		{
			TL = FileHandling.ReadAll(OpenFile(VPETL_filter));
		}

		public void LoadAndMerge()
		{
			TL.Merge(FileHandling.ReadAll(OpenFile(VPETL_filter)));
		}

		public void LoadSpecific()
		{
			ActiveSett = FileHandling.ReadSpecific(OpenFile(VPES_filter));
		}

		public void SaveTables()
		{
			string folder = GetFolder(SaveFile(VPETL_filter));
			if (folder != "N/A")
			{
				FileHandling.Save(TL, folder);
			}
		}

		public void SaveSpecific()
		{
			string folder = GetFolder(SaveFile(VPES_filter));
			if (folder != "N/A")
			{
				FileHandling.Save(ActiveSett, folder);
			}
		}

		public ushort GenerateRandNum()
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			return Generator.GenerateNum();
		}

		public ushort[] GenerateSpaceMinMax(ushort MinFrom = 4, ushort MinTo = 14, ushort MaxFrom = 16, ushort MaxTo = 30)
		{
			return Generator.GenerateSpaceMinMax(MinFrom, MinTo, MaxFrom, MaxTo);
		}

		public decimal[] GenerateRNDConsts()
		{
			return Generator.GenerateABM();
		}

		public string Encrypt(string inText)
		{
			C = new(ActiveSett);
			return C?.Encypt(inText);
		}

		public string Decrypt(string inText)
		{
			C = new(ActiveSett);
			return C?.Decypt(inText);
		}

		public string OpenMsgFile()
		{
			return FileHandling.ReadText(OpenFile(TXT_filter));
		}

		public void SaveMsgFile(string text)
		{
			FileHandling.SaveText(SaveFile(TXT_filter), text);
		}

		public void QuickSettGen()
		{
			ActiveSett = Generator.GenerateSetts();
		}

		public void QuickSettSave()
		{
			FileHandling.Save(ActiveSett, SaveFile(VPES_filter));
		}

		public void QuickSettOpen()
		{
			ActiveSett = FileHandling.ReadSpecific(OpenFile(VPES_filter));
		}

		public void InitializeRotorSelectors(ushort count)
		{
			for (ushort i = 0; i < count; i++)
			{
				C_UC_Rotor rotor = new();
				DataFromGUI_Sett_Rotors.Add(rotor);
			}
		}

		public void UpdateRotorSelectors()
		{
			List<string> rotorNums = new();
			foreach (Table rotor in TL.Rotors)
			{
				rotorNums.Add(rotor.Idx.ToString());
			}
			foreach (C_UC_Rotor rotorList in DataFromGUI_Sett_Rotors)
			{
				rotorList.RotorsStrs.Clear();
				rotorList.RotorsStrs = rotorNums;
			}
		}
		#endregion

		#region Private methods
		/// <summary>Zobrazí dialog otevření souboru a vrátí cestu k němu.</summary>
		/// <param name="ext">Extensiona.</param>
		/// <returns>Cesta k souboru.</returns>
		private string OpenFile(string ext)
		{
			OpenFileDialog OFD = new()
			{
				Filter = ext
			};
			bool? dia = OFD.ShowDialog();
			if (dia is not null)
			{
				if (dia.Value)
				{
					return OFD.FileName;
				}
			}
			return Invalid_File;
		}
		/// <summary>Zobrazí dialog uložení souboru a vrátí cestu k němu.</summary>
		/// <param name="ext">Extensiona.</param>
		/// <returns>Cesta k souboru.</returns>
		private string SaveFile(string ext)
		{
			SaveFileDialog SFD = new()
			{
				Filter = ext
			};
			bool? dia = SFD.ShowDialog();
			if (dia is not null)
			{
				if (dia.Value)
				{
					return SFD.FileName;
				}
			}
			return Invalid_File;
		}
		/// <summary>Extrahuje cestu ke složce, ve které je soubor.</summary>
		/// <param name="path">Soubor (cesta k).</param>
		/// <returns>Složka, ve které je soubor.</returns>
		private string GetFolder(string path)
		{
			if (path != Invalid_File)
			{
				int idx = path.LastIndexOf('\\');
				return path[..(idx + 1)];
			}
			return path;
		}
		#endregion
	}

	public class NDT_VM
	{

	}

	public class DTC_VM
	{

	}

	public class Fact_VM
	{

	}
}