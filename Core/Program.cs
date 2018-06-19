using System;
using System.IO;

namespace Core
{
    public class Memory {
        public Stream Stream { get; }
        public long Size { get; }
        public byte[] RawMemory { get; }

        public Memory(long size) {
            RawMemory = new byte[size];
            Stream = new MemoryStream(RawMemory);
            Size = size;
        }

        public Memory(byte[] raw) {
            RawMemory = raw;
            Stream = new MemoryStream(RawMemory);
            Size = raw.Length;
        }

        public void WriteRaw(long position, UInt32 value) {
            var bytes = BitConverter.GetBytes(value);
            for (var i = 0; i < bytes.Length; i++) {
                position %= Size;
                RawMemory[position++] = bytes[i];
            }
        }

        public UInt32 ReadRaw(long position) {
            var bytes = new byte[4];
            for (var i = 0; i < bytes.Length; i++) {
                position %= Size;
                bytes[i] = RawMemory[position++];
            }
            return BitConverter.ToUInt32(bytes, 0);
        }
    }

    public class CPU {
        private System.Collections.Generic.Dictionary<Operand.RegisterType, UInt32> _registers = new System.Collections.Generic.Dictionary<Operand.RegisterType, uint> {
            { Operand.RegisterType.A, 0 },
            { Operand.RegisterType.B, 0 },
            { Operand.RegisterType.C, 0 },
            { Operand.RegisterType.X, 0 },
            { Operand.RegisterType.Y, 0 },
            { Operand.RegisterType.Z, 0 },
            { Operand.RegisterType.I, 0 },
            { Operand.RegisterType.J, 0 },
        };

        public System.Collections.Generic.IReadOnlyDictionary<Operand.RegisterType, UInt32> Registers {get;}

        public int Position { get; private set; }

        private readonly Memory Memory;

        public CPU(Memory memory, int position = 0) {
            Memory = memory;
            Position = position;
            Registers = _registers;
        }

        public void Tick() {
            Memory.Stream.Seek(Position, SeekOrigin.Begin);
            Console.WriteLine($"Seeking to 0x{Position:X8}");
            var instruction = new Instruction(Memory.Stream);
            Console.WriteLine($"Instruction is {instruction.Size} bytes: {instruction}");
            Execute(instruction);
            Position += instruction.Size;
            Position %= (int)Memory.Size;
        }

        private UInt32 GetOperand(Operand operand) {
            switch (operand.OpType) {
                case Operand.OperandType.Register:
                    return _registers[operand.Register];
                case Operand.OperandType.DereferencedRegister:
                    return Memory.ReadRaw((long)_registers[operand.Register]);
                case Operand.OperandType.IndexedRegister:
                    var base_addr = _registers[operand.Register];
                    var index = GetOperand(operand.Index);
                    return Memory.ReadRaw(base_addr + (index * 4));
                case Operand.OperandType.Constant:
                    return operand.Value;
            }
            return 0xffffffff;
        }

        private void SetOperand(Operand operand, UInt32 value) {
            switch (operand.OpType) {
                case Operand.OperandType.Register:
                    _registers[operand.Register] = value;
                    break;
                case Operand.OperandType.DereferencedRegister:
                    Memory.WriteRaw((long)_registers[operand.Register], value);
                    break;
                case Operand.OperandType.IndexedRegister:
                    var base_addr = _registers[operand.Register];
                    var index = GetOperand(operand.Index);
                    Memory.WriteRaw(base_addr + (index * 4), value);
                    break;
                case Operand.OperandType.Constant:
                    Memory.WriteRaw(operand.Value, value);
                    break;
            }
        }

        private void Execute(Instruction instruction) {
            switch (instruction.Opcode) {
                case Opcodes.Opcode.Nop:
                    break;

                case Opcodes.Opcode.ADD: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    var result = value1 + value2;
                    Console.WriteLine($"{value1} + {value2} = {result}");
                    SetOperand(instruction.Operands[2], result);
                } break;

                case Opcodes.Opcode.SUB: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    SetOperand(instruction.Operands[2], value1 - value2);
                } break;

                case Opcodes.Opcode.MUL: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    SetOperand(instruction.Operands[2], value1 * value2);
                } break;

                case Opcodes.Opcode.DIV: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    SetOperand(instruction.Operands[2], value1 / value2);
                } break;

                default: {
                    throw new NotImplementedException($"Unimplemented opcode: {instruction.Opcode}");
                }
            }
        }
    }

    class Program {
        
        const int MEM_SIZE = 1024;
        static readonly byte[] mem = new byte[] {
            (byte)Opcodes.Opcode.ADD, 
            (byte)Operand.OperandType.Constant, 
            0x01, 0x00, 0x00, 0x00, // Note litte endianness, this is 0x00000064
            (byte)Operand.OperandType.Constant, 
            0x02, 0x00, 0x00, 0x00, // Note litte endianness, this is 0x00000064
            ((byte)Operand.OperandType.Register & (byte)Operand.RegisterType.A)
        };

        static Memory memory;
        static CPU cpu = new CPU(memory, 0);

        static void Main(string[] args) {
            memory = new Memory(mem);
            cpu = new CPU(memory, 0);
            cpu.Tick();
            Console.WriteLine($"Value of A is {cpu.Registers[Operand.RegisterType.A]}");
        }
    }
}
