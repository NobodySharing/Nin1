using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		private string RandCharSpcMinV;
		private string RandCharSpcMaxV;
		private string RotorGenCountStrV;
		private ushort? RotorGenCountNumV;
		private string SwapGenCountStrV;
		private string ReflGenCountStrV;

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
				if (ushort.TryParse(ConstShiftStrV, out ushort shiftNum))
				{
					return shiftNum;
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
				if (ushort.TryParse(VarShiftStrV, out ushort shiftNum))
				{
					return shiftNum;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string RandCharSpcMin
		{
			get
			{
				return RandCharSpcMinV;
			}
			set
			{
				if (RandCharSpcMinV != value)
				{
					RandCharSpcMinV = value;
					OnPropertyChanged("RandCharSpcMin");
				}
			}
		}
		public ushort? RandCharSpcMinNum
		{
			get
			{
				if (ushort.TryParse(RandCharSpcMinV, out ushort shiftNum))
				{
					return shiftNum;
				}
				else
				{
					return null; // Neplatné číslo.
				}
			}
		}
		public string RandCharSpcMax
		{
			get
			{
				return RandCharSpcMaxV;
			}
			set
			{
				if (RandCharSpcMaxV != value)
				{
					RandCharSpcMaxV = value;
					OnPropertyChanged("RandCharSpcMax");
				}
			}
		}
		public ushort? RandCharSpcMaxNum
		{
			get
			{
				if (ushort.TryParse(RandCharSpcMaxV, out ushort shiftNum))
				{
					return shiftNum;
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
					if (ushort.TryParse(RotorGenCountStrV, out ushort shiftNum))
					{
						RotorGenCountNumV = shiftNum;
					}
					else
					{
						RotorGenCountNumV = null; // Neplatné číslo.
					}
					OnPropertyChanged("RotorGenCountStr");
				}
			}
		}
		public ushort? RotorGenCountNum
		{
			get
			{
				return RotorGenCountNumV;
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
				if (ushort.TryParse(SwapGenCountStrV, out ushort shiftNum))
				{
					return shiftNum;
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
				if (ushort.TryParse(ReflGenCountStrV, out ushort shiftNum))
				{
					return shiftNum;
				}
				else
				{
					return null; // Neplatné číslo.
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
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}
}