using System;
using System.IO;
using System.Text;

namespace Core {
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

        public Operand(Stream input) {
            var buffer = new byte[4];
            var operand_span = buffer.AsSpan(0, 1);
            var const_span = buffer.AsSpan();
            input.Read(operand_span);

            OpType = (OperandType)(buffer[0] & REGISTER_MASK);

            switch (OpType) {
                case OperandType.IndexedRegister:
                    Index = new Operand(input);
                    goto case OperandType.Register; // Fallthrough

                case OperandType.Register:
                case OperandType.DereferencedRegister:
                    Register = (RegisterType)(buffer[0] & ~REGISTER_MASK);
                    break;

                case OperandType.Constant:
                    input.Read(const_span);
                    Value = BitConverter.ToUInt32(const_span);
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
                    return $"0x{Value:0X}";
            }
            return "ERROR";
        }
    }

    public class Instruction {
        public Opcodes.Opcode Opcode { get; }

        public Operand[] Operands { get; }

        public int OperandCount { get; }

        public Instruction(Stream input) {
            var read_buffer = new byte[1];
            var opcode_span = read_buffer.AsSpan(0, 1);
            input.Read(opcode_span);
            Opcode = (Opcodes.Opcode)read_buffer[0];

            OperandCount = Opcodes.OpcodeLength[Opcode];
            Operands = new Operand[OperandCount];

            for (int i = 0; i < OperandCount; i++) {
                Operands[i] = new Operand(input);
            }
        }

        public override string ToString() {
            var builder = new StringBuilder();

            builder.Append(Opcode);

            foreach (var operand in Operands) {
                builder.Append(" ");
                builder.Append(operand);
            }

            return builder.ToString();
        }
    }

    public static class Opcodes {
        public enum Opcode : byte {
            Nop,
            ADD,
            SUB,
            MUL,
            DIV,
            JEQ,
            JNE,
            JGT,
            JMP
        }

        public static System.Collections.Generic.Dictionary<Opcode, int> OpcodeLength = new System.Collections.Generic.Dictionary<Opcode, int> {
            {Opcode.Nop, 0},
            {Opcode.ADD, 3}
        };
    }

    class Program {
        
        const int MEM_SIZE = 1024;
        static readonly byte[] Memory = new byte[MEM_SIZE];

        static MemoryStream memStream = new MemoryStream(Memory);

        static void Main(string[] args) {
            byte[] opcode = new byte[1];
            memStream.Read(opcode.AsSpan());
            Console.WriteLine($"Hello World! Memory[0] is {(Opcodes.Opcode)opcode[0]}");
            Memory[0] = 1;
            memStream.Seek(0, SeekOrigin.Begin);
            memStream.Read(opcode.AsSpan());
            Console.WriteLine($"Hello World! Memory[0] is {(Opcodes.Opcode)opcode[0]}");
        }
    }
}
