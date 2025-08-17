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
	public delegate void SubmitVPESett();
	public partial class VPESettingsComp : Window
	{
		private readonly VPE_VM VPE;
		public event SubmitVPESett VPESubmitSett;
		private const ushort RotorsMax = 50, SwapsMax = 25; // Maximum count of rotors and swaps.
		public VPESettingsComp (ref VPE_VM VModel)
		{
			InitializeComponent();
			VPE = VModel;
			SetDataContexts();
			PopulateWindow();
		}
		#region GUI events
		private void B_Submit_Click (object sender, RoutedEventArgs e)
		{
			VPE.ChangeActiveKeyFromGUI();
			VPE.UpdateKeyesSelector();
			OnSubmitVPESett();
			Close();
		}

		private void B_GenRotors_Click (object sender, RoutedEventArgs e)
		{
			if (VPE.DataFromGUI_Key.RotorGenCountNum is not null)
			{
				VPE.GenerateRotors(VPE.DataFromGUI_Key.RotorGenCountNum.Value);
				VPE.UpdateRotorSelectors();
			}
		}

		private void B_GenRefl_Click (object sender, RoutedEventArgs e)
		{
			if (VPE.DataFromGUI_Key.ReflGenCountNum is not null)
			{
				VPE.GenerateReflectors(VPE.DataFromGUI_Key.ReflGenCountNum.Value);
				VPE.UpdateReflSelector();
			}	
		}

		private void B_GenSwaps_Click (object sender, RoutedEventArgs e)
		{
			if (VPE.DataFromGUI_Key.SwapGenCountNum is not null)
			{
				VPE.GenerateSwaps(VPE.DataFromGUI_Key.SwapGenCountNum.Value);
				VPE.UpdateSwapSelector();
			}
		}

		private void B_GenScrs_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.DataFromGUI_Key.ScrsGenCountNum is not null)
			{
				VPE.GenerateScramblers(VPE.DataFromGUI_Key.ScrsGenCountNum.Value);
				VPE.UpdateScramblerSelectors();
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
				VPE.AddSwapDataForGUI();
				SP_Swaps.Children.Add(ConstructCBSwap());
				B_Swaps_Add.IsEnabled = SP_Swaps.Children.Count < SwapsMax;
			}
			B_Swaps_Remove.IsEnabled = SP_Swaps.Children.Count > 0;
		}

		private void B_Swaps_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (SP_Swaps.Children.Count > 0)
			{
				SP_Swaps.Children.RemoveAt(SP_Swaps.Children.Count - 1);
				VPE.DataFromGUI_Swaps.RemoveAt(SP_Swaps.Children.Count);
				B_Swaps_Remove.IsEnabled = SP_Swaps.Children.Count > 0;
			}
			B_Swaps_Add.IsEnabled = SP_Swaps.Children.Count < SwapsMax;
		}

		private void B_SaveSett_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveSettings();
		}

		private void B_LoadSett_Click(object sender, RoutedEventArgs e)
		{
			if (VPE.LoadSettings())
			{
				DisplayActiveSettInGUI();
			}
		}

		private void B_SaveSettLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveSettingsLib();
		}

		private void B_LoadSettLib_Click(object sender, RoutedEventArgs e)
		{
			bool isRenumberingneeded = VPE.KL.Library.Count > 0; // If I load another lib from the disk but I already have something in memory, I know I'll merge them and they'll have to be renumbered.
			bool success = VPE.LoadSettingsLib();
			if (isRenumberingneeded)
			{
				VPE.KL.ReIndexSetts();
			}
			if (success)
			{
				VPE.UpdateKeyesSelector();
			}
		}

		private void B_SaveTableLib_Click(object sender, RoutedEventArgs e)
		{
			VPE.SaveTableLib();
		}

		private void B_LoadTableLib_Click(object sender, RoutedEventArgs e)
		{
			bool success = VPE.LoadTableLib();
			if (success)
			{
				VPE.UpdateRotorSelectors();
				VPE.UpdateReflSelector();
				VPE.UpdateSwapSelector();
				VPE.UpdateScramblerSelectors();
			}
		}

		private void B_GenerateConstShift_Click(object sender, RoutedEventArgs e)
		{
			VPE.DataFromGUI_Key.ConstShiftStr = VPE.GenerateRandNum().ToString();
		}

		private void B_GenerateVarShift_Click(object sender, RoutedEventArgs e)
		{
			VPE.DataFromGUI_Key.VarShiftStr = VPE.GenerateRandNum().ToString();
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

		private void CB_SettLib_SelChanged(object sender, SelectionChangedEventArgs e)
		{
			if (VPE.ActiveKey is null)
			{
				return;
			}
			VPE.SetUsingSelKeyName();
			DisplayActiveSettInGUI();
		}

		private void B_SaveChang_Click(object sender, RoutedEventArgs e)
		{
			VPE.ChangeActiveKeyFromGUI();
			VPE.UpdateKeyesSelector();
		}

		private void B_GenSwitchConsts_Click(object sender, RoutedEventArgs e)
		{
			VPE.GenerateABCDConsts();
		}

		private void B_Rename_Click(object sender, RoutedEventArgs e)
		{
			VPE.RenameSelKey();
		}

		private void B_RemoveActive_Click(object sender, RoutedEventArgs e)
		{
			VPE.RemoveSelKey();
		}
		#endregion
		#region Private utility methods
		private void DisplayActiveSettInGUI()
		{
			VPE.DisplaySettsInGUI(VPE.ActiveKey);
			SynchronizeRotorCountAndData();
			SynchronizeSwapCountAndData();
		}

		private void InitialRotorPopulation(ushort count)
		{
			VPE.InitializeRotorSelectors(count);
			for (ushort i = 0; i < count; i++)
			{
				UC_Rotor rotor = new(VPE.DataFromGUI_Rotors[i])
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
			Binding items = new("ItemsStrs")
			{
				Mode = BindingMode.OneWay,
				Source = VPE.DataFromGUI_Swaps[SP_Swaps.Children.Count],
			};
			Binding selected = new("SelectedStr")
			{
				Mode = BindingMode.TwoWay,
				Source = VPE.DataFromGUI_Swaps[SP_Swaps.Children.Count],
			};
			Binding idx = new("SelectedIdx")
			{
				Mode = BindingMode.TwoWay,
				Source = VPE.DataFromGUI_Swaps[SP_Swaps.Children.Count],
			};
			ComboBox swap = new()
			{
				Name = "CB_Swap_" + SP_Swaps.Children.Count.ToString(),
				DataContext = VPE.DataFromGUI_Swaps[SP_Swaps.Children.Count],
				Margin = new Thickness(4, 2, 4, 2),
			};
			swap.SetBinding(ComboBox.ItemsSourceProperty, items);
			swap.SetBinding(ComboBox.SelectedValueProperty, idx);
			swap.SetBinding(ComboBox.SelectedValueProperty, selected);
			return swap;
		}

		private void SynchronizeRotorCountAndData()
		{
			if (VPE.ActiveKey.Rotors.Count != SP_Rotors.Children.Count)
			{
				if (VPE.ActiveKey.Rotors.Count > SP_Rotors.Children.Count)
				{
					AddRotorsGUI(VPE.ActiveKey.Rotors.Count - SP_Rotors.Children.Count);
				}
				else
				{
					RemoveRotorsGUI(SP_Rotors.Children.Count - VPE.ActiveKey.Rotors.Count);
				}
			}
		}
		
		private void AddRotorsGUI(int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				if (SP_Rotors.Children.Count < RotorsMax)
				{
					UC_Rotor GUIRotor = new(VPE.DataFromGUI_Rotors[SP_Rotors.Children.Count])
					{
						Name = "UCR_" + SP_Rotors.Children.Count.ToString(),
					};
					SP_Rotors.Children.Add(GUIRotor);
				}
			}
		}

		private void RemoveRotorsGUI(int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				if (SP_Rotors.Children.Count > 1)
				{
					SP_Rotors.Children.RemoveAt(SP_Rotors.Children.Count - 1);
					B_Rotors_Remove.IsEnabled = SP_Rotors.Children.Count > 1;
				}
			}
		}

		private void SynchronizeSwapCountAndData()
		{
			if (VPE.ActiveKey.Swaps.Count != SP_Swaps.Children.Count)
			{
				if (VPE.ActiveKey.Swaps.Count > SP_Swaps.Children.Count)
				{
					AddSwapsGUI(VPE.ActiveKey.Swaps.Count - SP_Swaps.Children.Count);
				}
				else
				{
					RemoveSwapsGUI(SP_Swaps.Children.Count - VPE.ActiveKey.Swaps.Count);
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
				if (SP_Rotors.Children.Count > 1)
				{
					SP_Swaps.Children.RemoveAt(SP_Swaps.Children.Count - 1);
					B_Swaps_Remove.IsEnabled = SP_Rotors.Children.Count > 1;
				}
			}
		}

		private void SetDataContexts()
		{
			DataContext = VPE.DataFromGUI_Key;
			CB_SettLib.DataContext = VPE.DataFromGUI_SettSel;
			CB_Reflector.DataContext = VPE.DataFromGUI_Refl;
			CB_InScr.DataContext = VPE.DataFromGUI_InScr;
			CB_OutScr.DataContext = VPE.DataFromGUI_OutScr;
		}

		private void PopulateWindow()
		{
			if (VPE.ActiveKey is not null)
			{
				DisplayActiveSettInGUI();
				VPE.UpdateKeyesSelector();
			}
			else
			{
				InitialRotorPopulation(20);
				InitialSwapsPopulation(10);
				VPE.UpdateAll();
			}
		}
		#endregion
		#region Custom events
		public virtual void OnSubmitVPESett()
		{
			VPESubmitSett?.Invoke();
		}
		#endregion
	}
}