using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPE;
using System.Collections.ObjectModel;

namespace GUI
{
	#region Main win
	public class C_MainWin : INotifyPropertyChanged
	{
		private string VPE_PlainStrV;
		private string VPE_EncrypStrV;
		private string VPE_RotPozStrV;
		private string VPE_RotPozIdxStrV;
		private bool VPE_NumericRepresV;
		public string VPE_PlainStr
		{
			get
			{
				return VPE_PlainStrV;
			}
			set
			{
				if (VPE_PlainStrV != value)
				{
					VPE_PlainStrV = value;
					OnPropertyChanged("VPE_PlainStr");
				}
			}
		}
		public string VPE_EncrypStr
		{
			get
			{
				return VPE_EncrypStrV;
			}
			set
			{
				if (VPE_EncrypStrV != value)
				{
					VPE_EncrypStrV = value;
					OnPropertyChanged("VPE_EncrypStr");
				}
			}
		}
		public string VPE_RotPozStr
		{
			get
			{
				return VPE_RotPozStrV;
			}
			set
			{
				if (VPE_RotPozStrV != value)
				{
					VPE_RotPozStrV = value;
					OnPropertyChanged("VPE_RotPozStr");
				}
			}
		}
		public string VPE_RotPozIdxStr
		{
			get
			{
				return VPE_RotPozIdxStrV;
			}
			set
			{
				if (VPE_RotPozIdxStrV != value)
				{
					VPE_RotPozIdxStrV = value;
					OnPropertyChanged("VPE_RotPozIdxStr");
				}
			}
		}
		public bool VPE_NumericRepres
		{
			get
			{
				return VPE_NumericRepresV;
			}
			set
			{
				if (VPE_NumericRepresV != value)
				{
					VPE_NumericRepresV = value;
					OnPropertyChanged("VPE_NumericRepres");
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}
	#endregion
	#region VPE
	public class C_VPE_Sett : INotifyPropertyChanged
	{
		private string NameStrV;
		private string RotorGenCountStrV;
		private string SwapGenCountStrV;
		private string ReflGenCountStrV;
		private string ScrsGenCountStrV;
		private string ConstShiftStrV;
		private string VarShiftStrV;
		private string RandCharSpcMinStrV;
		private string RandCharSpcMaxStrV;
		private string RandCharGenAStrV;
		private string RandCharGenBStrV;
		private string RandCharGenMStrV;
		private string SwitchAStrV;
		private string SwitchBStrV;
		private string SwitchCStrV;
		private string SwitchDStrV;
		private ObservableCollection<C_PDC> RandCharAV = new();
		private ObservableCollection<C_PDC> RandCharBV = new();
		private ObservableCollection<C_PDC> RandCharMV = new();
		private ObservableCollection<C_PDC> SwitchAV = new();
		private ObservableCollection<C_PDC> SwitchBV = new();
		private ObservableCollection<C_PDC> SwitchCV = new();
		private ObservableCollection<C_PDC> SwitchDV = new();
		public string ConstShiftStr
		{
			get
			{
				return ConstShiftStrV;
			}
			set
			{
				if (ConstShiftStrV != value)
				{
					ConstShiftStrV = value;
					OnPropertyChanged("ConstShiftStr");
				}
			}
		}
		public uint? ConstShiftNum
		{
			get
			{
				if (uint.TryParse(ConstShiftStrV, out uint num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string VarShiftStr
		{
			get
			{
				return VarShiftStrV;
			}
			set
			{
				if (VarShiftStrV != value)
				{
					VarShiftStrV = value;
					OnPropertyChanged("VarShiftStr");
				}
			}
		}
		public uint? VarShiftNum
		{
			get
			{
				if (uint.TryParse(VarShiftStrV, out uint num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string RandCharSpcMinStr
		{
			get
			{
				return RandCharSpcMinStrV;
			}
			set
			{
				if (RandCharSpcMinStrV != value)
				{
					RandCharSpcMinStrV = value;
					OnPropertyChanged("RandCharSpcMinStr");
				}
			}
		}
		public ushort? RandCharSpcMinNum
		{
			get
			{
				if (ushort.TryParse(RandCharSpcMinStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string RandCharSpcMaxStr
		{
			get
			{
				return RandCharSpcMaxStrV;
			}
			set
			{
				if (RandCharSpcMaxStrV != value)
				{
					RandCharSpcMaxStrV = value;
					OnPropertyChanged("RandCharSpcMaxStr");
				}
			}
		}
		public ushort? RandCharSpcMaxNum
		{
			get
			{
				if (ushort.TryParse(RandCharSpcMaxStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string RandCharGenAStr
		{
			get
			{
				return RandCharGenAStrV;
			}
			set
			{
				if (RandCharGenAStrV != value)
				{
					RandCharGenAStrV = value;
					OnPropertyChanged("RandCharGenAStr");
				}
			}
		}
		public string RandCharGenBStr
		{
			get
			{
				return RandCharGenBStrV;
			}
			set
			{
				if (RandCharGenBStrV != value)
				{
					RandCharGenBStrV = value;
					OnPropertyChanged("RandCharGenBStr");
				}
			}
		}
		public string RandCharGenMStr
		{
			get
			{
				return RandCharGenMStrV;
			}
			set
			{
				if (RandCharGenMStrV != value)
				{
					RandCharGenMStrV = value;
					OnPropertyChanged("RandCharGenMStr");
				}
			}
		}
		public string SwitchAStr
		{
			get
			{
				return SwitchAStrV;
			}
			set
			{
				if (SwitchAStrV != value)
				{
					SwitchAStrV = value;
					OnPropertyChanged("SwitchAStr");
				}
			}
		}
		public string SwitchBStr
		{
			get
			{
				return SwitchBStrV;
			}
			set
			{
				if (SwitchBStrV != value)
				{
					SwitchBStrV = value;
					OnPropertyChanged("SwitchBStr");
				}
			}
		}
		public string SwitchCStr
		{
			get
			{
				return SwitchCStrV;
			}
			set
			{
				if (SwitchCStrV != value)
				{
					SwitchCStrV = value;
					OnPropertyChanged("SwitchCStr");
				}
			}
		}
		public string SwitchDStr
		{
			get
			{
				return SwitchDStrV;
			}
			set
			{
				if (SwitchDStrV != value)
				{
					SwitchDStrV = value;
					OnPropertyChanged("SwitchDStr");
				}
			}
		}
		public string RotorGenCountStr
		{
			get
			{
				return RotorGenCountStrV;
			}
			set
			{
				if (RotorGenCountStrV != value)
				{
					RotorGenCountStrV = value;
					OnPropertyChanged("RotorGenCountStr");
				}
			}
		}
		public ushort? RotorGenCountNum
		{
			get
			{
				if (ushort.TryParse(RotorGenCountStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string SwapGenCountStr
		{
			get
			{
				return SwapGenCountStrV;
			}
			set
			{
				if (SwapGenCountStrV != value)
				{
					SwapGenCountStrV = value;
					OnPropertyChanged("SwapGenCountStr");
				}
			}
		}
		public ushort? SwapGenCountNum
		{
			get
			{
				if (ushort.TryParse(SwapGenCountStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string ReflGenCountStr
		{
			get
			{
				return ReflGenCountStrV;
			}
			set
			{
				if (ReflGenCountStrV != value)
				{
					ReflGenCountStrV = value;
					OnPropertyChanged("ReflGenCountStr");
				}
			}
		}
		public ushort? ReflGenCountNum
		{
			get
			{
				if (ushort.TryParse(ReflGenCountStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string ScrsGenCountStr
		{
			get
			{
				return ScrsGenCountStrV;
			}
			set
			{
				if (ScrsGenCountStrV != value)
				{
					ScrsGenCountStrV = value;
					OnPropertyChanged("ScrsGenCountStr");
				}
			}
		}
		public ushort? ScrsGenCountNum
		{
			get
			{
				if (ushort.TryParse(ScrsGenCountStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string NameStr
		{
			get
			{
				return NameStrV;
			}
			set
			{
				if (NameStrV != value)
				{
					NameStrV = value;
					OnPropertyChanged("NameStr");
				}
			}
		}
		public ObservableCollection<C_PDC> RandCharA
		{
			get
			{
				return RandCharAV;
			}
			set
			{
				if (RandCharAV != value)
				{
					RandCharAV = value;
					OnPropertyChanged("RandCharA");
				}
			}
		}
		public ObservableCollection<C_PDC> RandCharB
		{
			get
			{
				return RandCharBV;
			}
			set
			{
				if (RandCharBV != value)
				{
					RandCharBV = value;
					OnPropertyChanged("RandCharB");
				}
			}
		}
		public ObservableCollection<C_PDC> RandCharM
		{
			get
			{
				return RandCharMV;
			}
			set
			{
				if (RandCharMV != value)
				{
					RandCharMV = value;
					OnPropertyChanged("RandCharM");
				}
			}
		}
		public ObservableCollection<C_PDC> SwitchA
		{
			get
			{
				return SwitchAV;
			}
			set
			{
				if (SwitchAV != value)
				{
					SwitchAV = value;
					OnPropertyChanged("SwitchA");
				}
			}
		}
		public ObservableCollection<C_PDC> SwitchB
		{
			get
			{
				return SwitchBV;
			}
			set
			{
				if (SwitchBV != value)
				{
					SwitchBV = value;
					OnPropertyChanged("SwitchB");
				}
			}
		}
		public ObservableCollection<C_PDC> SwitchC
		{
			get
			{
				return SwitchCV;
			}
			set
			{
				if (SwitchCV != value)
				{
					SwitchCV = value;
					OnPropertyChanged("SwitchC");
				}
			}
		}
		public ObservableCollection<C_PDC> SwitchD
		{
			get
			{
				return SwitchDV;
			}
			set
			{
				if (SwitchDV != value)
				{
					SwitchDV = value;
					OnPropertyChanged("SwitchD");
				}
			}
		}
		/// <summary>Sets the GUI using Settings class instance. Sets only what it can.</summary>
		/// <param name="s">Settings to use.</param>
		public void	SetUsingSettings(Settings s)
		{
			if(s == null)
			{
				return;
			}
			NameStr = s.Name;
			ConstShiftStr = s.ConstShift.ToString();
			VarShiftStr = s.VarShift.ToString();
			RandCharSpcMinStr = s.RandCharSpcMin.ToString();
			RandCharSpcMaxStr = s.RandCharSpcMax.ToString();
			RandCharGenAStr = s.RandCharConstA.ToString();
			RandCharGenBStr = s.RandCharConstB.ToString();
			RandCharGenMStr = s.RandCharConstM.ToString();
			SwitchAStr = s.SwitchConstA.ToString();
			SwitchBStr = s.SwitchConstB.ToString();
			SwitchCStr = s.SwitchConstC.ToString();
			SwitchDStr = s.SwitchConstD.ToString();
			RandCharA = FillDataGridClass(s.RandCharConstA);
			RandCharB = FillDataGridClass(s.RandCharConstB);
			RandCharM = FillDataGridClass(s.RandCharConstM);
			SwitchA = FillDataGridClass(s.SwitchConstA);
			SwitchB = FillDataGridClass(s.SwitchConstB);
			SwitchC = FillDataGridClass(s.SwitchConstC);
			SwitchD = FillDataGridClass(s.SwitchConstD);
		}

		public static ObservableCollection<C_PDC> FillDataGridClass(PrimeDefinedConstant data)
		{
			ObservableCollection<C_PDC> result = new();
			for (int i = 0; i < 8; i++)
			{
				C_PDC item = new()
				{
					IdxStr = data.PrimeIdxs[i].ToString(),
					ExpStr = data.Exponents[i].ToString(),
				};
				result.Add(item);
			}
			return result;
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}

	public class C_UC_Rotor : INotifyPropertyChanged
	{
		private string PozitionStrV;
		private List<string> RotorsV = new();
		private string SelectedRStrV;
		private int SelectedRIdxV;
		public string PozitionStr
		{
			get
			{
				return PozitionStrV;
			}
			set
			{
				if (PozitionStrV != value)
				{
					PozitionStrV = value;
					OnPropertyChanged("PozitionStr");
				}
			}
		}
		public ushort? PozitionNum
		{
			get
			{
				if (ushort.TryParse(PozitionStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public List<string> RotorsStrs
		{
			get
			{
				return RotorsV;
			}
			set
			{
				if (RotorsV != value)
				{
					RotorsV = value;
					OnPropertyChanged("RotorsStrs");
				}
			}
		}
		public int SelectedRIdx
		{
			get
			{
				return SelectedRIdxV;
			}
			set
			{
				if (SelectedRIdxV != value)
				{
					SelectedRIdxV = value;
					OnPropertyChanged(" SelectedRIdx");
				}
			}
		}
		public string SelectedRStr
		{
			get
			{
				return SelectedRStrV;
			}
			set
			{
				if (SelectedRStrV != value)
				{
					SelectedRStrV = value;
					OnPropertyChanged("SelectedRStr");
				}
			}
		}
		public ushort? SelectedRNum
		{
			get
			{
				if (ushort.TryParse(SelectedRStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}

	public class C_PDC : INotifyPropertyChanged
	{
		private string IdxStrV;
		private string ExpStrV;
		public string IdxStr
		{
			get
			{
				return IdxStrV;
			}
			set
			{
				if (IdxStrV != value)
				{
					IdxStrV = value;
					OnPropertyChanged("IdxStr");
				}
			}
		}
		public int? GetIdxNum()
		{
			if (int.TryParse(IdxStrV, out int num))
			{
				return num;
			}
			else
			{
				return null;
			}
		}
		public string ExpStr
		{
			get
			{
				return ExpStrV;
			}
			set
			{
				if (ExpStrV != value)
				{
					ExpStrV = value;
					OnPropertyChanged("ExpStr");
				}
			}
		}
		public ushort? GetExpNum()
		{
			if (ushort.TryParse(ExpStrV, out ushort num))
			{
				return num;
			}
			else
			{
				return null;
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}

	public class C_VPE_ComboBox : INotifyPropertyChanged
	{
		private List<string> ItemsStrsV = new();
		private string SelectedStrV;
		public List<string> ItemsStrs
		{
			get
			{
				return ItemsStrsV;
			}
			set
			{
				if (ItemsStrsV != value)
				{
					ItemsStrsV = value;
					OnPropertyChanged("ItemsStrs");
				}
			}
		}
		public string SelectedStr
		{
			get
			{
				return SelectedStrV;
			}
			set
			{
				if (SelectedStrV != value)
				{
					SelectedStrV = value;
					OnPropertyChanged("SelectedStr");
				}
			}
		}
		public ushort? SelectedNum
		{
			get
			{
				if (ushort.TryParse(SelectedStrV, out ushort num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}
	#endregion

	#region Neue DT
	#endregion

	#region DT calc
	#endregion

	#region Factorizator
	#endregion

	#region Maps downloader
	#endregion
}