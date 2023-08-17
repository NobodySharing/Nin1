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
using Common;
using System.IO;
using System.Windows.Shapes;
using System.Globalization;

namespace GUI
{
	public class Common_VM
	{
		#region Variables
		private readonly PersistentStorageManager PSM = new();
		public PersistentStorage PS;
		#endregion
		#region Classes for binding
		internal C_MainWin DataFromGUI_MainWin = new();
		#endregion
		#region Public methods
		/// <summary>Loads config from a file. Needs reference to VMs, for working with them.</summary>
		/// <param name="vpe">VPE's VM.</param>
		public void LoadConfig(ref VPE_VM vpe)
		{
			PS = PSM.ReadConfig();
			if (PS.IsDefault)
			{
				return;
			}
			else
			{
				bool CreateTL = true;
				if (File.Exists(PS.PathsToTableLib))
				{
					FileHandling.Load(PS.PathsToTableLib, out TableLibrary tl);
					vpe.TL = tl;
					vpe.TL.PathToThis = PS.PathsToTableLib;
					CreateTL = false;
				}
				if (File.Exists(PS.PathsToSettLib))
				{
					FileHandling.Load(PS.PathsToSettLib, out SettingsLibrary sl);
					vpe.SL = sl;
					vpe.SL.PathToThis = PS.PathsToSettLib;
					vpe.ActiveSett = vpe.SL.Library[vpe.SL.LastActive];
					if(CreateTL)
					{
						vpe.CopyTablesFromSLToTL();
					}
				}
			}
		}
		public void SaveConfig()
		{
			PSM.WriteConfig(PS);
		}
		#endregion
	}

