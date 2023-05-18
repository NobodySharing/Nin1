using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Win32;

using VPE;
using Factorizator;
using NeueDT;
using DTcalc;
using System.Windows.Media;
using System.Collections.ObjectModel;

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
		internal List<C_UC_Rotor> DataFromGUI_Rotors = new();
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
		
		public void GenerateRotors(uint count = 20)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			for (uint i = 0; i < count; i++)
			{
				TL.Rotors.Add(Generator.GenerateTable((ushort)TL.Rotors.Count));
			}
		}

		public void GenerateReflector(uint count = 5)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			for (uint i = 0; i < count; i++)
			{
				TL.Reflectors.Add(Generator.GeneratePairs((ushort)TL.Reflectors.Count));
			}
		}

		public void GenerateSwaps(uint count = 10)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			for (uint i = 0; i < count; i++)
			{
				TL.Swaps.Add(Generator.GeneratePairsWithSkips((ushort)TL.Swaps.Count, Generator.GenerateDoubleInRange(9d / 16d, 950d / 1024d)));
			}
		}
		/// <summary>Generates complete settings, adds them to library and sets the GUI accordingly.</summary>
		public void GenerateComplete()
		{
			ActiveSett = Generator.GenerateSetts();
			ActiveSett.UpdateStartPozitions();
			AddSettsToLib();
		}
		/// <summary>Sets active settings using what is in GUI.</summary>
		public void ChangeActiveSettsFromGUI()
		{
			List<ushort> rotors = new(), pozs = new(), swaps = new();
			foreach (C_UC_Rotor rotor in DataFromGUI_Rotors)
			{
				rotors.Add(rotor.SelectedRNum.Value);
				pozs.Add(rotor.PozitionNum.Value);
			}
			foreach (C_VPE_ComboBox swap in DataFromGUI_Swaps)
			{
				swaps.Add(swap.SelectedNum.Value);
			}
			ActiveSett = TL.Select(rotors, swaps, DataFromGUI_Refl.SelectedNum.Value);
			ActiveSett.ChangePozitions(pozs);
			ActiveSett.Name = DataFromGUI_Sett.NameStr;
			ActiveSett.ConstShift = DataFromGUI_Sett.ConstShiftNum.Value;
			ActiveSett.VarShift = DataFromGUI_Sett.VarShiftNum.Value;
			ActiveSett.RandCharSpcMin = DataFromGUI_Sett.RandCharSpcMinNum.Value;
			ActiveSett.RandCharSpcMax = DataFromGUI_Sett.RandCharSpcMaxNum.Value;
			ActiveSett.RandCharConstA = CopyPDCDataFromGUI(DataFromGUI_Sett.RandCharA);
			ActiveSett.RandCharConstB = CopyPDCDataFromGUI(DataFromGUI_Sett.RandCharB);
			ActiveSett.RandCharConstM = CopyPDCDataFromGUI(DataFromGUI_Sett.RandCharM);
			ActiveSett.SwitchConstA = CopyPDCDataFromGUI(DataFromGUI_Sett.SwitchA);
			ActiveSett.SwitchConstB = CopyPDCDataFromGUI(DataFromGUI_Sett.SwitchB);
			ActiveSett.SwitchConstC = CopyPDCDataFromGUI(DataFromGUI_Sett.SwitchC);
			ActiveSett.SwitchConstD = CopyPDCDataFromGUI(DataFromGUI_Sett.SwitchD);
		}

		public void UpdateActiveSettsFromGUI()
		{
			ChangeActiveSettsFromGUI(); // For now.
			// ToDo: Implement.
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

		public void GenerateABMConsts()
		{
			PrimeDefinedConstant[] consts = Generator.GenerateABM();
			DataFromGUI_Sett.RandCharGenAStr = consts[0].ToString();
			DataFromGUI_Sett.RandCharGenBStr = consts[1].ToString();
			DataFromGUI_Sett.RandCharGenMStr = consts[2].ToString();
		}

		public void GenerateABCDConsts()
		{
			PrimeDefinedConstant[] consts = Generator.GenerateABCD();
			DataFromGUI_Sett.SwitchAStr = consts[0].ToString();
			DataFromGUI_Sett.SwitchBStr = consts[1].ToString();
			DataFromGUI_Sett.SwitchCStr = consts[2].ToString();
			DataFromGUI_Sett.SwitchDStr = consts[3].ToString();
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
		/// <summary>Sets the active settings using what was selected in settings selector.</summary>
		public void SetUsingSelSettName()
		{
			ActiveSett = SL.Library.Find(x => x.Name == DataFromGUI_SettSel.SelectedStr);
		}

		public void InitializeRotorSelectors(ushort count)
		{
			for (ushort i = 0; i < count; i++)
			{
				C_UC_Rotor rotor = new();
				DataFromGUI_Rotors.Add(rotor);
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

		public void SynchronizeRotorDataForGUI()
		{
			List<string> rotors = TableLibrary.GetIDs(TL.Rotors);
			if (DataFromGUI_Rotors.Count != ActiveSett.Rotors.Count)
			{
				if (DataFromGUI_Rotors.Count < ActiveSett.Rotors.Count)
				{
					while(DataFromGUI_Rotors.Count != ActiveSett.Rotors.Count)
					{
						DataFromGUI_Rotors.Add(new());
					}
				}
				else
				{
					while(DataFromGUI_Rotors.Count != ActiveSett.Rotors.Count)
					{
						DataFromGUI_Rotors.RemoveAt(DataFromGUI_Rotors.Count - 1);
					}
				}
			}
			for (int i = 0; i < ActiveSett.Rotors.Count; i++)
			{
				DataFromGUI_Rotors[i].RotorsStrs = rotors;
				DataFromGUI_Rotors[i].PozitionStr = ActiveSett.Rotors[i].Pozition.ToString();
				DataFromGUI_Rotors[i].SelectedRIdx = (int)ActiveSett.Rotors[i].Idx;
				DataFromGUI_Rotors[i].SelectedRStr = ActiveSett.Rotors[i].Idx.ToString();
			}
		}

		public void SynchronizeSwapDataForGUI()
		{
			List<string> swaps = TableLibrary.GetIDs(TL.Swaps);
			if (DataFromGUI_Swaps.Count != ActiveSett.Swaps.Count)
			{
				if (DataFromGUI_Swaps.Count < ActiveSett.Swaps.Count)
				{
					while (DataFromGUI_Swaps.Count != ActiveSett.Swaps.Count)
					{
						DataFromGUI_Swaps.Add(new());
					}
				}
				else
				{
					while (DataFromGUI_Swaps.Count != ActiveSett.Swaps.Count)
					{
						DataFromGUI_Swaps.RemoveAt(DataFromGUI_Swaps.Count - 1);
					}
				}
			}
			for (int i = 0; i < ActiveSett.Swaps.Count; i++)
			{
				DataFromGUI_Swaps[i].ItemsStrs = swaps;
				DataFromGUI_Swaps[i].SelectedStr = ActiveSett.Swaps[i].Idx.ToString();
			}
		}

		public void SynchronizeReflectorDataForGUI()
		{
			List<string> refls = TableLibrary.GetIDs(TL.Reflectors);
			DataFromGUI_Refl.ItemsStrs = refls;
			DataFromGUI_Refl.SelectedStr = ActiveSett.Reflector.Idx.ToString();
		}

		public void UpdateRotorSelectors()
		{
			List<string> rotorNums = new();
			foreach (Table rotor in TL.Rotors)
			{
				rotorNums.Add(rotor.Idx.ToString());
			}
			foreach (C_UC_Rotor rotorList in DataFromGUI_Rotors)
			{
				rotorList.RotorsStrs.Clear();
				rotorList.RotorsStrs = rotorNums;
			}
		}

		public void UpdateReflSelector()
		{
			List<string> reflNums = new();
			foreach (Table refl in TL.Reflectors)
			{
				reflNums.Add(refl.Idx.ToString());
			}
			DataFromGUI_Refl.ItemsStrs.Clear();
			DataFromGUI_Refl.ItemsStrs = reflNums;
		}

		public void UpdateSwapSelector()
		{
			List<string> swapsNums = new();
			foreach (Table swap in TL.Swaps)
			{
				swapsNums.Add(swap.Idx.ToString());
			}
			foreach (C_VPE_ComboBox swap in DataFromGUI_Swaps)
			{
				swap.ItemsStrs.Clear();
				swap.ItemsStrs = swapsNums;
			}
		}

		public void UpdateSettingsSelector()
		{
			List<string> names = new();
			foreach (Settings s in SL.Library)
			{
				names.Add(s.Name);
			}
			DataFromGUI_SettSel.ItemsStrs.Clear();
			DataFromGUI_SettSel.ItemsStrs = names;
		}

		public void RenameSelSett()
		{
			ActiveSett.Name = DataFromGUI_Sett.NameStr;
		}

		public void SaveSettings()
		{
			FileHandling.Save(ActiveSett, SaveFile(VPES_filter));
		}

		public void LoadSettings(bool? OverrideOverwrite = null)
		{
			FileHandling.Load(OpenFile(VPES_filter), out Settings s);
			if (OverrideOverwrite is not null)
			{
				if (OverrideOverwrite.Value)
				{
					ActiveSett = s;
				}
			}
			else
			{
				if (Overwrite)
				{
					ActiveSett = s;
				}
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

		private static PrimeDefinedConstant CopyPDCDataFromGUI(ObservableCollection<C_PDC> gui)
		{
			PrimeDefinedConstant result = new();
			for (int i = 0; i < Math.Min(result.PrimeIdxs.Length, gui.Count); i++)
			{
				result.PrimeIdxs[i] = gui[i].GetIdxNum() == null? -1 : gui[i].GetIdxNum().Value;
				result.Exponents[i] = (byte)(gui[i].GetExpNum() == null ? 0 : gui[i].GetExpNum().Value);
			}
			return result;
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