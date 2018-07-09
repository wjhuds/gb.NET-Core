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
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //10 - 1F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //20 - 2F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //30 - 3F
                null, LD_SP_D16, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //40 - 4F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //50 - 5F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //60 - 6F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //70 - 7F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //80 - 8F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //90 - 9F
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //A0 - AF
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, XOR_A,
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
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
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
            if (_verbose) Console.WriteLine($"PC: 0x{Reg_PC:X}");

            //Fetch instruction from memory using program counter
            var opCode = _mmu.ReadByte(Reg_PC);

            if (_verbose) Console.WriteLine($"OPCODE: 0x{opCode:X}");

            //Increment program counter
            Reg_PC++;

            //Execute instruction
            try
            {
                Map[opCode]();     //currently doesnt factor in cbMap
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
        //General function templates
        private void LD_SP_D16()
        {
            Reg_SP = _mmu.ReadWord(Reg_PC);
            Reg_PC += 2;
            lastOpCycles = 12;
        }

        private void XOR_A()
        {
            Reg_A ^= Reg_A;
            if (Reg_A == 0) SetZeroFlag();
            Reg_PC++;
            lastOpCycles = 4;
        }
        #endregion

        #region Helper Functions
        private void SetZeroFlag() { Reg_F |= 1 << 7; }
        private void ClearZeroFlag() { Reg_F = (byte)(Reg_F & ~(1 << 7)); }
        private void SetSubFlag() { Reg_F |= 1 << 6; }
        private void ClearSubFlag() { Reg_F = (byte)(Reg_F & ~(1 << 6)); }
        private void SetHalfCarryFlag() { Reg_F |= 1 << 5; }
        private void ClearHalfCarryFlag() { Reg_F = (byte)(Reg_F & ~(1 << 5)); }
        private void SetCarryFlag() { Reg_F |= 1 << 4; }
        private void ClearCarryFlag() { Reg_F = (byte)(Reg_F & ~(1 << 4)); }

        private bool GetZeroFlag() { return (Reg_F & 128) == 128; }
        private bool GetSubFlag() { return (Reg_F & 64) == 64; }
        private bool GetHalfCarryFlag() { return (Reg_F & 32) == 32; }
        private bool GetCarryFlag() { return (Reg_F & 16) == 16; }
        #endregion

        #region Debug Functions
        public void Debug_LogRegisters()
        {
            byte y = 1 << 7;
            byte temp = (byte)~y;
            Console.WriteLine("- Registers -");
            Console.WriteLine($"A:  0x{Reg_A:X}");
            Console.WriteLine($"F:  0x{Reg_F:X}");
            Console.WriteLine($"B:  0x{Reg_B:X}");
            Console.WriteLine($"C:  0x{Reg_C:X}");
            Console.WriteLine($"D:  0x{Reg_D:X}");
            Console.WriteLine($"E:  0x{Reg_E:X}");
            Console.WriteLine($"H:  0x{Reg_H:X}");
            Console.WriteLine($"L:  0x{Reg_L:X}");
            Console.WriteLine($"SP: 0x{Reg_SP:X}");
            Console.WriteLine($"PC: 0x{Reg_PC:X}");
            Console.WriteLine($"BC: 0x{Reg_BC:X}");
            Console.WriteLine($"DE: 0x{Reg_DE:X}");
            Console.WriteLine($"HL: 0x{Reg_HL:X}");
        }
        #endregion
    }
}
