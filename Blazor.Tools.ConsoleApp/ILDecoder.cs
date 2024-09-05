
using System.Reflection.Emit;

namespace Blazor.Tools.ConsoleApp
{
    public class ILDecoder
    {
        public ILDecoder() { }
        public void DecodeIL(byte[] ilCode)
        {
            int index = 0;
            while (index < ilCode.Length)
            {
                OpCode opCode = OpCodes.Nop;

                // Check for single-byte or multi-byte opcode
                if (ilCode[index] != 0xFE)
                {
                    opCode = SingleByteOpCode(ilCode[index]);
                    index++;
                }
                else
                {
                    opCode = MultiByteOpCode(ilCode[index + 1]);
                    index += 2;
                }

                Console.WriteLine($"IL OpCode: {opCode.Name} (0x{opCode.Value:X})");

                // Handle the operands of the opcode if needed
                if (opCode.OperandType != System.Reflection.Emit.OperandType.InlineNone)
                {
                    index = HandleOperand(ilCode, index, opCode);
                }
            }
        }

        private OpCode SingleByteOpCode(byte code)
        {
            foreach (var field in typeof(OpCodes).GetFields())
            {
                if (field.GetValue(null) is OpCode opCode && opCode.Value == code)
                {
                    return opCode;
                }
            }
            return OpCodes.Nop; // Default to Nop if not found
        }

        private OpCode MultiByteOpCode(byte code)
        {
            foreach (var field in typeof(OpCodes).GetFields())
            {
                if (field.GetValue(null) is OpCode opCode && opCode.Value == (0xFE00 | code))
                {
                    return opCode;
                }
            }
            return OpCodes.Nop; // Default to Nop if not found
        }

        private int HandleOperand(byte[] ilCode, int index, OpCode opCode)
        {
            switch (opCode.OperandType)
            {
                case OperandType.ShortInlineI:
                    Console.WriteLine($"Operand: {ilCode[index]:X2}");
                    return index + 1;
                case OperandType.InlineI:
                    int operand = BitConverter.ToInt32(ilCode, index);
                    Console.WriteLine($"Operand: {operand:X8}");
                    return index + 4;
                // Add cases for other operand types (InlineMethod, InlineType, InlineString, etc.)
                default:
                    return index;
            }
        }

        public byte[] ConvertHexStringToByteArray(string hex)
        {
            string[] hexValues = hex.Split(' ');
            byte[] bytes = new byte[hexValues.Length];
            for (int i = 0; i < hexValues.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexValues[i], 16);
            }
            return bytes;
        }
    }
}
