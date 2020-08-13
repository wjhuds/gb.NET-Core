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

        private bool useCbMap;          //Flag to determine whether the next instruction should use CbMap

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

            useCbMap = false;
            inHaltedState = false;
            inStoppedState = false;
            interruptsEnabled = false;
            totalCycles = 0;
            lastOpCycles = 0;

            for (;;) //CPU loop for clock cycles
            {
                if (_verbose) Console.WriteLine("-- CPU Loop --");

                if (!inHaltedState || !inStoppedState) Tick();
                else totalCycles += 4;

                if (_verbose) Debug_LogRegisters();

                //Check for shutdown
                if (!continueOperation)
                {
                    if (!_verbose) Console.WriteLine("Shutting down CPU...");
                    break;
                }

                //TODO: Check for interrupts
            }
        }

        public void Tick()
        {
            if (_verbose) Console.WriteLine($"PC: 0x{Reg_PC:X2}");

            if (Reg_PC < 0x0000 || Reg_PC > 0xFFFF)
                throw new InstructionOutOfRangeException($"Attempted instruction out of range! Instruction: {Reg_PC:X2}");

            //Fetch instruction from memory using program counter
            var opCode = mmu.ReadByte(Reg_PC++);
            if (_verbose) Console.WriteLine($"OPCODE: 0x{opCode:X2}");

            //Increment program counter
            Reg_PC++;

            //Execute instruction
            try
            {
                if (!useCbMap) Map[opCode]();
                else CbMap[opCode]();
                useCbMap = false;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"!!! SYSTEM EXCEPTION: {exception.Message}");
                //TODO: Log to file
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
                // 00 - 0F
                () => NOP(0x00),        () => LD_n_nn(0x01),    () => LD_n_A(0x02),     () => INC_nn(0x03),
                () => INC_n(0x04),      () => DEC_n(0x05),      () => LD_nn_n(0x06),    () => RLCA(0x07),
                () => LD_nnm_SP(0x08),  () => ADD_HL_n(0x09),   () => LD_A_n(0x0A),     () => DEC_nn(0x0B),
                () => INC_n(0x0C),      () => DEC_n(0x0D),      () => LD_nn_n(0x0E),    () => RRCA(0x0F),
                // 10 - 1F
                () => STOP(0x10),       () => LD_n_nn(0x11),    () => LD_n_A(0x12),     () => INC_nn(0x13),
                () => INC_n(0x14),      () => DEC_n(0x15),      () => LD_nn_n(0x16),    () => RLA(0x17),
                () => JR_n(0x18),       () => ADD_HL_n(0x19),   () => LD_A_n(0x1A),     () => DEC_nn(0x1B),
                () => INC_n(0x1C),      () => DEC_n(0x1D),      () => LD_nn_n(0x1E),    () => RRA(0x1F),
                // 20 - 2F
                () => JR_cc_n(0x20),    () => LD_n_nn(0x21),    () => LDI_HLm_A(0x22),  () => INC_nn(0x23),
                () => INC_n(0x24),      () => DEC_n(0x25),      () => LD_nn_n(0x26),    () => DAA(0x27),
                () => JR_cc_n(0x28),    () => ADD_HL_n(0x29),   () => LDI_A_HLm(0x2A),  () => DEC_nn(0x2B),
                () => INC_n(0x2C),      () => DEC_n(0x2D),      () => LD_nn_n(0x2E),    () => CPL(0x2F),
                // 30 - 3F
                () => JR_cc_n(0x30),    () => LD_n_nn(0x31),    () => LDD_HLm_A(0x32),  () => INC_nn(0x33),
                () => INC_n(0x34),      () => DEC_n(0x35),      () => LD_r1_r2(0x36),   () => SCF(0x37),
                () => JR_cc_n(0x38),    () => ADD_HL_n(0x39),   () => LDD_A_HLm(0x3A),  () => DEC_nn(0x3B),
                () => INC_n(0x3C),      () => DEC_n(0x3D),      () => LD_A_n(0x3E),     () => CCF(0x3F),
                // 40 - 4F
                () => LD_r1_r2(0x40),   () => LD_r1_r2(0x41),   () => LD_r1_r2(0x42),   () => LD_r1_r2(0x43),
                () => LD_r1_r2(0x44),   () => LD_r1_r2(0x45),   () => LD_r1_r2(0x46),   () => LD_n_A(0x47),
                () => LD_r1_r2(0x48),   () => LD_r1_r2(0x49),   () => LD_r1_r2(0x4A),   () => LD_r1_r2(0x4B),
                () => LD_r1_r2(0x4C),   () => LD_r1_r2(0x4D),   () => LD_r1_r2(0x4E),   () => LD_n_A(0x4F),
                // 50 - 5F
                () => LD_r1_r2(0x50),   () => LD_r1_r2(0x51),   () => LD_r1_r2(0x52),   () => LD_r1_r2(0x53),
                () => LD_r1_r2(0x54),   () => LD_r1_r2(0x55),   () => LD_r1_r2(0x56),   () => LD_n_A(0x57),
                () => LD_r1_r2(0x58),   () => LD_r1_r2(0x59),   () => LD_r1_r2(0x5A),   () => LD_r1_r2(0x5B),
                () => LD_r1_r2(0x5C),   () => LD_r1_r2(0x5D),   () => LD_r1_r2(0x5E),   () => LD_n_A(0x5F),
                // 60 - 6F
                () => LD_r1_r2(0x60),   () => LD_r1_r2(0x61),   () => LD_r1_r2(0x62),   () => LD_r1_r2(0x63),
                () => LD_r1_r2(0x64),   () => LD_r1_r2(0x65),   () => LD_r1_r2(0x66),   () => LD_n_A(0x67),
                () => LD_r1_r2(0x68),   () => LD_r1_r2(0x69),   () => LD_r1_r2(0x6A),   () => LD_r1_r2(0x6B),
                () => LD_r1_r2(0x6C),   () => LD_r1_r2(0x6D),   () => LD_r1_r2(0x6E),   () => LD_n_A(0x6F),
                // 70 - 7F
                () => LD_r1_r2(0x70),   () => LD_r1_r2(0x71),   () => LD_r1_r2(0x72),   () => LD_r1_r2(0x73),
                () => LD_r1_r2(0x74),   () => LD_r1_r2(0x75),   () => HALT(0x76),       () => LD_n_A(0x77),
                () => LD_A_n(0x78),     () => LD_A_n(0x79),     () => LD_A_n(0x7A),     () => LD_A_n(0x7B),
                () => LD_A_n(0x7C),     () => LD_A_n(0x7D),     () => LD_A_n(0x7E),     () => LD_n_A(0x7F),
                // 80 - 8F
                () => ADD_A_n(0x80),    () => ADD_A_n(0x81),    () => ADD_A_n(0x82),    () => ADD_A_n(0x83),
                () => ADD_A_n(0x84),    () => ADD_A_n(0x85),    () => ADD_A_n(0x86),    () => ADD_A_n(0x87),
                () => ADC_A_n(0x88),    () => ADC_A_n(0x89),    () => ADC_A_n(0x8A),    () => ADC_A_n(0x8B),
                () => ADC_A_n(0x8C),    () => ADC_A_n(0x8D),    () => ADC_A_n(0x8E),    () => ADC_A_n(0x8F),
                // 90 - 9F
                () => SUB_A_n(0x90),    () => SUB_A_n(0x91),    () => SUB_A_n(0x92),    () => SUB_A_n(0x93),
                () => SUB_A_n(0x94),    () => SUB_A_n(0x95),    () => SUB_A_n(0x96),    () => SUB_A_n(0x97),
                () => SBC_A_n(0x98),    () => SBC_A_n(0x99),    () => SBC_A_n(0x9A),    () => SBC_A_n(0x9B),
                () => SBC_A_n(0x9C),    () => SBC_A_n(0x9D),    () => SBC_A_n(0x9E),    () => SBC_A_n(0x9F),
                // A0 - AF
                () => AND_n(0xA0),      () => AND_n(0xA1),      () => AND_n(0xA2),      () => AND_n(0xA3),
                () => AND_n(0xA4),      () => AND_n(0xA5),      () => AND_n(0xA6),      () => AND_n(0xA7),
                () => XOR_n(0xA8),      () => XOR_n(0xA9),      () => XOR_n(0xAA),      () => XOR_n(0xAB),
                () => XOR_n(0xAC),      () => XOR_n(0xAD),      () => XOR_n(0xAE),      () => XOR_n(0xAF),
                // B0 - BF
                () => OR_n(0xB0),       () => OR_n(0xB1),       () => OR_n(0xB2),       () => OR_n(0xB3),
                () => OR_n(0xB4),       () => OR_n(0xB5),       () => OR_n(0xB6),       () => OR_n(0xB7),
                () => CP_n(0xB8),       () => CP_n(0xB9),       () => CP_n(0xBA),       () => CP_n(0xBB),
                () => CP_n(0xBC),       () => CP_n(0xBD),       () => CP_n(0xBE),       () => CP_n(0xBF),
                // C0 - CF
                () => RET_cc(0xC0),     () => POP_nn(0xC1),     () => JP_cc_nn(0xC2),   () => JP_nn(0xC3),
                () => CALL_cc_nn(0xC4), () => PUSH_nn(0xC5),    () => ADD_A_n(0xC6),    () => RST_n(0xC7),
                () => RET_cc(0xC8),     () => RET(0xC9),        () => JP_cc_nn(0xCA),   () => CB(0xCB),
                () => CALL_cc_nn(0xCC), () => CALL_nn(0xCD),    () => ADC_A_n(0xCE),    () => RST_n(0xCF),
                // D0 - DF
                () => RET_cc(0xD0),     () => POP_nn(0xD1),     () => JP_cc_nn(0xD2),   null,
                () => CALL_cc_nn(0xD4), () => PUSH_nn(0xD5),    () => SUB_A_n(0xD6),    () => RST_n(0xD7),
                () => RET_cc(0xD8),     () => RETI(0xD9),       () => JP_cc_nn(0xDA),   null,
                () => CALL_cc_nn(0xDC), null,                   () => SBC_A_n(0xDE),    () => RST_n(0xDF),
                // E0 - EF
                () => LDH_nm_A(0xE0),   () => POP_nn(0xE1),     () => LD_Cm_A(0xE2),    null,
                null,                   () => PUSH_nn(0xE5),    () => AND_n(0xE6),      () => RST_n(0xE7),
                () => ADD_SP_n(0xE8),   () => JP_HLm(0xE9),     () => LD_n_A(0xEA),     null,
                null,                   null,                   () => XOR_n(0xEE),      () => RST_n(0xEF),
                // F0 - FF
                () => LDH_A_nm(0xF0),   () => POP_nn(0xF1),     () => LD_A_Cm(0xF2),    () => DI(0xF3),
                null,                   () => PUSH_nn(0xF5),    () => OR_n(0xF6),       () => RST_n(0xF7),
                () => LDHL_SP_n(0xF8),  () => LD_SP_HL(0xF9),   () => LD_A_n(0xFA),     () => EI(0xFB),
                null,                   null,                   () => CP_n(0xFE),       () => RST_n(0xFF),

            };
            CbMap = new List<Action>
            {
                // 00 - 0F
                () => RLC_n(0x00), () => RLC_n(0x01), () => RLC_n(0x02), () => RLC_n(0x03),
                () => RLC_n(0x04), () => RLC_n(0x05), () => RLC_n(0x06), () => RLC_n(0x07),
                () => RRC_n(0x08), () => RRC_n(0x09), () => RRC_n(0x0A), () => RRC_n(0x0B),
                () => RRC_n(0x0C), () => RRC_n(0x0D), () => RRC_n(0x0E), () => RRC_n(0x0F),
                // 10 - 1F
                () => RL_n(0x10), () => RL_n(0x11), () => RL_n(0x12), () => RL_n(0x13),
                () => RL_n(0x14), () => RL_n(0x15), () => RL_n(0x16), () => RL_n(0x17),
                () => RR_n(0x18), () => RR_n(0x19), () => RR_n(0x1A), () => RR_n(0x1B),
                () => RR_n(0x1C), () => RR_n(0x1D), () => RR_n(0x1E), () => RR_n(0x1F),
                // 20 - 2F
                () => SLA_n(0x20), () => SLA_n(0x21), () => SLA_n(0x22), () => SLA_n(0x23),
                () => SLA_n(0x24), () => SLA_n(0x25), () => SLA_n(0x26), () => SLA_n(0x27),
                () => SRA_n(0x28), () => SRA_n(0x29), () => SRA_n(0x2A), () => SRA_n(0x2B),
                () => SRA_n(0x2C), () => SRA_n(0x2D), () => SRA_n(0x2E), () => SRA_n(0x2F),
                // 30 - 3F
                () => SWAP_n(0x30), () => SWAP_n(0x31), () => SWAP_n(0x32), () => SWAP_n(0x33),
                () => SWAP_n(0x34), () => SWAP_n(0x35), () => SWAP_n(0x36), () => SWAP_n(0x37),
                () => SRL_n(0x38), () => SRL_n(0x39), () => SRL_n(0x3A), () => SRL_n(0x3B),
                () => SRL_n(0x3C), () => SRL_n(0x3D), () => SRL_n(0x3E), () => SRL_n(0x3F),
                // 40 - 4F
                () => BIT(0x40), () => BIT(0x41), () => BIT(0x42), () => BIT(0x43),
                () => BIT(0x44), () => BIT(0x45), () => BIT(0x46), () => BIT(0x47),
                () => BIT(0x48), () => BIT(0x49), () => BIT(0x4A), () => BIT(0x4B),
                () => BIT(0x4C), () => BIT(0x4D), () => BIT(0x4E), () => BIT(0x4F),
                // 50 - 5F
                () => BIT(0x50), () => BIT(0x51), () => BIT(0x52), () => BIT(0x53),
                () => BIT(0x54), () => BIT(0x55), () => BIT(0x56), () => BIT(0x57),
                () => BIT(0x58), () => BIT(0x59), () => BIT(0x5A), () => BIT(0x5B),
                () => BIT(0x5C), () => BIT(0x5D), () => BIT(0x5E), () => BIT(0x5F),
                // 60 - 6F
                () => BIT(0x60), () => BIT(0x61), () => BIT(0x62), () => BIT(0x63),
                () => BIT(0x64), () => BIT(0x65), () => BIT(0x66), () => BIT(0x67),
                () => BIT(0x68), () => BIT(0x69), () => BIT(0x6A), () => BIT(0x6B),
                () => BIT(0x6C), () => BIT(0x6D), () => BIT(0x6E), () => BIT(0x6F),
                // 70 - 7F
                () => BIT(0x70), () => BIT(0x71), () => BIT(0x72), () => BIT(0x73),
                () => BIT(0x74), () => BIT(0x75), () => BIT(0x76), () => BIT(0x77),
                () => BIT(0x78), () => BIT(0x79), () => BIT(0x7A), () => BIT(0x7B),
                () => BIT(0x7C), () => BIT(0x7D), () => BIT(0x7E), () => BIT(0x7F),
                // 80 - 8F
                () => RES(0x80), () => RES(0x81), () => RES(0x82), () => RES(0x83),
                () => RES(0x84), () => RES(0x85), () => RES(0x86), () => RES(0x87),
                () => RES(0x88), () => RES(0x89), () => RES(0x8A), () => RES(0x8B),
                () => RES(0x8C), () => RES(0x8D), () => RES(0x8E), () => RES(0x8F),
                // 90 - 9F
                () => RES(0x90), () => RES(0x91), () => RES(0x92), () => RES(0x93),
                () => RES(0x94), () => RES(0x95), () => RES(0x96), () => RES(0x97),
                () => RES(0x98), () => RES(0x99), () => RES(0x9A), () => RES(0x9B),
                () => RES(0x9C), () => RES(0x9D), () => RES(0x9E), () => RES(0x9F),
                // A0 - AF
                () => RES(0xA0), () => RES(0xA1), () => RES(0xA2), () => RES(0xA3),
                () => RES(0xA4), () => RES(0xA5), () => RES(0xA6), () => RES(0xA7),
                () => RES(0xA8), () => RES(0xA9), () => RES(0xAA), () => RES(0xAB),
                () => RES(0xAC), () => RES(0xAD), () => RES(0xAE), () => RES(0xAF),
                // B0 - BF
                () => RES(0xB0), () => RES(0xB1), () => RES(0xB2), () => RES(0xB3),
                () => RES(0xB4), () => RES(0xB5), () => RES(0xB6), () => RES(0xB7),
                () => RES(0xB8), () => RES(0xB9), () => RES(0xBA), () => RES(0xBB),
                () => RES(0xBC), () => RES(0xBD), () => RES(0xBE), () => RES(0xBF),
                // C0 - CF
                () => SET(0xC0), () => SET(0xC1), () => SET(0xC2), () => SET(0xC3),
                () => SET(0xC4), () => SET(0xC5), () => SET(0xC6), () => SET(0xC7),
                () => SET(0xC8), () => SET(0xC9), () => SET(0xCA), () => SET(0xCB),
                () => SET(0xCC), () => SET(0xCD), () => SET(0xCE), () => SET(0xCF),
                // D0 - DF
                () => SET(0xD0), () => SET(0xD1), () => SET(0xD2), () => SET(0xD3),
                () => SET(0xD4), () => SET(0xD5), () => SET(0xD6), () => SET(0xD7),
                () => SET(0xD8), () => SET(0xD9), () => SET(0xDA), () => SET(0xDB),
                () => SET(0xDC), () => SET(0xDD), () => SET(0xDE), () => SET(0xDF),
                // E0 - EF
                () => SET(0xE0), () => SET(0xE1), () => SET(0xE2), () => SET(0xE3),
                () => SET(0xE4), () => SET(0xE5), () => SET(0xE6), () => SET(0xE7),
                () => SET(0xE8), () => SET(0xE9), () => SET(0xEA), () => SET(0xEB),
                () => SET(0xEC), () => SET(0xED), () => SET(0xEE), () => SET(0xEF),
                // F0 - FF
                () => SET(0xF0), () => SET(0xF1), () => SET(0xF2), () => SET(0xF3),
                () => SET(0xF4), () => SET(0xF5), () => SET(0xF6), () => SET(0xF7),
                () => SET(0xF8), () => SET(0xF9), () => SET(0xFA), () => SET(0xFB),
                () => SET(0xFC), () => SET(0xFD), () => SET(0xFE), () => SET(0xFF),
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
        // N - Reset.
        // H - Reset.
        // C - Reset.
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
        // EI           -/-         FB      4
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
                    bit7 = GetBit(7, Reg_A);

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
                    bit7 = GetBit(7, Reg_A);

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
                    bit0 = GetBit(0, Reg_A);

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
                    bit0 = GetBit(0, Reg_A);

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
                    bit7 = GetBit(7, Reg_A);

                    result = (byte)(Reg_A << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x00:
                    bit7 = GetBit(7, Reg_B);

                    result = (byte)(Reg_B << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x01:
                    bit7 = GetBit(7, Reg_C);

                    result = (byte)(Reg_C << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x02:
                    bit7 = GetBit(7, Reg_D);

                    result = (byte)(Reg_D << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x03:
                    bit7 = GetBit(7, Reg_E);

                    result = (byte)(Reg_E << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x04:
                    bit7 = GetBit(7, Reg_H);

                    result = (byte)(Reg_H << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x05:
                    bit7 = GetBit(7, Reg_L);

                    result = (byte)(Reg_L << 1);
                    result = (byte)(result | (bit7 ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x06:
                    result = mmu.ReadByte(Reg_HL);
                    bit7 = GetBit(7, result);

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
        private void RL_n(byte opcode)
        {
            bool bit7;
            byte result;
            switch (opcode)
            {
                case 0x17:
                    bit7 = GetBit(7, Reg_A);

                    result = (byte)(Reg_A << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x10:
                    bit7 = GetBit(7, Reg_B);

                    result = (byte)(Reg_B << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x11:
                    bit7 = GetBit(7, Reg_C);

                    result = (byte)(Reg_C << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x12:
                    bit7 = GetBit(7, Reg_D);

                    result = (byte)(Reg_D << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x13:
                    bit7 = GetBit(7, Reg_E);

                    result = (byte)(Reg_E << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x14:
                    bit7 = GetBit(7, Reg_H);

                    result = (byte)(Reg_H << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x15:
                    bit7 = GetBit(7, Reg_L);

                    result = (byte)(Reg_L << 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x16:
                    result = mmu.ReadByte(Reg_HL);
                    bit7 = GetBit(7, result);

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
                    bit0 = GetBit(0, Reg_A);

                    result = (byte)(Reg_A >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x08:
                    bit0 = GetBit(0, Reg_B);

                    result = (byte)(Reg_B >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x09:
                    bit0 = GetBit(0, Reg_C);

                    result = (byte)(Reg_C >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x0A:
                    bit0 = GetBit(0, Reg_D);

                    result = (byte)(Reg_D >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x0B:
                    bit0 = GetBit(0, Reg_E);

                    result = (byte)(Reg_E >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x0C:
                    bit0 = GetBit(0, Reg_H);

                    result = (byte)(Reg_H >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x0D:
                    bit0 = GetBit(0, Reg_L);

                    result = (byte)(Reg_L >> 1);
                    result = (byte)(result | (bit0 ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x0E:
                    result = mmu.ReadByte(Reg_HL);
                    bit0 = GetBit(0, result);

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
                    bit0 = GetBit(0, Reg_A);

                    result = (byte)(Reg_A >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x08:
                    bit0 = GetBit(0, Reg_B);

                    result = (byte)(Reg_B >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x09:
                    bit0 = GetBit(0, Reg_C);

                    result = (byte)(Reg_C >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x0A:
                    bit0 = GetBit(0, Reg_D);

                    result = (byte)(Reg_D >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x0B:
                    bit0 = GetBit(0, Reg_E);

                    result = (byte)(Reg_E >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x0C:
                    bit0 = GetBit(0, Reg_H);

                    result = (byte)(Reg_H >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x0D:
                    bit0 = GetBit(0, Reg_L);

                    result = (byte)(Reg_L >> 1);
                    result = (byte)(result | (GetCarryFlag() ? 1 : 0));
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x0E:
                    result = mmu.ReadByte(Reg_HL);
                    bit0 = GetBit(0, result);

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
                    bit7 = GetBit(7, Reg_A);

                    result = (byte)(Reg_A << 1);
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x20:
                    bit7 = GetBit(7, Reg_B);

                    result = (byte)(Reg_B << 1);
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x21:
                    bit7 = GetBit(7, Reg_C);

                    result = (byte)(Reg_C << 1);
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x22:
                    bit7 = GetBit(7, Reg_D);

                    result = (byte)(Reg_D << 1);
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x23:
                    bit7 = GetBit(7, Reg_E);

                    result = (byte)(Reg_E << 1);
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x24:
                    bit7 = GetBit(7, Reg_H);

                    result = (byte)(Reg_H << 1);
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x25:
                    bit7 = GetBit(7, Reg_L);

                    result = (byte)(Reg_L << 1);
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x26:
                    result = mmu.ReadByte(Reg_HL);
                    bit7 = GetBit(7, result);

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
                    bit0 = GetBit(0, Reg_A);

                    result = (byte)(Reg_A >> 1);
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x28:
                    bit0 = GetBit(0, Reg_B);

                    result = (byte)(Reg_B >> 1);
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x29:
                    bit0 = GetBit(0, Reg_C);

                    result = (byte)(Reg_C >> 1);
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x2A:
                    bit0 = GetBit(0, Reg_D);

                    result = (byte)(Reg_D >> 1);
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x2B:
                    bit0 = GetBit(0, Reg_E);

                    result = (byte)(Reg_E >> 1);
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x2C:
                    bit0 = GetBit(0, Reg_H);

                    result = (byte)(Reg_H >> 1);
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x2D:
                    bit0 = GetBit(0, Reg_L);

                    result = (byte)(Reg_L >> 1);
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x2E:
                    result = mmu.ReadByte(Reg_HL);
                    bit0 = GetBit(0, result);

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

        // 11. SRL n (p.107)
        //
        // - Description -
        // Shift n right into Carry. MSB set to 0.
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
        // SRA           A           CB 3F   8
        // SRA           B           CB 38   8
        // SRA           C           CB 39   8
        // SRA           D           CB 3A   8
        // SRA           E           CB 3B   8
        // SRA           H           CB 3C   8
        // SRA           L           CB 3D   8
        // SRA           (HL)        CB 3E   16
        private void SRL_n(byte opcode)
        {
            bool bit0;
            byte result;
            switch (opcode)
            {
                case 0x3F:
                    bit0 = GetBit(0, Reg_A);

                    result = (byte)(Reg_A >> 1);
                    result = AffectBit(7, result, false);
                    Reg_A = result;

                    lastOpCycles = 8;
                    break;
                case 0x38:
                    bit0 = GetBit(0, Reg_B);

                    result = (byte)(Reg_B >> 1);
                    result = AffectBit(7, result, false);
                    Reg_B = result;

                    lastOpCycles = 8;
                    break;
                case 0x39:
                    bit0 = GetBit(0, Reg_C);

                    result = (byte)(Reg_C >> 1);
                    result = AffectBit(7, result, false);
                    Reg_C = result;

                    lastOpCycles = 8;
                    break;
                case 0x3A:
                    bit0 = GetBit(0, Reg_D);

                    result = (byte)(Reg_D >> 1);
                    result = AffectBit(7, result, false);
                    Reg_D = result;

                    lastOpCycles = 8;
                    break;
                case 0x3B:
                    bit0 = GetBit(0, Reg_E);

                    result = (byte)(Reg_E >> 1);
                    result = AffectBit(7, result, false);
                    Reg_E = result;

                    lastOpCycles = 8;
                    break;
                case 0x3C:
                    bit0 = GetBit(0, Reg_H);

                    result = (byte)(Reg_H >> 1);
                    result = AffectBit(7, result, false);
                    Reg_H = result;

                    lastOpCycles = 8;
                    break;
                case 0x3D:
                    bit0 = GetBit(0, Reg_L);

                    result = (byte)(Reg_L >> 1);
                    result = AffectBit(7, result, false);
                    Reg_L = result;

                    lastOpCycles = 8;
                    break;
                case 0x3E:
                    bit0 = GetBit(0, mmu.ReadByte(Reg_HL));

                    result = mmu.ReadByte(Reg_HL);
                    result = (byte)(result >> 1);
                    result = AffectBit(7, result, false);
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

        #region Bit Opcodes

        // 1. BIT b,r (p.108)
        //
        // - Description -
        // Test bit b in register r.
        //
        // - Use with -
        // b = 0 - 7, r = A, B, C, D, E, H, L, (HL)
        //
        // - Flags affected -
        // Z - Set if bit b of register r is 0.
        // N - Reset.
        // H - Set.
        // C - Not affected.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode          Cycles
        // BIT          b, A        CB [4-7][7/F]   8
        // BIT          b, B        CB [4-7][0/8]   8
        // BIT          b, C        CB [4-7][1/9]   8
        // BIT          b, D        CB [4-7][2/A]   8
        // BIT          b, E        CB [4-7][3/B]   8
        // BIT          b, H        CB [4-7][4/C]   8
        // BIT          b, L        CB [4-7][5/D]   8
        // BIT          b, (HL)     CB [4-7][6/E]   16
        private void BIT(byte opcode)
        {
            // Break opcode into most significant and least significant nybbles
            byte msn = (byte)((opcode >> 4) & 0xF);
            byte lsn = (byte)(opcode & 0xF);

            int bit;
            byte register;
            
            switch (msn)
            {
                case 0x04:
                    bit = (lsn < 0x08) ? 0 : 1;
                    break;
                case 0x05:
                    bit = (lsn < 0x08) ? 2 : 3;
                    break;
                case 0x06:
                    bit = (lsn < 0x08) ? 4 : 5;
                    break;
                case 0x07:
                    bit = (lsn < 0x08) ? 6 : 7;
                    break;
                default:
                    throw new ApplicationException($"OpCode called vas not a valid BIT operation! ({opcode})");
            }

            switch (lsn)
            {
                case 0x0:
                case 0x8:
                    register = Reg_B;
                    lastOpCycles = 8;
                    break;
                case 0x1:
                case 0x9:
                    register = Reg_C;
                    lastOpCycles = 8;
                    break;
                case 0x2:
                case 0xA:
                    register = Reg_D;
                    lastOpCycles = 8;
                    break;
                case 0x3:
                case 0xB:
                    register = Reg_E;
                    lastOpCycles = 8;
                    break;
                case 0x4:
                case 0xC:
                    register = Reg_H;
                    lastOpCycles = 8;
                    break;
                case 0x5:
                case 0xD:
                    register = Reg_L;
                    lastOpCycles = 8;
                    break;
                case 0x6:
                case 0xE:
                    register = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 16;
                    break;
                case 0x7:
                case 0xF:
                    register = Reg_A;
                    lastOpCycles = 8;
                    break;
                default:
                    throw new ApplicationException($"OpCode called vas not a valid BIT operation! ({opcode})");
            }

            bool result = GetBit(bit, register);

            AffectZeroFlag(!result);
            AffectSubFlag(false);
            AffectHalfCarryFlag(true);
        }

        // 2. SET b,r (p.109)
        //
        // - Description -
        // Set bit b in register r.
        //
        // - Use with -
        // b = 0 - 7, r = A, B, C, D, E, H, L, (HL)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode          Cycles
        // SET          b, A        CB [C-F][7/F]   8
        // SET          b, B        CB [C-F][0/8]   8
        // SET          b, C        CB [C-F][1/9]   8
        // SET          b, D        CB [C-F][2/A]   8
        // SET          b, E        CB [C-F][3/B]   8
        // SET          b, H        CB [C-F][4/C]   8
        // SET          b, L        CB [C-F][5/D]   8
        // SET          b, (HL)     CB [C-F][6/E]   16
        private void SET(byte opcode)
        {
            // Break opcode into most significant and least significant nybbles
            byte msn = (byte)((opcode >> 4) & 0xF);
            byte lsn = (byte)(opcode & 0xF);

            int bit;
            byte register;

            switch (msn)
            {
                case 0x0C:
                    bit = (lsn < 0x08) ? 0 : 1;
                    break;
                case 0x0D:
                    bit = (lsn < 0x08) ? 2 : 3;
                    break;
                case 0x0E:
                    bit = (lsn < 0x08) ? 4 : 5;
                    break;
                case 0x0F:
                    bit = (lsn < 0x08) ? 6 : 7;
                    break;
                default:
                    throw new ApplicationException($"OpCode called vas not a valid BIT operation! ({opcode})");
            }

            switch (lsn)
            {
                case 0x0:
                case 0x8:
                    register = Reg_B;
                    lastOpCycles = 8;
                    break;
                case 0x1:
                case 0x9:
                    register = Reg_C;
                    lastOpCycles = 8;
                    break;
                case 0x2:
                case 0xA:
                    register = Reg_D;
                    lastOpCycles = 8;
                    break;
                case 0x3:
                case 0xB:
                    register = Reg_E;
                    lastOpCycles = 8;
                    break;
                case 0x4:
                case 0xC:
                    register = Reg_H;
                    lastOpCycles = 8;
                    break;
                case 0x5:
                case 0xD:
                    register = Reg_L;
                    lastOpCycles = 8;
                    break;
                case 0x6:
                case 0xE:
                    register = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 16;
                    break;
                case 0x7:
                case 0xF:
                    register = Reg_A;
                    lastOpCycles = 8;
                    break;
                default:
                    throw new ApplicationException($"OpCode called vas not a valid BIT operation! ({opcode})");
            }

            AffectBit(bit, register, true);
        }

        // 3. RES b,r (p.110)
        //
        // - Description -
        // Reset bit b in register r.
        //
        // - Use with -
        // b = 0 - 7, r = A, B, C, D, E, H, L, (HL)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode          Cycles
        // RES          b, A        CB [8-B][7/F]   8
        // RES          b, B        CB [8-B][0/8]   8
        // RES          b, C        CB [8-B][1/9]   8
        // RES          b, D        CB [8-B][2/A]   8
        // RES          b, E        CB [8-B][3/B]   8
        // RES          b, H        CB [8-B][4/C]   8
        // RES          b, L        CB [8-B][5/D]   8
        // RES          b, (HL)     CB [8-B][6/E]   16
        private void RES(byte opcode)
        {
            // Break opcode into most significant and least significant nybbles
            byte msn = (byte)((opcode >> 4) & 0xF);
            byte lsn = (byte)(opcode & 0xF);

            int bit;
            byte register;

            switch (msn)
            {
                case 0x08:
                    bit = (lsn < 0x08) ? 0 : 1;
                    break;
                case 0x09:
                    bit = (lsn < 0x08) ? 2 : 3;
                    break;
                case 0x0A:
                    bit = (lsn < 0x08) ? 4 : 5;
                    break;
                case 0x0B:
                    bit = (lsn < 0x08) ? 6 : 7;
                    break;
                default:
                    throw new ApplicationException($"OpCode called vas not a valid BIT operation! ({opcode})");
            }

            switch (lsn)
            {
                case 0x0:
                case 0x8:
                    register = Reg_B;
                    lastOpCycles = 8;
                    break;
                case 0x1:
                case 0x9:
                    register = Reg_C;
                    lastOpCycles = 8;
                    break;
                case 0x2:
                case 0xA:
                    register = Reg_D;
                    lastOpCycles = 8;
                    break;
                case 0x3:
                case 0xB:
                    register = Reg_E;
                    lastOpCycles = 8;
                    break;
                case 0x4:
                case 0xC:
                    register = Reg_H;
                    lastOpCycles = 8;
                    break;
                case 0x5:
                case 0xD:
                    register = Reg_L;
                    lastOpCycles = 8;
                    break;
                case 0x6:
                case 0xE:
                    register = mmu.ReadByte(Reg_HL);
                    lastOpCycles = 16;
                    break;
                case 0x7:
                case 0xF:
                    register = Reg_A;
                    lastOpCycles = 8;
                    break;
                default:
                    throw new ApplicationException($"OpCode called vas not a valid BIT operation! ({opcode})");
            }

            AffectBit(bit, register, false);
        }

        #endregion

        #region Jumps

        // 1. JP nn (p.111)
        //
        // - Description -
        // Jump to address nn.
        //
        // - Use with -
        // nn = two byte immediate value. (LS byte first.)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // JP           nn          C3      12
        private void JP_nn(byte opcode)
        {
            bool bit7;
            byte result;
            switch (opcode)
            {
                case 0xC3:
                    Reg_PC = mmu.ReadWord(Reg_PC);
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 12;
        }

        // 2. JP cc,nn (p.111)
        //
        // - Description -
        // Jump to address nn if following condition is true:
        //   cc = NZ,   Jump if Z flag is reset.
        //   cc = Z,    Jump if Z flag is set.
        //   cc = NC,   Jump if C flag is reset.
        //   cc = C,    Jump if C flag is set.
        //
        // - Use with -
        // nn = two byte immediate value. (LS byte first.)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // JP           NZ, nn      C2      12
        // JP           Z, nn       CA      12
        // JP           NC, nn      D2      12
        // JP           C, nn       DA      12
        private void JP_cc_nn(byte opcode)
        {
            var allowJump = false;

            switch (opcode)
            {
                case 0xC2:
                    if (!GetZeroFlag())
                        allowJump = true;
                    break;
                case 0xCA:
                    if (!!GetZeroFlag())
                        allowJump = true;
                    break;
                case 0xD2:
                    if (!GetCarryFlag())
                        allowJump = true;
                    break;
                case 0xDA:
                    if (!!GetCarryFlag())
                        allowJump = true;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            if (allowJump)
                Reg_PC = mmu.ReadWord(Reg_PC);

            lastOpCycles = 12;
        }

        // 3. JP (HL) (p.112)
        //
        // - Description -
        // Jump to address contained in HL.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // JP           (HL)        E9      4
        private void JP_HLm(byte opcode)
        {
            switch (opcode)
            {
                case 0xE9:
                    Reg_PC = mmu.ReadWord(Reg_HL);
                    lastOpCycles = 4;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 4. JR n (p.112)
        //
        // - Description -
        // Add n to current address and jump to it.
        //
        // - Use with -
        // n = one byte signed immediate value
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // JR           n           18      8
        private void JR_n(byte opcode)
        {
            switch (opcode)
            {
                case 0xE9:
                    var signedVal = Reg_PC > 127
                        ? -((~Reg_PC + 1) & 0xFF)
                        : Reg_PC;
                    Reg_PC++;
                    Reg_PC = (ushort)(Reg_PC + signedVal);
                    lastOpCycles = 8;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 5. JR cc,n (p.113)
        //
        // - Description -
        // Add n to current address and jump to it if following condition is true:
        //   cc = NZ,   Jump if Z flag is reset.
        //   cc = Z,    Jump if Z flag is set.
        //   cc = NC,   Jump if C flag is reset.
        //   cc = C,    Jump if C flag is set.
        //
        // - Use with -
        // n = one byte signed immediate value
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // JR           NZ, n       20      8
        // JR           Z, n        28      8
        // JR           NC, n       30      8
        // JR           C, n        38      8
        private void JR_cc_n(byte opcode)
        {
            var allowJump = false;

            switch (opcode)
            {
                case 0x20:
                    if (!GetZeroFlag())
                        allowJump = true;
                    break;
                case 0x28:
                    if (!!GetZeroFlag())
                        allowJump = true;
                    break;
                case 0x30:
                    if (!GetCarryFlag())
                        allowJump = true;
                    break;
                case 0x38:
                    if (!!GetCarryFlag())
                        allowJump = true;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            if (allowJump)
            {
                var signedVal = Reg_PC > 127
                    ? -((~Reg_PC + 1) & 0xFF)
                    : Reg_PC;
                Reg_PC++;
                Reg_PC = (ushort)(Reg_PC + signedVal);
                lastOpCycles = 8;
            }
        }

        #endregion

        #region Calls

        // 1. CALL nn (p.114)
        //
        // - Description -
        // Push address of next instruction onto stack and then jump to address nn.
        //
        // - Use with -
        // nn = two byte immediate value. (LS byte first.)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // CALL         nn          CD      12
        private void CALL_nn(byte opcode)
        {
            switch (opcode)
            {
                case 0xC3:
                    mmu.WriteWord(Reg_SP, ++Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = mmu.ReadWord(Reg_PC);
                    lastOpCycles = 12;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }
        }

        // 2. CALL cc,nn (p.115)
        //
        // - Description -
        // Call address n if following condition is true:
        //   cc = NZ,   Jump if Z flag is reset.
        //   cc = Z,    Jump if Z flag is set.
        //   cc = NC,   Jump if C flag is reset.
        //   cc = C,    Jump if C flag is set.
        //
        // - Use with -
        // nn = two byte immediate value. (LS byte first.)
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // CALL         NZ, nn      C4      12
        // CALL         Z, nn       CC      12
        // CALL         NC, nn      D4      12
        // CALL         C, nn       DC      12
        private void CALL_cc_nn(byte opcode)
        {
            var allowCall = false;
            switch (opcode)
            {
                case 0xC4:
                    if (!GetZeroFlag())
                        allowCall = true;
                    break;
                case 0xCC:
                    if (!!GetZeroFlag())
                        allowCall = true;
                    break;
                case 0xD4:
                    if (!GetCarryFlag())
                        allowCall = true;
                    break;
                case 0xDC:
                    if (!!GetCarryFlag())
                        allowCall = true;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            if (allowCall)
            {
                mmu.WriteWord(Reg_SP, ++Reg_PC);
                Reg_SP -= 2;
                Reg_PC = mmu.ReadWord(Reg_PC);
                lastOpCycles = 12;
            }
        }

        #endregion

        #region Restarts

        // 1. RST n (p.116)
        //
        // - Description -
        // Push present address onto stack.
        // Jump to address 0x0000 + n.
        //
        // - Use with -
        // nn = 0x00, 0x08, 0x10, 0x18, 0x20, 0x28, 0x30, 0x38
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RST          0x00         C7      32
        // RST          0x08         CF      32
        // RST          0x10         D7      32
        // RST          0x18         DF      32
        // RST          0x20         E7      32
        // RST          0x28         EF      32
        // RST          0x30         F7      32
        // RST          0x38         FF      32
        private void RST_n(byte opcode)
        {
            switch (opcode)
            {
                case 0xC7:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x00;
                    break;
                case 0xCF:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x08;
                    break;
                case 0xD7:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x10;
                    break;
                case 0xDF:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x18;
                    break;
                case 0xE7:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x20;
                    break;
                case 0xEF:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x28;
                    break;
                case 0xF7:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x30;
                    break;
                case 0xFF:
                    mmu.WriteWord(Reg_SP, Reg_PC);
                    Reg_SP -= 2;
                    Reg_PC = 0x38;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 32;
        }

        #endregion

        #region Returns

        // 1. RET (p.117)
        //
        // - Description -
        // Pop two bytes from stack & jump to that address.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RET          -/-         C9      8
        private void RET(byte opcode)
        {
            switch (opcode)
            {
                case 0xC3:
                    Reg_PC = mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
        }

        // 2. RET cc (p.117)
        //
        // - Description -
        // Return if following condition is true:
        //   cc = NZ,   Jump if Z flag is reset.
        //   cc = Z,    Jump if Z flag is set.
        //   cc = NC,   Jump if C flag is reset.
        //   cc = C,    Jump if C flag is set.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RET          NZ          C0      8
        // RET          Z           C8      8
        // RET          NC          D0      8
        // RET          C           D8      8
        private void RET_cc(byte opcode)
        {
            var allowReturn = false;
            switch (opcode)
            {
                case 0xC0:
                    if (!GetZeroFlag())
                        allowReturn = true;
                    break;
                case 0xC8:
                    if (!!GetZeroFlag())
                        allowReturn = true;
                    break;
                case 0xD0:
                    if (!GetCarryFlag())
                        allowReturn = true;
                    break;
                case 0xD8:
                    if (!!GetCarryFlag())
                        allowReturn = true;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            if (allowReturn)
            {
                Reg_PC = mmu.ReadWord(Reg_SP);
                Reg_SP += 2;
            }
            lastOpCycles = 8;
        }

        // 3. RETI (p.118)
        //
        // - Description -
        // Pop two bytes from stack & jump to that address then
        // enable interrupts.
        //
        // - Opcodes -
        // Instruction  Parameters  Opcode  Cycles
        // RETI         -/-         D9      8
        private void RETI(byte opcode)
        {
            switch (opcode)
            {
                case 0xC3:
                    Reg_PC = mmu.ReadWord(Reg_SP);
                    Reg_SP += 2;
                    interruptsEnabled = true;
                    break;
                default:
                    throw new InstructionNotImplementedException($"Instruction not implemented! OpCode: {opcode}");
            }

            lastOpCycles = 8;
        }

        #endregion

        // CB Prefix handler
        private void CB(byte opcode)
        {
            useCbMap = true;
        }

        #endregion

        #region Helper Functions
        private bool GetBit(int bit, byte val)
        {
            return ((1 << bit) & val) != 0;
        }

        private byte AffectBit(int bit, byte val, bool set)
        {
            val = (byte)((1 << bit) | val);
            return val;
        }

        private bool GetZeroFlag()                  { return GetBit(7, Reg_F); }
        private bool GetSubFlag()                   { return GetBit(6, Reg_F); }
        private bool GetHalfCarryFlag()             { return GetBit(5, Reg_F); }
        private bool GetCarryFlag()                 { return GetBit(4, Reg_F); }

        private void AffectZeroFlag(bool set)       { AffectBit(7, Reg_F, set); }
        private void AffectSubFlag(bool set)        { AffectBit(6, Reg_F, set); }
        private void AffectHalfCarryFlag(bool set)  { AffectBit(5, Reg_F, set); }
        private void AffectCarryFlag(bool set)      { AffectBit(4, Reg_F, set); }
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
