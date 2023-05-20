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
		private C_VPE_MainWin DataFromGUI = new();
		private PersistentStorage PS = new();

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
			if(VPE.ActiveSett is not null)
			{
				DataFromGUI.VPE_EncrypStr = VPE.Encrypt(DataFromGUI.VPE_PlainStr);
			}
			else
			{
				string caption = "Error";
				string messageBoxText = "I am unable to encrypt your text. No encryption settings were selected.";
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Error;
				MessageBoxResult result;
				result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
			}
		}

		private void MI_VPE_Decrypt_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.ActiveSett is not null)
			{
				DataFromGUI.VPE_PlainStr = VPE.Decrypt(DataFromGUI.VPE_EncrypStr);
			}
			else
			{
				string caption = "Error";
				string messageBoxText = "I am unable to decrypt your text. No decryption settings were selected.";
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Error;
				MessageBoxResult result;
				result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
			}
		}

		private void MI_VPE_OpenUneMsgFile_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_PlainStr = VPE.OpenMsgFile();

		private void MI_VPE_OpenEncMsgFile_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_EncrypStr = VPE.OpenMsgFile();

		private void MI_VPE_SaveEncMsgFile_Click(object sender, RoutedEventArgs e) => VPE.SaveMsgFile(DataFromGUI.VPE_EncrypStr);

		private void MI_VPE_SaveUneMsgFile_Click(object sender, RoutedEventArgs e) => VPE.SaveMsgFile(DataFromGUI.VPE_PlainStr);

		private void MI_VPE_QuickSettGen_Click(object sender, RoutedEventArgs e) => VPE.SettGen();

		private void MI_VPE_QuickSettSave_Click(object sender, RoutedEventArgs e) => VPE.SaveSettings();

		private void MI_VPE_QuickSettOpen_Click(object sender, RoutedEventArgs e) => VPE.LoadSettings(true);
		
		private void MI_VPE_SettingsComp_Click(object sender, RoutedEventArgs e)
		{
			VPESettWin = new(ref VPE);
			VPESettWin.Show();
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