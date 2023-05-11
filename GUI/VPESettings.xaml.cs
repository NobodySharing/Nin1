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
		private const ushort TablesMax = 50, SwapsMax = 25; // Kolik tam může být maximálně tabulek a swapů, v GUI.
		public C_VPE_Sett DataFromGUI;
		public VPESettingsComp (ref VPE_VM VModel)
		{
			InitializeComponent ();
			VPE = VModel;
			DataFromGUI = VPE.DataFromGUI_Sett;
			DataContext = DataFromGUI;
			InitialRotorPopulation(10);
			InitialSwapsPopulation(5);
		}
		#region GUI eventy
		private void B_Submit_Click (object sender, RoutedEventArgs e)
		{
			VPE.ChangeActiveSetts();
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
			if (SP_Rotors.Children.Count < TablesMax)
			{
				C_UC_Rotor DataForRotorGUI = new()
				{
					RotorsStrs = VPE.DataFromGUI_Sett_Rotors[0].RotorsStrs,
				};
				VPE.DataFromGUI_Sett_Rotors.Add(DataForRotorGUI);
				UC_Rotor GUIRotor = new()
				{
					Name = "UCR_" + (SP_Rotors.Children.Count - 1).ToString(),
					DataContext = VPE.DataFromGUI_Sett_Rotors.Last(),
				};
				SP_Rotors.Children.Add(GUIRotor);
			}
		}

		private void B_Rotors_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Rotors.Children.Count > 0)
			{
				SP_Rotors.Children.RemoveAt(SP_Rotors.Children.Count - 1);
				VPE.DataFromGUI_Sett_Rotors.Remove(VPE.DataFromGUI_Sett_Rotors.Last());
				B_Rotors_Remove.IsEnabled = SP_Rotors.Children.Count > 0;
			}
		}

		private void B_Swaps_Add_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Swaps.Children.Count < SwapsMax)
			{
				SP_Swaps.Children.Add(ConstructCBSwap((ushort)SP_Swaps.Children.Count));
				B_Swaps_Add.IsEnabled = SP_Swaps.Children.Count < SwapsMax;
			}
		}

		private void B_Swaps_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Swaps.Children.Count > 0)
			{
				SP_Swaps.Children.RemoveAt(SP_Swaps.Children.Count - 1);
				VPE.DataFromGUI_Swaps.RemoveAt(SP_Swaps.Children.Count);
				B_Swaps_Remove.IsEnabled = SP_Swaps.Children.Count > 0;
			}
			B_Swaps_Add.IsEnabled = SP_Swaps.Children.Count > 0;
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

		private void InitialSwapsPopulation(ushort count)
		{
			VPE.InitializeSwapSelectors(count);
			for (ushort i = 0; i < count; i++)
			{
				SP_Swaps.Children.Add(ConstructCBSwap(i));
			}
		}

		private ComboBox ConstructCBSwap(ushort index)
		{
			Binding items = new("C_VPE_ComboBox")
			{
				Mode = BindingMode.OneWay,
				Source = VPE.DataFromGUI_Swaps[index].ItemsStrs,
			};
			Binding selected = new("C_VPE_ComboBox")
			{
				Mode = BindingMode.TwoWay,
				Source = VPE.DataFromGUI_Swaps[index].SelectedStr,
			};
			ComboBox swap = new()
			{
				Name = "CB_Swap_" + index.ToString(),
			};
			swap.SetBinding(ComboBox.ItemsSourceProperty, items);
			swap.SetBinding(ComboBox.SelectedValueProperty, selected);
			return swap;
		}
		#endregion
	}
}