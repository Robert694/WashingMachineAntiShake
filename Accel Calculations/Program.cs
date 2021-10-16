using System;
using System.Linq;
using System.Text;

namespace Accel_Calculations
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Test.ProcessDataToMagMax2(16);
        }

        public static void BitFun()
        {
            Random r = new Random();
            int lastState = -1;
            var subStates = 8;
            while (true)
            {
                bool[] states = new bool[subStates];
                for (int i = 0; i < subStates; i++)
                    states[i] = Convert.ToBoolean(r.Next(0, 2));
                int state = ToState(states);
                var stateText = StateToString(state, subStates);
                var stateBools = string.Join("|", states.Select(x => $"{x,-5}"));
                var resultStates = IntToStates(state, subStates);
                if (state == lastState)
                {
                    Console.WriteLine($"{stateBools} = {state,-5} ({stateText}) (Repeat)");
                    continue;
                }
                lastState = state;
                Console.WriteLine($"{stateBools} = {state,-5} ({stateText})");
                Console.ReadKey();
            }
        }

        public static int ToState(params bool[] states)
        {
            if (states.Length > sizeof(int) * 8)
                throw new ArgumentOutOfRangeException($"Can only store {sizeof(int) * 8} values in a single integer");
            int state = 0;
            for (int i = 0; i < states.Length; i++)
                state |= (states[i] ? 1 : 0) << (states.Length-1 - i);
            return state;
        }

        public static string StateToString(int value, int stateCount)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = sizeof(int) * 8; --i > 0;)
            {
                if(i > stateCount)continue;
                var bit = (value & (1 << (i - 1))) != 0;
                sb.Append(bit ? 1 : 0);
            }
            return sb.ToString();
        }

        public static bool[] IntToStates(int value, int stateCount)
        {
            bool[] returnStates = new bool[stateCount];
            int count = 0;
            for (int i = sizeof(int) * 8; --i > 0;)
            {
                if (i > stateCount) continue;
                var bit = (value & (1 << (i - 1))) != 0;
                returnStates[count++] = bit;
            }
            return returnStates;
        }
    }
}
