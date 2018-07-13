using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandbox.Core
{
    public partial class CPU
    {
        public readonly MMU _mmu;

        public List<Action> Map { get; private set; }
        public List<Action> CbMap { get; private set; }

        public int totalCycles;     //Number of clock cycles that have progressed since boot
        public int lastOpCycles;    //Number of clock cycles from last opCode

        private bool _verbose;      //Used only for debugging purposes
        public bool continueOperation = true;

        public void Start(bool verbose)
        {
            _verbose = verbose;

            for (; ; ) //CPU loop for clock cycles
            {
                if (_verbose) Console.WriteLine("-- CPU Loop --");

                Tick();

                if (_verbose) Debug_LogRegisters();

                //Check for break loop
                if (!continueOperation)
                {
                    if (!_verbose) Console.WriteLine("Shutting down CPU...");
                    break;
                }
            }
        }

        public void Tick()
        {
            if (_verbose) Console.WriteLine($"PC: 0x{Reg_PC:X2}");

            //Fetch instruction from memory using program counter
            var opCode = _mmu.ReadByte(Reg_PC);

            if (_verbose) Console.WriteLine($"OPCODE: 0x{opCode:X2}");

            //Increment program counter
            Reg_PC++;

            //Execute instruction
            try
            {
                Map[opCode]();
            }
            catch (InstructionNotImplementedException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"!!! SYSTEM EXCEPTION: {exception.Message}");
                //log to file later
            }

            //Update total clock time
            totalCycles += lastOpCycles;

            //Check for interrupts
        }

        public CPU(MMU mmu)
        {
            _mmu = mmu;

            Reg_A = 0x00;
            Reg_F = 0x00;
            Reg_B = 0x00;
            Reg_C = 0x00;
            Reg_D = 0x00;
            Reg_E = 0x00;
            Reg_H = 0x00;
            Reg_L = 0x00;
            Reg_SP = 0x0000;
            Reg_PC = 0x0000;

            Map = new List<Action>
            {
                //00 - 0F
                null,
                null,
                null,
                null,
                null,
                () => LD_d8(0x06),      //LD B,d8
                null,
                null,
                null,
                null,
                null,
                null,
                () => INC(0x0C),        //INC C
                null,
                () => LD_d8(0x0E),      //LD C,d8
                null,

                //10 - 1F
                null,
                () => LD_d16(0x11),     //LD DE,d16
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                () => LD_d8m(0x1A),     //LD A,(DE)
                null,
                null,
                null,
                null,
                null,

                //20 - 2F
                () => JR_CC_r8(0x20),   //JR NZ,r8
                () => LD_d16(0x21),     //LD HL,d16
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //30 - 3F
                null,
                () => LD_d16(0x31),     //LD SP,d16
                () => LDD_d8m(0x32),    //LD (HL-),A
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                () => LD_d8(0x3E),      //LD A,d8
                null,

                //40 - 4F
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //50 - 5F
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //60 - 6F
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //70 - 7F
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                () => LDr_d8m(0x77),       //LD (HL),A
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //80 - 8F
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //90 - 9F
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //A0 - AF
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                () => XOR_R8(0xAF),     //XOR A

                //B0 - BF
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //C0 - CF
                null,
                null,
                null,
                null,
                () => CALL_CC_d16m(0xC4),   //CALL NZ,a16
                null,
                null,
                null,
                null,
                null,
                null,
                () => CB(0xCB),             //CB
                null,
                () => CALL_d16m(0xCD),      //CALL a16
                null,
                null,

                //D0 - DF
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //E0 - EF
                () => LDH_d8m(0xE0),       //LDH (a8),a
                null,
                () => LDr_d8m(0xE2),       //LD (C),A
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                //F0 - FF
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
            };
            CbMap = new List<Action>
            {
                //00 - 0F
                null, null, null, null, null, RLC_HLm, null, null, null, null, null, null, null, null, null, null,
                //10 - 1F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //20 - 2F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //30 - 3F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //40 - 4F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //50 - 5F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //60 - 6F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //70 - 7F
                null, null, null, null, null, null, null, null, null, null, null, null, BIT7_H, null, null, null,
                //80 - 8F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //90 - 9F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //A0 - AF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //B0 - BF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //C0 - CF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //D0 - DF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //E0 - EF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //F0 - FF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
            };
        }

        #region Operations
        //Opcode switch idea was borrowed from https://github.com/pmcanseco/java-gb
        //References DP's 'GameBoy CPU Manual' for documentation (DP, <page#>)

        //Load immediate byte from memory to register (DP, 65)
        private void LD_d8(byte opcode)
        {
            switch (opcode)
            {
                case 0x06:
                    Reg_B = _mmu.ReadByte(Reg_PC++); 
                    break;
                case 0x0E:
                    Reg_C = _mmu.ReadByte(Reg_PC++);
                    break;
                case 0x3E:
                    Reg_A = _mmu.ReadByte(Reg_PC++);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
            return;
        }

        //Load immediate word from memory to register (DP, 65)
        private void LD_d16(byte opcode)
        {
            switch (opcode)
            {
                case 0x11:
                    Reg_DE = _mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
                case 0x21:
                    Reg_HL = _mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
                case 0x31:
                    Reg_SP = _mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 12;
            return;
        }

        //Load value found at location (0xFF00 + register1) to register2 (DP, 65)
        private void LD_d8m(byte opcode)
        {
            switch (opcode)
            {
                case 0x1A:
                    Reg_A = _mmu.ReadByte(Reg_DE);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
            return;
        }

        //Load value found at location (0xFF00 + register1) to register2 and decrement register1 (DP, 71)
        private void LDD_d8m(byte opcode)
        {
            switch (opcode)
            {
                case 0x32:
                    _mmu.WriteByte(Reg_HL, Reg_A);
                    Reg_HL--;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
            return;
        }

        //Load register1 into memory address (0xFF00 + register2) (DP, 65)
        private void LDr_d8m(byte opcode)
        {
            switch (opcode)
            {
                case 0x77:
                    _mmu.WriteByte(Reg_HL, Reg_A);
                    break;
                case 0xE2:
                    _mmu.WriteByte(Reg_C, Reg_A);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
        }

        //Load Reg_A into memory address 0xFF00 + (next byte in memory) (DP, 75)
        private void LDH_d8m(byte opcode)
        {
            switch (opcode)
            {
                case 0xE0:
                    var addr = (ushort)(0xFF00 + _mmu.ReadByte(Reg_PC++));
                    _mmu.WriteByte(addr, Reg_A);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 12;
        }

        //Increment value (DP, 88)
        private void INC(byte opcode)
        {
            byte val;
            switch (opcode)
            {
                case 0x0C:
                    val = Reg_C;
                    Reg_C++;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag((val + 1) == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag((((val & 0xF) + 1) & 0x10) == 0x10);

            lastOpCycles = 4;
            return;
        }

        //Exclusive OR register with Reg_A, result saved into Reg_A (DP, 86)
        private void XOR_R8(byte opcode)
        {
            byte val;
            switch (opcode)
            {
                case 0xAF:
                    Reg_A ^= Reg_A;
                    val = Reg_A;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(val == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(false);

            lastOpCycles = 4;
        }

        //Exclusive OR value from memory with Reg_A, result saved into Reg_A (DP, 86)
        private void XOR_R8m(byte opcode)
        {
            byte val;
            switch (opcode)
            {
                case 0xAF:
                    Reg_A ^= Reg_A;
                    val = Reg_A;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(val == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(false);

            lastOpCycles = 8;
        }

        //Push address of next instruction onto stack and then jump to address of next word in memory (DP, 114)
        private void CALL_d16m(byte opcode)
        {
            Reg_SP -= 2;
            _mmu.WriteWord(Reg_SP, (ushort)(Reg_PC + 2));
            Reg_PC = _mmu.ReadWord(Reg_PC);

            lastOpCycles = 4;
        }

        //Call address of next word in memory if condition is met (DP, 115)
        private void CALL_CC_d16m(byte opcode)
        {
            switch (opcode)
            {
                case 0xC4:
                    if (GetZeroFlag())
                    {
                        lastOpCycles = 12;
                        return;
                    }
                    break;
            }

            Reg_SP -= 2;
            _mmu.WriteWord(Reg_SP, (ushort)(Reg_PC + 2));
            Reg_PC = _mmu.ReadWord(Reg_PC);

            lastOpCycles = 24;
        }

        //Jump to address PC + (next memory location as signed bit) if condition is met (DP, 113)
        private void JR_CC_r8(byte opcode)
        {
            switch (opcode)
            {
                case 0x20:
                    if (GetZeroFlag())
                    {
                        lastOpCycles = 8;
                        return;
                    }
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            var signed = unchecked((sbyte)_mmu.ReadByte(Reg_PC++));

            Reg_PC = signed < 0
                    ? (ushort)(Reg_PC - (ushort)(signed * (-1)))
                    : (ushort)(Reg_PC + (ushort)signed);

            lastOpCycles = 12;
        }

        //Call instruction at index PC+1 from CB map
        private void CB(byte opcode)
        {
            CbMap[_mmu.ReadByte(Reg_PC++)]();
            lastOpCycles += 4;
        }


        //CB codes
        private void BIT7_H()       //BIT 7,H
        {
            if (!GetBit7(Reg_H)) AffectZeroFlag(true);
            AffectSubFlag(false);
            AffectHalfCarryFlag(true);

            lastOpCycles = 8;
        }

        public void RLC_HLm()       //RLC (HL)
        {
            bool toCarry = GetBit7(_mmu.ReadByte(Reg_HL));
            _mmu.WriteByte(Reg_HL, (byte)((_mmu.ReadByte(Reg_HL) << 1) + (toCarry ? 1 : 0)));

            AffectZeroFlag(_mmu.ReadByte(Reg_HL) == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(toCarry);

            lastOpCycles = 16;
        }
        #endregion

        #region Helper Functions
        private bool GetBit7(byte val)              { return (val & 0x80) == 0x80; }
        private bool GetBit6(byte val)              { return (val & 0x40) == 0x40; }
        private bool GetBit5(byte val)              { return (val & 0x20) == 0x20; }
        private bool GetBit4(byte val)              { return (val & 0x10) == 0x10; }
        private bool GetBit3(byte val)              { return (val & 0x08) == 0x08; }
        private bool GetBit2(byte val)              { return (val & 0x04) == 0x04; }
        private bool GetBit1(byte val)              { return (val & 0x02) == 0x02; }
        private bool GetBit0(byte val)              { return (val & 0x01) == 0x01; }

        private bool GetZeroFlag()                  { return GetBit7(Reg_F); }
        private bool GetSubFlag()                   { return GetBit6(Reg_F); }
        private bool GetHalfCarryFlag()             { return GetBit5(Reg_F); }
        private bool GetCarryFlag()                 { return GetBit4(Reg_F); }

        private void AffectZeroFlag(bool set)       { Reg_F = set ? (byte)(Reg_F | (1 << 7)) : (byte)(Reg_F & ~(1 << 7)); }
        private void AffectSubFlag(bool set)        { Reg_F = set ? (byte)(Reg_F | (1 << 6)) : (byte)(Reg_F & ~(1 << 6)); }
        private void AffectHalfCarryFlag(bool set)  { Reg_F = set ? (byte)(Reg_F | (1 << 5)) : (byte)(Reg_F & ~(1 << 5)); }
        private void AffectCarryFlag(bool set)      { Reg_F = set ? (byte)(Reg_F | (1 << 4)) : (byte)(Reg_F & ~(1 << 4)); }
        #endregion

        #region Debug Functions
        public void Debug_LogRegisters()
        {
            Console.WriteLine("- Registers -");
            Console.WriteLine($"A:  0x{Reg_A:X2}");
            Console.WriteLine($"F:  0x{Reg_F:X2}");
            Console.WriteLine($"B:  0x{Reg_B:X2}");
            Console.WriteLine($"C:  0x{Reg_C:X2}");
            Console.WriteLine($"D:  0x{Reg_D:X2}");
            Console.WriteLine($"E:  0x{Reg_E:X2}");
            Console.WriteLine($"H:  0x{Reg_H:X2}");
            Console.WriteLine($"L:  0x{Reg_L:X2}");
            Console.WriteLine($"SP: 0x{Reg_SP:X4}");
            Console.WriteLine($"PC: 0x{Reg_PC:X4}");
            Console.WriteLine($"BC: 0x{Reg_BC:X4}");
            Console.WriteLine($"DE: 0x{Reg_DE:X4}");
            Console.WriteLine($"HL: 0x{Reg_HL:X4}");
        }
        #endregion
    }
}
