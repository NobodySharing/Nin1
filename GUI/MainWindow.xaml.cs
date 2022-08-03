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

namespace GUI
{
	/// <summary>Interaction logic for MainWindow.xaml</summary>
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent ();
		}
		#region VPE
		private string InText { get; set; }
		private string OutText { get; set; }
		private VPE_VM VPE = new VPE_VM();
		private void B_VPE_Encrypt_Click (object sender, RoutedEventArgs e)
		{
			OutText = VPE.Encrypt(InText);
		}

		private void B_VPE_Decrypt_Click (object sender, RoutedEventArgs e)
		{
			OutText = VPE.Decrypt(InText);
		}

		private void B_VPE_Settings_Click(object sender, RoutedEventArgs e)
		{
			VPESettings VPESettWin = new(ref VPE);
			VPESettWin.Show();
		}

		private void B_VPE_OpenUneMsgFile_Click(object sender, RoutedEventArgs e)
		{
			InText = VPE.OpenMsgFile();
		}

		private void B_VPE_OpenEncMsgFile_Click(object sender, RoutedEventArgs e)
		{
			OutText = VPE.OpenMsgFile();
		}

		private void B_VPE_SaveEncMsgFile_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveMsgFile(OutText);
		}

		private void B_VPE_QuickSettGen_Click(object sender, RoutedEventArgs e)
		{
			VPE.QuickSettGen();
		}

		private void B_VPE_QuickSettSave_Click(object sender, RoutedEventArgs e)
		{
			VPE.QuickSettSave();
		}

		private void B_VPE_QuickSettOpen_Click(object sender, RoutedEventArgs e)
		{
			VPE.QuickSettOpen();
		}
		#endregion
	}
}