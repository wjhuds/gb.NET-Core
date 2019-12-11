using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandbox.Core
{
    public partial class CPU
    {
        public readonly MMU mmu;

        public List<Action> Map { get; private set; }
        public List<Action> CbMap { get; private set; }

        private int totalCycles;        //Number of clock cycles that have progressed since boot
        private int lastOpCycles;       //Number of clock cycles from last opCode

        private bool inHaltedState;     //Skips the tick loop while cpu is in a reduced power state
        private bool inStoppedState;    //Halts the cpu and lcd both
        private bool interruptsEnabled; //Enables or disables the check for interrupts

        private bool _verbose;          //Determines whether verbose logging is enabled
        public bool continueOperation = true;

        public void Start(bool verbose)
        {
            _verbose = verbose;

            for (; ; ) //CPU loop for clock cycles
            {
                if (_verbose) Console.WriteLine("-- CPU Loop --");

                if (!inHaltedState || !inStoppedState)
                    Tick();
                else
                    totalCycles += 4;

                if (_verbose) Debug_LogRegisters();

                //Check for interrupts

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
            var opCode = mmu.ReadByte(Reg_PC);

            if (_verbose) Console.WriteLine($"OPCODE: 0x{opCode:X2}");

            //Increment program counter
            Reg_PC++;

            //Execute instruction
            try
            {
                Map[opCode]();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"!!! SYSTEM EXCEPTION: {exception.Message}");
                //log to file later
            }

            //Update total clock time
            totalCycles += lastOpCycles;

        }

        public CPU(MMU mmu)
        {
            this.mmu = mmu;

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
                () => NOP(0x00),        //NOP
                () => LD_n_nn(0x01),     //LD BC,d16
                () => LDr_r8(0x02),     //LD (BC),A
                () => INC_16(0x03),        //INC BC
                () => INC(0x04),        //INC B
                () => DEC(0x05),        //DEC B
                () => LD_nn_n(0x06),      //LD B,d8
                () => RLCA(0x07),       //RLCA
                () => LDm_r16(0x08),    //LD (a16),SP
                () => ADD_HL(0x09),     //ADD HL,BC
                () => LD_r16m(0x0A),    //LD A,(BC)
                () => DEC(0x0B),        //DEC BC
                () => INC(0x0C),        //INC C
                () => DEC(0x0D),        //DEC C
                () => LD_nn_n(0x0E),      //LD C,d8
                null,

                //10 - 1F
                null,
                () => LD_n_nn(0x11),     //LD DE,d16
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                () => LD_r16m(0x1A),     //LD A,(DE)
                null,
                null,
                null,
                null,
                null,

                //20 - 2F
                () => JR_CC_r8(0x20),   //JR NZ,r8
                () => LD_n_nn(0x21),     //LD HL,d16
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
                () => LD_n_nn(0x31),     //LD SP,d16
                () => LDD_r8(0x32),    //LD (HL-),A
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
                () => LD_nn_n(0x3E),      //LD A,d8
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
                () => LDr_r8(0x77),       //LD (HL),A
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
                () => XOR_r8(0xAF),     //XOR A

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
                () => LDr_r8(0xE2),       //LD (C),A
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

        #region 8-Bit Loads

        // 1. LD nn,n (p.65)
        //
        // - Description -
        // Put value nn into n.
        //
        // - Use with -
        // nn = B, C, D, E, H, L, BC, DE, HL, SP
        // n = 8 bit immediate value
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           B, n        06      8
        // LD           C, n        0E      8
        // LD           D, n        16      8
        // LD           E, n        1E      8
        // LD           H, n        26      8
        // LD           L, n        2E      8
        private void LD_nn_n(byte opcode)
        {
            switch (opcode)
            {
                case 0x06:
                    Reg_B = mmu.ReadByte(Reg_PC++);
                    break;
                case 0x0E:
                    Reg_C = mmu.ReadByte(Reg_PC++);
                    break;
                case 0x3E:
                    Reg_A = mmu.ReadByte(Reg_PC++);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
        }

        // 2. LD r1,r2 (p.66)
        //
        // - Description -
        // Put value r2 into r1.
        //
        // - Use with -
        // r1, r2 = A, B, C, D, E, H L, (HL)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           A, A        7F      4
        // LD           A, B        78      4
        // LD           A, C        79      4
        // LD           A, D        7A      4
        // LD           A, E        7B      4
        // LD           A, H        7C      4
        // LD           A, L        7D      4
        // LD           A, (HL)     7E      8
        // LD           B, B        40      4
        // LD           B, C        41      4
        // LD           B, D        42      4
        // LD           B, E        43      4
        // LD           B, H        44      4
        // LD           B, L        45      4
        // LD           B, (HL)     46      8
        // LD           C, B        48      4
        // LD           C, C        49      4
        // LD           C, D        4A      4
        // LD           C, E        4B      4
        // LD           C, H        4C      4
        // LD           C, L        4D      4
        // LD           C, (HL)     4E      8
        // LD           D, B        50      4
        // LD           D, C        51      4
        // LD           D, D        52      4
        // LD           D, E        53      4
        // LD           D, H        54      4
        // LD           D, L        55      4
        // LD           D, (HL)     56      8
        // LD           E, B        58      4
        // LD           E, C        59      4
        // LD           E, D        5A      4
        // LD           E, E        5B      4
        // LD           E, H        5C      4
        // LD           E, L        5D      4
        // LD           E, (HL)     5E      8
        // LD           H, B        60      4
        // LD           H, C        61      4
        // LD           H, D        62      4
        // LD           H, E        63      4
        // LD           H, H        64      4
        // LD           H, L        65      4
        // LD           H, (HL)     66      8
        // LD           L, B        68      4
        // LD           L, C        69      4
        // LD           L, D        6A      4
        // LD           L, E        6B      4
        // LD           L, H        6C      4
        // LD           L, L        6D      4
        // LD           L, (HL)     6E      8
        // LD           (HL), B     70      8
        // LD           (HL), C     71      8
        // LD           (HL), D     72      8
        // LD           (HL), E     73      8
        // LD           (HL), H     74      8
        // LD           (HL), L     75      8
        // LD           (HL), n     36     12
        private void LD_r1_r2(byte opcode)
        {
            switch (opcode)
            {
                case 0x7F:
                    Reg_A = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x78:
                    Reg_A = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x79:
                    Reg_A = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x7A:
                    Reg_A = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x7B:
                    Reg_A = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x7C:
                    Reg_A = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x7D:
                    Reg_A = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x7E:
                    Reg_A = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x40:
                    Reg_B = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x41:
                    Reg_B = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x42:
                    Reg_B = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x43:
                    Reg_B = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x44:
                    Reg_B = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x45:
                    Reg_B = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x46:
                    Reg_B = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x48:
                    Reg_C = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x49:
                    Reg_C = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x4A:
                    Reg_C = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x4B:
                    Reg_C = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x4C:
                    Reg_C = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x4D:
                    Reg_C = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x4E:
                    Reg_C = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x50:
                    Reg_D = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x51:
                    Reg_D = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x52:
                    Reg_D = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x53:
                    Reg_D = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x54:
                    Reg_D = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x55:
                    Reg_D = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x56:
                    Reg_D = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x58:
                    Reg_E = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x59:
                    Reg_E = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x5A:
                    Reg_E = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x5B:
                    Reg_E = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x5C:
                    Reg_E = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x5D:
                    Reg_E = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x5E:
                    Reg_E = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x60:
                    Reg_H = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x61:
                    Reg_H = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x62:
                    Reg_H = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x63:
                    Reg_H = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x64:
                    Reg_H = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x65:
                    Reg_H = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x66:
                    Reg_H = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x68:
                    Reg_L = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x69:
                    Reg_L = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x6A:
                    Reg_L = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x6B:
                    Reg_L = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x6C:
                    Reg_L = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x6D:
                    Reg_L = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x6E:
                    Reg_L = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x70:
                    mmu.WriteByte(Reg_HL, Reg_B);
                    lastOpCycles = 8;
                    break;
                case 0x71:
                    mmu.WriteByte(Reg_HL, Reg_C);
                    lastOpCycles = 8;
                    break;
                case 0x72:
                    mmu.WriteByte(Reg_HL, Reg_D);
                    lastOpCycles = 8;
                    break;
                case 0x73:
                    mmu.WriteByte(Reg_HL, Reg_E);
                    lastOpCycles = 8;
                    break;
                case 0x74:
                    mmu.WriteByte(Reg_HL, Reg_H);
                    lastOpCycles = 8;
                    break;
                case 0x75:
                    mmu.WriteByte(Reg_HL, Reg_L);
                    lastOpCycles = 8;
                    break;
                case 0x36:
                    mmu.WriteByte(Reg_HL, mmu.ReadByte(Reg_PC++));
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 3. LD A,n (p.68)
        //
        // - Description -
        // Put value n into A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (BC), (DE), (HL), (nn), #
        // n = two byte immediate value. (LS byte first.)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           A, A        7F      4
        // LD           A, B        78      4
        // LD           A, C        79      4
        // LD           A, D        7A      4
        // LD           A, E        7B      4
        // LD           A, H        7C      4
        // LD           A, L        7D      4
        // LD           A, (BC)     0A      8
        // LD           A, (DE)     1A      8
        // LD           A, (HL)     7E      8
        // LD           A, (nn)     FA     16
        // LD           A, #        3E      8
        private void LD_A_n(byte opcode)
        {
            switch (opcode)
            {
                case 0x7F:
                    Reg_A = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x78:
                    Reg_A = Reg_B;
                    lastOpCycles = 4;
                    break;
                case 0x79:
                    Reg_A = Reg_C;
                    lastOpCycles = 4;
                    break;
                case 0x7A:
                    Reg_A = Reg_D;
                    lastOpCycles = 4;
                    break;
                case 0x7B:
                    Reg_A = Reg_E;
                    lastOpCycles = 4;
                    break;
                case 0x7C:
                    Reg_A = Reg_H;
                    lastOpCycles = 4;
                    break;
                case 0x7D:
                    Reg_A = Reg_L;
                    lastOpCycles = 4;
                    break;
                case 0x0A:
                    Reg_A = mmu.ReadByte(Reg_BC);
                    lastOpCycles = 8;
                    break;
                case 0x1A:
                    Reg_A = mmu.ReadByte(Reg_DE);
                    lastOpCycles = 8;
                    break;
                case 0x7E:
                    Reg_A = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0xFA:
                    Reg_A = mmu.ReadByte(mmu.ReadWord(Reg_PC));
                    Reg_PC += 2;
                    lastOpCycles = 16;
                    break;
                case 0x3E:
                    Reg_A = mmu.ReadByte(Reg_PC++);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 4. LD n,A (p.69)
        //
        // - Description -
        // Put value A into n.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (BC), (DE), (HL), (nn)
        // n = two byte immediate value. (LS byte first.)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           A, A        7F      4
        // LD           B, A        47      4
        // LD           C, A        4F      4
        // LD           D, A        57      4
        // LD           E, A        5F      4
        // LD           H, A        67      4
        // LD           L, A        6F      4
        // LD           (BC), A     02      8
        // LD           (DE), A     12      8
        // LD           (HL), A     77      8
        // LD           (nn), A     EA     16
        private void LD_n_A(byte opcode)
        {
            switch (opcode)
            {
                case 0x7F:
                    Reg_A = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x47:
                    Reg_B = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x4F:
                    Reg_C = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x57:
                    Reg_D = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x5F:
                    Reg_E = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x67:
                    Reg_H = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x6F:
                    Reg_L = Reg_A;
                    lastOpCycles = 4;
                    break;
                case 0x02:
                    mmu.WriteByte(Reg_BC, Reg_A);
                    lastOpCycles = 8;
                    break;
                case 0x12:
                    mmu.WriteByte(Reg_DE, Reg_A);
                    lastOpCycles = 8;
                    break;
                case 0x77:
                    mmu.WriteByte(Reg_HL, Reg_A);
                    lastOpCycles = 8;
                    break;
                case 0xEA:
                    mmu.WriteByte(mmu.ReadWord(Reg_PC), Reg_A);
                    Reg_PC += 2;
                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 5. LD A,(C) (p.70)
        //
        // - Description -
        // Put value value at address $FF00 + register C into A.
        // Same as: LD A, ($FF00+C)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           A, (C)      F2      8
        private void LD_A_Cm(byte opcode)
        {
            switch (opcode)
            {
                case 0xF2:
                    Reg_A = mmu.ReadByte((byte)(0xFF00 + Reg_C));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 6. LD (C),A (p.70)
        //
        // - Description -
        // Put A into address $FF00 + register C.
        //
        // - Opcodes -
        // Instruction  Parameters      Opcode  Cycles
        // LD           ($FF00+C), A    E2      8
        private void LD_Cm_A(byte opcode)
        {
            switch (opcode)
            {
                case 0xF2:
                    mmu.WriteByte((byte)(0xFF00 + Reg_C), Reg_A);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 7/8/9. LDD A,(HL) (p.71)
        //
        // - Description -
        // Put value at address HL into A. Decrement HL.
        // Same as: LD A,(HL) - DEC HL
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           A, (HLD)    3A      8
        // LD           A, (HL-)    3A      8
        // LDD          A, (HL)     3A      8
        private void LDD_A_HLm(byte opcode)
        {
            switch (opcode)
            {
                case 0x3A:
                    Reg_A = mmu.ReadByte(Reg_HL--);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 10/11/12. LDD (HL),A (p.72)
        //
        // - Description -
        // Put A into memory address HL. Decrement HL.
        // Same as: LD (HL),A - DEC HL
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           (HLD), A    32      8
        // LD           (HL-), A    32      8
        // LDD          (HL), A     32      8
        private void LDD_HLm_A(byte opcode)
        {
            switch (opcode)
            {
                case 0x32:
                    mmu.WriteByte(Reg_HL--, Reg_A);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 13/14/15. LDI A,(HL) (p.73)
        //
        // - Description -
        // Put value at address HL into A. Increment HL.
        // Same as: LD A,(HL) - INC HL
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           A, (HLI)    2A      8
        // LD           A, (HL+)    2A      8
        // LDI          A, (HL)     2A      8
        private void LDI_A_HLm(byte opcode)
        {
            switch (opcode)
            {
                case 0x3A:
                    Reg_A = mmu.ReadByte(Reg_HL++);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 16/17/18. LDI (HL),A (p.74)
        //
        // - Description -
        // Put A into memory address HL. Increment HL.
        // Same as: LD (HL),A - INC HL
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           (HLI), A    22      8
        // LD           (HL+), A    22      8
        // LDI          (HL), A     22      8
        private void LDI_HLm_A(byte opcode)
        {
            switch (opcode)
            {
                case 0x32:
                    mmu.WriteByte(Reg_HL++, Reg_A);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 19. LDH (n),A (p.75)
        //
        // - Description -
        // Put A into memory address $FF00+n.
        //
        // - Use with -
        // n = one byte immediate value.
        //
        // - Opcodes -
        // Instruction  Parameters      Opcode  Cycles
        // LD           ($FF00+n), A    E0      12
        private void LDH_nm_A(byte opcode)
        {
            switch (opcode)
            {
                case 0xE0:
                    mmu.WriteByte((ushort)(0xFF00 + mmu.ReadByte(Reg_PC++)), Reg_A);
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 20. LDH A,(n) (p.75)
        //
        // - Description -
        // Put memory address $FF00+n into A.
        //
        // - Use with -
        // n = one byte immediate value.
        //
        // - Opcodes -
        // Instruction  Parameters      Opcode  Cycles
        // LD           A, ($FF00+n)    F0      12
        private void LDH_A_nm(byte opcode)
        {
            switch (opcode)
            {
                case 0xE0:
                    Reg_A = mmu.ReadByte((byte)(0xFF00 + mmu.ReadByte(Reg_PC++)));
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        #endregion

        #region 16-Bit Loads

        // 1. LD n,nn (p.76)
        //
        // - Description -
        // Put value nn into n.
        //
        // - Use with -
        // n = BC, DE, HL, SP
        // nn = 16 bit immediate value
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           BC, n       01      12
        // LD           DE, n       11      12
        // LD           HL, n       21      12
        // LD           SP, n       31      12
        private void LD_n_nn(byte opcode)
        {
            switch (opcode)
            {
                case 0x01:
                    Reg_BC = mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
                case 0x11:
                    Reg_DE = mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
                case 0x21:
                    Reg_HL = mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
                case 0x31:
                    Reg_SP = mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 12;
        }

        // 2. LD SP,HL (p.76)
        //
        // - Description -
        // Put HL into Stack Pointer (SP).
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           SP, HL      F9      8
        private void LD_SP_HL(byte opcode)
        {
            switch (opcode)
            {
                case 0xF9:
                    Reg_SP = Reg_HL;
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 3/4. LDHL SP,n (p.77)
        //
        // - Description -
        // Put SP + n effective address into HL.
        //
        // - Use with -
        // n = one byte signed immediate value.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LDHL         SP, n       F8      12
        private void LDHL_SP_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0xF8:
                    var signedVal = Reg_PC > 127
                        ? -((~Reg_PC + 1) & 0xFF)
                        : Reg_PC;
                    Reg_PC++;
                    result = (ushort)(Reg_SP + signedVal);
                    Reg_HL = result;
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(false);
            AffectSubFlag(false);
            AffectHalfCarryFlag((((result & 0xF) + 1) & 0x10) == 0x10);
            AffectCarryFlag((((result & 0xFF) + 1) & 0x100) == 0x100);
        }

        // 5. LD (nn),SP (p.78)
        //
        // - Description -
        // Put Stack Pointer (SP) at address n.
        //
        // - Use with -
        // nn = two byte immediate address.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // LD           (nn), SP    08      20
        private void LD_nnm_SP(byte opcode)
        {
            switch(opcode)
            {
                case 0x08:
                    mmu.WriteWord((ushort)(mmu.ReadWord(Reg_PC)), Reg_SP);
                    Reg_PC += 2;
                    lastOpCycles = 20;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 6. PUSH nn (p.78)
        //
        // - Description -
        // Push register pair nn onto stack.
        // Decrement Stack Pointer (SP) twice.
        //
        // - Use with -
        // nn = AF, BC, DE, HL
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // PUSH         AF          F5      16
        // PUSH         BC          C5      16
        // PUSH         DE          D5      16
        // PUSH         HL          E5      16
        private void PUSH_nn(byte opcode)
        {
            switch (opcode)
            {
                case 0xF5:
                    mmu.WriteWord(Reg_SP, Reg_AF);
                    Reg_SP -= 2;
                    lastOpCycles = 16;
                    break;
                case 0xC5:
                    mmu.WriteWord(Reg_SP, Reg_BC);
                    Reg_SP -= 2;
                    lastOpCycles = 16;
                    break;
                case 0xD5:
                    mmu.WriteWord(Reg_SP, Reg_DE);
                    Reg_SP -= 2;
                    lastOpCycles = 16;
                    break;
                case 0xE5:
                    mmu.WriteWord(Reg_SP, Reg_HL);
                    Reg_SP -= 2;
                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 7. POP nn (p.79)
        //
        // - Description -
        // Pop two bytes off stack into register pair nn.
        // Increment Stack Pointer (SP) twice.
        //
        // - Use with -
        // nn = AF, BC, DE, HL
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // POP          AF          F1      12
        // POP          BC          C1      12
        // POP          DE          D1      12
        // POP          HL          E1      12
        private void POP_nn(byte opcode)
        {
            switch (opcode)
            {
                case 0xF1:
                    Reg_AF = mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    lastOpCycles = 12;
                    break;
                case 0xC1:
                    Reg_BC = mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    lastOpCycles = 12;
                    break;
                case 0xD1:
                    Reg_DE = mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    lastOpCycles = 12;
                    break;
                case 0xE1:
                    Reg_HL = mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        #endregion

        #region 8-Bit ALU

        // 1. ADD A,n (p.80)
        //
        // - Description -
        // Add n to A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Set if carry from bit 3.
        // C - Set if carry from bit 7.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // ADD          A, A        87      4
        // ADD          A, B        80      4
        // ADD          A, C        81      4
        // ADD          A, D        82      4
        // ADD          A, E        83      4
        // ADD          A, H        84      4
        // ADD          A, L        85      4
        // ADD          A, (HL)     86      8
        // ADD          A, #        C6      8
        private void ADD_A_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0x87:
                    result = (ushort)(Reg_A + Reg_A);
                    lastOpCycles = 4;
                    break;
                case 0x80:
                    result = (ushort)(Reg_A + Reg_B);
                    lastOpCycles = 4;
                    break;
                case 0x81:
                    result = (ushort)(Reg_A + Reg_C);
                    lastOpCycles = 4;
                    break;
                case 0x82:
                    result = (ushort)(Reg_A + Reg_D);
                    lastOpCycles = 4;
                    break;
                case 0x83:
                    result = (ushort)(Reg_A + Reg_E);
                    lastOpCycles = 4;
                    break;
                case 0x84:
                    result = (ushort)(Reg_A + Reg_H);
                    lastOpCycles = 4;
                    break;
                case 0x85:
                    result = (ushort)(Reg_A + Reg_L);
                    lastOpCycles = 4;
                    break;
                case 0x86:
                    result = (ushort)(Reg_A + mmu.ReadByte(Reg_HL));
                    lastOpCycles = 8;
                    break;
                case 0xC6:
                    result = (ushort)(Reg_A + mmu.ReadByte(Reg_PC++));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_A = (byte)result;

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag((((result & 0xF) + 1) & 0x10) == 0x10);
            AffectCarryFlag((((result & 0xFF) + 1) & 0x100) == 0x100);
        }

        // 2. ADC A,n (p.81)
        //
        // - Description -
        // Add n + Carry flag to A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Set if carry from bit 3.
        // C - Set if carry from bit 7.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // ADC          A, A        8F      4
        // ADC          A, B        88      4
        // ADC          A, C        89      4
        // ADC          A, D        8A      4
        // ADC          A, E        8B      4
        // ADC          A, H        8C      4
        // ADC          A, L        8D      4
        // ADC          A, (HL)     8E      8
        // ADC          A, #        CE      8
        private void ADC_A_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0x87:
                    result = (ushort)(Reg_A + Reg_A + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x80:
                    result = (ushort)(Reg_A + Reg_B + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x81:
                    result = (ushort)(Reg_A + Reg_C + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x82:
                    result = (ushort)(Reg_A + Reg_D + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x83:
                    result = (ushort)(Reg_A + Reg_E + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x84:
                    result = (ushort)(Reg_A + Reg_H + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x85:
                    result = (ushort)(Reg_A + Reg_L + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x86:
                    result = (ushort)(Reg_A + mmu.ReadByte(Reg_HL) + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 8;
                    break;
                case 0xC6:
                    result = (ushort)(Reg_A + mmu.ReadByte(Reg_PC++) + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_A = (byte)result;

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag((((result & 0xF) + 1) & 0x10) == 0x10);
            AffectCarryFlag((((result & 0xFF) + 1) & 0x100) == 0x100);
        }

        // 3. SUB A,n (p.82)
        //
        // - Description -
        // Subtract n from A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Set.
        // H - Set if no borrow from bit 4.
        // C - Set if no borrow.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // SUB          A, A        97      4
        // SUB          A, B        90      4
        // SUB          A, C        91      4
        // SUB          A, D        92      4
        // SUB          A, E        93      4
        // SUB          A, H        94      4
        // SUB          A, L        95      4
        // SUB          A, (HL)     96      8
        // SUB          A, #        D6      8
        private void SUB_A_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0x97:
                    result = (ushort)(Reg_A - Reg_A);
                    lastOpCycles = 4;
                    break;
                case 0x90:
                    result = (ushort)(Reg_A - Reg_B);
                    lastOpCycles = 4;
                    break;
                case 0x91:
                    result = (ushort)(Reg_A - Reg_C);
                    lastOpCycles = 4;
                    break;
                case 0x92:
                    result = (ushort)(Reg_A - Reg_D);
                    lastOpCycles = 4;
                    break;
                case 0x93:
                    result = (ushort)(Reg_A - Reg_E);
                    lastOpCycles = 4;
                    break;
                case 0x94:
                    result = (ushort)(Reg_A - Reg_H);
                    lastOpCycles = 4;
                    break;
                case 0x95:
                    result = (ushort)(Reg_A - Reg_L);
                    lastOpCycles = 4;
                    break;
                case 0x96:
                    result = (ushort)(Reg_A - mmu.ReadByte(Reg_HL));
                    lastOpCycles = 8;
                    break;
                case 0xD6:
                    result = (ushort)(Reg_A - mmu.ReadByte(Reg_PC++));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_A = (byte)result;

            AffectZeroFlag(result == 0);
            AffectSubFlag(true);
            AffectHalfCarryFlag((result & 0x10) == 0x10);
            AffectCarryFlag((result & 0x100) == 0x100);
        }

        // 4. SBC A,n (p.83)
        //
        // - Description -
        // Subtract n + Carry flag from A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Set.
        // H - Set if no borrow from bit 4.
        // C - Set if no borrow.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // SBC          A, A        9F      4
        // SBC          A, B        98      4
        // SBC          A, C        99      4
        // SBC          A, D        9A      4
        // SBC          A, E        9B      4
        // SBC          A, H        9C      4
        // SBC          A, L        9D      4
        // SBC          A, (HL)     9E      8
        // SBC          A, #        DE      8
        private void SBC_A_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0x87:
                    result = (ushort)(Reg_A - Reg_A + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x80:
                    result = (ushort)(Reg_A - Reg_B + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x81:
                    result = (ushort)(Reg_A - Reg_C + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x82:
                    result = (ushort)(Reg_A - Reg_D + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x83:
                    result = (ushort)(Reg_A - Reg_E + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x84:
                    result = (ushort)(Reg_A - Reg_H + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x85:
                    result = (ushort)(Reg_A - Reg_L + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 4;
                    break;
                case 0x86:
                    result = (ushort)(Reg_A - mmu.ReadByte(Reg_HL) + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 8;
                    break;
                case 0xC6:
                    result = (ushort)(Reg_A - mmu.ReadByte(Reg_PC++) + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_A = (byte)result;

            AffectZeroFlag(result == 0);
            AffectSubFlag(true);
            AffectHalfCarryFlag((result & 0x10) == 0x10);
            AffectCarryFlag((result & 0x100) == 0x100);
        }

        // 5. AND n (p.84)
        //
        // - Description -
        // Logically AND n with A, result in A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Set.
        // C - Reset.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // AND          A           A7      4
        // AND          B           A0      4
        // AND          C           A1      4
        // AND          D           A2      4
        // AND          E           A3      4
        // AND          H           A4      4
        // AND          L           A5      4
        // AND          (HL)        A6      8
        // AND          #           E6      8
        private void AND_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0xA7:
                    result = (ushort)(Reg_A + Reg_A);
                    lastOpCycles = 4;
                    break;
                case 0xA0:
                    result = (ushort)(Reg_A + Reg_B);
                    lastOpCycles = 4;
                    break;
                case 0xA1:
                    result = (ushort)(Reg_A + Reg_C);
                    lastOpCycles = 4;
                    break;
                case 0xA2:
                    result = (ushort)(Reg_A + Reg_D);
                    lastOpCycles = 4;
                    break;
                case 0xA3:
                    result = (ushort)(Reg_A + Reg_E);
                    lastOpCycles = 4;
                    break;
                case 0xA4:
                    result = (ushort)(Reg_A + Reg_H);
                    lastOpCycles = 4;
                    break;
                case 0xA5:
                    result = (ushort)(Reg_A + Reg_L);
                    lastOpCycles = 4;
                    break;
                case 0xA6:
                    result = (ushort)(Reg_A + mmu.ReadByte(Reg_HL));
                    lastOpCycles = 8;
                    break;
                case 0xE6:
                    result = (ushort)(Reg_A + mmu.ReadByte(Reg_PC++));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_A = (byte)result;

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(true);
            AffectCarryFlag(false);
        }

        // 6. OR n (p.85)
        //
        // - Description -
        // Logical OR n with register A, result in A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Reset.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // OR           A           B7      4
        // OR           B           B0      4
        // OR           C           B1      4
        // OR           D           B2      4
        // OR           E           B3      4
        // OR           H           B4      4
        // OR           L           B5      4
        // OR           (HL)        B6      8
        // OR           #           F6      8
        private void OR_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0xB7:
                    result = (ushort)(Reg_A | Reg_A);
                    lastOpCycles = 4;
                    break;
                case 0xB0:
                    result = (ushort)(Reg_A | Reg_B);
                    lastOpCycles = 4;
                    break;
                case 0xB1:
                    result = (ushort)(Reg_A | Reg_C);
                    lastOpCycles = 4;
                    break;
                case 0xB2:
                    result = (ushort)(Reg_A | Reg_D);
                    lastOpCycles = 4;
                    break;
                case 0xB3:
                    result = (ushort)(Reg_A | Reg_E);
                    lastOpCycles = 4;
                    break;
                case 0xB4:
                    result = (ushort)(Reg_A | Reg_H);
                    lastOpCycles = 4;
                    break;
                case 0xB5:
                    result = (ushort)(Reg_A | Reg_L);
                    lastOpCycles = 4;
                    break;
                case 0xB6:
                    result = (ushort)(Reg_A | mmu.ReadByte(Reg_HL));
                    lastOpCycles = 8;
                    break;
                case 0xF6:
                    result = (ushort)(Reg_A | mmu.ReadByte(Reg_PC++));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_A = (byte)result;

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(false);
        }

        // 7. XOR n (p.86)
        //
        // - Description -
        // Logical exclusive OR n with register A, result in A.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Reset.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // XOR          A           AF      4
        // XOR          B           A8      4
        // XOR          C           A9      4
        // XOR          D           AA      4
        // XOR          E           AB      4
        // XOR          H           AC      4
        // XOR          L           AD      4
        // XOR          (HL)        AE      8
        // XOR          #           EE      8
        private void XOR_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0xAF:
                    result = (ushort)(Reg_A ^ Reg_A);
                    lastOpCycles = 4;
                    break;
                case 0xA8:
                    result = (ushort)(Reg_A ^ Reg_B);
                    lastOpCycles = 4;
                    break;
                case 0xA9:
                    result = (ushort)(Reg_A ^ Reg_C);
                    lastOpCycles = 4;
                    break;
                case 0xAA:
                    result = (ushort)(Reg_A ^ Reg_D);
                    lastOpCycles = 4;
                    break;
                case 0xAB:
                    result = (ushort)(Reg_A ^ Reg_E);
                    lastOpCycles = 4;
                    break;
                case 0xAC:
                    result = (ushort)(Reg_A ^ Reg_H);
                    lastOpCycles = 4;
                    break;
                case 0xAD:
                    result = (ushort)(Reg_A ^ Reg_L);
                    lastOpCycles = 4;
                    break;
                case 0xAE:
                    result = (ushort)(Reg_A ^ mmu.ReadByte(Reg_HL));
                    lastOpCycles = 8;
                    break;
                case 0xEE:
                    result = (ushort)(Reg_A ^ mmu.ReadByte(Reg_PC++));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_A = (byte)result;

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(false);
        }

        // 8. CP n (p.87)
        //
        // - Description -
        // Compare A with n. This is basically an A - n
        // subtraction instruction but the results are thrown
        // away.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL), #
        //
        // - Flags affected -
        // Z - Set if result is zero. (Set if A = n)
        // N - Set.
        // H - Set if no borrow from bit 4.
        // C - Set for no borrow. (Set if A < n)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // CP           A           BF      4
        // CP           B           B8      4
        // CP           C           B9      4
        // CP           D           BA      4
        // CP           E           BB      4
        // CP           H           BC      4
        // CP           L           BD      4
        // CP           (HL)        BE      8
        // CP           #           FE      8
        private void CP_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0xBF:
                    result = (ushort)(Reg_A - Reg_A);
                    lastOpCycles = 4;
                    break;
                case 0xB8:
                    result = (ushort)(Reg_A - Reg_B);
                    lastOpCycles = 4;
                    break;
                case 0xB9:
                    result = (ushort)(Reg_A - Reg_C);
                    lastOpCycles = 4;
                    break;
                case 0xBA:
                    result = (ushort)(Reg_A - Reg_D);
                    lastOpCycles = 4;
                    break;
                case 0xBB:
                    result = (ushort)(Reg_A - Reg_E);
                    lastOpCycles = 4;
                    break;
                case 0xBC:
                    result = (ushort)(Reg_A - Reg_H);
                    lastOpCycles = 4;
                    break;
                case 0xBD:
                    result = (ushort)(Reg_A - Reg_L);
                    lastOpCycles = 4;
                    break;
                case 0xBE:
                    result = (ushort)(Reg_A - mmu.ReadByte(Reg_HL));
                    lastOpCycles = 8;
                    break;
                case 0xFE:
                    result = (ushort)(Reg_A - mmu.ReadByte(Reg_PC++));
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(true);
            AffectHalfCarryFlag((result & 0x10) == 0x10);
            AffectCarryFlag((result & 0x100) == 0x100);
        }

        // 9. INC n (p.88)
        //
        // - Description -
        // Increment register n.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Set if carry from bit 3.
        // C - Not affected.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // INC          A           3C      4
        // INC          B           04      4
        // INC          C           0C      4
        // INC          D           14      4
        // INC          E           1C      4
        // INC          H           24      4
        // INC          L           2C      4
        // INC          (HL)        34      12
        private void INC_n(byte opcode)
        {
            ushort original;
            switch (opcode)
            {
                case 0x3C:
                    original = Reg_A++;
                    lastOpCycles = 4;
                    break;
                case 0x04:
                    original = Reg_B++;
                    lastOpCycles = 4;
                    break;
                case 0x0C:
                    original = Reg_C++;
                    lastOpCycles = 4;
                    break;
                case 0x14:
                    original = Reg_D++;
                    lastOpCycles = 4;
                    break;
                case 0x1C:
                    original = Reg_E++;
                    lastOpCycles = 4;
                    break;
                case 0x24:
                    original = Reg_H++;
                    lastOpCycles = 4;
                    break;
                case 0x2C:
                    original = Reg_L++;
                    lastOpCycles = 4;
                    break;
                case 0x34:
                    original = mmu.ReadByte(Reg_HL);
                    mmu.WriteByte(Reg_HL, (byte)(mmu.ReadByte(Reg_HL) + 1));
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag((byte)(original + 1) == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(((byte)(original + 1) & 0x10) == 0x10);
        }

        // 10. DEC n (p.89)
        //
        // - Description -
        // Decrement register n.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Set.
        // H - Set if borrow from bit 4.
        // C - Not affected.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // DEC          A           3D      4
        // DEC          B           05      4
        // DEC          C           0D      4
        // DEC          D           15      4
        // DEC          E           1D      4
        // DEC          H           25      4
        // DEC          L           2D      4
        // DEC          (HL)        35      12
        private void DEC_n(byte opcode)
        {
            ushort original;
            switch (opcode)
            {
                case 0x3D:
                    original = Reg_A--;
                    lastOpCycles = 4;
                    break;
                case 0x05:
                    original = Reg_B--;
                    lastOpCycles = 4;
                    break;
                case 0x0D:
                    original = Reg_C--;
                    lastOpCycles = 4;
                    break;
                case 0x15:
                    original = Reg_D--;
                    lastOpCycles = 4;
                    break;
                case 0x1D:
                    original = Reg_E--;
                    lastOpCycles = 4;
                    break;
                case 0x25:
                    original = Reg_H--;
                    lastOpCycles = 4;
                    break;
                case 0x2D:
                    original = Reg_L--;
                    lastOpCycles = 4;
                    break;
                case 0x35:
                    original = mmu.ReadByte(Reg_HL);
                    mmu.WriteByte(Reg_HL, (byte)(mmu.ReadByte(Reg_HL) - 1));
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag((byte)(original - 1) == 0);
            AffectSubFlag(true);
            AffectHalfCarryFlag(((byte)(original - 1) & 0x10) == 0x10);
        }

        #endregion

        #region 16-Bit Arithmetic

        // 1. ADD HL,n (p.90)
        //
        // - Description -
        // Add n to HL.
        //
        // - Use with -
        // n = BC, DE, HL, SP
        //
        // - Flags affected -
        // Z - Not affected.
        // N - Reset.
        // H - Set if borrow from bit 11.
        // C - Set if borrow from bit 15.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // ADD          HL, BC      09      8
        // ADD          HL, DE      19      8
        // ADD          HL, HL      29      8
        // ADD          HL, SP      39      8
        private void ADD_HL_n(byte opcode)
        {
            uint result;
            switch (opcode)
            {
                case 0x09:
                    result = (uint)(Reg_HL + Reg_BC);
                    lastOpCycles = 8;
                    break;
                case 0x19:
                    result = (uint)(Reg_HL + Reg_DE);
                    lastOpCycles = 8;
                    break;
                case 0x29:
                    result = (uint)(Reg_HL + Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x39:
                    result = (uint)(Reg_HL + Reg_SP);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_HL = (ushort)result;

            AffectSubFlag(false);
            AffectHalfCarryFlag((((result & 0xFFF) + 1) & 0x1000) == 0x1000);
            AffectCarryFlag((((result & 0xFFFF) + 1) & 0x10000) == 0x10000);
        }

        // 2. ADD SP,n (p.91)
        //
        // - Description -
        // Add n to SP.
        //
        // - Use with -
        // n = one byte signed immediate value (#).
        //
        // - Flags affected -
        // Z - Reset.
        // N - Reset.
        // H - Set or reset according to operation.
        // C - Set or reset according to operation.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // ADD          SP, #       E8      16
        private void ADD_SP_n(byte opcode)
        {
            uint result;
            switch (opcode)
            {
                case 0xE8:
                    var signedVal = Reg_PC > 127
                        ? -((~Reg_PC + 1) & 0xFF)
                        : Reg_PC;
                    Reg_PC++;
                    result = (uint)(Reg_SP + signedVal);
                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            Reg_SP = (ushort)result;

            AffectZeroFlag(false);
            AffectSubFlag(false);
            AffectHalfCarryFlag((((result & 0xFFF) + 1) & 0x1000) == 0x1000);
            AffectCarryFlag((((result & 0xFFFF) + 1) & 0x10000) == 0x10000);
        }

        // 3. INC nn (p.92)
        //
        // - Description -
        // Increment register nn.
        //
        // - Use with -
        // n = BC, DE, HL, SP
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // INC          BC          03      8
        // INC          DE          13      8
        // INC          HL          23      8
        // INC          SP          33      8
        private void INC_nn(byte opcode)
        {
            switch (opcode)
            {
                case 0x03:
                    Reg_BC++;
                    lastOpCycles = 8;
                    break;
                case 0x13:
                    Reg_DE++;
                    lastOpCycles = 8;
                    break;
                case 0x23:
                    Reg_HL++;
                    lastOpCycles = 8;
                    break;
                case 0x33:
                    Reg_SP++;
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 4. DEC nn (p.93)
        //
        // - Description -
        // Decrement register nn.
        //
        // - Use with -
        // n = BC, DE, HL, SP
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // DEC          BC          0B      8
        // DEC          DE          1B      8
        // DEC          HL          2B      8
        // DEC          SP          3B      8
        private void DEC_nn(byte opcode)
        {
            switch (opcode)
            {
                case 0x0B:
                    Reg_BC--;
                    lastOpCycles = 8;
                    break;
                case 0x1B:
                    Reg_DE--;
                    lastOpCycles = 8;
                    break;
                case 0x2B:
                    Reg_HL--;
                    lastOpCycles = 8;
                    break;
                case 0x3B:
                    Reg_SP--;
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        #endregion

        #region Miscellaneous

        // 1. SWAP n (p.94)
        //
        // - Description -
        // Swap upper & lower nybbles of n.
        //
        // - Use with -
        // n = A, B, C, D, E, H, L, (HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.7 = GetBit7(Reg_A);ycles
        // SWAP         A           CB 37   8
        // SWAP         B           CB 30   8
        // SWAP         C           CB 31   8
        // SWAP         D           CB 32   8
        // SWAP         E           CB 33   8
        // SWAP         H           CB 34   8
        // SWAP         L           CB 35   8
        // SWAP         (HL)        CB 36   16
        private void SWAP_n(byte opcode)
        {
            ushort result;
            switch (opcode)
            {
                case 0x37:
                    result = (ushort)((Reg_A & 0x0F) << 4 | (Reg_A & 0xF0));
                    Reg_A = (byte)result;
                    lastOpCycles = 8;
                    break;
                case 0x30:
                    result = (ushort)((Reg_B & 0x0F) << 4 | (Reg_B & 0xF0));
                    Reg_B = (byte)result;
                    lastOpCycles = 8;
                    break;
                case 0x31:
                    result = (ushort)((Reg_C & 0x0F) << 4 | (Reg_C & 0xF0));
                    Reg_C = (byte)result;
                    lastOpCycles = 8;
                    break;
                case 0x32:
                    result = (ushort)((Reg_D & 0x0F) << 4 | (Reg_D & 0xF0));
                    Reg_D = (byte)result;
                    lastOpCycles = 8;
                    break;
                case 0x33:
                    result = (ushort)((Reg_E & 0x0F) << 4 | (Reg_E & 0xF0));
                    Reg_E = (byte)result;
                    lastOpCycles = 8;
                    break;
                case 0x34:
                    result = (ushort)((Reg_H & 0x0F) << 4 | (Reg_H & 0xF0));
                    Reg_H = (byte)result;
                    lastOpCycles = 8;
                    break;
                case 0x35:
                    result = (ushort)((Reg_L & 0x0F) << 4 | (Reg_L & 0xF0));
                    Reg_L = (byte)result;
                    lastOpCycles = 8;
                    break;
                case 0x36:
                    var dataFromMem = mmu.ReadByte(Reg_HL);
                    result = (ushort)((dataFromMem & 0x0F) << 4 | (dataFromMem & 0xF0));
                    mmu.WriteByte(Reg_HL, (byte)result);
                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(false);
        }

        // 2. DAA (p.95)
        //
        // - Description -
        // Decimal adjust register A.
        // This instruction adjusts register A so that the
        // correct representation of Binary Coded Decimal (BCD)
        // is obtained.
        //
        // - Flags affected -
        // Z - Set if register A is zero.
        // N - Not affected.
        // H - Reset.
        // C - Set or reset according to operation.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // DAA          -/-         27      4
        //
        // Implementation borrowed from https://ehaskins.com/2018-01-30%20Z80%20DAA/
        private void DAA(byte opcode)
        {
            byte result;
            bool setCarryFlag = false;
            switch (opcode)
            {
                case 0x27:
                    byte correction = 0;
                    if (GetHalfCarryFlag() || (!GetSubFlag() && (Reg_A & 0xF) > 9))
                    {
                        correction |= 0x6;
                    }
                    if (GetCarryFlag() || (!GetSubFlag() && Reg_A > 0x99))
                    {
                        correction |= 0x60;
                        setCarryFlag = GetCarryFlag();
                    }
                    result = (byte)(Reg_A + (GetSubFlag() ? -correction : correction));
                    Reg_A = result;
                    lastOpCycles = 4;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(setCarryFlag);
        }

        // 3. CPL (p.95)
        //
        // - Description -
        // Complement A register. (Flip all bits.)
        //
        // - Flags affected -
        // Z - Not affected.
        // N - Set.
        // H - Set.
        // C - Not affected.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // CPL          -/-         2F      4
        private void CPL(byte opcode)
        {
            byte result;
            switch (opcode)
            {
                case 0x2F:
                    result = (byte)~Reg_A;
                    Reg_A = result;
                    lastOpCycles = 4;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectSubFlag(true);
            AffectHalfCarryFlag(true);
        }

        // 4. CCF (p.96)
        //
        // - Description -
        // Complement carry flag.
        // If C flag is set, then reset it.
        // If C flag is reset, then set it.
        //
        // - Flags affected -
        // Z - Not affected.
        // N - Reset.
        // H - Reset.
        // C - Complemented.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // CCF          -/-         3F      4
        private void CCF(byte opcode)
        {
            lastOpCycles = 4;

            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(!GetCarryFlag());
        }

        // 5. SCF (p.96)
        //
        // - Description -
        // Set carry flag.
        //
        // - Flags affected -
        // Z - Not affected.
        // N - Reset.
        // H - Reset.
        // C - Set.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // SCF          -/-         37      4
        private void SCF(byte opcode)
        {
            lastOpCycles = 4;

            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(true);
        }

        // 6. NOP (p.97)
        //
        // - Description -
        // No operation.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // NOP          -/-         00      4
        private void NOP(byte opcode)
        {
            lastOpCycles = 4;
        }

        // 7. HALT (p.97)
        //
        // - Description -
        // Power down CPU until an interrupt occurs. Use this
        // whenever possible to reduce energy consumption.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // HALT         -/-         76      4
        private void HALT(byte opcode)
        {
            lastOpCycles = 4;
            inHaltedState = true;
        }

        // 8. STOP (p.97)
        //
        // - Description -
        // Halt CPU & LCD display until button pressed.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // STOP         -/-         10 00   4
        private void STOP(byte opcode)
        {
            lastOpCycles = 4;
            inHaltedState = true;
            inStoppedState = true;
        }

        // 9. DI (p.98)
        //
        // - Description -
        // This instruction disables interrups but not
        // immediately. Interrupts are disabled after
        // instruction following DI is executed.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // DI           -/-         F3      4
        private void DI(byte opcode)
        {
            lastOpCycles = 4;
            interruptsEnabled = false;
        }

        // 10. EI (p.98)
        //
        // - Description -
        // Enable interrupts. This instruction enables interrupts
        // but not immediately. Interrupts are enabled after
        // instruction following EI is executed.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // EI           -/-         F3      4
        private void EI(byte opcode)
        {
            lastOpCycles = 4;
            interruptsEnabled = true;
        }

        #endregion

        #region Rotates & Shifts

        // 1. RLCA (p.99)
        //
        // - Description -
        // Rotate A left. Old bit 7 to Carry flag.
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 7 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RLCA         -/-         07      4
        private void RLCA(byte opcode)
        {
            bool bit7;
            byte result;
            switch (opcode)
            {
                case 0x07:
                    bit7 = GetBit7(Reg_A);

                    result = (byte)(Reg_A << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 4;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit7);
        }

        // 2. RLA (p.99)
        //
        // - Description -
        // Rotate A left through Carry flag.
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 7 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RLA          -/-         17      4
        private void RLA(byte opcode)
        {
            bool bit7;
            byte result;
            switch (opcode)
            {
                case 0x17:
                    bit7 = GetBit7(Reg_A);

                    result = (byte)(Reg_A << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 4;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit7);
        }

        // 3. RRCA (p.100)
        //
        // - Description -
        // Rotate A right. Old bit 0 to Carry flag.
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 0 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RRCA         -/-         0F      4
        private void RRCA(byte opcode)
        {
            bool bit0;
            byte result;
            switch (opcode)
            {
                case 0x0F:
                    bit0 = GetBit0(Reg_A);

                    result = (byte)(Reg_A >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 4;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit0);
        }

        // 4. RRA (p.100)
        //
        // - Description -
        // Rotate A right through Carry flag.
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 0 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RRA          -/-         1F      4
        private void RRA(byte opcode)
        {
            bool bit0;
            byte result;
            switch (opcode)
            {
                case 0x0F:
                    bit0 = GetBit0(Reg_A);

                    result = (byte)(Reg_A >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 4;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit0);
        }

        // 5. RLC n (p.101)
        //
        // - Description -
        // Rotate n left. Old bit 7 to Carry flag.
        //
        // - Use with -
        // n = A,B,C,D,E,H,L,(HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 7 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RLC          A           CB 07   8
        // RLC          B           CB 00   8
        // RLC          C           CB 01   8
        // RLC          D           CB 02   8
        // RLC          E           CB 03   8
        // RLC          H           CB 04   8
        // RLC          L           CB 05   8
        // RLC          (HL)        CB 06   16
        private void RLC_n(byte opcode)
        {
            bool bit7;
            byte result;
            switch (opcode)
            {
                case 0x07:
                    bit7 = GetBit7(Reg_A);

                    result = (byte)(Reg_A << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x00:
                    bit7 = GetBit7(Reg_B);

                    result = (byte)(Reg_B << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x01:
                    bit7 = GetBit7(Reg_C);

                    result = (byte)(Reg_C << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x02:
                    bit7 = GetBit7(Reg_D);

                    result = (byte)(Reg_D << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x03:
                    bit7 = GetBit7(Reg_E);

                    result = (byte)(Reg_E << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x04:
                    bit7 = GetBit7(Reg_H);

                    result = (byte)(Reg_H << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x05:
                    bit7 = GetBit7(Reg_L);

                    result = (byte)(Reg_L << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x06:
                    result = mmu.ReadByte(Reg_HL);
                    bit7 = GetBit7(result);

                    result = (byte)(result << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    mmu.WriteByte(Reg_HL, result);

                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit7);
        }

        // 6. RL n (p.102)
        //
        // - Description -
        // Rotate n left through Carry flag.
        //
        // - Use with -
        // n = A,B,C,D,E,H,L,(HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 7 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RL           A           CB 17   8
        // RL           B           CB 10   8
        // RL           C           CB 11   8
        // RL           D           CB 12   8
        // RL           E           CB 13   8
        // RL           H           CB 14   8
        // RL           L           CB 15   8
        // RL           (HL)        CB 16   16
        private void Rl_n(byte opcode)
        {
            bool bit7;
            byte result;
            switch (opcode)
            {
                case 0x17:
                    bit7 = GetBit7(Reg_A);

                    result = (byte)(Reg_A << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x10:
                    bit7 = GetBit7(Reg_B);

                    result = (byte)(Reg_B << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x11:
                    bit7 = GetBit7(Reg_C);

                    result = (byte)(Reg_C << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x12:
                    bit7 = GetBit7(Reg_D);

                    result = (byte)(Reg_D << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x13:
                    bit7 = GetBit7(Reg_E);

                    result = (byte)(Reg_E << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x14:
                    bit7 = GetBit7(Reg_H);

                    result = (byte)(Reg_H << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x15:
                    bit7 = GetBit7(Reg_L);

                    result = (byte)(Reg_L << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x16:
                    result = mmu.ReadByte(Reg_HL);
                    bit7 = GetBit7(result);

                    result = (byte)(result << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    mmu.WriteByte(Reg_HL, result);

                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit7);
        }

        // 7. RRC n (p.103)
        //
        // - Description -
        // Rotate n right. Old bit 0 to Carry flag.
        //
        // - Use with -
        // n = A,B,C,D,E,H,L,(HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 0 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RRC          A           CB 0F   8
        // RRC          B           CB 08   8
        // RRC          C           CB 09   8
        // RRC          D           CB 0A   8
        // RRC          E           CB 0B   8
        // RRC          H           CB 0C   8
        // RRC          L           CB 0D   8
        // RRC          (HL)        CB 0E   16
        private void RRC_n(byte opcode)
        {
            bool bit0;
            byte result;
            switch (opcode)
            {
                case 0x0F:
                    bit0 = GetBit0(Reg_A);

                    result = (byte)(Reg_A >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x08:
                    bit0 = GetBit0(Reg_B);

                    result = (byte)(Reg_B >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x09:
                    bit0 = GetBit0(Reg_C);

                    result = (byte)(Reg_C >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x0A:
                    bit0 = GetBit0(Reg_D);

                    result = (byte)(Reg_D >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x0B:
                    bit0 = GetBit0(Reg_E);

                    result = (byte)(Reg_E >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x0C:
                    bit0 = GetBit0(Reg_H);

                    result = (byte)(Reg_H >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x0D:
                    bit0 = GetBit0(Reg_L);

                    result = (byte)(Reg_L >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x0E:
                    result = mmu.ReadByte(Reg_HL);
                    bit0 = GetBit0(result);

                    result = (byte)(result >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    mmu.WriteByte(Reg_HL, result);

                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit0);
        }

        // 8. RR n (p.104)
        //
        // - Description -
        // Rotate n right through Carry flag.
        //
        // - Use with -
        // n = A,B,C,D,E,H,L,(HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 0 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RR           A           CB 1F   8
        // RR           B           CB 18   8
        // RR           C           CB 19   8
        // RR           D           CB 1A   8
        // RR           E           CB 1B   8
        // RR           H           CB 1C   8
        // RR           L           CB 1D   8
        // RR           (HL)        CB 1E   16
        private void RR_n(byte opcode)
        {
            bool bit0;
            byte result;
            switch (opcode)
            {
                case 0x0F:
                    bit0 = GetBit0(Reg_A);

                    result = (byte)(Reg_A >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x08:
                    bit0 = GetBit0(Reg_B);

                    result = (byte)(Reg_B >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x09:
                    bit0 = GetBit0(Reg_C);

                    result = (byte)(Reg_C >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x0A:
                    bit0 = GetBit0(Reg_D);

                    result = (byte)(Reg_D >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x0B:
                    bit0 = GetBit0(Reg_E);

                    result = (byte)(Reg_E >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x0C:
                    bit0 = GetBit0(Reg_H);

                    result = (byte)(Reg_H >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x0D:
                    bit0 = GetBit0(Reg_L);

                    result = (byte)(Reg_L >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x0E:
                    result = mmu.ReadByte(Reg_HL);
                    bit0 = GetBit0(result);

                    result = (byte)(result >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    mmu.WriteByte(Reg_HL, result);

                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit0);
        }

        // 9. SLA n (p.105)
        //
        // - Description -
        // Shift n left into Carry. LSB of n set to 0.
        //
        // - Use with -
        // n = A,B,C,D,E,H,L,(HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 7 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // SLA           A           CB 27   8
        // SLA           B           CB 20   8
        // SLA           C           CB 21   8
        // SLA           D           CB 22   8
        // SLA           E           CB 23   8
        // SLA           H           CB 24   8
        // SLA           L           CB 25   8
        // SLA           (HL)        CB 26   16
        private void SLA_n(byte opcode)
        {
            bool bit7;
            byte result;
            switch (opcode)
            {
                case 0x27:
                    bit7 = GetBit7(Reg_A);

                    result = (byte)(Reg_A << 1);
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x20:
                    bit7 = GetBit7(Reg_B);

                    result = (byte)(Reg_B << 1);
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x21:
                    bit7 = GetBit7(Reg_C);

                    result = (byte)(Reg_C << 1);
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x22:
                    bit7 = GetBit7(Reg_D);

                    result = (byte)(Reg_D << 1);
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x23:
                    bit7 = GetBit7(Reg_E);

                    result = (byte)(Reg_E << 1);
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x24:
                    bit7 = GetBit7(Reg_H);

                    result = (byte)(Reg_H << 1);
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x25:
                    bit7 = GetBit7(Reg_L);

                    result = (byte)(Reg_L << 1);
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x26:
                    result = mmu.ReadByte(Reg_HL);
                    bit7 = GetBit7(result);

                    result = (byte)(result << 1);
                    mmu.WriteByte(Reg_HL, result);

                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit7);
        }

        // 10. SRA n (p.106)
        //
        // - Description -
        // Shift n right into Carry. MSB doesn't change.
        //
        // - Use with -
        // n = A,B,C,D,E,H,L,(HL)
        //
        // - Flags affected -
        // Z - Set if result is zero.
        // N - Reset.
        // H - Reset.
        // C - Contains old bit 0 data.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // SRA           A           CB 2F   8
        // SRA           B           CB 28   8
        // SRA           C           CB 29   8
        // SRA           D           CB 2A   8
        // SRA           E           CB 2B   8
        // SRA           H           CB 2C   8
        // SRA           L           CB 2D   8
        // SRA           (HL)        CB 2E   16
        private void SRA_n(byte opcode)
        {
            bool bit0;
            byte result;
            switch (opcode)
            {
                case 0x2F:
                    bit0 = GetBit0(Reg_A);

                    result = (byte)(Reg_A >> 1);
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x28:
                    bit0 = GetBit0(Reg_B);

                    result = (byte)(Reg_B >> 1);
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x29:
                    bit0 = GetBit0(Reg_C);

                    result = (byte)(Reg_C >> 1);
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x2A:
                    bit0 = GetBit0(Reg_D);

                    result = (byte)(Reg_D >> 1);
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x2B:
                    bit0 = GetBit0(Reg_E);

                    result = (byte)(Reg_E >> 1);
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x2C:
                    bit0 = GetBit0(Reg_H);

                    result = (byte)(Reg_H >> 1);
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x2D:
                    bit0 = GetBit0(Reg_L);

                    result = (byte)(Reg_L >> 1);
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x2E:
                    result = mmu.ReadByte(Reg_HL);
                    bit0 = GetBit0(result);

                    result = (byte)(result >> 1);
                    mmu.WriteByte(Reg_HL, result);

                    lastOpCycles = 16;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag(result == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(bit0);
        }

        #endregion

        //Load into register1 from memory address stored in register2 (DP, 65)
        private void LD_r16m(byte opcode)
        {
            switch (opcode)
            {
                case 0x0A:
                    Reg_A = mmu.ReadByte(Reg_BC);
                    break;
                case 0x1A:
                    Reg_A = mmu.ReadByte(Reg_DE);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
            return;
        }

        //Load value into memory address given by next word in memory
        private void LDm_r16(byte opcode)
        {
            switch (opcode)
            {
                case 0x08:
                    var addr = (ushort)(mmu.ReadWord(Reg_PC));
                    Reg_PC += 2;
                    mmu.WriteWord(addr, Reg_SP);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 20;
        }

        //Load value of register1 to register2 and decrement register2 (DP, 71)
        private void LDD_r8(byte opcode)
        {
            switch (opcode)
            {
                case 0x32:
                    mmu.WriteByte(Reg_HL, Reg_A);
                    Reg_HL--;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
            return;
        }

        //Load register1 into memory address (0xFF00 + register2) (DP, 65)
        private void LDr_r8(byte opcode)
        {
            switch (opcode)
            {
                case 0x02:
                    mmu.WriteByte(Reg_BC, Reg_A);
                    break;
                case 0x77:
                    mmu.WriteByte(Reg_HL, Reg_A);
                    break;
                case 0xE2:
                    mmu.WriteByte(Reg_C, Reg_A);
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
                    var addr = (ushort)(0xFF00 + mmu.ReadByte(Reg_PC++));
                    mmu.WriteByte(addr, Reg_A);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 12;
        }

        //8-bit ALU Increment (DP, 88)
        private void INC(byte opcode)
        {
            ushort val;
            switch (opcode)
            {
                case 0x04:
                    val = Reg_B;
                    Reg_B++;
                    break;
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

        //16-bit ALU Increment (DP, 92)
        private void INC_16(byte opcode)
        {
            ushort val;
            switch (opcode)
            {
                case 0x03:
                    val = Reg_BC;
                    Reg_BC++;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
            return;
        }

        //8-bit ALU Decrement (DP, 89)
        private void DEC(byte opcode)
        {
            ushort val;
            switch (opcode)
            {
                case 0x05:
                    val = Reg_B;
                    Reg_B--;
                    break;
                case 0x0B:
                    val = Reg_BC;
                    Reg_BC--;
                    break;
                case 0x0D:
                    val = Reg_D;
                    Reg_D--;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag((val - 1) == 0);
            AffectSubFlag(true);
            AffectHalfCarryFlag((val & 0xF) - 1 < 0);
        }

        //16-bit ALU Decrement (DP, 93)
        private void DEC_16(byte opcode)
        {
            ushort val;
            switch (opcode)
            {
                case 0x05:
                    val = Reg_B;
                    Reg_B--;
                    break;
                case 0x0B:
                    val = Reg_BC;
                    Reg_BC--;
                    break;
                case 0x0D:
                    val = Reg_D;
                    Reg_D--;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        private void ADD_HL(byte opcode)
        {
            ushort delta;
            switch (opcode)
            {
                case 0x09:
                    delta = Reg_BC;
                    Reg_HL += Reg_BC;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectSubFlag(false);
            AffectHalfCarryFlag((((Reg_HL & 0xFFF) + (delta & 0xFFF)) & 0x1000) == 0x1000);
            AffectCarryFlag((Reg_HL + delta) > 0xFFFF);
        }

        //Exclusive OR register with Reg_A, result saved into Reg_A (DP, 86)
        private void XOR_r8(byte opcode)
        {
            byte val;
            switch (opcode)
            {
                case 0xAF:
                    val = Reg_A;
                    Reg_A ^= Reg_A;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag((val ^ Reg_A) == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(false);

            lastOpCycles = 4;
        }

        //Exclusive OR value from memory with Reg_A, result saved into Reg_A (DP, 86)
        private void XOR_r8m(byte opcode)
        {
            byte val;
            switch (opcode)
            {
                case 0xAF:
                    val = Reg_A;
                    Reg_A ^= Reg_A;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            AffectZeroFlag((val ^ Reg_A) == 0);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(false);

            lastOpCycles = 8;
        }

        //Push address of next instruction onto stack and then jump to address of next word in memory (DP, 114)
        private void CALL_d16m(byte opcode)
        {
            Reg_SP -= 2;
            mmu.WriteWord(Reg_SP, (ushort)(Reg_PC + 2));
            Reg_PC = mmu.ReadWord(Reg_PC);

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
            mmu.WriteWord(Reg_SP, (ushort)(Reg_PC + 2));
            Reg_PC = mmu.ReadWord(Reg_PC);

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

            var signed = unchecked((sbyte)mmu.ReadByte(Reg_PC++));

            Reg_PC = signed < 0
                    ? (ushort)(Reg_PC - (ushort)(signed * (-1)))
                    : (ushort)(Reg_PC + (ushort)signed);

            lastOpCycles = 12;
        }

        //Call instruction at index PC+1 from CB map
        private void CB(byte opcode)
        {
            CbMap[mmu.ReadByte(Reg_PC++)]();
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
            bool toCarry = GetBit7(mmu.ReadByte(Reg_HL));
            mmu.WriteByte(Reg_HL, (byte)((mmu.ReadByte(Reg_HL) << 1) + (toCarry ? 1 : 0)));

            AffectZeroFlag(mmu.ReadByte(Reg_HL) == 0);
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
