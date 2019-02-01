using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Core
{
    public partial class CPU
    {
        //Real registers
        public byte Reg_A { get; set; }
        public byte Reg_F { get; set; }
        public byte Reg_B { get; set; }
        public byte Reg_C { get; set; }
        public byte Reg_D { get; set; }
        public byte Reg_E { get; set; }
        public byte Reg_H { get; set; }
        public byte Reg_L { get; set; }
        public ushort Reg_SP { get; set; }
        public ushort Reg_PC { get; set; }

        //Computed registers
        public ushort Reg_AF
        {
            get { return (ushort)((Reg_A << 8) + Reg_F); }
            set { Reg_A = (byte)(value << 8); Reg_F = (byte)(value & 0xFF); }
        }
        public ushort Reg_BC
        {
            get { return (ushort)((Reg_B << 8) + Reg_C); }
            set { Reg_B = (byte)(value >> 8); Reg_C = (byte)(value & 0xFF); }
        }
        public ushort Reg_DE
        {
            get { return (ushort)((Reg_D << 8) + Reg_E); }
            set { Reg_D = (byte)(value >> 8); Reg_E = (byte)(value & 0xFF); }
        }
        public ushort Reg_HL
        {
            get { return (ushort)((Reg_H << 8) + Reg_L); }
            set { Reg_H = (byte)(value >> 8); Reg_L = (byte)(value & 0xFF); }
        }
    }
}
