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
		private ushort RotorsInGUI = 10, SwapsInGUI = 5;
		private const ushort TablesMax = 50, SwapsMax = 20; // Kolik tam může být maximálně tabulek a swapů, v GUI.
		public C_VPE_Sett DataFromGUI;
		public VPESettingsComp (ref VPE_VM VModel)
		{
			InitializeComponent ();
			VPE = VModel;
			DataFromGUI = VPE.DataFromGUI_Sett;
			DataContext = DataFromGUI;
			InitialRotorPopulation(RotorsInGUI);
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
				VPE.GenerateRotors(DataFromGUI.RotorGenCountNum.Value);
				VPE.UpdateRotorSelectors();
			}
		}

		private void B_GenRefl_Click (object sender, RoutedEventArgs e)
		{
			if (DataFromGUI.ReflGenCountNum is not null)
			{
				VPE?.GenerateReflector(DataFromGUI.ReflGenCountNum.Value);
			}	
		}

		private void B_GenSwaps_Click (object sender, RoutedEventArgs e)
		{
			if (DataFromGUI.SwapGenCountNum is not null)
			{
				VPE?.GenerateSwaps(DataFromGUI.SwapGenCountNum.Value);
			}
		}

		private void B_Rotors_Add_Click(object sender, RoutedEventArgs e)
		{
			if (RotorsInGUI < TablesMax)
			{

				RotorsInGUI++;
			}
		}

		private void B_Rotors_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Rotors.Children.Count > 0)
			{
				SP_Rotors.Children.RemoveAt(SP_Rotors.Children.Count - 1);
				B_Rotors_Remove.IsEnabled = SP_Rotors.Children.Count > 0;
			}
			RotorsInGUI--;
			B_Rotors_Remove.IsEnabled = RotorsInGUI > 0;
		}

		private void B_Swaps_Add_Click(object sender, RoutedEventArgs e)
		{
			SwapsInGUI = Convert.ToUInt16(SP_Swaps.Children.Count);
			if (SwapsInGUI < SwapsMax)
			{
				ComboBox swap = new()
				{
					Name = "CB_Swap_" + (SwapsInGUI - 1).ToString(),
					ItemsSource = VPE.Swaps,
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
			B_Swaps_Add.IsEnabled = SwapsInGUI > 0;
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
			ushort[] space = VPE?.GenerateSpaceMinMax();
			DataFromGUI.RandCharSpcMinStr = space[0].ToString();
			DataFromGUI.RandCharSpcMaxStr = space[1].ToString();
		}

		private void B_GenRNDConsts_Click(object sender, RoutedEventArgs e)
		{
			decimal[] consts = VPE?.GenerateRNDConsts();
			DataFromGUI.RandCharGenAStr = consts[0].ToString();
			DataFromGUI.RandCharGenBStr = consts[1].ToString();
			DataFromGUI.RandCharGenMStr = consts[2].ToString();
		}

		private void B_AllRandom_Click(object sender, RoutedEventArgs e)
		{
			VPE.GenerateComplete();
			DataFromGUI.SetUsingSettings(VPE.ActiveSett);
		}
		#endregion
		#region Private
		private void InitialRotorPopulation(ushort count)
		{
			VPE.InitializeRotorSelectors(count);
			for (ushort i = 0; i < count; i++)
			{
				UC_Rotor rotor = new(VPE.DataFromGUI_Sett_Rotors[i])
				{
					Name = "UCR_" + i.ToString(),
					DataContext = VPE.DataFromGUI_Sett_Rotors[i],
				};
				SP_Rotors.Children.Add(rotor);
			}
		}
		#endregion
	}
}