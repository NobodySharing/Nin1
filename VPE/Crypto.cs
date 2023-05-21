using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace VPE
{
	/// <summary>The main class doing all the en/decrypting.</summary>
	public class Crypto
	{
		private readonly Settings Sett;
		private const int MultithreadingThreshold = 16384; // Threshold for message length. Longer messages will get processed multithreadly.
		/// <summary>Set of numbers representing the message. My working memory.</summary>
		private List<ushort> Message;
		/// <summary>Initializes a new instance.</summary>
		/// <param name="S">Settings for en/decryption.</param>
		public Crypto(Settings S)
		{
			Sett = S;
		}
		/// <summary>Encrypts text.</summary>
		/// <param name="Text">Text to encrypt.</param>
		/// <returns>Encrypted text.</returns>
		public string Encypt(string Text)
		{
			ConvertToNums(Text);
			AddRandomChars();
			SwitchCharPoz(0);
			OrderShift();
			MultithreadedPartEnc();
			ConstantShift();
			VariableShift();
			AddRandomChars();
			SwitchCharPoz(1);
			return ConvertToString();
		}
		/// <summary>Decrypts text.</summary>
		/// <param name="Text">Text to decrypt.</param>
		/// <returns>Decrypted text.</returns>
		public string Decypt(string Text)
		{
			ConvertToNums(Text);
			UnSwitchCharPoz(1);
			RemoveRandomChars();
			UnVariableShift();
			UnConstantShift();
			MultithreadedPartDec();
			UnOrderShift();
			UnSwitchCharPoz(0);
			RemoveRandomChars();
			return ConvertToString();
		}
		/// <summary>The main block of encrypting, which can be executed in multiple threads. Decides if that is needed.</summary>
		private void MultithreadedPartEnc()
		{
			if (Message.Count < MultithreadingThreshold)
			{
				Swap();
				Scramble();
				Unswap();
			}
			else
			{
				ushort[][] parts = PrepareData(out Settings_Rotors[] rotors);
				ParallelLoopResult result = Parallel.For(0, Environment.ProcessorCount, (i, state) =>
				{
					ForwardPassThroughTables(ref parts[i], rotors[i]);
				});
				if (result.IsCompleted)
				{
					CombineMessage(parts);
				}
				_ = Sett.SetPozitions(rotors.Last().GetPozitions());
			}
		}
		/// <summary>The main block of decrypting, which can be executed in multiple threads. Decides if that is needed.</summary>
		private void MultithreadedPartDec()
		{
			if (Message.Count < MultithreadingThreshold)
			{
				Swap();
				Unscramble();
				Unswap();
			}
			else
			{
				ushort[][] parts = PrepareData(out Settings_Rotors[] rotors);
				ParallelLoopResult result = Parallel.For(0, Environment.ProcessorCount, (i, state) =>
				{
					BackwardPassThroughTables(ref parts[i], rotors[i]);
				});
				if (result.IsCompleted)
				{
					CombineMessage(parts);
				}
				_ = Sett.SetPozitions(rotors.Last().GetPozitions());
			}
		}
		/// <summary>Divides the message to segments. Number of those is dependant on logical processor count.
		/// Last segment should be bit longer, of length of the message is not dividible by processor count.</summary>
		/// <returns>Divided message.</returns>
		private ushort[][] DivideMessage()
		{
			ushort[][] parts = new ushort[Environment.ProcessorCount][];
			int segLen = Message.Count / Environment.ProcessorCount;
			for (int j = 0; j < (Environment.ProcessorCount - 1); j++)
			{
				parts[j] = new ushort[segLen];
				Message.CopyTo(j * segLen, parts[j], 0, segLen);
			}
			int lastSize = Message.Count - ((Environment.ProcessorCount - 1) * segLen);
			ushort[] last = new ushort[lastSize];
			Message.CopyTo((Environment.ProcessorCount - 1) * segLen, last, 0, 30);
			parts[^1] = last;
			return parts;
		}
		/// <summary>Does all the preparations for multithreaded en/decryption. Divides the message and prepares settings.</summary>
		/// <param name="rotors">Prepared settings.</param>
		/// <returns>Divided message (by number of threads).</returns>
		private ushort[][] PrepareData(out Settings_Rotors[] rotors)
		{
			ushort[][] parts = DivideMessage();
			rotors = Sett.CloneRotors(Environment.ProcessorCount);
			for (int i = 1; i < Environment.ProcessorCount; i++)
			{
				rotors[i].IncrementPozitions((uint)(parts[0].Length * i));
			}
			return parts;
		}
		/// <summary>Combines the message from it's parts to whole message.</summary>
		/// <param name="parts">Parts of the message.</param>
		private void CombineMessage(ushort[][] parts)
		{
			Message.Clear();
			foreach (ushort[] part in parts)
			{
				Message.AddRange(part);
			}
		}
		/// <summary>Zkonvertuje textovou zprávu na číselnou reprezentaci podle tabulky znaků.</summary>
		/// <param name="Text">Textová zpráva.</param>
		private void ConvertToNums(string Text)
		{
			Message = new(Text.Length);
			for (int i = 0; i < Text.Length; i++)
			{
				char C = Text[i]; // Současný znak bokem.
				char N = i == (Text.Length - 1)? '\0' : Text[i + 1]; // Následující znak. Pokud jsem na konci, dávám \0, jako znamení toho stavu.
				if (C == '\r')
				{
					if ((N == '\n') || (N == '\0'))
					{
						Message.Add((ushort)Codepage.CharSet.IndexOf("\r\n"));
						i++;
						continue;
					}
					else
					{
						Message.Add((ushort)Codepage.CharSet.IndexOf("\r\n"));
						continue;
					}
				}
				else if (C == '\n')
				{
					Message.Add((ushort)Codepage.CharSet.IndexOf("\r\n"));
					continue;
				}
				int index = Codepage.CharSet.IndexOf(Convert.ToString(C));
				if (index >= 0)
				{
					Message.Add((ushort)index);
				}
				else
				{ // Pokud znak neznám, přeskakuji ho.
					continue;
				}
			}
		}
		/// <summary>Převede sadu čísel na text.</summary>
		/// <returns>Text.</returns>
		private string ConvertToString()
		{
			StringBuilder SB = new();
			foreach (ushort Num in Message)
			{
				if (Num < Codepage.Limit)
				{
					SB.Append(Codepage.CharSet[Num]);
				}
				else
				{
					SB.Append('✳');
				}
			}
			return SB.ToString();
		}
		/// <summary>Multithreaded version of passing chars through all tables of all kinds.</summary>
		/// <param name="message">Part of the message.</param>
		/// <param name="rotors">Rotor settings for this thread.</param>
		private void ForwardPassThroughTables(ref ushort[] message, Settings_Rotors rotors)
		{
			Swap(ref message);
			Scramble(ref message, rotors);
			Unswap(ref message);
		}
		/// <summary>Multithreaded version of passing chars back through all tables of all kinds.</summary>
		/// <param name="message">Part of the message.</param>
		/// <param name="rotors">Rotor settings for this thread.</param>
		private void BackwardPassThroughTables(ref ushort[] message, Settings_Rotors rotors)
		{
			Unswap(ref message);
			Unscramble(ref message, rotors);
			Swap(ref message);
		}
		/// <summary>Singlethreaded version of passing chars through all swap tables. Always all chars through 1 swap.</summary>
		private void Swap()
		{
			ushort swapped;
			for (int i = 0; i < Sett.Swaps.Count; i++)
			{
				for (int j = 0; j < Message.Count; j++)
				{
					swapped = Sett.Swaps[i].FindValueUsingIndex(Message[j]);
					if (swapped == Table.Outside || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[j] = swapped;
					}
				}
			}
		}
		/// <summary>Multithreaded version of passing chars through all swap tables. Always all chars through 1 swap.</summary>
		/// <param name="message">Part of the message.</param>
		private void Swap(ref ushort[] message)
		{
			ushort swapped;
			for (int i = 0; i < Sett.Swaps.Count; i++)
			{
				for (int j = 0; j < message.Length; j++)
				{
					swapped = Sett.Swaps[i].FindValueUsingIndex(message[j]);
					if (swapped == Table.Outside || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						message[j] = swapped;
					}
				}
			}
		}
		/// <summary>Singlethreaded version of passing chars back through all swap tables. Always all chars through 1 swap.</summary>
		private void Unswap()
		{
			ushort swapped;
			for (int i = Sett.Swaps.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < Message.Count; j++)
				{
					swapped = Sett.Swaps[i].FindIndexUsingValue(Message[j]);
					if (swapped == Table.NotFound || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						Message[j] = swapped;
					}
				}
			}
		}
		/// <summary>Multithreaded version of passing chars back through all swap tables. Always all chars through 1 swap.</summary>
		/// <param name="message">Part of the message.</param>
		private void Unswap(ref ushort[] message)
		{
			ushort swapped;
			for (int i = Sett.Swaps.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < message.Length; j++)
				{
					swapped = Sett.Swaps[i].FindIndexUsingValue(message[j]);
					if (swapped == Table.NotFound || swapped == Table.Empty || swapped == Table.Blank)
					{
						continue;
					}
					else
					{
						message[j] = swapped;
					}
				}
			}
		}
		/// <summary>Singlethreaded version of the main scrambling part. Scrambles chars 1 by 1, every time pass through all rotors, reflect, then pass back.</summary>
		private void Scramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i);
				Reflect(i);
				BackwardScramble(i);
				Sett.IncrementPozitions();
			}
		}
		/// <summary>Multithreaded version of the main scrambling part. Scrambles chars 1 by 1, every time pass through all rotors, reflect, then pass back.</summary>
		/// <param name="message">Part of the message.</param>
		/// <param name="sett">Rotor settings for this thread.</param>
		private void Scramble(ref ushort[] message, Settings_Rotors sett)
		{
			for (int i = 0; i < message.Length; i++)
			{
				ForwardScramble(i, ref message, sett);
				Reflect(i, ref message);
				BackwardScramble(i, ref message, sett);
				sett.IncrementPozitions();
			}
		}
		/// <summary>Singlethreaded version of the main unscrambling part. Unscrambles chars 1 by 1, every time pass through all rotors, reflect, then pass back.</summary>
		private void Unscramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i);
				Reflect(i);
				BackwardScramble(i);
				Sett.IncrementPozitions();
			}
		}
		/// <summary>Multithreaded version of the main unscrambling part. Unscrambles chars 1 by 1, every time pass through all rotors, reflect, then pass back.</summary>
		/// <param name="message">Part of the message.</param>
		/// <param name="sett">Rotor settings for this thread.</param>
		private void Unscramble(ref ushort[] message, Settings_Rotors sett)
		{
			for (int i = 0; i < message.Length; i++)
			{
				BackwardScramble(i, ref message, sett);
				Reflect(i, ref message);
				ForwardScramble(i, ref message, sett);
				sett.IncrementPozitions();
			}
		}
		/// <summary>Shifts all chars by constant amount.</summary>
		private void ConstantShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				long temp = Message[i];
				temp += Sett.ConstShift;
				Message[i] = Convert.ToUInt16(temp % Codepage.Limit);
			}
		}
		/// <summary>Shifts all chars back by constant amount.</summary>
		private void UnConstantShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				long temp = Message[i];
				temp -= Sett.ConstShift;
				temp %= Codepage.Limit;
				Message[i] = temp < 0 ? (ushort)(temp + Codepage.Limit) : (ushort)temp;
			}
		}
		/// <summary>Shifts all chars by their index.</summary>
		private void OrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] + i) % Codepage.Limit);
			}
		}
		/// <summary>Shifts all chars back by their index.</summary>
		private void UnOrderShift()
		{
			int temp;
			for (int i = 0; i < Message.Count; i++)
			{
				temp = (Message[i] - i) % Codepage.Limit;
				Message[i] = temp < 0 ? (ushort)(temp + Codepage.Limit) : (ushort)temp;
			}
		}
		/// <summary>Shifts all chars by variable amount.</summary>
		private void VariableShift ()
		{
			long v, c = Sett.ConstShift >= (Codepage.Limit / 2) ? Sett.ConstShift : Codepage.Limit - Sett.ConstShift;
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] + (Sett.VarShift * (i % c));
				v %= Codepage.Limit;
				Message[i] = v < 0 ? (ushort)(v + Codepage.Limit) : (ushort)v;
			}
		}
		/// <summary>Shifts all chars back by variable amount.</summary>
		private void UnVariableShift()
		{
			long v, c = Sett.ConstShift >= (Codepage.Limit / 2) ? Sett.ConstShift : Codepage.Limit - Sett.ConstShift;
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] - (Sett.VarShift * (i % c));
				v %= Codepage.Limit;
				Message[i] = v < 0 ? (ushort)(v + Codepage.Limit) : (ushort)v;
			}
		}
		/// <summary>Forward pass through all rotors, singlethreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		private void ForwardScramble(int i)
		{
			ushort temp = ModuloSum(Message[i], Sett.Rotors[0].Pozition); // Calculation of the input pozition as the rotor sees it.
			temp = Sett.Rotors[0].FindValueUsingIndex(temp); // Pass through the table.
			for (int j = 1; j < Sett.Rotors.Count; j++)
			{
				temp = ModuloSum(temp, Sett.Rotors[j - 1].Pozition); // Calculation of the output pozition, in absolute terms.
				temp = ModuloSum(temp, Sett.Rotors[j].Pozition); // Calculation of the input pozition as the (next) rotor sees it.
				temp = Sett.Rotors[j].FindValueUsingIndex(temp); // Pass through the table.
			}
			Message[i] = ModuloSum(temp, Sett.Rotors.Last().Pozition); // Calculation of the exiting pozition, in absolute terms.
		}
		/// <summary>Forward pass through all rotors, multithreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		/// <param name="message">Part of the message.</param>
		/// <param name="sett">Settings for this thread.</param>
		private void ForwardScramble(int i, ref ushort[] message, Settings_Rotors sett)
		{
			ushort temp = ModuloSum(message[i], sett.Rotors[0].Pozition); // Calculation of the input pozition as the rotor sees it.
			temp = sett.Rotors[0].FindValueUsingIndex(temp); // Pass through the table.
			for (int j = 1; j < sett.Rotors.Count; j++)
			{
				temp = ModuloSum(temp, sett.Rotors[j - 1].Pozition); // Calculation of the output pozition, in absolute terms.
				temp = ModuloSum(temp, sett.Rotors[j].Pozition); // Calculation of the input pozition as the (next) rotor sees it.
				temp = sett.Rotors[j].FindValueUsingIndex(temp); // Pass through the table.
			}
			message[i] = ModuloSum(temp, sett.Rotors.Last().Pozition); // Calculation of the exiting pozition, in absolute terms.
		}
		/// <summary>Backward pass through all rotors, singlethreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		private void BackwardScramble(int i)
		{
			ushort temp = ModuloDiff(Message[i], Sett.Rotors.Last().Pozition);  // Calculation of the input pozition as the rotor sees it.
			temp = Sett.Rotors.Last().FindIndexUsingValue(temp); // Pass through the table.
			for (int j = Sett.Rotors.Count - 2; j > -1; j--)
			{
				temp = ModuloDiff(temp, Sett.Rotors[j + 1].Pozition); // Calculation of the output pozition, in absolute terms.
				temp = ModuloDiff(temp, Sett.Rotors[j].Pozition); // Calculation of the input pozition as the (next) rotor sees it.
				temp = Sett.Rotors[j].FindIndexUsingValue(temp); // Pass through the table.
			}
			Message[i] = ModuloDiff(temp, Sett.Rotors[0].Pozition); // Calculation of the exiting pozition, in absolute terms.
		}
		/// <summary>Backward pass through all rotors, multithreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		/// <param name="message">Part of the message.</param>
		/// <param name="sett">Settings for this thread.</param>
		private void BackwardScramble(int i, ref ushort[] message, Settings_Rotors sett)
		{
			ushort temp = ModuloDiff(message[i], sett.Rotors.Last().Pozition);  // Calculation of the input pozition as the rotor sees it.
			temp = sett.Rotors.Last().FindIndexUsingValue(temp); // Pass through the table.
			for (int j = sett.Rotors.Count - 2; j > -1; j--)
			{
				temp = ModuloDiff(temp, sett.Rotors[j + 1].Pozition); // Calculation of the output pozition, in absolute terms.
				temp = ModuloDiff(temp, sett.Rotors[j].Pozition); // Calculation of the input pozition as the (next) rotor sees it.
				temp = sett.Rotors[j].FindIndexUsingValue(temp); // Pass through the table.
			}
			message[i] = ModuloDiff(temp, sett.Rotors[0].Pozition); // Calculation of the exiting pozition, in absolute terms.
		}
		/// <summary>Reflects given value. By design of the reflector, it's the same method for both directions. Singlethreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		private void Reflect(int i)
		{
			Message[i] = Sett.Reflector.FindValueUsingIndex(Message[i]);
		}
		/// <summary>eflects given value. By design of the reflector, it's the same method for both directions. Multithreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		/// <param name="message">Part of the message.</param>
		private void Reflect(int i, ref ushort[] message)
		{
			message[i] = Sett.Reflector.FindValueUsingIndex(message[i]);
		}
		/// <summary>Adds random chars to the message.</summary>
		private void AddRandomChars()
		{
			BigInteger seed0 = 1;
			foreach (Table rotor in Sett.Rotors)
			{
				seed0 *= rotor.Pozition;
			}
			seed0 %= (DateTime.MaxValue.Ticks - DateTime.MinValue.Ticks); 
			long seed1 = DateTime.MinValue.Ticks + (long)seed0;
			Generators Gen = new (Codepage.Limit, seed1);
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2; // Index inicialization, where I will add random char.
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin); // Inicializace mezery. V jádru „náhodný“ výpočet, pak dání do rozsahu a posunutí o minimum.
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant(); // I have to precompute the constants here, so I don't compute them every time.
			while (index < Message.Count)
			{
				Message.Insert(index, Gen.GenerateNum());
				IncrementSpace(ref index, ref space, gap, A, B, M);
			}
			if (index == Message.Count)
			{
				Message.Add(Gen.GenerateNum());
			}
		}
		/// <summary>Removes random chars from the message.</summary>
		private void RemoveRandomChars()
		{
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2;
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin);
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant();
			List<int> IdxToRemove = new()
			{
				index
			};
			IncrementSpace(ref index, ref space, gap, A, B, M);
			while (index < Message.Count)
			{
				IdxToRemove.Add(index);
				IncrementSpace(ref index, ref space, gap, A, B, M);
			}
			for (int i = IdxToRemove.Count - 1; i > -1; i--)
			{
				Message.RemoveAt(IdxToRemove[i]);
			}
		}
		/// <summary>Increments index by new pseudorandom gap.</summary>
		/// <param name="index">Previous index.</param>
		/// <param name="space">Previous gap.</param>
		/// <param name="gap">Max gap size.</param>
		/// <param name="A">A in y = (Ax + B) % M.</param>
		/// <param name="B">B in y = (Ax + B) % M.</param>
		/// <param name="M">M in y = (Ax + B) % M.</param>
		private void IncrementSpace (ref int index, ref BigInteger space, int gap, BigInteger A, BigInteger B, BigInteger M)
		{
			space = (A * space + B) % M;
			index +=  (int)(space % gap) + Sett.RandCharSpcMin;
		}
		/// <summary>Switches the pozitions of all characters in the message. Pseudorandomly.</summary>
		/// <param name="stage">Which stage? So far only 0 or 1.</param>
		private void SwitchCharPoz(byte stage)
		{
			List<int> switchTable = ConstructSwitchTable(stage);
			List<ushort> original = new();
			original.AddRange(Message);
			for (int i = 0; i < switchTable.Count; i++)
			{
				Message[i] = original[switchTable[i]];
			}
		}
		/// <summary>Switches the pozitions of all characters in the message back.</summary>
		/// <param name="stage">Which stage? So far only 0 or 1.</param>
		private void UnSwitchCharPoz(byte stage)
		{
			List<int> switchTable = ConstructSwitchTable(stage);
			List<ushort> original = new();
			original.AddRange(Message);
			for (int i = 0; i < switchTable.Count; i++)
			{
				Message[switchTable[i]] = original[i];
			}
		}
		/// <summary>Constructs the char switching table.</summary>
		/// <param name="stage">Which stage? So far only 0 or 1.</param>
		/// <returns>Char switching table.</returns>
		private List<int> ConstructSwitchTable(byte stage)
		{
			List<int> switchTable = new();
			BigInteger A, B;
			if (stage == 0)
			{
				A = Sett.SwitchConstA.ComputeConstant();
				B = Sett.SwitchConstB.ComputeConstant();
			}
			else
			{
				A = Sett.SwitchConstC.ComputeConstant();
				B = Sett.SwitchConstD.ComputeConstant();
			}
			uint m = (uint)Message.Count;
			if (m % (A * B) == 0)
			{
				m--;
			}
			for (int i = 0; i < m; i++)
			{
				switchTable.Add((int)((A * i + B) % m));
			}
			return switchTable;
		}
		/// <summary>Computes the modulo sumation. Result will be in range 0 to Codepage.Limit - 1.</summary>
		/// <param name="A">A number.</param>
		/// <param name="B">B number.</param>
		/// <returns>Sumation modulated to 0 to Codepage.Limit - 1.</returns>
		private ushort ModuloSum (ushort A, ushort B)
		{
			return (ushort)((A + B) % Codepage.Limit);
		}
		/// <summary>Computes the modulo differentiation. Result will be in range 0 to Codepage.Limit - 1.</summary>
		/// <param name="A">A number. It matters which is it.</param>
		/// <param name="B">B number. It matters which is it.</param>
		/// <returns>Differentiation modulated to 0 to Codepage.Limit - 1.</returns>
		private ushort ModuloDiff(ushort A, ushort B)
		{
			return (ushort)((A - B + Codepage.Limit) % Codepage.Limit);
		}
	}
}