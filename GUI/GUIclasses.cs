using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPE;

namespace GUI
{
	public class C_UC_Table : INotifyPropertyChanged
	{
		private string PozitionV;
		private List<uint> RotorsV = new();
		public uint SelectedR { get; set; }
		public string Pozition
		{
			get
			{
				return PozitionV;
			}
			set
			{
				if (PozitionV != value)
				{
					PozitionV = value;
					OnPropertyChanged("Pozition");
				}
			}
		}

		public List<uint> Rotors
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
					OnPropertyChanged("Rotors");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}

	public class C_VPE_Sett : INotifyPropertyChanged
	{
		private List<ushort> SwapsV = new();
		private List<ushort> ReflsV = new();
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
		public List<ushort> Swaps
		{
			get
			{
				return SwapsV;
			}
			set
			{
				if (SwapsV != value)
				{
					SwapsV = value;
					OnPropertyChanged("Swaps");
				}
			}
		}
		public List<ushort> Refls
		{
			get
			{
				return ReflsV;
			}
			set
			{
				if (ReflsV != value)
				{
					ReflsV = value;
					OnPropertyChanged("Refls");
				}
			}
		}

		public void	SetUsingSettings(Settings s)
		{
			ConstShiftStr = s.ConstShift.ToString();
			VarShiftStr = s.VarShift.ToString();
			RandCharSpcMinStr = s.RandCharSpcMin.ToString();
			RandCharSpcMaxStr = s.RandCharSpcMax.ToString();
			RandCharGenAStr = s.RandCharConstA.ToString();
			RandCharGenBStr = s.RandCharConstB.ToString();
			RandCharGenMStr = s.RandCharConstM.ToString();
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}
}