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
	public delegate void SubmitVPESett();
	public partial class MainWindow : Window
	{
		
		#region Common
		private readonly C_VPE_MainWin DataFromGUI = new();
		//private readonly PersistentStorage PS = new(); // ToDo: Implement.

		public MainWindow()
		{
			InitializeComponent();
			DataContext = DataFromGUI;
		}
		#endregion

		#region VPE
		private VPE_VM VPE = new();
		VPESettingsComp VPESettWin;
		public event SubmitVPESett Submit;
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

		private void MI_VPE_QuickSettOpen_Click(object sender, RoutedEventArgs e) => VPE.LoadSettings(true);
		
		private void MI_VPE_SettingsComp_Click(object sender, RoutedEventArgs e)
		{
			VPESettWin = new(ref VPE);
			VPESettWin.Show();
		}

		private void B_VPE_RotPozBack_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.SelectedPozs == 0)
			{
				VPE.SelectedPozs = VPE.ActiveSett.GetLastRotorPozitionsIdx;
			}
			else
			{
				VPE.SelectedPozs--;
			}
			DataFromGUI.VPE_SelPozSetStr = VPE.SelectedPozs.ToString();
			DisplayRotorPozs();
		}

		private void B_VPE_RotPozForw_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.SelectedPozs == VPE.ActiveSett.GetLastRotorPozitionsIdx)
			{
				VPE.SelectedPozs = 0;
			}
			else
			{
				VPE.SelectedPozs++;
			}
			DataFromGUI.VPE_SelPozSetStr = VPE.SelectedPozs.ToString();
			DisplayRotorPozs();
		}

		private void B_VPE_UseSelPozSet_Click(object sender, RoutedEventArgs e)
		{
			if (DataFromGUI.VPE_SelPozSetStr is not null)
			{
				if (DataFromGUI.VPE_SelPozSetStr != "")
				{
					if (int.TryParse(DataFromGUI.VPE_SelPozSetStr, out int idx))
					{
						if (idx >= -2 && idx <= VPE.ActiveSett.GetLastRotorPozitionsIdx)
						{
							VPE.ActiveSett.SelectedPozitions = VPE.SelectedPozs = idx;
							DisplayRotorPozs();
						}
					}
				}
			}
		}

		public void DisplayRotorPozs(int which = -2)
		{
			DataFromGUI.VPE_RotPozStr = VPE.GetPozsStrings(which);
			if (which < -2)
			{
				DataFromGUI.VPE_SelPozSetStr = "";
			}
			else if (which == -2)
			{
				DataFromGUI.VPE_SelPozSetStr = (VPE.ActiveSett.SelectedPozitions).ToString();
			}
			else if (which == -1)
			{
				DataFromGUI.VPE_SelPozSetStr = (VPE.ActiveSett.GetLastRotorPozitionsIdx).ToString();
			}
			else
			{
				DataFromGUI.VPE_SelPozSetStr = which.ToString();
			}
		}

		protected virtual void OnSubmitVPESett()
		{
			Submit?.Invoke();
		}

		public void SetRotorPozs()
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