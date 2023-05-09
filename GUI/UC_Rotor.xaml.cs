using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPE;

namespace GUI
{
	public partial class UC_Rotor : UserControl
	{
		private readonly Generators Generator = new(Codepage.Limit, DateTime.Now.Ticks);

		public ushort SelTable { get; set; }

		public C_UC_Rotor DataFromGUI;

		public UC_Rotor()
		{
			InitializeComponent();
		}

		public UC_Rotor(C_UC_Rotor Binding)
		{
			InitializeComponent();
			DataFromGUI = Binding;
			DataContext = DataFromGUI;
		}

		private void B_Rand_Shift_Click (object sender, RoutedEventArgs e)
		{
			Generator.UpdateSeed(DateTime.Now.Ticks);
			DataFromGUI.PozitionStr = Generator.GenerateNum().ToString();
		}
	}
}