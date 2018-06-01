using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandbox.Core
{
    public class MMU
    {
        private bool _inBios = true; //Indicates system is loading data from BIOS and not ROM

        private byte[] _bios = new byte[256];
        private byte[] _rom = new byte[32768];
        private byte[] _wram = new byte[8192];
        private byte[] _eram = new byte[8192];
        private byte[] _zram = new byte[128];

        public MMU(byte[] bios)
        {
            _bios = bios;
        }

        public void LoadRom(byte[] rom)
        {
            try
            {
                _rom = rom;
            }
            catch(IndexOutOfRangeException exception)
            {
                Console.WriteLine("Error: rom data did not fit in 32k array! (TO BE IMPLEMENTED)");
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Error: Unknown error occurred loading the ROM! Exception message: {exception.Message}");
            }
        }

        public byte ReadByte(ushort pc)
        {
            switch(pc & 0xF000)
            {
                //BIOS / Rom bank 0
                case 0x0000:
                    if(_inBios)
                    {
                        if (pc < 0x0100) return _bios[pc];
                        _inBios = false;
                    }
                    return _rom[pc];

                case 0x1000:
                case 0x2000:
                case 0x3000:
                    return _rom[pc];

                //ROM bank 01
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    return _rom[pc];
                
                //GPU VRAM
                case 0x8000:
                case 0x9000:
                    throw new NotImplementedException("Error: GPU features not implemented");

                //External ram (eram)
                case 0xA000:
                case 0xB000:
                    return _eram[pc & 0x1FFF];  //Address is truncated to 13 bits to fit 8k array

                //Working ram (wram)
                case 0xC000:
                case 0xD000:
                    return _wram[pc & 0x1FFF];

                //Working ram shadow
                case 0xE000:
                    return _wram[pc & 0x1FFF];
                
                //Working ram shadow / I/O / Zero-page RAM
                case 0xF000:
                    switch (pc & 0x0F00)
                    {
                        //Working ram shadow
                        case 0x000:
                        case 0x100:
                        case 0x200:
                        case 0x300:
                        case 0x400:
                        case 0x500:
                        case 0x600:
                        case 0x700:
                        case 0x800:
                        case 0x900:
                        case 0xA00:
                        case 0xB00:
                        case 0xC00:
                        case 0xD00:
                            return _wram[pc & 0x1FFF];

                        //Graphics object attribute memory
                        case 0xE00:
                            if (pc < 0xFEA0) throw new NotImplementedException("Error: GPU features not implemented");
                            throw new MemoryReadException($"Error: Invalid address specified ({"0x" + pc.ToString("X")}");

                        case 0xF00:
                            if (pc < 0xFF81) return _zram[pc & 0x7F]; //Address truncated to 7 bits to fit 128 bit array
                            throw new NotImplementedException("Error: I/O control features not implemented");

                        default:
                            throw new MemoryReadException($"Error: MMU could not read data at address {"0x" + pc.ToString("X")}");
                    }

                default:
                    throw new MemoryReadException($"Error: MMU could not read data at address {"0x" + pc.ToString("X")}");
            }
        }
    }
}
