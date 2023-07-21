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
			DisplayRotorPozs(-1);
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			VPE.UpdateSettings();
			VPE.UpdateSettingsLib();
			VPE.UpdateTableLib();
			Cmn.PS.IsDefault = false;
			Cmn.PS.PathsToSettLib = VPE.SL.PathToThis;
			Cmn.PS.PathsToTableLib = VPE.TL.PathToThis;
			Cmn.SaveConfig();
		}
		#endregion

		#region VPE
		private VPE_VM VPE = new();
		VPESettingsComp VPESettWin;
		private void MI_VPE_Encrypt_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveSett is not null)
			{
				Cmn.DataFromGUI_MainWin.VPE_EncrypStr = VPE.Encrypt(Cmn.DataFromGUI_MainWin.VPE_PlainStr);
				DisplayRotorPozs(VPE.ActiveSett.SelectedPozitions);
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
			if (VPE.ActiveSett is not null)
			{
				Cmn.DataFromGUI_MainWin.VPE_PlainStr = VPE.Decrypt(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);
				DisplayRotorPozs(VPE.ActiveSett.SelectedPozitions);
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

		private void MI_VPE_OpenUneMsgFile_Click(object sender, RoutedEventArgs e) => Cmn.DataFromGUI_MainWin.VPE_PlainStr = VPE_VM.OpenMsgFile();

		private void MI_VPE_OpenEncMsgFile_Click(object sender, RoutedEventArgs e) => Cmn.DataFromGUI_MainWin.VPE_EncrypStr = VPE_VM.OpenMsgFile();

		private void MI_VPE_SaveEncMsgFile_Click(object sender, RoutedEventArgs e) => VPE_VM.SaveMsgFile(Cmn.DataFromGUI_MainWin.VPE_EncrypStr);

		private void MI_VPE_SaveUneMsgFile_Click(object sender, RoutedEventArgs e) => VPE_VM.SaveMsgFile(Cmn.DataFromGUI_MainWin.VPE_PlainStr);

		private void MI_VPE_QuickSettGen_Click(object sender, RoutedEventArgs e)
		{
			VPE.GenerateComplete();
			DisplayRotorPozs(VPE.ActiveSett.SelectedPozitions);
		}

		private void MI_VPE_QuickSettSave_Click(object sender, RoutedEventArgs e) => VPE.SaveSettings();

		private void MI_VPE_QuickSettOpen_Click(object sender, RoutedEventArgs e) => VPE.LoadSettings();
		
		private void MI_VPE_SettingsComp_Click(object sender, RoutedEventArgs e)
		{
			VPESettWin = new(ref VPE);
			VPESettWin.Show();
			VPESettWin.VPESubmitSett += VPESettWin_VPESubmitSett;
		}

		private void VPESettWin_VPESubmitSett()
		{
			DisplayRotorPozs(VPE.SelectedPozs);
		}

		private void B_VPE_RotPozMinus_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.SelectedPozs == 0)
			{
				VPE.SelectedPozs = VPE.ActiveSett.GetLastRotorPozitionsIdx;
			}
			else
			{
				VPE.SelectedPozs--;
			}
			VPE.ActiveSett.SelectedPozitions = VPE.SelectedPozs;
			DisplayRotorPozs(VPE.SelectedPozs);
		}

		private void B_VPE_RotPozPlus_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.SelectedPozs == VPE.ActiveSett.GetLastRotorPozitionsIdx)
			{
				VPE.SelectedPozs = 0;
			}
			else
			{
				VPE.SelectedPozs++;
			}
			VPE.ActiveSett.SelectedPozitions = VPE.SelectedPozs;
			DisplayRotorPozs(VPE.SelectedPozs);
		}

		private void B_VPE_UseCustPozSet_Click(object sender, RoutedEventArgs e)
		{
			if (Cmn.DataFromGUI_MainWin.VPE_RotPozStr is not null)
			{
				if (Cmn.DataFromGUI_MainWin.VPE_RotPozStr != "")
				{
					if (VPE.AddPozFromString(Cmn.DataFromGUI_MainWin.VPE_RotPozStr))
					{
						DisplayRotorPozs(-1);
						VPE.SelectedPozs = VPE.ActiveSett.SelectedPozitions = VPE.ActiveSett.GetLastRotorPozitionsIdx;
					}
				}
			}
		}

		private void B_VPE_RemSelRotPoz_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveSett.GetLastRotorPozitionsIdx > 0)
			{
				VPE.ActiveSett.RemovePozitions(VPE.SelectedPozs);
			}
		}

		private void DisplayRotorPozs(int which = -2)
		{
			Cmn.DataFromGUI_MainWin.VPE_RotPozStr = VPE.GetPozsStrings(which);
			if (which < -2)
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = "";
			}
			else if (which == -2)
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = (VPE.ActiveSett.SelectedPozitions).ToString();
			}
			else if (which == -1)
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = (VPE.ActiveSett.GetLastRotorPozitionsIdx).ToString();
			}
			else
			{
				Cmn.DataFromGUI_MainWin.VPE_RotPozIdxStr = which.ToString();
			}
		}

		private void AddRotorPozs()
		{
			VPE.ActiveSett.AddPozitionsUsingString(Cmn.DataFromGUI_MainWin.VPE_RotPozStr);
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