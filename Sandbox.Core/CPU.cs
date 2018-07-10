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
                null, null, null, null, null, null, null, null, null, null, null, null, INC_C, null, LD_C_d8, null,
                //10 - 1F
                null, LD_DE_d16, null, null, null, null, null, null, null, null, LD_A_DEm, null, null, null, null, null,
                //20 - 2F
                JR_NZ_r8, LD_HL_d16, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //30 - 3F
                null, LD_SP_d16, LDD_HLm_A, null, null, null, null, null, null, null, null, null, null, null, LD_A_d8, null,
                //40 - 4F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //50 - 5F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //60 - 6F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //70 - 7F
                null, null, null, null, null, null, null, LD_HLm_A, null, null, null, null, null, null, null, null,
                //80 - 8F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //90 - 9F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //A0 - AF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, XOR_A,
                //B0 - BF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //C0 - CF
                null, null, null, null, null, null, null, null, null, null, null, CB, null, CALL_d16m, null, null,
                //D0 - DF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //E0 - EF
                LDH_d8m_A, null, LD_Cm_A, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //F0 - FF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
            };
            CbMap = new List<Action>
            {
                //00 - 0F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
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

        #region CPU_Functions
        public void Start(bool verbose)
        {
            _verbose = verbose;

            for (;;) //CPU loop for clock cycles
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
            catch(OpCodeException exception)
            {
                Console.WriteLine($"!!! OPCODE EXCEPTION: {exception.Message}");
            }
            catch(Exception exception)
            {
                Console.WriteLine($"!!! SYSTEM EXCEPTION: {exception.Message}");
                //log to file later
            }

            //Update total clock time
            totalCycles += lastOpCycles;

            //Check for interrupts
        }
        #endregion

        #region Operations
        //Main codes
        private void CALL_d16m()    //CALL a16
        {

        }
        
        private void CB()           //CB
        {
            CbMap[_mmu.ReadByte(Reg_PC++)]();
        }

        private void INC_C()        //INC C
        {
            Reg_C++;
            lastOpCycles = 4;
        }

        private void JR_NZ_r8()     //JR NZ,r8
        {
            var signed = unchecked((sbyte)_mmu.ReadByte(Reg_PC++));
            if (!GetZeroFlag())
            {
                Reg_PC = signed < 0
                    ? (ushort)(Reg_PC - (ushort)(signed * (-1)))
                    : (ushort)(Reg_PC + (ushort)signed);
                lastOpCycles = 12;
            }
            else
            {
                lastOpCycles = 8;
            }
        }

        private void LD_A_d8()      //LD A,d8
        {
            Reg_A = _mmu.ReadByte(Reg_PC++);
            lastOpCycles = 8;
        }

        private void LD_A_DEm()     //LD A,(DE)
        {
            Reg_A = _mmu.ReadByte(Reg_DE);
            lastOpCycles = 8;
        }

        private void LDH_d8m_A()    //LD (a8),A
        {
            var addr = (ushort)(0xFF00 + _mmu.ReadByte(Reg_PC++));
            _mmu.WriteByte(addr, Reg_A);
            lastOpCycles = 12;
        }

        private void LD_C_d8()      //LD C,d8
        {
            Reg_C = _mmu.ReadByte(Reg_PC++);
            lastOpCycles = 8;
        }

        private void LD_Cm_A()      //LD (CM),A
        {
            _mmu.WriteByte(Reg_C, Reg_A);
            lastOpCycles = 8;
        }

        private void LD_SP_d16()    //LD SP,d16
        {
            Reg_SP = _mmu.ReadWord(Reg_PC);
            Reg_PC += 2;
            lastOpCycles = 12;
        }

        private void LD_DE_d16()    //LD DE,d16
        {
            Reg_DE = _mmu.ReadWord(Reg_PC);
            Reg_PC += 2;
            lastOpCycles = 12;
        }

        private void LD_HL_d16()    //LD HL,d16
        {
            Reg_HL = _mmu.ReadWord(Reg_PC);
            Reg_PC += 2;
            lastOpCycles = 12;
        }

        private void LD_HLm_A()    //LD (HL),A
        {
            _mmu.WriteByte(Reg_HL, Reg_A);
            lastOpCycles = 8;
        }

        private void LDD_HLm_A()    //LD (HL-),A
        {
            _mmu.WriteByte(Reg_HL, Reg_A);
            Reg_HL--;
            lastOpCycles = 8;
        }

        private void XOR_A()        //XOR A
        {
            Reg_A ^= Reg_A;
            if (Reg_A == 0) SetZeroFlag();
            lastOpCycles = 4;
        }

        //CB codes
        private void BIT7_H()       //BIT 7,H
        {
            if (GetBit7(Reg_H)) ClearZeroFlag();
            else SetZeroFlag();
        }
        #endregion

        #region Helper Functions
        private bool GetBit7(byte val)      { return (val & 128) == 128; }
        private bool GetBit6(byte val)      { return (val & 64) == 64; }
        private bool GetBit5(byte val)      { return (val & 32) == 32; }
        private bool GetBit4(byte val)      { return (val & 16) == 16; }
        private bool GetBit3(byte val)      { return (val & 8) == 8; }
        private bool GetBit2(byte val)      { return (val & 4) == 4; }
        private bool GetBit1(byte val)      { return (val & 2) == 2; }
        private bool GetBit0(byte val)      { return (val & 1) == 1; }

        private bool GetZeroFlag()          { return GetBit7(Reg_F); }
        private bool GetSubFlag()           { return GetBit6(Reg_F); }
        private bool GetHalfCarryFlag()     { return GetBit5(Reg_F); }
        private bool GetCarryFlag()         { return GetBit4(Reg_F); }

        private void SetZeroFlag()          { Reg_F |= 1 << 7; }
        private void SetSubFlag()           { Reg_F |= 1 << 6; }
        private void SetHalfCarryFlag()     { Reg_F |= 1 << 5; }
        private void SetCarryFlag()         { Reg_F |= 1 << 4; }

        private void ClearZeroFlag()        { Reg_F = (byte)(Reg_F & ~(1 << 7)); }
        private void ClearSubFlag()         { Reg_F = (byte)(Reg_F & ~(1 << 6)); }
        private void ClearHalfCarryFlag()   { Reg_F = (byte)(Reg_F & ~(1 << 5)); }
        private void ClearCarryFlag()       { Reg_F = (byte)(Reg_F & ~(1 << 4)); }
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
