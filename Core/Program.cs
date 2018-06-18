using System;
using System.IO;

namespace Core {
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
        const byte REGISTER_MASK = 0x3F;
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
