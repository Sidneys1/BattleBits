using System;
using System.IO;

namespace Core
{
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

        public long Position { get; private set; }

        public byte ID { get; }

        private readonly Memory Memory;

        public CPU(byte id, Memory memory, long position = 0) {
            Memory = memory;
            Position = position;
            Registers = _registers;
            ID = id;
        }

        public void Tick() {
            // System.Console.WriteLine($"CPU - Seeking to 0x{Position:X8}");
            Memory.Stream.Seek(Position, SeekOrigin.Begin);
            var instruction = new Instruction(Memory.Stream);
            // Console.WriteLine($"CPU - Executing {instruction.Size} byte instruction {instruction}");
            if (Execute(instruction)) {
                Position += instruction.Size;
                Position %= (int)Memory.Size;
            }
        }

        private UInt32 GetRegister(Operand.RegisterType register) {
            if (register == Operand.RegisterType.IP)
                return (UInt32)this.Position;
            return _registers[register];
        }

        private UInt32 GetOperand(Operand operand) {
            switch (operand.OpType) {
                case Operand.OperandType.Register:
                    return GetRegister(operand.Register);
                case Operand.OperandType.DereferencedRegister:
                    return Memory.ReadRaw((long)GetRegister(operand.Register));
                case Operand.OperandType.IndexedRegister:
                    var base_addr = GetRegister(operand.Register);
                    var index = GetOperand(operand.Index);
                    return Memory.ReadRaw(base_addr + (index * 4));
                case Operand.OperandType.Constant:
                    return operand.Value;
            }
            return 0xffffffff;
        }

        public void SetRegister(Operand.RegisterType register, UInt32 value) {
            if (register == Operand.RegisterType.IP)
                return;
            // System.Console.WriteLine($"CPU - {register} is now 0x{value:X}");
            _registers[register] = value;
        }

        private void SetOperand(Operand operand, UInt32 value) {
            switch (operand.OpType) {
                case Operand.OperandType.Register:
                    SetRegister(operand.Register, value);
                    break;
                case Operand.OperandType.DereferencedRegister:
                    Memory.WriteRaw((long)GetRegister(operand.Register), value, ID);
                    break;
                case Operand.OperandType.IndexedRegister:
                    var base_addr = GetRegister(operand.Register);
                    var index = GetOperand(operand.Index);
                    Memory.WriteRaw(base_addr + index, value, ID);
                    break;
                case Operand.OperandType.Constant:
                    Memory.WriteRaw(operand.Value, value, ID);
                    break;
            }
        }

        private bool Execute(Instruction instruction) {
            switch (instruction.Opcode) {
                case Opcodes.Opcode.Nop:
                    break;

                case Opcodes.Opcode.JMP: {
                    this.Position = (long)GetOperand(instruction.Operands[0]);
                    return false;
                }

                case Opcodes.Opcode.ADD: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    var result = value1 + value2;
                    // System.Console.WriteLine($"CPU - {value1} + {value2} = {result}");
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

                case Opcodes.Opcode.JEQ: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    var value3 = GetOperand(instruction.Operands[2]);
                    if (value1 == value2) {
                        // Console.WriteLine($"CPU - Values were equal, jumping to 0x{value3:X8}");
                        this.Position = (long)value3;
                        return false;
                    }
                } break;

                case Opcodes.Opcode.JNE: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    var value3 = GetOperand(instruction.Operands[2]);
                    if (value1 != value2) {
                        // Console.WriteLine($"CPU - Values were not equal, jumping to 0x{value3:X8}");
                        this.Position = (long)value3;
                        return false;
                    }
                } break;

                case Opcodes.Opcode.JGT: {
                    var value1 = GetOperand(instruction.Operands[0]);
                    var value2 = GetOperand(instruction.Operands[1]);
                    var value3 = GetOperand(instruction.Operands[2]);
                    if (value1 >= value2) {
                        // Console.WriteLine($"CPU - Value 1 was greater, jumping to 0x{value3:X8}");
                        this.Position = (long)value3;
                        return false;
                    }
                } break;

                case Opcodes.Opcode.INT: {
                    var interrupt = GetOperand(instruction.Operands[0]);
                    switch (interrupt)
                    {
                        case 1:
                            System.Console.WriteLine(GetRegister(Operand.RegisterType.A));
                            break;
                        
                        default:
                            throw new NotImplementedException($"Unimplemented interrupt: {interrupt}");
                    }
                } break;

                default: {
                    throw new NotImplementedException($"Unimplemented opcode: {instruction.Opcode}");
                }
            }
            return true;
        }
    }
}
