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
		/// <summary>This is the key.</summary>
		private readonly Key Key;
		/// <summary>Maximum length of a message per thread, after which messages will get processed multithreadingly. Used for calculating ParallelThreshold.</summary>
		private static readonly int SingleThreadThreshold = 1048576;
		/// <summary>Threshold for message length. Longer messages will get processed multithreadingly. Given as num of chars per thread.</summary>
		private static readonly int ParallelThreshold = SingleThreadThreshold * Environment.ProcessorCount;
		/// <summary>Set of numbers representing the message. My working memory.</summary>
		private List<ushort> Message;
		/// <summary>Initializes a new instance.</summary>
		/// <param name="K">Settings for en/decryption.</param>
		public Crypto(Key K) => Key = K;
		/// <summary>Encrypts text.</summary>
		/// <param name="text">Text to encrypt.</param>
		/// <param name="outputType">Specifies the output format.</param>
		/// <returns>Encrypted text (in desired format).</returns>
		public string Encrypt(string text, Codepage.Encoding outputType)
		{
			Key.DuplicateAndSetToActivePositions(Key.SelectedPositions);
			Message = Codepage.Decode(text, Codepage.Encoding.Text);
			ScrambleCharTable(0, true);
			Differentiate();
			AddRandomChars();
			SwitchCharPoz(0);
			OrderShift();
			ParallelPart();
			ConstantShift();
			VariableShift();
			AddRandomChars();
			Differentiate();
			SwitchCharPoz(1);
			//AffineModulation();
			ScrambleCharTable(1, true);
			Key.PrunePositions();
			return Codepage.Encode(Message, outputType);
		}
		/// <summary>Decrypts encrypted message.</summary>
		/// <param name="text">Text to decrypt.</param>
		/// <param name="inputType">Specifies the input format.</param>
		/// <returns>Decrypted text.</returns>
		public string Decrypt(string text, Codepage.Encoding inputType)
		{
			Key.DuplicateAndSetToActivePositions(Key.SelectedPositions);
			Message = Codepage.Decode(text, inputType);
			ScrambleCharTable(1, false);
			//AffineDemodulation();
			UnSwitchCharPoz(1);
			Undifferentiate();
			RemoveRandomChars();
			UnVariableShift();
			UnConstantShift();
			ParallelPart();
			UnOrderShift();
			UnSwitchCharPoz(0);
			RemoveRandomChars();
			Undifferentiate();
			ScrambleCharTable(0, false);
			Key.PrunePositions();
			return Codepage.Encode(Message, Codepage.Encoding.Text);
		}
		/// <summary>Gets the message as list of ushorts, for converting to a binary file.</summary>
		/// <returns>Message as list of ushort numbers.</returns>
		public List<ushort> GetRawMessage()
		{
			return Message;
		}
		/// <summary>Sets the message from supplied list of ushorts, for converting from a binary file.</summary>
		/// <param name="message">Message as list of ushort numbers.</param>
		public void SetRawMessage(List<ushort> message)
		{
			Message = message;
		}
		/// <summary>Does simple char switch, designed for randomization of the char table without changing it.</summary>
		/// <param name="stage">0 for begin, 1 for end.</param>
		/// <param name="encrypt">True for encrypting, flase for decrypting.</param>
		private void ScrambleCharTable(byte stage, bool encrypt)
		{
			Table table = stage switch
			{
				1 => Key.OutputScrambler,
				_ => Key.InputScrambler,
			};
			if (encrypt)
			{
				for (int i = 0; i < Message.Count; i++)
				{
					Message[i] = table.FindValueUsingIndex(Message[i]);
				}
			}
			else
			{
				for (int i = 0; i < Message.Count; i++)
				{
					Message[i] = table.FindIndexUsingValue(Message[i]);
				}
			}
		}
		/// <summary>Does the differentiation. Forward and then backward.</summary>
		private void Differentiate()
		{
			for (int i = 1; i < Message.Count; i++)
			{
				Message[i] = ModuloSub(Message[i - 1], Message[i]);
			}
			for (int i = Message.Count - 2; i >= 0; i--)
			{
				Message[i] = ModuloSub(Message[i + 1], Message[i]);
			}
		}
		/// <summary>Undoes the differentiation.</summary>
		private void Undifferentiate()
		{
			ushort[] temps = new ushort[Message.Count];
			temps[^1] = Message[^1];
			for (int i = Message.Count - 2; i >= 0; i--)
			{
				temps[i] = ModuloSub(Message[i + 1], Message[i]);
			}
			Message[0] = temps[0];
			for (int i = 1; i < Message.Count; i++)
			{
				Message[i] = ModuloSub(temps[i - 1], temps[i]);
			}
		}
		/// <summary>The main block of encrypting, which can be executed in multiple threads. Decides if that is needed.</summary>
		private void ParallelPart()
		{
			if (Message.Count < ParallelThreshold)
			{
				Swap();
				Scramble();
				Unswap();
			}
			else
			{
				ushort[][] parts = PrepareData(out ushort[][] threadedPozs);
				ParallelLoopResult result = Parallel.For(0, Environment.ProcessorCount, (i, state) =>
				{
					PassThroughTables(ref parts[i], threadedPozs[i]);
				});
				if (result.IsCompleted)
				{
					CombineMessage(parts);
					Key.AddPositions([.. threadedPozs[^1]]);
				}
			}
		}
		/// <summary>Divides the message to segments. Number of those is dependant on logical processor count.
		/// Last segment should be bit longer, of length of the message is not dividible by processor count.</summary>
		/// <returns>Divided message.</returns>
		private ushort[][] DivideMessage()
		{
			ushort[][] parts = new ushort[Environment.ProcessorCount][];
			int index = 0, segLen = Message.Count / Environment.ProcessorCount;
			int leftover = Message.Count - (segLen * Environment.ProcessorCount);
			for (int i = 0; i < Environment.ProcessorCount; i++)
			{
				if (leftover > 0)
				{
					parts[i] = new ushort[segLen + 1];
					Message.CopyTo(index, parts[i], 0, segLen + 1);
					index += segLen + 1;
					leftover--;
				}
				else
				{
					parts[i] = new ushort[segLen];
					Message.CopyTo(index, parts[i], 0, segLen);
					index += segLen;
				}
			}
			return parts;
		}
		/// <summary>Does all the preparations for multithreaded en/decryption. Divides the message and prepares settings.</summary>
		/// <param name="pozs">Prepared positions.</param>
		/// <returns>Divided message (by number of threads).</returns>
		private ushort[][] PrepareData(out ushort[][] pozs)
		{
			ushort[][] parts = DivideMessage();
			pozs = new ushort[Environment.ProcessorCount][];
			List<ushort> initialPozs = Key.GetPositions();
			uint cumulativeLength = 0;
			for (int i = 0; i < pozs.Length; i++)
			{
				pozs[i] = [.. initialPozs];
				Key.IncrementCustomPositions(ref pozs[i], cumulativeLength);
				cumulativeLength += (uint)pozs[i].Length;
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
		/// <summary>Parallel version of passing chars through all tables of all kinds.</summary>
		/// <param name="message">Part of the message.</param>
		/// <param name="threadPozs">Rotor positions for this thread.</param>
		private void PassThroughTables(ref ushort[] message, ushort[] threadPozs)
		{
			Swap(ref message);
			Scramble(ref message, threadPozs);
			Unswap(ref message);
		}
		/// <summary>Singlethreaded version of passing chars through all swap tables. Always all chars through 1 swap.</summary>
		private void Swap()
		{
			ushort swapped;
			for (int i = 0; i < Key.Swaps.Count; i++)
			{
				for (int j = 0; j < Message.Count; j++)
				{
					swapped = Key.Swaps[i].FindValueUsingIndex(Message[j]);
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
		/// <summary>Parallel version of passing chars through all swap tables. Always all chars through 1 swap.</summary>
		/// <param name="message">Part of the message.</param>
		private void Swap(ref ushort[] message)
		{
			ushort swapped;
			for (int i = 0; i < Key.Swaps.Count; i++)
			{
				for (int j = 0; j < message.Length; j++)
				{
					swapped = Key.Swaps[i].FindValueUsingIndex(message[j]);
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
			for (int i = Key.Swaps.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < Message.Count; j++)
				{
					swapped = Key.Swaps[i].FindIndexUsingValue(Message[j]);
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
		/// <summary>Parallel version of passing chars back through all swap tables. Always all chars through 1 swap.</summary>
		/// <param name="message">Part of the message.</param>
		private void Unswap(ref ushort[] message)
		{
			ushort swapped;
			for (int i = Key.Swaps.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < message.Length; j++)
				{
					swapped = Key.Swaps[i].FindIndexUsingValue(message[j]);
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
				Key.IncrementPositions();
			}
		}
		/// <summary>Parallel version of the main scrambling part. Scrambles chars 1 by 1, every time pass through all rotors, reflect, then pass back.</summary>
		/// <param name="message">Part of the message.</param>
		/// <param name="threadPozs">Rotor positions for this thread.</param>
		private void Scramble(ref ushort[] message, ushort[] threadPozs)
		{
			for (int i = 0; i < message.Length; i++)
			{
				ForwardScramble(i, ref message, threadPozs);
				Reflect(i, ref message);
				BackwardScramble(i, ref message, threadPozs);
				Key.IncrementCustomPositions(ref threadPozs, 1);
			}
		}
		/// <summary>Shifts all chars by constant amount.</summary>
		private void ConstantShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				long temp = Message[i];
				temp += Key.ConstShift;
				Message[i] = Convert.ToUInt16(temp % Codepage.Limit);
			}
		}
		/// <summary>Shifts all chars back by constant amount.</summary>
		private void UnConstantShift()
		{
			for (int i = 0; i < Message.Count; i++)
			{
				long temp = Message[i];
				temp -= Key.ConstShift;
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
			long v, c = Key.ConstShift >= (Codepage.Limit / 2) ? Key.ConstShift : Codepage.Limit - Key.ConstShift;
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] + (Key.VarShift * (i % c));
				v %= Codepage.Limit;
				Message[i] = v < 0 ? (ushort)(v + Codepage.Limit) : (ushort)v;
			}
		}
		/// <summary>Shifts all chars back by variable amount.</summary>
		private void UnVariableShift()
		{
			long v, c = Key.ConstShift >= (Codepage.Limit / 2) ? Key.ConstShift : Codepage.Limit - Key.ConstShift;
			for (int i = 0; i < Message.Count; i++)
			{
				v = Message[i] - (Key.VarShift * (i % c));
				v %= Codepage.Limit;
				Message[i] = v < 0 ? (ushort)(v + Codepage.Limit) : (ushort)v;
			}
		}
		/// <summary>Forward pass through all rotors, singlethreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		private void ForwardScramble(int i)
		{
			ushort temp = ModuloSum(Message[i], Key.Rotors[0].Positions[Key.SelectedPositions]); // Calculation of the input position as the rotor sees it.
			temp = Key.Rotors[0].FindValueUsingIndex(temp); // Pass through the table.
			for (int j = 1; j < Key.Rotors.Count; j++)
			{
				temp = ModuloSum(temp, Key.Rotors[j - 1].Positions[Key.SelectedPositions]); // Calculation of the output position, in absolute terms.
				temp = ModuloSum(temp, Key.Rotors[j].Positions[Key.SelectedPositions]); // Calculation of the input position as the (next) rotor sees it.
				temp = Key.Rotors[j].FindValueUsingIndex(temp); // Pass through the table.
			}
			Message[i] = ModuloSum(temp, Key.Rotors[^1].Positions[Key.SelectedPositions]); // Calculation of the exiting position, in absolute terms.
		}
		/// <summary>Forward pass through all rotors, multithreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		/// <param name="message">Part of the message.</param>
		/// <param name="threadPozs">Settings for this thread.</param>
		private void ForwardScramble(int i, ref ushort[] message, ushort[] threadPozs)
		{
			ushort temp = ModuloSum(message[i], threadPozs[0]); // Calculation of the input position as the rotor sees it.
			temp = Key.Rotors[0].FindValueUsingIndex(temp); // Pass through the table.
			for (int j = 1; j < threadPozs.Length; j++)
			{
				temp = ModuloSum(temp, threadPozs[j - 1]); // Calculation of the output position, in absolute terms.
				temp = ModuloSum(temp, threadPozs[j]); // Calculation of the input position as the (next) rotor sees it.
				temp = Key.Rotors[j].FindValueUsingIndex(temp); // Pass through the table.
			}
			message[i] = ModuloSum(temp, threadPozs[^1]); // Calculation of the exiting position, in absolute terms.
		}
		/// <summary>Backward pass through all rotors, singlethreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		private void BackwardScramble(int i)
		{
			ushort temp = ModuloSub(Message[i], Key.Rotors[^1].Positions[Key.SelectedPositions]);  // Calculation of the input position as the rotor sees it.
			temp = Key.Rotors.Last().FindIndexUsingValue(temp); // Pass through the table.
			for (int j = Key.Rotors.Count - 2; j > -1; j--)
			{
				temp = ModuloSub(temp, Key.Rotors[j + 1].Positions[Key.SelectedPositions]); // Calculation of the output position, in absolute terms.
				temp = ModuloSub(temp, Key.Rotors[j].Positions[Key.SelectedPositions]); // Calculation of the input position as the (next) rotor sees it.
				temp = Key.Rotors[j].FindIndexUsingValue(temp); // Pass through the table.
			}
			Message[i] = ModuloSub(temp, Key.Rotors[0].Positions[Key.SelectedPositions]); // Calculation of the exiting position, in absolute terms.
		}
		/// <summary>Backward pass through all rotors, multithreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		/// <param name="message">Part of the message.</param>
		/// <param name="threadPozs">Settings for this thread.</param>
		private void BackwardScramble(int i, ref ushort[] message, ushort[] threadPozs)
		{
			ushort temp = ModuloSub(message[i], threadPozs[^1]);  // Calculation of the input position as the rotor sees it.
			temp = Key.Rotors[^1].FindIndexUsingValue(temp); // Pass through the table.
			for (int j = threadPozs.Length - 2; j > -1; j--)
			{
				temp = ModuloSub(temp, threadPozs[j + 1]); // Calculation of the output position, in absolute terms.
				temp = ModuloSub(temp, threadPozs[j]); // Calculation of the input position as the (next) rotor sees it.
				temp = Key.Rotors[j].FindIndexUsingValue(temp); // Pass through the table.
			}
			message[i] = ModuloSub(temp, threadPozs[0]); // Calculation of the exiting position, in absolute terms.
		}
		/// <summary>Reflects given value. By design of the reflector, it's the same method for both directions. Singlethreaded version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		private void Reflect(int i)
		{
			Message[i] = Key.Reflector.FindValueUsingIndex(Message[i]);
		}
		/// <summary>Reflects given value. By design of the reflector, it's the same method for both directions. Parallel version.</summary>
		/// <param name="i">Index of a char in the message.</param>
		/// <param name="message">Part of the message.</param>
		private void Reflect(int i, ref ushort[] message)
		{
			message[i] = Key.Reflector.FindValueUsingIndex(message[i]);
		}
		/// <summary>Adds random chars to the message.</summary>
		private void AddRandomChars()
		{ // ToDo: Avoid frequent insertion to a list.
			int index = (Key.RandCharSpcMin + Key.RandCharSpcMax) % 2; // Index inicialization, where I will add random char.
			int gap = Key.RandCharSpcMax - Key.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Key.RandCharSpcMin); // Gap inicialization. In core calculation of “random” number, then putting in limits and shift by minimum.
			BigInteger A = Key.RandCharConstA.ComputeConstant(1), B = Key.RandCharConstB.ComputeConstant(), M = Key.RandCharConstM.ComputeConstant(); // I have to precompute the constants here, so I don't compute them every time.
			while (index < Message.Count)
			{
				Message.Insert(index, Generators.GenerateNum());
				IncrementSpace(ref index, ref space, gap, A, B, M);
			}
			if (index == Message.Count)
			{
				Message.Add(Generators.GenerateNum());
			}
		}
		/// <summary>Removes random chars from the message.</summary>
		private void RemoveRandomChars()
		{
			int index = (Key.RandCharSpcMin + Key.RandCharSpcMax) % 2;
			int gap = Key.RandCharSpcMax - Key.RandCharSpcMin;
			BigInteger space = (BigInteger)(Math.Floor(Convert.ToDouble(Codepage.Limit / gap)) % gap + Key.RandCharSpcMin);
			BigInteger A = Key.RandCharConstA.ComputeConstant(1), B = Key.RandCharConstB.ComputeConstant(), M = Key.RandCharConstM.ComputeConstant();
			List<int> IdxToRemove =
			[
				index
			];
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
			index +=  (int)(space % gap) + Key.RandCharSpcMin;
		}
		/// <summary>Switches the positions of all characters in the message. Pseudorandomly.</summary>
		/// <param name="stage">Which stage? So far only 0 or 1.</param>
		private void SwitchCharPoz(byte stage)
		{
			List<int> switchTable = ConstructSwitchTable(stage);
			List<ushort> original = [.. Message];
			for (int i = 0; i < switchTable.Count; i++)
			{
				Message[i] = original[switchTable[i]];
			}
		}
		/// <summary>Switches the positions of all characters in the message back.</summary>
		/// <param name="stage">Which stage? So far only 0 or 1.</param>
		private void UnSwitchCharPoz(byte stage)
		{
			List<int> switchTable = ConstructSwitchTable(stage);
			List<ushort> original = [.. Message];
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
			List<int> switchTable = [];
			BigInteger A = stage == 0 ? Key.SwitchConstA.ComputeConstant() : Key.SwitchConstC.ComputeConstant();
			BigInteger B = stage == 0 ? Key.SwitchConstB.ComputeConstant() : Key.SwitchConstD.ComputeConstant();
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
		/// <summary></summary>
		private void AffineModulation()
		{ // ToDo: This is not finished. For now, this is just a placeholder. 299792458 should be replaced by some CSPRGN. Part of the key.
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)(((Codepage.Limit - 1) * Message[i] + 299792458) % Codepage.Limit);
			}
		}
		/// <summary></summary>
		private void AffineDemodulation()
		{ // ToDo: This is not finished. For now, this is just a placeholder. 299792458 should be replaced by some CSPRGN. Part of the key.
			for (int i = 0; i < Message.Count; i++)
			{
				Message[i] = (ushort)(((Codepage.Limit - 1) * (Message[i] - 299792458)) % Codepage.Limit);
			}
		}
		/// <summary>Computes the modulo sumation. Result will be in range 0 to Codepage.Limit - 1.</summary>
		/// <param name="A">A number.</param>
		/// <param name="B">B number.</param>
		/// <returns>Sumation modulated to 0 to Codepage.Limit - 1.</returns>
		private static ushort ModuloSum(ushort A, ushort B)
		{
			return (ushort)((A + B) % Codepage.Limit);
		}
		/// <summary>Computes the modulo substraction. Result will be in range 0 to Codepage.Limit - 1.</summary>
		/// <param name="A">A number. It matters which is it.</param>
		/// <param name="B">B number. It matters which is it.</param>
		/// <returns>Substraction modulated to 0 to Codepage.Limit - 1.</returns>
		private static ushort ModuloSub(ushort A, ushort B)
		{
			return (ushort)((A - B + Codepage.Limit) % Codepage.Limit);
		}
	}
}