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
		//public C_VPE_Sett DataFromGUI;
		public VPESettingsComp (ref VPE_VM VModel)
		{
			InitializeComponent ();
			VPE = VModel;
			DataContext = VPE.DataFromGUI_Sett;
			SP_Refls.DataContext = VPE.DataFromGUI_Refl;
			CB_SettLib.DataContext = VPE.DataFromGUI_SettSel;
			InitialRotorPopulation(10);
			InitialSwapsPopulation(5);
		}
		#region GUI eventy
		private void B_Submit_Click (object sender, RoutedEventArgs e)
		{
			VPE.ChangeActiveSettsFromGUI();
			Close ();
		}

		private void B_GenRotors_Click (object sender, RoutedEventArgs e)
		{
			if (VPE.DataFromGUI_Sett.RotorGenCountNum is not null)
			{
				VPE.GenerateRotors(VPE.DataFromGUI_Sett.RotorGenCountNum.Value);
				VPE.UpdateRotorSelectors();
			}
		}

		private void B_GenRefl_Click (object sender, RoutedEventArgs e)
		{
			if (VPE.DataFromGUI_Sett.ReflGenCountNum is not null)
			{
				VPE?.GenerateReflector(VPE.DataFromGUI_Sett.ReflGenCountNum.Value);
			}	
		}

		private void B_GenSwaps_Click (object sender, RoutedEventArgs e)
		{
			if (VPE.DataFromGUI_Sett.SwapGenCountNum is not null)
			{
				VPE?.GenerateSwaps(VPE.DataFromGUI_Sett.SwapGenCountNum.Value);
			}
		}

		private void B_Rotors_Add_Click(object sender, RoutedEventArgs e)
		{
			AddRotorsGUI();
		}

		private void B_Rotors_Remove_Click(object sender, RoutedEventArgs e)
		{
			RemoveRotorsGUI();
		}

		private void B_Swaps_Add_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Swaps.Children.Count < SwapsMax)
			{
				SP_Swaps.Children.Add(ConstructCBSwap());
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

		private void B_SaveSett_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveSettings();
		}

		private void B_LoadSett_Click(object sender, RoutedEventArgs e)
		{
			VPE.LoadSettings();
		}

		private void B_SaveSettLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveSettingsLib();
		}

		private void B_LoadSettLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.LoadSettingsLib();
		}

		private void B_SaveTableLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveTableLib();
		}

		private void B_LoadTableLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.LoadTableLib();
		}

		private void B_GenerateConstShift_Click(object sender, RoutedEventArgs e)
		{
			VPE.DataFromGUI_Sett.ConstShiftStr = VPE.GenerateRandNum().ToString();
		}

		private void B_GenerateVarShift_Click(object sender, RoutedEventArgs e)
		{
			VPE.DataFromGUI_Sett.VarShiftStr = VPE.GenerateRandNum().ToString();
		}

		private void B_RandCharSpc_Click(object sender, RoutedEventArgs e)
		{
			VPE.GenerateSpaceMinMax();
		}

		private void B_GenRNDConsts_Click(object sender, RoutedEventArgs e)
		{
			VPE.GenerateABMConsts();
		}

		private void B_AllRandom_Click(object sender, RoutedEventArgs e)
		{
			VPE.GenerateComplete();
			DisplayActiveSettInGUI();
		}

		private void ChB_Overwrite_Click(object sender, RoutedEventArgs e)
		{
			VPE.Overwrite = !VPE.Overwrite;
		}

		private void CB_SettLib_SelChanged(object sender, SelectionChangedEventArgs e)
		{
			VPE.SetUsingSelSettName();
			DisplayActiveSettInGUI();
		}

		private void B_Save_Click(object sender, RoutedEventArgs e)
		{
			VPE.ChangeActiveSettsFromGUI();
		}
		#endregion
		#region 
		private void DisplayActiveSettInGUI()
		{
			VPE.DataFromGUI_Sett.SetUsingSettings(VPE.ActiveSett);
			// ToDo: Vygeneruj bindingový data pro nový swapy, rotory apod.
			SynchronizeRotorCount();
			SynchronizeSwapCount();
			SynchronizeRotorSelAndPoz();
			SynchronizeSwapSel();
			VPE.DataFromGUI_Refl.SelectedStr = VPE.ActiveSett.Reflector.Idx.ToString();
		}
		
		private void InitialRotorPopulation(ushort count)
		{
			VPE.InitializeRotorSelectors(count);
			for (ushort i = 0; i < count; i++)
			{
				UC_Rotor rotor = new(VPE.DataFromGUI_Sett_Rotors[i])
				{
					Name = "UCR_" + i.ToString(),
				};
				SP_Rotors.Children.Add(rotor);
			}
		}

		private void InitialSwapsPopulation(ushort count)
		{
			VPE.InitializeSwapSelectors(count);
			for (ushort i = 0; i < count; i++)
			{
				SP_Swaps.Children.Add(ConstructCBSwap());
			}
		}

		private ComboBox ConstructCBSwap()
		{
			Binding items = new("C_VPE_ComboBox")
			{
				Mode = BindingMode.OneWay,
				Source = VPE.DataFromGUI_Swaps[SP_Swaps.Children.Count].ItemsStrs,
			};
			Binding selected = new("C_VPE_ComboBox")
			{
				Mode = BindingMode.TwoWay,
				Source = VPE.DataFromGUI_Swaps[SP_Swaps.Children.Count].SelectedStr,
			};
			ComboBox swap = new()
			{
				Name = "CB_Swap_" + SP_Swaps.Children.Count.ToString(),
				DataContext = VPE.DataFromGUI_Swaps[SP_Swaps.Children.Count],
				Margin = new Thickness(4, 2, 4, 2),
			};
			swap.SetBinding(ComboBox.ItemsSourceProperty, items);
			swap.SetBinding(ComboBox.SelectedValueProperty, selected);
			return swap;
		}

		private void SynchronizeRotorCount()
		{
			if (VPE.ActiveSett.Rotors.Count != SP_Rotors.Children.Count)
			{
				if (VPE.ActiveSett.Rotors.Count > SP_Rotors.Children.Count)
				{
					AddRotorsGUI(VPE.ActiveSett.Rotors.Count - SP_Rotors.Children.Count);
				}
				else
				{
					RemoveRotorsGUI(SP_Rotors.Children.Count - VPE.ActiveSett.Rotors.Count);
				}
			}
		}
		
		private void AddRotorsGUI(int count = 1)
		{
			for (int i = 0; i <= count; i++)
			{
				if (SP_Rotors.Children.Count < TablesMax)
				{
					C_UC_Rotor DataForRotorGUI = new()
					{
						RotorsStrs = VPE.DataFromGUI_Sett_Rotors[0].RotorsStrs,
					};
					VPE.DataFromGUI_Sett_Rotors.Add(DataForRotorGUI);
					UC_Rotor GUIRotor = new(DataForRotorGUI)
					{
						Name = "UCR_" + (SP_Rotors.Children.Count - 1).ToString(),
					};
					SP_Rotors.Children.Add(GUIRotor);
				}
			}
		}

		private void RemoveRotorsGUI(int count = 1)
		{
			for (int i = 0; i <= count; i++)
			{
				if (SP_Rotors.Children.Count > 0)
				{
					SP_Rotors.Children.RemoveAt(SP_Rotors.Children.Count - 1);
					VPE.DataFromGUI_Sett_Rotors.Remove(VPE.DataFromGUI_Sett_Rotors.Last());
					B_Rotors_Remove.IsEnabled = SP_Rotors.Children.Count > 0;
				}
			}
		}

		private void SynchronizeSwapCount()
		{
			if (VPE.ActiveSett.Swaps.Count != SP_Swaps.Children.Count)
			{
				if (VPE.ActiveSett.Swaps.Count > SP_Swaps.Children.Count)
				{
					AddSwapsGUI(VPE.ActiveSett.Swaps.Count - SP_Swaps.Children.Count);
				}
				else
				{
					RemoveSwapsGUI(SP_Swaps.Children.Count - VPE.ActiveSett.Swaps.Count);
				}
			}
		}

		private void AddSwapsGUI(int count = 1)
		{
			for (ushort i = 0; i < count; i++)
			{
				SP_Swaps.Children.Add(ConstructCBSwap());
			}
		}

		private void RemoveSwapsGUI(int count = 1)
		{
			for (ushort i = 0; i < count; i++)
			{
				SP_Swaps.Children.RemoveAt(SP_Swaps.Children.Count - 1);
				VPE.DataFromGUI_Swaps.RemoveAt(VPE.DataFromGUI_Swaps.Count - 1);
			}
		}

		private void SynchronizeRotorSelAndPoz()
		{
			for(int i = 0; i < VPE.DataFromGUI_Sett_Rotors.Count; i++)
			{
				VPE.DataFromGUI_Sett_Rotors[i].SelectedRStr = VPE.ActiveSett.Rotors[i].Idx.ToString();
				VPE.DataFromGUI_Sett_Rotors[i].PozitionStr = VPE.ActiveSett.Rotors[i].Pozition.ToString();
			}
		}

		private void SynchronizeSwapSel()
		{
			for (int i = 0; i < VPE.DataFromGUI_Swaps.Count; i++)
			{
				VPE.DataFromGUI_Swaps[i].SelectedStr = VPE.ActiveSett.Swaps[i].Idx.ToString();
			}
		}
		#endregion
	}
}