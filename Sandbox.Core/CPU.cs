using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandbox.Core
{
    public class CPU
    {
        public readonly MMU _mmu;

        public List<Action> Map { get; private set; }
        public List<Action> CbMap { get; private set; }

        private int remainingCycles;    //Number of clock cycles remaining for the current operation

        private bool _verbose;           //Used only for debugging purposes
        public bool continueOperation = true;

        public CPU(MMU mmu)
        {
            _mmu = mmu;

            Registers.A = 0x00;
            Registers.F = 0x00;
            Registers.B = 0x00;
            Registers.C = 0x00;
            Registers.D = 0x00;
            Registers.E = 0x00;
            Registers.H = 0x00;
            Registers.L = 0x00;
            Registers.SP = 0x0000;
            Registers.PC = 0x0000;

            Map = new List<Action>
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
                if (remainingCycles == 0)
                {
                    //Check for interrupts
                    //Check for break loop
                    if (!continueOperation)
                    {
                        if (!verbose) Console.WriteLine("Shutting down CPU...");
                        break;
                    }
                }

                if (_verbose) Console.WriteLine("-- CPU Cycle --");

                Tick();
            }
        }

        public void Tick()
        {
            if (_verbose) Console.WriteLine($"PC: {Registers.PC}");

            //Fetch instruction from memory using program counter
            var opCode = _mmu.ReadByte(Registers.PC);

            if (_verbose) Console.WriteLine($"OPCODE: {opCode}");

            //Increment program counter
            Registers.PC++;

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
            
        }
        #endregion

        #region Operations
        #endregion
    }
}
