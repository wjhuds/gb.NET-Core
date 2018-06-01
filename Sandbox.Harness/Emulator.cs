using Sandbox.Core;
using System;
using System.IO;

namespace Sandbox.Harness
{
    class Emulator
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("USAGE: gbnet rom_path [verbose]");
                Console.ReadKey();  //Temporary while system is run inside VS
                System.Environment.Exit(1);
            }

            var biosPath = "bios.gb";
            var romPath = args[0];

            var verbose = (args.Length > 1 && args[1] == "true") ? true : false;

            Console.WriteLine($"Verbose mode: <{(verbose ? "ON" : "OFF")}>");

            Console.WriteLine("Bootstrapping emulator components...");

            Console.WriteLine("Decoding BIOS into byte values...");
            byte[] bios = File.ReadAllBytes(biosPath);

            Console.WriteLine("Initializing component: MMU...");
            var mmu = new MMU(bios);

            Console.WriteLine("Initializing component: CPU...");
            var cpu = new CPU(mmu);

            Console.WriteLine("Decoding ROM into byte values...");
            byte[] rom = File.ReadAllBytes(romPath);

            Console.WriteLine("Loading ROM data into MMU...");
            mmu.LoadRom(rom);

            Console.WriteLine("Starting CPU cycle...");
            cpu.Start(verbose);

            Console.WriteLine("System terminated. Presss any key to exit...");
            Console.ReadKey();
            System.Environment.Exit(1);
        }
    }
}
