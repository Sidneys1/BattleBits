using System;
using Sprache;
using System.Linq;

namespace Core
{
    static class Parser {
        static Parser<string> identifier =
            // from leading in Parse.WhiteSpace.Except(Parse.LineTerminator).Many()
            from rest in Parse.Letter.AtLeastOnce().Text()
            select rest;

        public static Parser<Opcodes.Opcode> opcode =
            from id in identifier
            select (Opcodes.Opcode)Enum.Parse(typeof(Opcodes.Opcode), id, true);

        public static Parser<Operand.RegisterType> register = 
            from id in identifier
            select (Operand.RegisterType)Enum.Parse(typeof(Operand.RegisterType), id, true);

        public static Parser<Operand> registerOp =
            from reg in register
            select new Operand(Operand.OperandType.Register, reg);

        public static Parser<Operand> dereferencedRegister =
            from lead in Parse.Char('*').Once()
            from reg in register
            select new Operand(Operand.OperandType.DereferencedRegister, reg);

        public static Parser<Operand> indexedRegister =
            from reg in register
            from idx in operand.Contained(Parse.Char('['), Parse.Char(']'))
            select new Operand(Operand.OperandType.IndexedRegister, reg, idx);

        public static Parser<UInt32> decimalParser =
            from num in Parse.Number
            select UInt32.Parse(num);

        public static Parser<UInt32> hexParser =
            from lead in Parse.String("0x")
            from num in Parse.Numeric.Or(Parse.Chars("ABCDEFabcdef")).Repeat(1, 8).Text()
            select Convert.ToUInt32(num, 16);

        public static Parser<Operand> staticNumber =
            from num in hexParser.Or(decimalParser)
            select new Operand(Operand.OperandType.Constant, value: num);

        public static Parser<Operand> operand =
            from ws in Parse.WhiteSpace.Except(Parse.LineTerminator).Many()
            from op in staticNumber.Or(dereferencedRegister).Or(indexedRegister).Or(registerOp)
            select op;

        public static Parser<string> comment =
            from ws in Parse.WhiteSpace.Many()
            from ind in Parse.Char(';')
            from comment in Parse.AnyChar.Except(Parse.LineTerminator).Many().Optional()
            select "";

        public static Parser<Instruction> instruction =
            from ws in Parse.WhiteSpace.Many()
            from op in opcode
            from oper in operand.Many().Optional()
            from c in comment.Optional()
            from newl in Parse.LineTerminator
            select new Instruction(op, oper.IsDefined ? oper.Get().ToArray() : new Operand[0]);
    }
}
