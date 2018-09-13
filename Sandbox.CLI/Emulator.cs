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
                Console.WriteLine("USAGE: gbnet bios_path rom_path [verbose]");
                Console.ReadKey();  //Temporary while system is run inside VS
                System.Environment.Exit(1);
            }

            var biosPath = args[0];
            var romPath = args[1];

            var verbose = (args.Length > 2 && args[2] == "true") ? true : false;

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
