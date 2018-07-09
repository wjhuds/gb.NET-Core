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
        private byte interruptEnable = 0x00;

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
            catch (IndexOutOfRangeException)
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
            //BIOS / Rom bank 0
            if (0x0000 <= pc && pc <= 0x3FFF)
            {
                if (_inBios)
                {
                    if (pc < 0x0100) return _bios[pc];
                    _inBios = false;
                }
                return _rom[pc];
            }

            //Rom bank 1
            if (0x4000 <= pc && pc <= 0x7FFF)
            {
                return _rom[pc];
            }

            //GPU VRAM
            if (0x8000 <= pc && pc <= 0x9FFF)
            {
                throw new NotImplementedException("Error: GPU features not implemented");
            }

            //External ram (eram)
            if (0xA000 <= pc && pc <= 0xBFFF)
            {
                //pcess is truncated to 13 bits to fit 8k array
                return _eram[pc & 0x1FFF];
            }

            //Working ram (wram)
            if (0xC000 <= pc && pc <= 0xDFFF)
            {
                return _wram[pc & 0x1FFF];
            }

            //Working ram shadow
            if (0xE000 <= pc && pc <= 0xFDFF)
            {
                return _wram[pc & 0x1FFF];
            }

            //Graphics object attribute memory
            if (0xFE00 <= pc && pc <= 0xFE9F)
            {
                throw new NotImplementedException("Error: GPU features not implemented");
            }
            
            //Hardware I/O registers
            if (0xFF00 <= pc && pc <= 0xFF7F)
            {
                throw new NotImplementedException("Error: I/O features not implemented");
            }

            //High ram (hram)
            if (0xFF80 <= pc && pc <= 0xFFFE)
            {
                return _zram[pc & 0x7F];
            }

            //Interrupt enable register
            if (pc == 0xFFFF)
            {
                return interruptEnable;
            }

            throw new MemoryReadException($"Error: MMU could not read data at address 0x{pc:X}");
        }

        public ushort ReadWord(ushort pc)
        {
            return (ushort)((ReadByte((ushort)(pc + 1)) << 8) | ReadByte(pc));
        }

        public void WriteByte(ushort addr, byte val)
        {
            //Read only area
            if (addr < 0x8000)
            {
                throw new MemoryWriteException($"Error: Attempted to write data to a read-only address! Address: 0x{addr:X}");
            }

            //Restricted area
            if (0xFEA0 <= addr && addr <= 0xFEFE)
            {
                throw new MemoryWriteException($"Error: Attempted to write data to a restricted address! Address: 0x{addr:X}");
            }

            //GPU VRAM
            if (0x8000 <= addr && addr <= 0x9FFF)
            {
                throw new NotImplementedException("Error: GPU features not implemented");
            }

            //External ram (eram)
            if (0xA000 <= addr && addr <= 0xBFFF)
            {
                //Address is truncated to 13 bits to fit 8k array
                _eram[addr & 0x1FFF] = val;
                return;
            }

            //Working ram (wram)
            if (0xC000 <= addr && addr <= 0xDFFF)
            {
                _wram[addr & 0x1FFF] = val;
                return;
            }

            //Working ram shadow
            if (0xE000 <= addr && addr <= 0xFDFF)
            {
                _wram[addr & 0x1FFF] = val;
                return;
            }

            //Graphics object attribute memory
            if (0xFE00 <= addr && addr <= 0xFE9F)
            {
                throw new NotImplementedException("Error: GPU features not implemented");
            }
            
            //Hardware I/O registers
            if (0xFF00 <= addr && addr <= 0xFF7F)
            {
                throw new NotImplementedException("Error: I/O features not implemented");
            }

            //High ram (hram)
            if (0xFF80 <= addr && addr <= 0xFFFE)
            {
                _zram[addr & 0x7F] = val;
                return;
            }

            //Interrupt enable register
            if (addr == 0xFFFF)
            {
                interruptEnable = val;
            }

            throw new MemoryWriteException($"Error: MMU could not write data at address 0x{addr:X}");
        }
    }
}
