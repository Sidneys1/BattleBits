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

        public Instruction(Opcodes.Opcode op, params Operand[] operands) {
            Opcode = op;
            OperandCount = operands.Length;
            Operands = operands;
            for (int i = 0; i < OperandCount; i++) {
                Size += Operands[i].Size;
            }
        }

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

        public byte[] ToBytes() {
            var ret = new byte[Size];
            ret[0] = (byte)Opcode;
            var view = ret.AsSpan(1);
            foreach (var op in Operands) {
                op.WriteInto(view);
                view = view.Slice(op.Size);
            }
            return ret;
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
