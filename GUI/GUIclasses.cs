using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using VPE;

namespace GUI
{
	#region Main win
	public class C_VPE_MainWin : INotifyPropertyChanged
	{
		private string VPE_PlainStrV;
		private string VPE_EncrypStrV;
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
		private string ConstShiftStrV;
		private string VarShiftStrV;
		private string RandCharSpcMinStrV;
		private string RandCharSpcMaxStrV;
		private string RandCharGenAStrV;
		private string RandCharGenBStrV;
		private string RandCharGenMStrV;
		private string RotorGenCountStrV;
		private string SwapGenCountStrV;
		private string ReflGenCountStrV;
		private string SwitchAStrV;
		private string SwitchBStrV;
		private string SwitchCStrV;
		private string SwitchDStrV;
		private string NameStrV;
		
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
		public ushort? ConstShiftNum
		{
			get
			{
				if (ushort.TryParse(ConstShiftStrV, out ushort num))
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
		public ushort? VarShiftNum
		{
			get
			{
				if (ushort.TryParse(VarShiftStrV, out ushort num))
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
		public decimal? RandCharGenANum
		{
			get
			{
				if (decimal.TryParse(RandCharGenAStrV, out decimal num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
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
		public decimal? RandCharGenBNum
		{
			get
			{
				if (decimal.TryParse(RandCharGenBStrV, out decimal num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
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
		public decimal? RandCharGenMNum
		{
			get
			{
				if (decimal.TryParse(RandCharGenMStrV, out decimal num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
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
		public BigInteger? SwitchANum
		{
			get
			{
				if (BigInteger.TryParse(SwitchAStrV, out BigInteger num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
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
		public BigInteger? SwitchBNum
		{
			get
			{
				if (BigInteger.TryParse(SwitchBStrV, out BigInteger num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
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
		public BigInteger? SwitchCNum
		{
			get
			{
				if (BigInteger.TryParse(SwitchCStrV, out BigInteger num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
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
		public BigInteger? SwitchDNum
		{
			get
			{
				if (BigInteger.TryParse(SwitchDStrV, out BigInteger num))
				{
					return num;
				}
				else
				{
					return null; // Neplatné číslo.
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
		/// <summary>Sets the GUI using Settings class instance. Sets only what it can.</summary>
		/// <param name="s">Settings to use.</param>
		public void	SetUsingSettings(Settings s)
		{
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

	#region Mapy.cz downloader
	#endregion
}