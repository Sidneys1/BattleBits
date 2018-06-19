namespace Core
{
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
            JMP,
            INT
        }

        public static System.Collections.Generic.Dictionary<Opcode, int> OpcodeLength = new System.Collections.Generic.Dictionary<Opcode, int> {
            {Opcode.Nop, 0},

            {Opcode.JMP, 1},
            {Opcode.INT, 1},

            {Opcode.ADD, 3},
            {Opcode.SUB, 3},
            {Opcode.MUL, 3},
            {Opcode.DIV, 3},
            {Opcode.JEQ, 3},
            {Opcode.JNE, 3},
            {Opcode.JGT, 3},
        };
    }
}
