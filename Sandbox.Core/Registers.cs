using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Core
{
    public static class Registers
    {
        //Real registers
        public static byte A { get; set; }
        public static byte F { get; set; }
        public static byte B { get; set; }
        public static byte C { get; set; }
        public static byte D { get; set; }
        public static byte E { get; set; }
        public static byte H { get; set; }
        public static byte L { get; set; }
        public static ushort SP { get; set; }
        public static ushort PC { get; set; }

        //Computed registers
        public static ushort BC
        {
            get { return (ushort)((B << 8) + C); }
            set { B = (byte)(value >> 8); C = (byte)(value & 0xFF); }
        }
        public static ushort DE
        {
            get { return (ushort)((D << 8) + E); }
            set { D = (byte)(value >> 8); E = (byte)(value & 0xFF); }
        }
        public static ushort HL
        {
            get { return (ushort)((H << 8) + L); }
            set { H = (byte)(value >> 8); L = (byte)(value & 0xFF); }
        }
    }
}
