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
					FileHandling.Load(PS.PathsToSettLib, out KeyesLibrary sl);
					vpe.KL = sl;
					vpe.KL.PathToThis = PS.PathsToSettLib;
					vpe.ActiveKey = vpe.KL.Library[vpe.KL.LastActive];
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
		public KeyesLibrary KL = new();
		public TableLibrary TL = new();
		public Key ActiveKey;
		public Crypto C;
		public int SelectedPoss = 0;
		public string PathToIndividualKey = Invalid_File;
		#endregion
		#region Classes for binding
		internal C_VPE_Sett DataFromGUI_Key = new();
		internal List<C_UC_Rotor> DataFromGUI_Rotors = [];
		internal List<C_VPE_ComboBox> DataFromGUI_Swaps = [];
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
		private const string VPEM_filter = "VPE (binary) message files (*.vpem)|*.vpem";
		private const string TXT_filter = "Text files (*.txt)|*.txt";
		#endregion

		#region Public methods
		#region Generators
		/// <summary>Generates rotors.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateRotors(uint count = 40)
		{
			for (uint i = 0; i < count; i++)
			{
				TL.Rotors.Add(Generators.GenerateTable((ushort)TL.Rotors.Count));
			}
		}
		/// <summary>Generates reflectors.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateReflectors(uint count = 5)
		{
			for (uint i = 0; i < count; i++)
			{
				TL.Reflectors.Add(Generators.GeneratePairs((ushort)TL.Reflectors.Count));
			}
		}
		/// <summary>Generates swaps.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateSwaps(uint count = 20)
		{
			for (uint i = 0; i < count; i++)
			{
				TL.Swaps.Add(Generators.GeneratePairsWithSkips((ushort)TL.Swaps.Count, Generators.GenerateDoubleInRange(9d / 16d, 950d / 1024d)));
			}
		}
		/// <summary>Generates scramblers.</summary>
		/// <param name="count">How many of them?</param>
		public void GenerateScramblers(uint count = 10)
		{
			for (uint i = 0; i < count; i++)
			{
				TL.Rotors.Add(Generators.GenerateTableWithoutPoss((ushort)TL.Scramblers.Count));
			}
		}
		/// <summary>Generates complete key, adds them to library and sets the GUI accordingly.</summary>
		public void GenerateComplete()
		{
			string name = DataFromGUI_Key.NameStr ?? "";
			ActiveKey = Generators.GenerateKey([(uint)TL.Rotors.Count, (uint)TL.Swaps.Count, (uint)TL.Reflectors.Count, (uint)TL.Scramblers.Count, (uint)KL.Library.Count], name);
			AddKeyToLib();
			UpdateKeyesSelector();
			DataFromGUI_SettSel.SelectedStr = ActiveKey.Name;
		}
		/// <summary>Generates min and max length of space between random chars.</summary>
		/// <param name="MinFrom">Lower bound of minumum, including.</param>
		/// <param name="MinTo">Upper bound of minumum, excluding.</param>
		/// <param name="MaxFrom">Lower bound of maximum, including.</param>
		/// <param name="MaxTo">Upper bound of maximum, excluding.</param>
		public void GenerateSpaceMinMax(ushort MinFrom = 2, ushort MinTo = 8, ushort MaxFrom = 10, ushort MaxTo = 20)
		{
			ushort[] space = Generators.GenerateSpaceMinMax(MinFrom, MinTo, MaxFrom, MaxTo);
			DataFromGUI_Key.RandCharSpcMinStr = space[0].ToString();
			DataFromGUI_Key.RandCharSpcMaxStr = space[1].ToString();
		}
		/// <summary>Generates A, B, and M constants.</summary>
		public void GenerateABMConsts()
		{
			PrimeDefinedConstant[] consts = Generators.GenerateABM();
			DataFromGUI_Key.RandCharGenAStr = consts[0].ToString();
			DataFromGUI_Key.RandCharGenBStr = consts[1].ToString();
			DataFromGUI_Key.RandCharGenMStr = consts[2].ToString();
			DataFromGUI_Key.RandCharA = C_VPE_Sett.FillDataGridClass(consts[0]);
			DataFromGUI_Key.RandCharB = C_VPE_Sett.FillDataGridClass(consts[1]);
			DataFromGUI_Key.RandCharM = C_VPE_Sett.FillDataGridClass(consts[2]);
		}
		/// <summary>Generates A, B, C and D constants.</summary>
		public void GenerateABCDConsts()
		{
			PrimeDefinedConstant[] consts = Generators.GenerateABCD();
			DataFromGUI_Key.SwitchAStr = consts[0].ToString();
			DataFromGUI_Key.SwitchBStr = consts[1].ToString();
			DataFromGUI_Key.SwitchCStr = consts[2].ToString();
			DataFromGUI_Key.SwitchDStr = consts[3].ToString();
			DataFromGUI_Key.SwitchA = C_VPE_Sett.FillDataGridClass(consts[0]);
			DataFromGUI_Key.SwitchB = C_VPE_Sett.FillDataGridClass(consts[1]);
			DataFromGUI_Key.SwitchC = C_VPE_Sett.FillDataGridClass(consts[2]);
			DataFromGUI_Key.SwitchD = C_VPE_Sett.FillDataGridClass(consts[3]);
		}
		/// <summary>Simply generates random number in codepage limits, after updating the seed.</summary>
		/// <returns>Random number.</returns>
		public ushort GenerateRandNum()
		{
			return Generators.GenerateNum();
		}
		#endregion
		#region (Keyes window:) Updating, displaying and syncing from/to the GUI
		/// <summary>Sets active settings using what is in GUI.</summary>
		public void ChangeActiveKeyFromGUI()
		{
			List<ushort> rotors = [], poss = [], swaps = [];
			foreach (C_UC_Rotor rotor in DataFromGUI_Rotors)
			{
				rotors.Add(rotor.SelectedRNum.Value);
				poss.Add(rotor.PositionNum.Value);
			}
			foreach (C_VPE_ComboBox swap in DataFromGUI_Swaps)
			{
				swaps.Add(swap.SelectedNum.Value);
			}
			ActiveKey = TL.Select(rotors, swaps, DataFromGUI_Refl.SelectedNum.Value, [DataFromGUI_InScr.SelectedNum.Value, DataFromGUI_OutScr.SelectedNum.Value]);
			_ = ActiveKey.AddPositions(poss);
			ActiveKey.Name = DataFromGUI_Key.NameStr;
			ActiveKey.ConstShift = DataFromGUI_Key.ConstShiftNum.Value;
			ActiveKey.VarShift = DataFromGUI_Key.VarShiftNum.Value;
			ActiveKey.RandCharSpcMin = DataFromGUI_Key.RandCharSpcMinNum.Value;
			ActiveKey.RandCharSpcMax = DataFromGUI_Key.RandCharSpcMaxNum.Value;
			ActiveKey.RandCharConstA = CopyPDCDataFromGUI(DataFromGUI_Key.RandCharA);
			ActiveKey.RandCharConstB = CopyPDCDataFromGUI(DataFromGUI_Key.RandCharB);
			ActiveKey.RandCharConstM = CopyPDCDataFromGUI(DataFromGUI_Key.RandCharM);
			ActiveKey.SwitchConstA = CopyPDCDataFromGUI(DataFromGUI_Key.SwitchA);
			ActiveKey.SwitchConstB = CopyPDCDataFromGUI(DataFromGUI_Key.SwitchB);
			ActiveKey.SwitchConstC = CopyPDCDataFromGUI(DataFromGUI_Key.SwitchC);
			ActiveKey.SwitchConstD = CopyPDCDataFromGUI(DataFromGUI_Key.SwitchD);
		}
		/// <summary>Updates every selector.</summary>
		public void UpdateAll()
		{
			UpdateRotorSelectors();
			UpdateReflSelector();
			UpdateSwapSelector();
			UpdateScramblerSelectors();
			UpdateKeyesSelector();
		}
		/// <summary>Updates all rotor selectors.</summary>
		public void UpdateRotorSelectors()
		{
			List<string> rotorNums = [];
			if (TL.Rotors.Count > 0)
			{
				foreach (Table rotor in TL.Rotors)
				{
					rotorNums.Add(rotor.Idx.ToString());
				}
			}
			else
			{
				foreach (Key settings in KL.Library)
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
			List<string> reflNums = [];
			if (TL.Reflectors.Count > 0)
			{
				foreach (Table reflector in TL.Reflectors)
				{
					reflNums.Add(reflector.Idx.ToString());
				}
			}
			else
			{
				foreach (Key settings in KL.Library)
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
			List<string> swapsNums = [];
			if (TL.Swaps.Count > 0)
			{
				foreach (Table swaps in TL.Swaps)
				{
					swapsNums.Add(swaps.Idx.ToString());
				}
			}
			else
			{
				foreach (Key settings in KL.Library)
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
			List<string> scramblersNums = [];
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
				foreach (Key settings in KL.Library)
				{
					scramblersNums.Add(settings.InputScrambler.Idx.ToString());
					scramblersNums.Add(settings.OutputScrambler.Idx.ToString());
				}
			}
			DataFromGUI_InScr.ItemsStrs = scramblersNums;
			DataFromGUI_OutScr.ItemsStrs = scramblersNums;
		}
		/// <summary>Updates settings selector.</summary>
		public void UpdateKeyesSelector()
		{
			if (KL.Library.Count > 0)
			{
				int selected = DataFromGUI_SettSel.SelectedIdx;
				List<string> names = [];
				foreach (Key s in KL.Library)
				{
					names.Add(s.Name);
				}
				DataFromGUI_SettSel.ItemsStrs.Clear();
				DataFromGUI_SettSel.ItemsStrs = names;
				DataFromGUI_SettSel.SelectedIdx = selected;
			}
		}
		/// <summary>Displays provided/active settings in GUI, the settings composer window.</summary>
		/// <param name="s">Which settings? Null for active.</param>
		public void DisplaySettsInGUI(Key s = null)
		{
			s ??= ActiveKey;
			if (s == null)
			{
				return;
			}
			DataFromGUI_Key.SetUsingKey(s);
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
			if (DataFromGUI_Rotors.Count != ActiveKey.Rotors.Count)
			{
				if (DataFromGUI_Rotors.Count < ActiveKey.Rotors.Count)
				{
					while (DataFromGUI_Rotors.Count != ActiveKey.Rotors.Count)
					{
						DataFromGUI_Rotors.Add(new());
					}
				}
				else
				{
					while (DataFromGUI_Rotors.Count != ActiveKey.Rotors.Count)
					{
						DataFromGUI_Rotors.RemoveAt(DataFromGUI_Rotors.Count - 1);
					}
				}
			}
			for (int i = 0; i < ActiveKey.Rotors.Count; i++)
			{
				DataFromGUI_Rotors[i].RotorsStrs = rotors;
				DataFromGUI_Rotors[i].PositionStr = ActiveKey.Rotors[i].Positions[ActiveKey.SelectedPositions].ToString();
				DataFromGUI_Rotors[i].SelectedRIdx = (int)ActiveKey.Rotors[i].Idx;
				DataFromGUI_Rotors[i].SelectedRStr = ActiveKey.Rotors[i].Idx.ToString();
			}
		}
		/// <summary>Synchronizes the counts of instances of class for binding with swaps selectors.</summary>
		public void SynchronizeSwapDataForGUI()
		{
			List<string> swaps = TableLibrary.GetIDs(TL.Swaps);
			if (DataFromGUI_Swaps.Count != ActiveKey.Swaps.Count)
			{
				if (DataFromGUI_Swaps.Count < ActiveKey.Swaps.Count)
				{
					while (DataFromGUI_Swaps.Count != ActiveKey.Swaps.Count)
					{
						DataFromGUI_Swaps.Add(new());
					}
				}
				else
				{
					while (DataFromGUI_Swaps.Count != ActiveKey.Swaps.Count)
					{
						DataFromGUI_Swaps.RemoveAt(DataFromGUI_Swaps.Count - 1);
					}
				}
			}
			for (int i = 0; i < ActiveKey.Swaps.Count; i++)
			{
				DataFromGUI_Swaps[i].ItemsStrs = swaps;
				DataFromGUI_Swaps[i].SelectedStr = ActiveKey.Swaps[i].Idx.ToString();
			}
		}
		/// <summary>Synchronizes the counts of instances of class for binding with reflector selector.</summary>
		public void SynchronizeReflectorDataForGUI()
		{
			List<string> refls = TableLibrary.GetIDs(TL.Reflectors);
			DataFromGUI_Refl.ItemsStrs = refls;
			DataFromGUI_Refl.SelectedStr = ActiveKey.Reflector.Idx.ToString();
		}
		/// <summary>Synchronizes the counts of instances of class for binding with scrambler selectors.</summary>
		public void SynchronizeScramblerDataForGUI()
		{
			List<string> scrs = TableLibrary.GetIDs(TL.Scramblers);
			DataFromGUI_InScr.ItemsStrs = scrs;
			DataFromGUI_InScr.SelectedStr = ActiveKey.InputScrambler.Idx.ToString();
			DataFromGUI_OutScr.ItemsStrs = scrs;
			DataFromGUI_OutScr.SelectedStr = ActiveKey.OutputScrambler.Idx.ToString();
		}
		#endregion
		#region (Main window:) Updating, displaying and syncing from/to the GUI
		/// <summary>Encrypts the message and outputs it in specified encoding as a text.</summary>
		/// <param name="inText">Message as plaintext.</param>
		/// <param name="outType">Encoding of the encrypted message.</param>
		/// <returns></returns>
		public string Encrypt(string inText, Codepage.Encoding outType)
		{
			C = new(ActiveKey);
			//List<ushort> message = Codepage.ConvertToNums(inText);

			return C.Encrypt(inText, outType);
		}
		/// <summary>Decrypts the message in specified encoding, outputs decrypted text.</summary>
		/// <param name="inText"></param>
		/// <param name="inType"></param>
		/// <returns></returns>
		public string Decrypt(string inText, Codepage.Encoding inType)
		{
			C = new(ActiveKey);
			

			return C.Decrypt(inText, inType);
		}
		/// <summary></summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string[] CalculateEntropy(string message)
		{
			double[] entropyNums = Codepage.CalculateMessageEntropy(message);
			return [entropyNums[0].ToString().Replace('.', ','), entropyNums[1].ToString(), entropyNums[2].ToString()];
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
			FileHandling.Save(ActiveKey, SaveFile(VPES_filter));
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
			FileHandling.Load(path, out Key s);
			PathToIndividualKey = path;
			ActiveKey ??= s;
			AddKeyToLib(s);
			KL.ReIndexSetts();
			UpdateKeyesSelector();
			return true;
		}

		public void UpdateSettings()
		{
			if (PathToIndividualKey == Invalid_File)
			{
				return;
			}
			FileInfo fi = new(PathToIndividualKey);
			fi.Delete();
			FileHandling.Save(ActiveKey, PathToIndividualKey);
		}

		public void SaveSettingsLib()
		{
			FileHandling.Save(KL, SaveFile(VPESL_filter));
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
			FileHandling.Load(path, out KeyesLibrary sl);
			KL.PathToThis = path;
			foreach (Key s in sl.Library)
			{
				AddKeyToLib(s);
			}
			ActiveKey = KL.Library[KL.LastActive];
			return true;
		}

		public void UpdateSettingsLib()
		{
			bool createNew = false;
			if (KL.PathToThis == null)
			{
				createNew = true;
			}
			if (KL.PathToThis == "")
			{
				createNew = true;
			}
			if (KL.PathToThis == Invalid_File)
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
				sb.Remove(sb.Length - 1, 1); // Removing the „Z“ at the end.
				sb.Append(',');
				sb.Append(now.ToString("fff"));
				sb.Append(".vpesl");
				KL.PathToThis = sb.ToString().Replace(":", "-");
			}
			else
			{
				FileInfo fi = new(KL.PathToThis);
				fi.Delete();
			}
			FileHandling.Save(KL, KL.PathToThis);
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
				string[] parts = KL.PathToThis.Split('.');
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
		#region Support operation
		/// <summary>Copies all tables from settings library to table library.</summary>
		public void CopyTablesFromSLToTL()
		{
			foreach (Key s in KL.Library)
			{
				TL.Rotors.AddRange(s.Rotors);
				TL.Swaps.AddRange(s.Swaps);
				TL.Reflectors.Add(s.Reflector);
				TL.Scramblers.Add(s.InputScrambler);
				TL.Scramblers.Add(s.OutputScrambler);
			}
			string[] parts = KL.PathToThis.Split('.');
			StringBuilder sb = new();
			for (int i = 0; i < parts.Length - 1; i++)
			{
				sb.Append(parts[i]);
			}
			sb.Append("\'s Tables");
			sb.Append(".vpetl");
			TL.PathToThis = sb.ToString();
		}
		
		#endregion
		/// <summary>Sets the active settings using what was selected in settings selector.</summary>
		public void SetUsingSelKeyName()
		{
			ActiveKey = KL.Library.Find(x => x.Name == DataFromGUI_SettSel.SelectedStr);
			KL.LastActive = (int)ActiveKey.Idx;
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
		
		public string GetPossStrings(int idx = -2)
		{
			if (ActiveKey is not null)
			{
				return ActiveKey.GetPositionsString(idx);
			}
			else
			{
				return "";
			}
		}

		public bool AddPosFromString(string pozsStr)
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
			if (chopped.Length != ActiveKey.Rotors.Count)
			{
				return false;
			}
			List<ushort> pozsNums = [];
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
			return ActiveKey.AddPositions(pozsNums);
		}

		public void RenameSelKey()
		{
			if (ActiveKey != null)
			{
				ActiveKey.Name = DataFromGUI_Key.NameStr;
			}
			UpdateKeyesSelector();
		}

		public void RemoveSelKey()
		{
			if(KL.Library.Count > 0)
			{
				if (ActiveKey is not null)
				{
					int removed = (int)ActiveKey.Idx;
					KL.Library.RemoveAt(removed);
					if (KL.Library.Count > 0)
					{
						if (KL.Library.Count - 1 >= removed)
						{
							ActiveKey = KL.Library[removed];
						}
						else
						{
							ActiveKey = KL.Library[removed - 1];
						}
					}
					UpdateKeyesSelector();
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
		/// <summary>Adds active key to library.</summary>
		private void AddKeyToLib()
		{
			KL.Library.Add(ActiveKey);
			TL.Reflectors.Add(ActiveKey.Reflector);
			TL.Rotors.AddRange(ActiveKey.Rotors);
			TL.Swaps.AddRange(ActiveKey.Swaps);
			TL.Scramblers.Add(ActiveKey.InputScrambler);
			TL.Scramblers.Add(ActiveKey.OutputScrambler);
			DataFromGUI_SettSel.ItemsStrs.Add(ActiveKey.Name);
		}
		/// <summary>Adds supplied key to library (if it isn't there already).</summary>
		private void AddKeyToLib(Key s)
		{
			if (!KL.Library.Contains(s))
			{
				KL.Library.Add(s);
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