using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPE;
using Common;

namespace GUI
{
	public partial class MainWindow : Window
	{
		#region Common
		private readonly Common_VM Cmn = new();
		public MainWindow()
		{
			InitializeComponent();
			DataContext = Cmn.DataFromGUI_MainWin;
			Cmn.LoadConfig(ref VPE);
			if (!Cmn.PS.IsDefault && VPE.ActiveKey is not null)
			{
				DisplayRotorPozs(-1);
			}
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			VPE.UpdateSettings();
			VPE.UpdateSettingsLib();
			VPE.UpdateTableLib();
			Cmn.PS.IsDefault = false;
			Cmn.PS.PathsToSettLib = VPE.KL.PathToThis;
			Cmn.PS.PathsToTableLib = VPE.TL.PathToThis;
			Cmn.SaveConfig();
		}
		#endregion

		#region VPE
		private VPE_VM VPE = new();
		VPESettingsComp VPESettWin;
		#region GUI events
		private void B_VPE_Encrypt(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveKey is not null)
			{
				Cmn.DataFromGUI_MainWin.VPE_EncrypStr = VPE.Encrypt(Cmn.DataFromGUI_MainWin.VPE_PlainStr, Cmn.DataFromGUI_MainWin.VPE_OutputType);
				// Temp code start:
				string[] left = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_PlainStr);
				string[] right = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);
				Cmn.DataFromGUI_MainWin.VPE_Left_EntropyStr = left[0];
				Cmn.DataFromGUI_MainWin.VPE_Left_UniqueCharCountStr = left[1];
				Cmn.DataFromGUI_MainWin.VPE_Left_LengthStr = left[2];
				Cmn.DataFromGUI_MainWin.VPE_Right_EntropyStr = right[0];
				Cmn.DataFromGUI_MainWin.VPE_Right_UniqueCharCountStr = right[1];
				Cmn.DataFromGUI_MainWin.VPE_Right_LengthStr = right[2];
				// Temp code end.
				DisplayRotorPozs(VPE.ActiveKey.SelectedPositions);
			}
			else
			{
				string caption = "No encryption settings";
				string messageBoxText = "I am unable to encrypt your text. No encryption settings were selected.";
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Error;
				_ = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
			}
		}

		private void B_VPE_Decrypt(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveKey is not null)
			{
				Cmn.DataFromGUI_MainWin.VPE_PlainStr = VPE.Decrypt(Cmn.DataFromGUI_MainWin.VPE_EncrypStr, Cmn.DataFromGUI_MainWin.VPE_OutputType);
				// Temp code start:
				string[] left = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_PlainStr);
				string[] right = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);
				Cmn.DataFromGUI_MainWin.VPE_Left_EntropyStr = left[0];
				Cmn.DataFromGUI_MainWin.VPE_Left_UniqueCharCountStr = left[1];
				Cmn.DataFromGUI_MainWin.VPE_Left_LengthStr = left[2];
				Cmn.DataFromGUI_MainWin.VPE_Right_EntropyStr = right[0];
				Cmn.DataFromGUI_MainWin.VPE_Right_UniqueCharCountStr = right[1];
				Cmn.DataFromGUI_MainWin.VPE_Right_LengthStr = right[2];
				// Temp code end.
				DisplayRotorPozs(VPE.ActiveKey.SelectedPositions);
			}
			else
			{
				string caption = "No decryption settings";
				string messageBoxText = "I am unable to decrypt your text. No decryption settings were selected.";
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Error;
				_ = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
			}
		}
		
		private void MI_VPE_Encrypt_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveKey is not null)
			{
				Cmn.DataFromGUI_MainWin.VPE_EncrypStr = VPE.Encrypt(Cmn.DataFromGUI_MainWin.VPE_PlainStr, Cmn.DataFromGUI_MainWin.VPE_OutputType);
				// Temp code start:
				string[] left = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_PlainStr);
				string[] right = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);
				Cmn.DataFromGUI_MainWin.VPE_Left_EntropyStr = left[0];
				Cmn.DataFromGUI_MainWin.VPE_Left_UniqueCharCountStr = left[1];
				Cmn.DataFromGUI_MainWin.VPE_Left_LengthStr = left[2];
				Cmn.DataFromGUI_MainWin.VPE_Right_EntropyStr = right[0];
				Cmn.DataFromGUI_MainWin.VPE_Right_UniqueCharCountStr = right[1];
				Cmn.DataFromGUI_MainWin.VPE_Right_LengthStr = right[2];
				// Temp code end.
				DisplayRotorPozs(VPE.ActiveKey.SelectedPositions);
			}
			else
			{
				string caption = "No encryption settings";
				string messageBoxText = "I am unable to encrypt your text. No encryption settings were selected.";
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Error;
				_ = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
			}
		}

		private void MI_VPE_Decrypt_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveKey is not null)
			{
				Cmn.DataFromGUI_MainWin.VPE_PlainStr = VPE.Decrypt(Cmn.DataFromGUI_MainWin.VPE_EncrypStr, Cmn.DataFromGUI_MainWin.VPE_OutputType);
				// Temp code start:
				string[] left = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_PlainStr);
				string[] right = VPE_VM.CalculateEntropy(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);
				Cmn.DataFromGUI_MainWin.VPE_Left_EntropyStr = left[0];
				Cmn.DataFromGUI_MainWin.VPE_Left_UniqueCharCountStr = left[1];
				Cmn.DataFromGUI_MainWin.VPE_Left_LengthStr = left[2];
				Cmn.DataFromGUI_MainWin.VPE_Right_EntropyStr = right[0];
				Cmn.DataFromGUI_MainWin.VPE_Right_UniqueCharCountStr = right[1];
				Cmn.DataFromGUI_MainWin.VPE_Right_LengthStr = right[2];
				// Temp code end.
				DisplayRotorPozs(VPE.ActiveKey.SelectedPositions);
			}
			else
			{
				string caption = "No decryption settings";
				string messageBoxText = "I am unable to decrypt your text. No decryption settings were selected.";
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Error;
				_ = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
			}
		}

		private void MI_VPE_OpenPlaMsgTxtFile_Click(object sender, RoutedEventArgs e) => Cmn.DataFromGUI_MainWin.VPE_PlainStr = VPE_VM.OpenMsgFile();

		private void MI_VPE_OpenEncMsgBinFile_Click(object sender, RoutedEventArgs e) => Cmn.DataFromGUI_MainWin.VPE_EncrypStr = VPE_VM.OpenMsgFile();

		private void MI_VPE_OpenEncMsgTxtFile_Click(object sender, RoutedEventArgs e) => Cmn.DataFromGUI_MainWin.VPE_EncrypStr = VPE_VM.OpenMsgFile();

		private void MI_VPE_SavePlaMsgTxtFile_Click(object sender, RoutedEventArgs e) => VPE_VM.SaveMsgFile(Cmn.DataFromGUI_MainWin.VPE_PlainStr);

		private void MI_VPE_SaveEncMsgBinFile_Click(object sender, RoutedEventArgs e) => VPE_VM.SaveMsgFile(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);

		private void MI_VPE_SaveEncMsgTxtFile_Click(object sender, RoutedEventArgs e) => VPE_VM.SaveMsgFile(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);

		private void MI_VPE_QuickSettGen_Click(object sender, RoutedEventArgs e)
		{
			VPE.GenerateComplete();
			DisplayRotorPozs(VPE.ActiveKey.SelectedPositions);
		}

		private void MI_VPE_QuickSettSave_Click(object sender, RoutedEventArgs e) => VPE.SaveSettings();

		private void MI_VPE_QuickSettOpen_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.LoadSettings())
			{
				// ToDo: Update stuff!
			}
		}

		private void MI_VPE_SettingsComp_Click(object sender, RoutedEventArgs e)
		{
			VPESettWin = new(ref VPE);
			VPESettWin.Show();
			VPESettWin.VPESubmitSett += VPESettWin_VPESubmitSett;
		}

		private void MI_VPE_SaveSett_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveSettings();
		}

		private void MI_VPE_LoadSett_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.LoadSettings())
			{
				// ToDo: Update stuff!
			}
		}

		private void MI_VPE_SaveSettLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveSettingsLib();
		}

		private void MI_VPE_LoadSettLib_Click(object sender, RoutedEventArgs e)
		{
			bool isRenumberingNeeded = VPE.KL.Library.Count > 0; // If I load another lib from the disk but I already have something in memory, I know I'll merge them and they'll have to be renumbered.
			bool success = VPE.LoadSettingsLib();
			if (isRenumberingNeeded)
			{
				VPE.KL.ReIndexSetts();
			}
			if (success)
			{
				VPE.UpdateKeyesSelector();
			}
		}

		private void MI_VPE_SaveTableLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveTableLib();
		}

		private void MI_VPE_LoadTableLib_Click(object sender, RoutedEventArgs e)
		{
			bool success = VPE.LoadTableLib();
			if (success)
			{
				VPE.UpdateRotorSelectors();
				VPE.UpdateReflSelector();
				VPE.UpdateSwapSelector();
				VPE.UpdateScramblerSelectors();
			}
		}

		private void VPESettWin_VPESubmitSett()
		{
			DisplayRotorPozs(VPE.SelectedPoss);
		}

		private void B_VPE_RotPozMinus_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.SelectedPoss == 0)
			{
				VPE.SelectedPoss = VPE.ActiveKey.GetLastRotorPositionsIdx;
			}
			else
			{
				VPE.SelectedPoss--;
			}
			VPE.ActiveKey.SelectedPositions = VPE.SelectedPoss;
			DisplayRotorPozs(VPE.SelectedPoss);
		}

		private void B_VPE_RotPozPlus_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.SelectedPoss == VPE.ActiveKey.GetLastRotorPositionsIdx)
			{
				VPE.SelectedPoss = 0;
			}
			else
			{
				VPE.SelectedPoss++;
			}
			VPE.ActiveKey.SelectedPositions = VPE.SelectedPoss;
			DisplayRotorPozs(VPE.SelectedPoss);
		}

		private void B_VPE_UseCustPozSet_Click(object sender, RoutedEventArgs e)
		{
			if (Cmn.DataFromGUI_MainWin.VPE_RotPozStr is not null)
			{
				if (Cmn.DataFromGUI_MainWin.VPE_RotPozStr != "")
				{
					if (VPE.AddPosFromString(Cmn.DataFromGUI_MainWin.VPE_RotPozStr))
					{
						DisplayRotorPozs(-1);
						VPE.SelectedPoss = VPE.ActiveKey.SelectedPositions = VPE.ActiveKey.GetLastRotorPositionsIdx;
					}
				}
			}
		}

		private void B_VPE_RemSelRotPoz_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveKey.GetLastRotorPositionsIdx > 0)
			{
				VPE.ActiveKey.RemovePositions(VPE.SelectedPoss);
			}
		}

		#endregion
		/// <summary>Displays the rotor positions, does index of position set and the positions themselves.</summary>
		/// <param name="which">Which to display? -1 means the last one. -2 for selected ones.</param>
		private void DisplayRotorPozs(int which = -2)
		{
			Cmn.DataFromGUI_MainWin.VPE_RotPozStr = VPE.GetPossStrings(which);
			if (which < -2)
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = "";
			}
			else if (which == -2)
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = (VPE.ActiveKey.SelectedPositions).ToString();
				VPE.SelectedPoss = VPE.ActiveKey.SelectedPositions;
			}
			else if (which == -1)
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = (VPE.ActiveKey.GetLastRotorPositionsIdx).ToString();
				VPE.SelectedPoss = VPE.ActiveKey.GetLastRotorPositionsIdx;
			}
			else
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = which.ToString();
				VPE.SelectedPoss = which;
			}
		}
		#endregion

		#region NeueDT
		#endregion

		#region DTCalc
		#endregion

		#region Factorizator
		#endregion

		#region PswdGen
		#endregion

		#region MapyCzDownloader
		#endregion
	}
}