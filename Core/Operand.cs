using System;
using System.IO;

namespace Core
{
    public class Operand {
        public enum OperandType : byte {
            Register = 0x00, // 00
            DereferencedRegister = 0x40, // 01
            IndexedRegister = 0x80, // 10
            Constant = 0xC0 // 11
        }

        public enum RegisterType : byte {
            A,
            B,
            C, 
            D,
            X,
            Y,
            Z,
            I,
            J
        }

        const byte REGISTER_MASK = 0x3F;

        public OperandType OpType { get; }
        public RegisterType Register { get; }
        public Operand Index { get; }
        public UInt32 Value { get; }
        public int Size { get; } = 1;

        public Operand(Stream input) {
            var buffer = new byte[4];
            var operand_span = buffer.AsSpan(0, 1);
            var const_span = buffer.AsSpan();
            input.Read(operand_span);

            OpType = (OperandType)(buffer[0] & ~REGISTER_MASK);

            switch (OpType) {
                case OperandType.IndexedRegister:
                    Index = new Operand(input);
                    Size += Index.Size;
                    goto case OperandType.Register; // Fallthrough

                case OperandType.Register:
                case OperandType.DereferencedRegister:
                    Register = (RegisterType)(buffer[0] & REGISTER_MASK);
                    break;

                case OperandType.Constant:
                    input.Read(const_span);
                    Value = BitConverter.ToUInt32(const_span);
                    Size += 4;
                    break;
            }
        }

        public override string ToString() {
            switch (OpType) {
                case OperandType.Register:
                    return Register.ToString();
                case OperandType.DereferencedRegister:
                    return $"*{Register}";
                case OperandType.IndexedRegister:
                    return $"{Register}[{Index}]";
                case OperandType.Constant:
                    return $"0x{Value:X}";
            }
            return "ERROR";
        }
    }
}
