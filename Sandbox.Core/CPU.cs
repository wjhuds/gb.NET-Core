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

            for(;;) //CPU loop for clock cycles
            {

                if (_verbose) Console.WriteLine("-- CPU Cycle --");

                Tick();

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
        private void LD_SP_D16()
        {
            Reg_HL = _mmu.ReadWord(Reg_PC);
            Reg_PC += 2;
            lastOpCycles = 12;
        }
        #endregion
    }
}
