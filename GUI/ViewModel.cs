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
		private readonly Generators Generator = new(Codepage.Limit, DateTime.Now.Ticks);
		public SettingsLibrary SL = new();
		public TableLibrary TL = new();
		public Settings ActiveSett;
		public Crypto C;
		public bool Overwrite = false;
		#endregion
		#region Classes for binding
		internal C_VPE_Sett DataFromGUI_Sett = new();
		internal List<C_UC_Rotor> DataFromGUI_Sett_Rotors = new();
		internal List<C_VPE_ComboBox> DataFromGUI_Swaps = new();
		internal C_VPE_ComboBox DataFromGUI_Refl = new();
		internal C_VPE_ComboBox DataFromGUI_SettSel = new();
		#endregion
		#region Private constants for file filtering
		private const string Invalid_File = "N/A";
		private const string VPESL_filter = "VPE Settings Library files (*.vpesl)|*.vpesl";
		private const string VPETL_filter = "VPE Table Library files (*.vpetl)|*.vpetl";
		private const string VPES_filter = "VPE Settings files (*.vpes)|*.vpes";
		private const string TXT_filter = "Text files (*.txt)|*.txt";
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
				Generator.UpdateSeed(DateTime.Now.Ticks);
				TL.Rotors.Add(Generator.GenerateTable((ushort)TL.Rotors.Count));
			}
		}

		public void GenerateReflector(uint count = 5)
		{
			for (uint i = 0; i < count; i++)
			{
				Generator.UpdateSeed(DateTime.Now.Ticks);
				TL.Reflectors.Add(Generator.GeneratePairs((ushort)TL.Reflectors.Count));
			}
		}

		public void GenerateSwaps(uint count = 10)
		{
			for (uint i = 0; i < count; i++)
			{
				Generator.UpdateSeed(DateTime.Now.Ticks);
				TL.Swaps.Add(Generator.GeneratePairsWithSkips((ushort)TL.Swaps.Count));
			}
		}
		/// <summary>Generates complete settings, adds them to library and sets the GUI accordingly.</summary>
		public void GenerateComplete()
		{
			ActiveSett = Generator.GenerateSetts();
			AddSettsToLib();
		}
		/// <summary>Sets active settings using what is in GUI.</summary>
		public void ChangeActiveSettsFromGUI()
		{
			List<ushort> rotors = new(), pozs = new(), swaps = new();
			foreach (C_UC_Rotor rotor in DataFromGUI_Sett_Rotors)
			{ // Potencially problematic:
				rotors.Add(rotor.SelectedRNum.Value);
				pozs.Add(rotor.PozitionNum.Value);
			}
			foreach (C_VPE_ComboBox swap in DataFromGUI_Swaps)
			{ // Potencially problematic:
				swaps.Add(swap.SelectedNum.Value);
			}
			ActiveSett = TL.Select(rotors, swaps, DataFromGUI_Refl.SelectedNum.Value);
			ActiveSett.ChangePozitions(pozs);
			ActiveSett.Name = DataFromGUI_Sett.NameStr;
			ActiveSett.ConstShift = DataFromGUI_Sett.ConstShiftNum.Value;
			ActiveSett.VarShift = DataFromGUI_Sett.VarShiftNum.Value;
			ActiveSett.RandCharConstA = DataFromGUI_Sett.RandCharGenANum.Value;
			ActiveSett.RandCharConstB = DataFromGUI_Sett.RandCharGenBNum.Value;
			ActiveSett.RandCharConstM = DataFromGUI_Sett.RandCharGenMNum.Value;
			ActiveSett.RandCharSpcMin = DataFromGUI_Sett.RandCharSpcMinNum.Value;
			ActiveSett.RandCharSpcMax = DataFromGUI_Sett.RandCharSpcMaxNum.Value;
			ActiveSett.SwitchConstAIdx = DataFromGUI_Sett.SwitchANum.Value;
			ActiveSett.SwitchConstBIdx = DataFromGUI_Sett.SwitchBNum.Value;
		}

		public ushort GenerateRandNum()
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			return Generator.GenerateNum();
		}

		public void GenerateSpaceMinMax(ushort MinFrom = 2, ushort MinTo = 8, ushort MaxFrom = 10, ushort MaxTo = 20)
		{
			ushort[] space = Generator.GenerateSpaceMinMax(MinFrom, MinTo, MaxFrom, MaxTo);
			DataFromGUI_Sett.RandCharSpcMinStr = space[0].ToString();
			DataFromGUI_Sett.RandCharSpcMaxStr = space[1].ToString();
		}

		public void GenerateRNDConsts()
		{
			decimal[] consts = Generator.GenerateABM();
			DataFromGUI_Sett.RandCharGenAStr = consts[0].ToString();
			DataFromGUI_Sett.RandCharGenBStr = consts[1].ToString();
			DataFromGUI_Sett.RandCharGenMStr = consts[2].ToString();
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
			return FileHandling.LoadText(OpenFile(TXT_filter));
		}

		public void SaveMsgFile(string text)
		{
			FileHandling.SaveText(SaveFile(TXT_filter), text);
		}

		public void SettGen()
		{
			ActiveSett = Generator.GenerateSetts();
			AddSettsToLib();
		}
		/// <summary>Sets the GUI and active settings using what was selected in settings selector.</summary>
		public void SetUsingSelSettName()
		{
			ActiveSett = SL.Library.Find(x => x.Name == DataFromGUI_SettSel.SelectedStr);
		}

		public void InitializeRotorSelectors(ushort count)
		{
			for (ushort i = 0; i < count; i++)
			{
				C_UC_Rotor rotor = new();
				DataFromGUI_Sett_Rotors.Add(rotor);
			}
		}

		public void InitializeSwapSelectors(ushort count)
		{
			for (ushort i = 0; i < count; i++)
			{
				C_VPE_ComboBox swap = new();
				DataFromGUI_Swaps.Add(swap);
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

		public void SaveSettings()
		{
			FileHandling.Save(ActiveSett, SaveFile(VPES_filter));
		}

		public void LoadSettings()
		{
			FileHandling.Load(OpenFile(VPES_filter), out Settings s);
			if (Overwrite)
			{
				ActiveSett = s;
			}
			AddSettsToLib(s);
		}

		public void SaveSettingsLib()
		{
			FileHandling.Save(SL, SaveFile(VPESL_filter));
		}

		public void LoadSettingsLib()
		{
			if (Overwrite)
			{
				SL.Library.Clear();
			}
			FileHandling.Load(OpenFile(VPESL_filter), out SettingsLibrary sl);
			foreach (Settings s in sl.Library)
			{
				AddSettsToLib(s);
			}
		}

		public void SaveTableLib()
		{
			FileHandling.Save(TL, SaveFile(VPETL_filter));
		}

		public void LoadTableLib()
		{
			if (Overwrite)
			{
				TL.Reflectors.Clear();
				TL.Swaps.Clear();
				TL.Rotors.Clear();
			}
			FileHandling.Load(OpenFile(VPETL_filter), out TableLibrary tl);
			TL.Reflectors.AddRange(tl.Reflectors);
			TL.Swaps.AddRange(tl.Swaps);
			TL.Rotors.AddRange(tl.Rotors);
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
		/// <summary>Adds active settings to library.</summary>
		private void AddSettsToLib()
		{
			SL.Library.Add(ActiveSett);
			TL.Reflectors.Add(ActiveSett.Reflector);
			TL.Rotors.AddRange(ActiveSett.Rotors);
			TL.Swaps.AddRange(ActiveSett.Swaps);
		}
		/// <summary>Adds supplied settings to library.</summary>
		private void AddSettsToLib(Settings s)
		{
			SL.Library.Add(s);
			TL.Reflectors.Add(s.Reflector);
			TL.Rotors.AddRange(s.Rotors);
			TL.Swaps.AddRange(s.Swaps);
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