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
		public MainWindow ()
		{
			InitializeComponent ();
			DataContext = DataFromGUI;
		}
		#region Common
		private C_VPE_MainWin DataFromGUI = new();
		private PersistentStorage PS = new();
		
		#endregion

		#region VPE
		private VPE_VM VPE = new();
		VPESettingsComp VPESettWin;
		VPESettingsSelector VPESettingsSelector;

		private void MI_VPE_Encrypt_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_EncrypStr = VPE.Encrypt(DataFromGUI.VPE_NormalStr);

		private void MI_VPE_Decrypt_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_NormalStr = VPE.Decrypt(DataFromGUI.VPE_EncrypStr);

		private void MI_VPE_SettingsComp_Click(object sender, RoutedEventArgs e)
		{
			VPESettWin = new(ref VPE);
			VPESettWin.Show();
		}

		private void MI_VPE_SettingsSel_Click(object sender, RoutedEventArgs e)
		{
			VPESettingsSelector = new();
			VPESettingsSelector.Show();
		}

		private void MI_VPE_OpenUneMsgFile_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_NormalStr = VPE.OpenMsgFile();

		private void MI_VPE_OpenEncMsgFile_Click(object sender, RoutedEventArgs e) => DataFromGUI.VPE_EncrypStr = VPE.OpenMsgFile();

		private void MI_VPE_SaveEncMsgFile_Click(object sender, RoutedEventArgs e) => VPE.SaveMsgFile(DataFromGUI.VPE_EncrypStr);

		private void MI_VPE_SaveUneMsgFile_Click(object sender, RoutedEventArgs e) => VPE.SaveMsgFile(DataFromGUI.VPE_NormalStr);

		private void MI_VPE_QuickSettGen_Click(object sender, RoutedEventArgs e) => VPE.QuickSettGen();

		private void MI_VPE_QuickSettSave_Click(object sender, RoutedEventArgs e) => VPE.QuickSettSave();

		private void MI_VPE_QuickSettOpen_Click(object sender, RoutedEventArgs e) => VPE.QuickSettOpen();
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