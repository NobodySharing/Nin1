using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace VPE
{
	public class Crypto
	{
		private readonly Settings Sett;
		private const int MultithreadingThreshold = 16384;
		/// <summary>Sada èísel reprezentující zprávu. Promìnná, se kterou v celém procesu pracuji.</summary>
		private List<ushort> Message;
		public Crypto(Settings S)
		{
			Sett = S;
		}
		/// <summary>Zašifruje text.</summary>
		/// <param name="Text">Text na zašifrování.</param>
		/// <returns>Zašifrovaný text.</returns>
		public string Encypt(string Text)
		{
			ConvertToNums(Text); // 
			AddRandomChars(); // 
			SwitchCharPoz(0); // 
			OrderShift(); // 
			MultithreadedPartEnc(); //
			ConstantShift(); // 
			VariableShift(); // 
			AddRandomChars(); // 
			SwitchCharPoz(1); //
			return ConvertToString(); // 
		}
		/// <summary>Dešifruje text.</summary>
		/// <param name="Text">Zašifrovaný text.</param>
		/// <returns>Dešifrovaný text.</returns>
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
		/// <summary></summary>
		private void MultithreadedPartEnc()
		{
			if (Message.Count < MultithreadingThreshold)
			{
				Swap();
				Scramble();
				Swap();
			}
			else
			{
				List<ushort>[] parts = PrepareData(out Settings_Rotors[] rotors);
				ParallelLoopResult result = Parallel.For(0, Environment.ProcessorCount, (i, state) =>
				{
					ForwardPassThroughTables(ref parts[i], rotors[i]);
				});
				if (result.IsCompleted)
				{
					CombineMessage(parts);
				}
			}
		}
		/// <summary></summary>
		private void MultithreadedPartDec()
		{
			if (Message.Count < MultithreadingThreshold)
			{
				Unswap();
				Unscramble();
				Unswap();
			}
			else
			{
				List<ushort>[] parts = PrepareData(out Settings_Rotors[] rotors);
				ParallelLoopResult result = Parallel.For(0, Environment.ProcessorCount, (i, state) =>
				{
					BackwardPassThroughTables(ref parts[i], rotors[i]);
				});
				if (result.IsCompleted)
				{
					CombineMessage(parts);
				}
			}
		}
		/// <summary>Divides the message to segments. Number of those is dependant on logical processor count.
		/// Last segment should be bit longer, of length of the message is not dividible by processor count.</summary>
		/// <returns>Divided message.</returns>
		private List<ushort>[] DivideMessage()
		{ // All divisions on ints are intentionally done this way. It'll return me only the whole part, which is exactly what I need.
			List<ushort>[] parts = new List<ushort>[Environment.ProcessorCount];
			int segLen = Message.Count / Environment.ProcessorCount, i, seg;
			for (i = 0; i < Message.Count; i++)
			{
				seg = i / segLen;
				if (seg < Environment.ProcessorCount)
				{
					parts[seg].Add(Message[i]);
				}
				else
				{
					parts[Environment.ProcessorCount - 1].Add(Message[i]);
				}
			}
			return parts;
		}
		/// <summary></summary>
		/// <param name="rotors"></param>
		/// <returns></returns>
		private List<ushort>[] PrepareData(out Settings_Rotors[] rotors)
		{
			List<ushort>[] parts = DivideMessage();
			rotors = Sett.CopyPrimitives(Environment.ProcessorCount);
			uint sum = 0;
			for (int i = 1; i < rotors.Length; i++)
			{
				sum += (uint)parts[i - 1].Count;
				rotors[i].IncrementPozitions(sum);
			}
			return parts;
		}
		/// <summary></summary>
		/// <param name="parts"></param>
		private void CombineMessage(List<ushort>[] parts)
		{
			Message.Clear();
			foreach (List<ushort> part in parts)
			{
				Message.AddRange(part);
			}
		}
		/// <summary>Zkonvertuje textovou zprávu na èíselnou reprezentaci podle tabulky znakù.</summary>
		/// <param name="Text">Textová zpráva.</param>
		private void ConvertToNums(string Text)
		{
			Message = new(Text.Length);
			for (int i = 0; i < Text.Length; i++)
			{
				char C = Text[i]; // Souèasný znak bokem.
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
				{ // Pokud znak neznám, pøeskakuji ho.
					continue;
				}
			}
		}
		/// <summary>Pøevede sadu èísel na text.</summary>
		/// <returns>Text.</returns>
		private string ConvertToString()
		{
			StringBuilder SB = new();
			foreach (ushort Num in Message)
			{
				SB.Append(Codepage.CharSet[Num]);
			}
			return SB.ToString();
		}
		/// <summary></summary>
		/// <param name="message"></param>
		/// <param name="rotors"></param>
		private void ForwardPassThroughTables(ref List<ushort> message, Settings_Rotors rotors)
		{
			Swap(ref message);
			Scramble(ref message, rotors);
			Swap(ref message);
		}
		/// <summary></summary>
		/// <param name="message"></param>
		/// <param name="rotors"></param>
		private void BackwardPassThroughTables(ref List<ushort> message, Settings_Rotors rotors)
		{
			Unswap(ref message);
			Unscramble(ref message, rotors);
			Unswap(ref message);
		}
		/// <summary></summary>
		private void Swap()
		{
			ushort swapped;
			for (int i = 0; i < Message.Count; i++)
			{
				foreach (Table T in Sett.Swaps)
				{
					swapped = T.FindValueUsingIndex(Message[i]);
					if (swapped < Table.InvalidArea)
					{
						Message[i] = swapped >= Table.InvalidArea ? Message[i] : swapped;
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary></summary>
		private void Swap(ref List<ushort> message)
		{
			ushort swapped;
			for (int i = 0; i < message.Count; i++)
			{
				foreach (Table T in Sett.Swaps)
				{
					swapped = T.FindValueUsingIndex(message[i]);
					if (swapped < Table.InvalidArea)
					{
						message[i] = swapped >= Table.InvalidArea ? message[i] : swapped;
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary></summary>
		private void Unswap()
		{
			ushort swapped;
			for (int j = Sett.Swaps.Count - 1; j >= 0; j--)
			{
				for (int i = Message.Count - 1; i >= 0; i--)
				{
					swapped = Sett.Swaps[j].FindIndexUsingValue(Message[i]);
					if (swapped != Table.NotFound)
					{
						Message[i] = swapped >= Table.InvalidArea ? Message[i] : swapped;
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary></summary>
		private void Unswap(ref List<ushort> message)
		{
			ushort swapped;
			for (int j = Sett.Swaps.Count - 1; j >= 0; j--)
			{
				for (int i = message.Count - 1; i >= 0; i--)
				{
					swapped = Sett.Swaps[j].FindIndexUsingValue(Message[i]);
					if (swapped != Table.NotFound)
					{
						message[i] = swapped >= Table.InvalidArea ? message[i] : swapped;
					}
					else
					{
						continue;
					}
				}
			}
		}
		/// <summary></summary>
		private void Scramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				ForwardScramble(i);
				Reflect(i);
				BackwardScramble(i);
				int Sum = Message[i] + Sett.Rotors[^1].Pozition;
				Message[i] = (ushort)(Sum % Codepage.Limit);
				Sett.IncrementPozitions();
			}
		}
		/// <summary></summary>
		private void Scramble(ref List<ushort> message, Settings_Rotors sett)
		{
			for (int i = 0; i < message.Count; i++)
			{
				ForwardScramble(i);
				Reflect(i);
				BackwardScramble(i);
				int Sum = message[i] + sett.Rotors[^1].Pozition;
				message[i] = (ushort)(Sum % Codepage.Limit);
				sett.IncrementPozitions();
			}
		}
		/// <summary></summary>
		private void Unscramble()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				BackwardScramble(i);
				ReflectBack(i);
				ForwardScramble(i);
				int Sum = Message[i] + Sett.Rotors[^1].Pozition;
				Message[i] = (ushort)(Sum % Codepage.Limit);
				Sett.IncrementPozitions();
			}
		}
		/// <summary></summary>
		private void Unscramble(ref List<ushort> message, Settings_Rotors sett)
		{
			for (int i = 0; i < message.Count; i++)
			{
				BackwardScramble(i);
				ReflectBack(i);
				ForwardScramble(i);
				int Sum = message[i] + sett.Rotors[sett.Rotors.Count - 1].Pozition;
				message[i] = (ushort)(Sum % Codepage.Limit);
				sett.IncrementPozitions();
			}
		}
		/// <summary>Posune èísla podle posunového parametru.</summary>
		private void ConstantShift()
		{
			for (int i = 0; Message.Count > 0; i++)
			{
				uint temp = Message[i];
				temp += Sett.ConstShift;
				Message[i] = Convert.ToUInt16(temp % Codepage.Limit);
			}
		}
		/// <summary>Posune èísla zpìt podle posunového parametru.</summary>
		private void UnConstantShift()
		{
			for (int i = 0; Message.Count > 0; i++)
			{
				uint temp = Message[i];
				temp -= Sett.ConstShift;
				Message[i] = Convert.ToUInt16(temp % Codepage.Limit);
			}
		}
		/// <summary>Posune èísla podle poøadí.</summary>
		private void OrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] + i) % Codepage.Limit);
			}
		}
		/// <summary>Posune èísla zpìt podle poøadí.</summary>
		private void UnOrderShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)((Message[i] - i) % Codepage.Limit);
			}
		}
		/// <summary>Provede promìnný posun èísel.</summary>
		private void VariableShift ()
		{
			uint v, c = Sett.ConstShift > (Codepage.Limit / 2) ? Sett.ConstShift : (uint)(Codepage.Limit - Sett.ConstShift);
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] + (uint)(Sett.VarShift * (i % c));
				Message[i] = (ushort)(v % Codepage.Limit);
			}
		}
		/// <summary>Provede promìnný posun zpìt èísel.</summary>
		private void UnVariableShift()
		{
			uint v, c = Sett.ConstShift > (Codepage.Limit / 2) ? Sett.ConstShift : (uint)(Codepage.Limit - Sett.ConstShift);
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] - (uint)(Sett.VarShift * (i % c));
				Message[i] = (ushort)(v % Codepage.Limit);
			}
		}
		/// <summary>Dopøedný prùchod 1 znaku všemi tabulkami.</summary>
		/// <param name="i">Index znaku.</param>
		private void ForwardScramble(int i)
		{
			for (int j = 0; j < Sett.Rotors.Count; j++)
			{
				int Sum = j == 0 ? Message[i] + Sett.Rotors[j].Pozition : Message[i] + (Sett.Rotors[j].Pozition - Sett.Rotors[j - 1].Pozition); // Kalkulace pozice po vstupu do tabulky.
				ushort Remain = (ushort)(Sum % Codepage.Limit); // Zùstání v rozsahu.
				Message[i] = Sett.Rotors[j].FindValueUsingIndex(Remain); // Prùchod tabulkou.
			}
		}
		/// <summary>Odrazí znak dle tabulky.</summary>
		/// <param name="i">Index znaku.</param>
		/// <returns>Odražený znak.</returns>
		private void Reflect(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindValueUsingIndex(Remain);
		}
		/// <summary>Zpìtnì odrazí znak dle tabulky.</summary>
		/// <param name="i">Index znaku.</param>
		/// <returns>Odražený znak.</returns>
		private void ReflectBack(int i)
		{
			int Sum = Message[i] + Sett.Rotors.Last().Pozition;
			ushort Remain = (ushort)(Sum % Codepage.Limit);
			Message[i] = Sett.Reflector.FindIndexUsingValue(Remain);
		}
		/// <summary>Zpìtný prùchod 1 znaku všemi tabulkami.</summary>
		/// <param name="i">Index znaku.</param>
		private void BackwardScramble(int i)
		{
			for (int j = Sett.Rotors.Count - 1; j >= 0; j--)
			{
				int Sum = j == (Sett.Rotors.Count - 1) ? Message[i] + Sett.Rotors[j].Pozition : Message[i] + (Sett.Rotors[j].Pozition - Sett.Rotors[j + 1].Pozition);
				ushort Remain = (ushort)(Sum % Codepage.Limit);
				Message[i] = Sett.Rotors[j].FindIndexUsingValue(Remain);
			}
		}
		/// <summary>Adds random chars to the message.</summary>
		private void AddRandomChars()
		{
			Generators Gen = new (Codepage.Limit, DateTime.Now.Ticks);
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2; // Index inicialization, where I will add random char.
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin); // Inicializace mezery. V jádru „náhodný“ výpoèet, pak dání do rozsahu a posunutí o minimum.
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant(); // I have to precompute the constants here, so I don't compute them every time.
			while (index < Message.Count)
			{
				Message.Insert(index, Gen.GenerateNum());
				IncrementSpace(ref index, ref space, gap, A, B, M);
			}
		}
		/// <summary>Removes random chars from the message.</summary>
		private void RemoveRandomChars()
		{
			int index = (Sett.RandCharSpcMin + Sett.RandCharSpcMax) % 2;
			Message.RemoveAt(index);
			int gap = Sett.RandCharSpcMax - Sett.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Sett.RandCharSpcMin);
			BigInteger A = Sett.RandCharConstA.ComputeConstant(1), B = Sett.RandCharConstB.ComputeConstant(), M = Sett.RandCharConstM.ComputeConstant();
			while (index <= Message.Count)
			{
				IncrementSpace(ref index, ref space, gap, A, B, M);
				Message.RemoveAt(index);
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
		/// <summary></summary>
		/// <param name="stage"></param>
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
		/// <summary></summary>
		/// <param name="stage"></param>
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
		/// <summary></summary>
		/// <param name="stage"></param>
		/// <returns></returns>
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
	}
}