using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandbox.Core
{
    public class CPU
    {
        public readonly MMU _mmu;

        public byte ProgramCounter { get; private set; }
        public Dictionary<string, byte[]> Registers { get; private set; }
        public List<Action> Map { get; private set; }
        public List<Action> CbMap { get; private set; }

        private int remainingCycles;    //Number of clock cycles remaining for the current operation

        private bool _verbose;           //Used only for debugging purposes
        public bool continueOperation = true;

        public CPU(MMU mmu)
        {
            _mmu = mmu;

            ProgramCounter = 0;
            Registers = new Dictionary<string, byte[]>
            {
                {"A", new byte[] { 0x00 } },
                {"B", new byte[] { 0x00 } },
                {"C", new byte[] { 0x00 } },
                {"D", new byte[] { 0x00 } },
                {"E", new byte[] { 0x00 } },
                {"H", new byte[] { 0x00 } },
                {"L", new byte[] { 0x00 } },
                {"PC", new byte[] { 0x00, 0x00 } },
                {"SP", new byte[] { 0x00, 0x00 } }
            };
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
            if (_verbose) Console.WriteLine($"PC: {ProgramCounter}");

            //Fetch instruction from memory using program counter
            var opCode = _mmu.ReadByte(ProgramCounter);

            if (_verbose) Console.WriteLine($"OPCODE: {opCode}");

            //Increment program counter
            ProgramCounter++;

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
