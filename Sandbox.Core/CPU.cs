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
                    Reg_A = _mmu.ReadByte(Reg_HL);
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
                    Reg_B = _mmu.ReadByte(Reg_HL);
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
                    Reg_C = _mmu.ReadByte(Reg_HL);
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
                    Reg_D = _mmu.ReadByte(Reg_HL);
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
                    Reg_E = _mmu.ReadByte(Reg_HL);
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
                    Reg_H = _mmu.ReadByte(Reg_HL);
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
                    Reg_L = _mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0x70:
                    _mmu.WriteByte(Reg_HL, Reg_B);
                    lastOpCycles = 8;
                    break;
                case 0x71:
                    _mmu.WriteByte(Reg_HL, Reg_C);
                    lastOpCycles = 8;
                    break;
                case 0x72:
                    _mmu.WriteByte(Reg_HL, Reg_D);
                    lastOpCycles = 8;
                    break;
                case 0x73:
                    _mmu.WriteByte(Reg_HL, Reg_E);
                    lastOpCycles = 8;
                    break;
                case 0x74:
                    _mmu.WriteByte(Reg_HL, Reg_H);
                    lastOpCycles = 8;
                    break;
                case 0x75:
                    _mmu.WriteByte(Reg_HL, Reg_L);
                    lastOpCycles = 8;
                    break;
                case 0x36:
                    _mmu.WriteByte(Reg_HL, _mmu.ReadByte(Reg_PC++));
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
                    Reg_A = _mmu.ReadByte(Reg_BC);
                    lastOpCycles = 8;
                    break;
                case 0x1A:
                    Reg_A = _mmu.ReadByte(Reg_DE);
                    lastOpCycles = 8;
                    break;
                case 0x7E:
                    Reg_A = _mmu.ReadByte(Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0xFA:
                    Reg_A = _mmu.ReadByte(_mmu.ReadWord(Reg_PC));
                    Reg_PC += 2;
                    lastOpCycles = 16;
                    break;
                case 0x3E:
                    Reg_A = _mmu.ReadByte(Reg_PC++);
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
                    _mmu.WriteByte(Reg_BC, Reg_A);
                    lastOpCycles = 8;
                    break;
                case 0x12:
                    _mmu.WriteByte(Reg_DE, Reg_A);
                    lastOpCycles = 8;
                    break;
                case 0x77:
                    _mmu.WriteByte(Reg_HL, Reg_A);
                    lastOpCycles = 8;
                    break;
                case 0xEA:
                    _mmu.WriteByte(_mmu.ReadWord(Reg_PC), Reg_A);
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
                    Reg_A = _mmu.ReadByte((byte)(0xFF00 + Reg_C));
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
                    _mmu.WriteByte((byte)(0xFF00 + Reg_C), Reg_A);
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
                    Reg_A = _mmu.ReadByte(Reg_HL--);
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
                    _mmu.WriteByte(Reg_HL--, Reg_A);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 13/14/15. LDI A,(HL) (p.73)
        //
        // - Description -
        // Put value at address HL into A. Decrement HL.
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
                    Reg_A = _mmu.ReadByte(Reg_HL++);
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
                    _mmu.WriteByte(Reg_HL++, Reg_A);
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
                    _mmu.WriteByte((ushort)(0xFF00 + _mmu.ReadByte(Reg_PC++)), Reg_A);
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
                    Reg_A = _mmu.ReadByte((byte)(0xFF00 + _mmu.ReadByte(Reg_PC++)));
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
                    Reg_BC = _mmu.ReadWord(Reg_PC);
                    Reg_PC += 2;
                    break;
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
                    _mmu.WriteWord((ushort)(_mmu.ReadWord(Reg_PC)), Reg_SP);
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
                    _mmu.WriteWord(Reg_SP, Reg_AF);
                    Reg_SP -= 2;
                    lastOpCycles = 16;
                    break;
                case 0xC5:
                    _mmu.WriteWord(Reg_SP, Reg_BC);
                    Reg_SP -= 2;
                    lastOpCycles = 16;
                    break;
                case 0xD5:
                    _mmu.WriteWord(Reg_SP, Reg_DE);
                    Reg_SP -= 2;
                    lastOpCycles = 16;
                    break;
                case 0xE5:
                    _mmu.WriteWord(Reg_SP, Reg_HL);
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
                    Reg_AF = _mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    lastOpCycles = 12;
                    break;
                case 0xC1:
                    Reg_BC = _mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    lastOpCycles = 12;
                    break;
                case 0xD1:
                    Reg_DE = _mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    lastOpCycles = 12;
                    break;
                case 0xE1:
                    Reg_HL = _mmu.ReadWord(Reg_SP);
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
                    result = (ushort)(Reg_A + Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0xC6:
                    result = (ushort)(Reg_A + _mmu.ReadByte(Reg_PC++));
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
                    result = (ushort)(Reg_A + Reg_HL + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 8;
                    break;
                case 0xC6:
                    result = (ushort)(Reg_A + _mmu.ReadByte(Reg_PC++) + (GetCarryFlag() ? 1 : 0));
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
                    result = (ushort)(Reg_A - Reg_HL);
                    lastOpCycles = 8;
                    break;
                case 0xD6:
                    result = (ushort)(Reg_A - _mmu.ReadByte(Reg_PC++));
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
                    result = (ushort)(Reg_A - Reg_HL + (GetCarryFlag() ? 1 : 0));
                    lastOpCycles = 8;
                    break;
                case 0xC6:
                    result = (ushort)(Reg_A - _mmu.ReadByte(Reg_PC++) + (GetCarryFlag() ? 1 : 0));
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

        #endregion

        //No operation (DP, 97)
        private void NOP(byte opcode)
        {
            lastOpCycles = 4;
            return;
        }

        //Load into register1 from memory address stored in register2 (DP, 65)
        private void LD_r16m(byte opcode)
        {
            switch (opcode)
            {
                case 0x0A:
                    Reg_A = _mmu.ReadByte(Reg_BC);
                    break;
                case 0x1A:
                    Reg_A = _mmu.ReadByte(Reg_DE);
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
                    var addr = (ushort)(_mmu.ReadWord(Reg_PC));
                    Reg_PC += 2;
                    _mmu.WriteWord(addr, Reg_SP);
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
        private void LDr_r8(byte opcode)
        {
            switch (opcode)
            {
                case 0x02:
                    _mmu.WriteByte(Reg_BC, Reg_A);
                    break;
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

        //Rotate Reg_A left. Carry flag is set to value previously in bit 7
        private void RLCA(byte opcode)
        {
            byte val = Reg_A;
            Reg_A = (byte)(Reg_A << 1);

            AffectZeroFlag(false);
            AffectSubFlag(false);
            AffectHalfCarryFlag(false);
            AffectCarryFlag(GetBit7(val));
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
