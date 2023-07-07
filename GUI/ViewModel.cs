using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Security.Policy;

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
		public int SelectedPozs = 0;
		#endregion
		#region Classes for binding
		internal C_VPE_MainWin DataFromGUI_MainWin = new(); // This does nothing yet.
		internal C_VPE_Sett DataFromGUI_Sett = new();
		internal List<C_UC_Rotor> DataFromGUI_Rotors = new();
		internal List<C_VPE_ComboBox> DataFromGUI_Swaps = new();
		internal C_VPE_ComboBox DataFromGUI_Refl = new();
		internal C_VPE_ComboBox DataFromGUI_InScr = new();
		internal C_VPE_ComboBox DataFromGUI_OutScr = new();
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
		
		public void GenerateRotors(uint count = 40)
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

		public void GenerateSwaps(uint count = 20)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			for (uint i = 0; i < count; i++)
			{
				TL.Swaps.Add(Generator.GeneratePairsWithSkips((ushort)TL.Swaps.Count, Generator.GenerateDoubleInRange(9d / 16d, 950d / 1024d)));
			}
		}

		public void GenerateScramblers(uint count = 10)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			for (uint i = 0; i < count; i++)
			{
				TL.Rotors.Add(Generator.GenerateTableWithoutPoz((ushort)TL.Scramblers.Count));
			}
		}
		/// <summary>Generates complete settings, adds them to library and sets the GUI accordingly.</summary>
		public void GenerateComplete()
		{
			string name = DataFromGUI_Sett.NameStr ?? "";
			ActiveSett = Generator.GenerateSetts(new uint[] { (uint)TL.Rotors.Count, (uint)TL.Swaps.Count, (uint)TL.Reflectors.Count, (uint)TL.Scramblers.Count, (uint)SL.Library.Count }, name);
			AddSettsToLib();
			UpdateSettingsSelector();
			DataFromGUI_SettSel.SelectedStr = ActiveSett.Name;
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
			ActiveSett = TL.Select(rotors, swaps, DataFromGUI_Refl.SelectedNum.Value, new ushort[] { DataFromGUI_InScr.SelectedNum.Value, DataFromGUI_OutScr.SelectedNum.Value });
			_ = ActiveSett.AddPozitions(pozs);
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

		public void DisplaySettsInGUI(Settings s = null)
		{
			s ??= ActiveSett;
			DataFromGUI_Sett.SetUsingSettings(s);
			SynchronizeSwapDataForGUI();
			SynchronizeRotorDataForGUI();
			SynchronizeReflectorDataForGUI();
			SynchronizeScramblerDataForGUI();
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
			DataFromGUI_Sett.RandCharA = C_VPE_Sett.FillDataGridClass(consts[0]);
			DataFromGUI_Sett.RandCharB = C_VPE_Sett.FillDataGridClass(consts[1]);
			DataFromGUI_Sett.RandCharM = C_VPE_Sett.FillDataGridClass(consts[2]);
		}

		public void GenerateABCDConsts()
		{
			PrimeDefinedConstant[] consts = Generator.GenerateABCD();
			DataFromGUI_Sett.SwitchAStr = consts[0].ToString();
			DataFromGUI_Sett.SwitchBStr = consts[1].ToString();
			DataFromGUI_Sett.SwitchCStr = consts[2].ToString();
			DataFromGUI_Sett.SwitchDStr = consts[3].ToString();
			DataFromGUI_Sett.SwitchA = C_VPE_Sett.FillDataGridClass(consts[0]);
			DataFromGUI_Sett.SwitchB = C_VPE_Sett.FillDataGridClass(consts[1]);
			DataFromGUI_Sett.SwitchC = C_VPE_Sett.FillDataGridClass(consts[2]);
			DataFromGUI_Sett.SwitchD = C_VPE_Sett.FillDataGridClass(consts[3]);
		}

		public string Encrypt(string inText)
		{
			C = new(ActiveSett);
			return C.Encypt(inText);
		}

		public string Decrypt(string inText)
		{
			C = new(ActiveSett);
			return C.Decypt(inText);
		}

		public static string OpenMsgFile()
		{
			return FileHandling.LoadText(OpenFile(TXT_filter));
		}

		public static void SaveMsgFile(string text)
		{
			FileHandling.SaveText(SaveFile(TXT_filter), text);
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
				DataFromGUI_Rotors[i].PozitionStr = ActiveSett.Rotors[i].Pozitions[ActiveSett.SelectedPozitions].ToString();
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

		public void SynchronizeScramblerDataForGUI()
		{
			List<string> scrs = TableLibrary.GetIDs(TL.Scramblers);
			DataFromGUI_InScr.ItemsStrs = scrs;
			DataFromGUI_InScr.SelectedStr = ActiveSett.InputScrambler.Idx.ToString();
			DataFromGUI_OutScr.ItemsStrs = scrs;
			DataFromGUI_OutScr.SelectedStr = ActiveSett.OutputScrambler.Idx.ToString();
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

		public void UpdateScramblerSelectors()
		{
			List<string> scramblersNums = new();
			foreach (Table scrambler in TL.Scramblers)
			{
				scramblersNums.Add(scrambler.Idx.ToString());
			}
			DataFromGUI_InScr.ItemsStrs = scramblersNums;
			DataFromGUI_OutScr.ItemsStrs = scramblersNums;
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

		public string GetPozsStrings(int idx = -2)
		{
			if (ActiveSett is not null)
			{
				return ActiveSett.GetPozitionsString(idx);
			}
			else
			{
				return "";
			}
		}

		public bool AddPozFromString(string pozsStr)
		{
			if (pozsStr == null)
			{
				return false;
			}
			if (pozsStr == "")
			{
				return false;
			}
			string[] chopped = pozsStr.Split(',');
			if (chopped.Length != ActiveSett.Rotors.Count)
			{
				return false;
			}
			List<ushort> pozsNums = new();
			foreach(string part in chopped)
			{
				if (ushort.TryParse(part, out ushort num))
				{
					pozsNums.Add(num);
				}
				else
				{
					return false;
				}
			}
			return ActiveSett.AddPozitions(pozsNums);
		}

		public void RenameSelSett()
		{
			ActiveSett.Name = DataFromGUI_Sett.NameStr;
			UpdateSettingsSelector();
		}

		public void SaveSettings()
		{
			FileHandling.Save(ActiveSett, SaveFile(VPES_filter));
		}

		public void LoadSettings()
		{
			string path = OpenFile(VPES_filter);
			if (path == null)
			{
				return;
			}
			if (path == "")
			{
				return;
			}
			if (path == Invalid_File)
			{
				return;
			}
			FileHandling.Load(path, out Settings s);
			AddSettsToLib(s);
			UpdateSettingsSelector();
		}

		public void UpdateSettings()
		{

		}

		public void SaveSettingsLib()
		{
			FileHandling.Save(SL, SaveFile(VPESL_filter));
		}

		public void LoadSettingsLib()
		{
			string path = OpenFile(VPESL_filter);
			if (path == null)
			{
				return;
			}
			if (path == "")
			{
				return;
			}
			if (path == Invalid_File)
			{
				return;
			}
			FileHandling.Load(path, out SettingsLibrary sl);
			foreach (Settings s in sl.Library)
			{
				AddSettsToLib(s);
			}
		}

		public void UpdateSettingsLib()
		{

		}

		public void SaveTableLib()
		{
			FileHandling.Save(TL, SaveFile(VPETL_filter));
		}

		public void LoadTableLib()
		{
			string path = OpenFile(VPETL_filter);
			if (path == null)
			{
				return;
			}
			if (path == "")
			{
				return;
			}
			if (path == Invalid_File)
			{
				return;
			}
			FileHandling.Load(path, out TableLibrary tl);
			TL.Reflectors.AddRange(tl.Reflectors);
			TL.Swaps.AddRange(tl.Swaps);
			TL.Rotors.AddRange(tl.Rotors);
			TL.Scramblers.AddRange(tl.Scramblers);
		}

		public void UpdateTableLib()
		{

		}
		#endregion

		#region Private methods
		/// <summary>Zobrazí dialog otevření souboru a vrátí cestu k němu.</summary>
		/// <param name="ext">Extension.</param>
		/// <returns>Cesta k souboru.</returns>
		private static string OpenFile(string ext)
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
		/// <param name="ext">Extension.</param>
		/// <returns>Cesta k souboru.</returns>
		private static string SaveFile(string ext)
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
		/// <summary>Adds active settings to library.</summary>
		private void AddSettsToLib()
		{
			SL.Library.Add(ActiveSett);
			TL.Reflectors.Add(ActiveSett.Reflector);
			TL.Rotors.AddRange(ActiveSett.Rotors);
			TL.Swaps.AddRange(ActiveSett.Swaps);
			TL.Scramblers.Add(ActiveSett.InputScrambler);
			TL.Scramblers.Add(ActiveSett.OutputScrambler);
			DataFromGUI_SettSel.ItemsStrs.Add(ActiveSett.Name);
		}
		/// <summary>Adds supplied settings to library.</summary>
		private void AddSettsToLib(Settings s)
		{
			SL.Library.Add(s);
			TL.Reflectors.Add(s.Reflector);
			TL.Rotors.AddRange(s.Rotors);
			TL.Swaps.AddRange(s.Swaps);
			TL.Scramblers.Add(s.InputScrambler);
			TL.Scramblers.Add(s.OutputScrambler);
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