	public class VPE_VM
	{
		#region Variables
		private readonly Generators Generator = new();
		public SettingsLibrary SL = new();
		public TableLibrary TL = new();
		public Settings ActiveSett;
		public Crypto C;
		public int SelectedPozs = 0;
		public string PathToIndividualSetts = Invalid_File;
		#endregion
		#region Classes for binding
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
		#region Generators
		/// <summary>Generates rotors.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateRotors(uint count = 40)
		{
			Generator.UpdateSeed();
			for (uint i = 0; i < count; i++)
			{
				TL.Rotors.Add(Generator.GenerateTable((ushort)TL.Rotors.Count));
			}
		}
		/// <summary>Generates reflectors.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateReflectors(uint count = 5)
		{
			Generator.UpdateSeed();
			for (uint i = 0; i < count; i++)
			{
				TL.Reflectors.Add(Generator.GeneratePairs((ushort)TL.Reflectors.Count));
			}
		}
		/// <summary>Generates swaps.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateSwaps(uint count = 20)
		{
			Generator.UpdateSeed();
			for (uint i = 0; i < count; i++)
			{
				TL.Swaps.Add(Generator.GeneratePairsWithSkips((ushort)TL.Swaps.Count, Generator.GenerateDoubleInRange(9d / 16d, 950d / 1024d)));
			}
		}
		/// <summary>Generates scramblers.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateScramblers(uint count = 10)
		{
			Generator.UpdateSeed();
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
		/// <summary>Generates min and max length of space between random chars.</summary>
		/// <param name="MinFrom">Lower bound of minumum, including.</param>
		/// <param name="MinTo">Upper bound of minumum, excluding.</param>
		/// <param name="MaxFrom">Lower bound of maximum, including.</param>
		/// <param name="MaxTo">Upper bound of maximum, excluding.</param>
		public void GenerateSpaceMinMax(ushort MinFrom = 2, ushort MinTo = 8, ushort MaxFrom = 10, ushort MaxTo = 20)
		{
			ushort[] space = Generator.GenerateSpaceMinMax(MinFrom, MinTo, MaxFrom, MaxTo);
			DataFromGUI_Sett.RandCharSpcMinStr = space[0].ToString();
			DataFromGUI_Sett.RandCharSpcMaxStr = space[1].ToString();
		}
		/// <summary>Generates A, B, and M constants.</summary>
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
		/// <summary>Generates A, B, C and D constants.</summary>
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
		/// <summary>Simply generates random number in codepage limits, after updating the seed.</summary>
		/// <returns>Rnadom number.</returns>
		public ushort GenerateRandNum()
		{
			Generator.UpdateSeed();
			return Generator.GenerateNum();
		}
		#endregion
		#region Updating, displaying and syncing from/to the GUI
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
		/// <summary>Updates every selector.</summary>
		public void UpdateAll()
		{
			UpdateRotorSelectors();
			UpdateReflSelector();
			UpdateSwapSelector();
			UpdateScramblerSelectors();
			UpdateSettingsSelector();
		}
		/// <summary>Updates all rotor selectors.</summary>
		public void UpdateRotorSelectors()
		{
			List<string> rotorNums = new();
			if (TL.Rotors.Count > 0)
			{
				foreach (Table rotor in TL.Rotors)
				{
					rotorNums.Add(rotor.Idx.ToString());
				}
			}
			else
			{
				foreach (Settings settings in SL.Library)
				{
					foreach (Table rotor in settings.Rotors)
					{
						rotorNums.Add(rotor.Idx.ToString());
					}
				}
			}
			foreach (C_UC_Rotor rotorList in DataFromGUI_Rotors)
			{
				rotorList.RotorsStrs.Clear();
				rotorList.RotorsStrs = rotorNums;
			}
		}
		/// <summary>Updates all reflector selectors.</summary>
		public void UpdateReflSelector()
		{
			List<string> reflNums = new();
			if (TL.Reflectors.Count > 0)
			{
				foreach (Table reflector in TL.Reflectors)
				{
					reflNums.Add(reflector.Idx.ToString());
				}
			}
			else
			{
				foreach (Settings settings in SL.Library)
				{
					reflNums.Add(settings.Reflector.Idx.ToString());
				}
			}
			DataFromGUI_Refl.ItemsStrs.Clear();
			DataFromGUI_Refl.ItemsStrs = reflNums;
		}
		/// <summary>Updates swap selector.</summary>
		public void UpdateSwapSelector()
		{
			List<string> swapsNums = new();
			if (TL.Swaps.Count > 0)
			{
				foreach (Table swaps in TL.Swaps)
				{
					swapsNums.Add(swaps.Idx.ToString());
				}
			}
			else
			{
				foreach (Settings settings in SL.Library)
				{
					foreach (Table swaps in settings.Swaps)
					{
						swapsNums.Add(swaps.Idx.ToString());
					}
				}
			}
			foreach (C_VPE_ComboBox swap in DataFromGUI_Swaps)
			{
				swap.ItemsStrs.Clear();
				swap.ItemsStrs = swapsNums;
			}
		}
		/// <summary>Updates all scramblers selectors.</summary>
		public void UpdateScramblerSelectors()
		{
			List<string> scramblersNums = new();
			foreach (Table scrambler in TL.Scramblers)
			{
				scramblersNums.Add(scrambler.Idx.ToString());
			}
			if (TL.Reflectors.Count > 0)
			{
				foreach (Table scramblers in TL.Scramblers)
				{
					scramblersNums.Add(scramblers.Idx.ToString());
				}
			}
			else
			{
				foreach (Settings settings in SL.Library)
				{
					scramblersNums.Add(settings.InputScrambler.Idx.ToString());
					scramblersNums.Add(settings.OutputScrambler.Idx.ToString());
				}
			}
			DataFromGUI_InScr.ItemsStrs = scramblersNums;
			DataFromGUI_OutScr.ItemsStrs = scramblersNums;
		}
		/// <summary>Updates settings selector.</summary>
		public void UpdateSettingsSelector()
		{
			List<string> names = new();
			foreach (Settings s in SL.Library)
			{
				names.Add(s.Name);
			}
			DataFromGUI_SettSel.ItemsStrs.Clear();
			DataFromGUI_SettSel.ItemsStrs = names;
			DataFromGUI_SettSel.SelectedStr = ActiveSett?.Name;
		}
		/// <summary>Displays provided/active settings in GUI, the settings composer window.</summary>
		/// <param name="s">Which settings? Null for active.</param>
		public void DisplaySettsInGUI(Settings s = null)
		{
			s ??= ActiveSett;
			if (s == null)
			{
				return;
			}
			DataFromGUI_Sett.SetUsingSettings(s);
			SynchronizeAllDataForGUI();
		}
		/// <summary>Synchronizes binding data for all selectors.</summary>
		public void SynchronizeAllDataForGUI()
		{
			SynchronizeSwapDataForGUI();
			SynchronizeRotorDataForGUI();
			SynchronizeReflectorDataForGUI();
			SynchronizeScramblerDataForGUI();
		}
		/// <summary>Synchronizes the counts of instances of class for binding with rotor selectors.</summary>
		public void SynchronizeRotorDataForGUI()
		{
			List<string> rotors = TableLibrary.GetIDs(TL.Rotors);
			if (DataFromGUI_Rotors.Count != ActiveSett.Rotors.Count)
			{
				if (DataFromGUI_Rotors.Count < ActiveSett.Rotors.Count)
				{
					while (DataFromGUI_Rotors.Count != ActiveSett.Rotors.Count)
					{
						DataFromGUI_Rotors.Add(new());
					}
				}
				else
				{
					while (DataFromGUI_Rotors.Count != ActiveSett.Rotors.Count)
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
		/// <summary>Synchronizes the counts of instances of class for binding with swaps selectors.</summary>
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
		/// <summary>Synchronizes the counts of instances of class for binding with reflector selector.</summary>
		public void SynchronizeReflectorDataForGUI()
		{
			List<string> refls = TableLibrary.GetIDs(TL.Reflectors);
			DataFromGUI_Refl.ItemsStrs = refls;
			DataFromGUI_Refl.SelectedStr = ActiveSett.Reflector.Idx.ToString();
		}
		/// <summary>Synchronizes the counts of instances of class for binding with scrambler selectors.</summary>
		public void SynchronizeScramblerDataForGUI()
		{
			List<string> scrs = TableLibrary.GetIDs(TL.Scramblers);
			DataFromGUI_InScr.ItemsStrs = scrs;
			DataFromGUI_InScr.SelectedStr = ActiveSett.InputScrambler.Idx.ToString();
			DataFromGUI_OutScr.ItemsStrs = scrs;
			DataFromGUI_OutScr.SelectedStr = ActiveSett.OutputScrambler.Idx.ToString();
		}
		#endregion
		#region File operations

		public static string OpenMsgFile()
		{
			return FileHandling.LoadText(OpenFile(TXT_filter));
		}

		public static void SaveMsgFile(string text)
		{
			FileHandling.SaveText(SaveFile(TXT_filter), text);
		}

		public void SaveSettings()
		{
			FileHandling.Save(ActiveSett, SaveFile(VPES_filter));
		}

		public bool LoadSettings()
		{
			string path = OpenFile(VPES_filter);
			if (path == null)
			{
				return false;
			}
			if (path == "")
			{
				return false;
			}
			if (path == Invalid_File)
			{
				return false;
			}
			FileHandling.Load(path, out Settings s);
			PathToIndividualSetts = path;
			AddSettsToLib(s);
			SL.ReIndexSetts();
			UpdateSettingsSelector();
			return true;
		}

		public void UpdateSettings()
		{
			if (PathToIndividualSetts == Invalid_File)
			{
				return;
			}
			FileInfo fi = new(PathToIndividualSetts);
			fi.Delete();
			FileHandling.Save(ActiveSett, PathToIndividualSetts);
		}

		public void SaveSettingsLib()
		{
			FileHandling.Save(SL, SaveFile(VPESL_filter));
		}

		public bool LoadSettingsLib()
		{
			string path = OpenFile(VPESL_filter);
			if (path == null)
			{
				return false;
			}
			if (path == "")
			{
				return false;
			}
			if (path == Invalid_File)
			{
				return false;
			}
			FileHandling.Load(path, out SettingsLibrary sl);
			SL.PathToThis = path;
			foreach (Settings s in sl.Library)
			{
				AddSettsToLib(s);
			}
			ActiveSett = SL.Library[SL.LastActive];
			return true;
		}

		public void UpdateSettingsLib()
		{
			bool createNew = false;
			if (SL.PathToThis == null)
			{
				createNew = true;
			}
			if (SL.PathToThis == "")
			{
				createNew = true;
			}
			if (SL.PathToThis == Invalid_File)
			{
				createNew = true;
			}
			if (createNew)
			{
				DateTime now = DateTime.Now;
				StringBuilder sb = new();
				sb.Append(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
				sb.Append("\\Nin1\\Automatically saved at ");
				sb.Append(now.ToString("u"));
				sb.Append(',');
				sb.Append(now.ToString("fff"));
				sb.Append(".vpesl");
				SL.PathToThis = sb.ToString().Replace('/', '-').Replace(":", "-");
			}
			else
			{
				FileInfo fi = new(SL.PathToThis);
				fi.Delete();
			}
			FileHandling.Save(SL, SL.PathToThis);
		}

		public void SaveTableLib()
		{
			FileHandling.Save(TL, SaveFile(VPETL_filter));
		}

		public bool LoadTableLib()
		{
			string path = OpenFile(VPETL_filter);
			if (path == null)
			{
				return false;
			}
			if (path == "")
			{
				return false;
			}
			if (path == Invalid_File)
			{
				return false;
			}
			FileHandling.Load(path, out TableLibrary tl);
			TL.PathToThis = path;
			TL.Reflectors.AddRange(tl.Reflectors);
			TL.Swaps.AddRange(tl.Swaps);
			TL.Rotors.AddRange(tl.Rotors);
			TL.Scramblers.AddRange(tl.Scramblers);
			return true;
		}

		public void UpdateTableLib()
		{
			bool createNew = false;
			if (TL.PathToThis == null)
			{
				createNew = true;
			}
			if (TL.PathToThis == "")
			{
				createNew = true;
			}
			if (TL.PathToThis == Invalid_File)
			{
				createNew = true;
			}
			if (createNew)
			{
				string[] parts = SL.PathToThis.Split('.');
				StringBuilder sb = new();
				for (int i = 0; i < parts.Length - 1; i++)
				{
					sb.Append(parts[i]);
				}
				sb.Append("\'s Tables");
				sb.Append(".vpetl");
				TL.PathToThis = sb.ToString();
			}
			else
			{
				FileInfo fi = new(TL.PathToThis);
				fi.Delete();
			}
			FileHandling.Save(TL, TL.PathToThis);
		}
		#endregion

		public void CopyTablesFromSLToTL()
		{
			foreach (Settings s in SL.Library)
			{
				TL.Rotors.AddRange(s.Rotors);
				TL.Swaps.AddRange(s.Swaps);
				TL.Reflectors.Add(s.Reflector);
				TL.Scramblers.Add(s.InputScrambler);
				TL.Scramblers.Add(s.OutputScrambler);
			}
			string[] parts = SL.PathToThis.Split('.');
			StringBuilder sb = new();
			for (int i = 0; i < parts.Length - 1; i++)
			{
				sb.Append(parts[i]);
			}
			sb.Append("\'s Tables");
			sb.Append(".vpetl");
			TL.PathToThis = sb.ToString();
		}

		public string Encrypt(string inText, bool numOut)
		{
			C = new(ActiveSett);
			return C.Encypt(inText, numOut);
		}

		public string Decrypt(string inText, bool numIn)
		{
			C = new(ActiveSett);
			return C.Decypt(inText, numIn);
		}

		public static string ConvertToNumRepres(string message)
		{
			return Codepage.ConvertToNumeric(message);
		}

		public static string ConvertFromNumRepres(string message)
		{
			return Codepage.ConvertFromNumeric(message);
		}
		/// <summary>Sets the active settings using what was selected in settings selector.</summary>
		public void SetUsingSelSettName()
		{
			ActiveSett = SL.Library.Find(x => x.Name == DataFromGUI_SettSel.SelectedStr);
			SL.LastActive = (int)ActiveSett.Idx;
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

		public void AddSwapDataForGUI()
		{
			C_VPE_ComboBox toAdd = new()
			{
				ItemsStrs = DataFromGUI_Swaps[0].ItemsStrs,
			};
			DataFromGUI_Swaps.Add(toAdd);
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

		public void RemoveSelSett()
		{
			if(SL.Library.Count > 0)
			{
				if (ActiveSett is not null)
				{
					int removed = (int)ActiveSett.Idx;
					SL.Library.RemoveAt(removed);
					if (SL.Library.Count > 0)
					{
						if (SL.Library.Count - 1 >= removed)
						{
							ActiveSett = SL.Library[removed];
						}
						else
						{
							ActiveSett = SL.Library[removed - 1];
						}
					}
					UpdateSettingsSelector();
					DisplaySettsInGUI();
				}
			}
		}

		#endregion

		#region Private methods
		/// <summary>Shows file open dialog and returns a path to selected file.</summary>
		/// <param name="ext">Extension.</param>
		/// <returns>Path to a file or invalid marker.</returns>
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
		/// <summary>Shows file save dialog and returns a path to selected file.</summary>
		/// <param name="ext">Extension.</param>
		/// <returns>Path to a file or invalid marker.</returns>
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
		/// <summary>Adds supplied settings to library (if it isn't there already).</summary>
		private void AddSettsToLib(Settings s)
		{
			if (!SL.Library.Contains(s))
			{
				SL.Library.Add(s);
				TL.Reflectors.Add(s.Reflector);
				TL.Rotors.AddRange(s.Rotors);
				TL.Swaps.AddRange(s.Swaps);
				TL.Scramblers.Add(s.InputScrambler);
				TL.Scramblers.Add(s.OutputScrambler);
			}
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

	public class PSWD_VM
	{

	}

	public class Maps_VM
	{

	}
}