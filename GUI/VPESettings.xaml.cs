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
using System.Windows.Shapes;
using VPE;

namespace GUI
{
	public partial class VPESettingsComp : Window
	{
		private readonly VPE_VM VPE;
		private ushort TablesInGUI = 0, SwapsInGUI = 0;
		private const ushort TablesMax = 50, SwapsMax = 20; // Kolik tam může být maximálně tabulek a swapů, v GUI.
		public C_VPE_Sett DataFromGUI = new();
		public VPESettingsComp (ref VPE_VM VModel)
		{
			InitializeComponent ();
			VPE = VModel;
			//DataContext = DataFromGUI;
		}
		#region GUI eventy
		private void B_Submit_Click (object sender, RoutedEventArgs e)
		{ // ToDo: Everything.
			
			Close ();
		}

		private void B_GenRotors_Click (object sender, RoutedEventArgs e)
		{
			if (DataFromGUI.RotorGenCountNum is not null)
			{
				VPE?.GenerateRotors(DataFromGUI.RotorGenCountNum.Value);
				foreach (UC_Table rotor in SP_Rotors.Children)
				{
					rotor.UpdateTableList(VPE.Rotors);
				}
				return;
			}
		}

		private void B_GenRefl_Click (object sender, RoutedEventArgs e)
		{
			VPE?.GenerateReflector ();
		}

		private void B_GenSwaps_Click (object sender, RoutedEventArgs e)
		{
			VPE?.GenerateSwaps ();
		}

		private void B_Rotors_Add_Click(object sender, RoutedEventArgs e)
		{
			TablesInGUI = Convert.ToUInt16(SP_Rotors.Children.Count);
			if (TablesInGUI < TablesMax)
			{
				UC_Table table = new()
				{
					Name = "UCT_" + (TablesInGUI - 1).ToString(),
				};
				SP_Rotors.Children.Add(table);
				B_Rotors_Add.IsEnabled = TablesInGUI < TablesMax;
			}
		}

		private void B_Rotors_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Rotors.Children.Count > 0)
			{
				SP_Rotors.Children.RemoveAt(SP_Rotors.Children.Count - 1);
				B_Rotors_Remove.IsEnabled = SP_Rotors.Children.Count > 0;
			}
		}

		private void B_Swaps_Add_Click(object sender, RoutedEventArgs e)
		{
			SwapsInGUI = Convert.ToUInt16(SP_Swaps.Children.Count);
			if (SwapsInGUI < SwapsMax)
			{
				ComboBox swap = new()
				{
					Name = "CB_Swap_" + (SwapsInGUI - 1).ToString(),
				};
				SP_Swaps.Children.Add(swap);
				B_Swaps_Add.IsEnabled = SwapsInGUI < SwapsMax;
			}
		}

		private void B_Swaps_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Swaps.Children.Count > 0)
			{
				SP_Swaps.Children.RemoveAt(SP_Swaps.Children.Count - 1);
				B_Swaps_Remove.IsEnabled = SP_Swaps.Children.Count > 0;
			}
		}

		private void B_Save_Click(object sender, RoutedEventArgs e)
		{
			VPE?.SaveTables();
		}

		private void B_Load_Click(object sender, RoutedEventArgs e)
		{
			VPE.LoadTables();
		}

		private void B_LoadMerge_Click(object sender, RoutedEventArgs e)
		{
			VPE.LoadAndMerge();
		}

		private void B_Exp_Click(object sender, RoutedEventArgs e)
		{
			VPE?.SaveSpecific();
		}

		private void B_Imp_Click(object sender, RoutedEventArgs e)
		{
			VPE?.LoadSpecific();
		}

		private void B_GenerateConstShift_Click(object sender, RoutedEventArgs e)
		{
			DataFromGUI.ConstShiftStr = VPE?.GenerateRandNum().ToString();
		}

		private void B_GenerateVarShift_Click(object sender, RoutedEventArgs e)
		{
			DataFromGUI.VarShiftStr = VPE?.GenerateRandNum().ToString();
		}

		private void B_RandCharSpc_Click(object sender, RoutedEventArgs e)
		{
			DataFromGUI.RandCharSpcMin = VPE?.GenerateRandNum().ToString();
		}

		private void B_GenRNDConsts_Click(object sender, RoutedEventArgs e)
		{
			decimal[] consts = VPE?.GenerateRNDConsts();
			
		}

		private void B_AllRandom_Click(object sender, RoutedEventArgs e)
		{
			
		}
		#endregion
	}
}