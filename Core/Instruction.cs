using System;
using System.IO;
using System.Text;

namespace Core
{
    public class Instruction {
        public Opcodes.Opcode Opcode { get; }

        public Operand[] Operands { get; }

        public int OperandCount { get; }

        public int Size { get; } = 1;

        public Instruction(Stream input) {
            var read_buffer = new byte[1];
            var opcode_span = read_buffer.AsSpan(0, 1);
            input.Read(opcode_span);
            Opcode = (Opcodes.Opcode)read_buffer[0];

            OperandCount = Opcodes.OpcodeLength[Opcode];
            Operands = new Operand[OperandCount];

            for (int i = 0; i < OperandCount; i++) {
                Operands[i] = new Operand(input);
                Size += Operands[i].Size;
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
}
