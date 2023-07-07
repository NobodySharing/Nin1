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
		private readonly C_VPE_MainWin DataFromGUI = new(); // ToDo: Put this in VPE's VM.
		private readonly PersistentStorageManager PS = new(); // ToDo: Implement.

		public MainWindow()
		{
			InitializeComponent();
			DataContext = DataFromGUI;
		}
		#endregion

		#region VPE
		private VPE_VM VPE = new();
		VPESettingsComp VPESettWin;
		private void MI_VPE_Encrypt_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveSett is not null)
			{
				DataFromGUI.VPE_EncrypStr = VPE.Encrypt(DataFromGUI.VPE_PlainStr);
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
				DataFromGUI.VPE_PlainStr = VPE.Decrypt(DataFromGUI.VPE_EncrypStr);
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

		private void MI_VPE_OpenUneMsgFile_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_PlainStr = VPE_VM.OpenMsgFile();

		private void MI_VPE_OpenEncMsgFile_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_EncrypStr = VPE_VM.OpenMsgFile();

		private void MI_VPE_SaveEncMsgFile_Click(object sender, RoutedEventArgs e) => VPE_VM.SaveMsgFile(DataFromGUI.VPE_EncrypStr);

		private void MI_VPE_SaveUneMsgFile_Click(object sender, RoutedEventArgs e) => VPE_VM.SaveMsgFile(DataFromGUI.VPE_PlainStr);

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
			if (DataFromGUI.VPE_RotPozStr is not null)
			{
				if (DataFromGUI.VPE_RotPozStr != "")
				{
					if (VPE.AddPozFromString(DataFromGUI.VPE_RotPozStr))
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
			DataFromGUI.VPE_RotPozStr = VPE.GetPozsStrings(which);
			if (which < -2)
			{
				DataFromGUI.VPE_RotPozIdxStr = "";
			}
			else if (which == -2)
			{
				DataFromGUI.VPE_RotPozIdxStr = (VPE.ActiveSett.SelectedPozitions).ToString();
			}
			else if (which == -1)
			{
				DataFromGUI.VPE_RotPozIdxStr = (VPE.ActiveSett.GetLastRotorPozitionsIdx).ToString();
			}
			else
			{
				DataFromGUI.VPE_RotPozIdxStr = which.ToString();
			}
		}

		private void AddRotorPozs()
		{
			VPE.ActiveSett.AddPozitionsUsingString(DataFromGUI.VPE_RotPozStr);
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