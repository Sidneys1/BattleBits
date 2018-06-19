using System;
using System.Linq;
using System.Diagnostics;
using Sprache;

namespace Core {
    class Program {
        
        const int MEM_SIZE = 64;
        static Memory memory = new Memory(MEM_SIZE, 8);
        static CPU cpu1, cpu2;

        static void Main(string[] args) {
            var instructions = Core.Parser.instruction.AtLeastOnce().Parse(@"
                ADD 0 1 B ;0x00000000 12 Set initial value
                ADD B C C ;0x0000000C Add B and C into C
                ADD 0 C A ;0x00000010 Load C into A
                INT 0x01  ;0x00000018 Print A
                ADD B C B ;0x0000001E Add B and C into B
                ADD 0 B A ;0x00000022 Load B into A
                INT 0x01  ;0x0000002A Print A
                JMP 0xC   ;0x00000030 Jump back to second instruction
            ");

            System.Collections.Generic.IEnumerable<byte> all_bytes = new byte[0].AsEnumerable();
            var pos = 0;
            foreach (var ins in instructions) {
                Console.Write("0x{0:X8}: {1}", pos, ins);
                pos += ins.Size;
                var bytes = ins.ToBytes();
                all_bytes = all_bytes.Concat(bytes);
                foreach (var b in bytes)
                {
                    Console.Write("0x{0:X2} ", b);
                }
                System.Console.WriteLine();
            }
            System.Console.WriteLine($"All count: {all_bytes.Count()}");

            all_bytes.ToArray().CopyTo(memory.RawMemory, 0);
            for (int i = 0; i < all_bytes.Count(); i++) {
                memory.MemoryOwnership.SetOwner(i, 1);
            }

            var sw = new Stopwatch();
            cpu1 = new CPU(1, memory, 0);
            sw.Start();
            for (int i = 0; i < 100; i++) {
                cpu1.Tick();
            }
            sw.Stop();
            System.Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Memory ownership:{memory.MemoryOwnership}");
        }
    }
}
