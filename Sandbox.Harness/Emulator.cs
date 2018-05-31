using Sandbox.Core;
using System;

namespace Sandbox.Harness
{
    class Emulator
    {
        static void Main(string[] args)
        {
            var verbose = args[0] == "true" ? true : false;

            Console.WriteLine("Starting emulation...");
            Console.WriteLine($"Verbose mode: <{(verbose ? "ON" : "OFF")}>");

            Console.WriteLine("Initializing component: MMU...");
            var mmu = new MMU();

            Console.WriteLine("Initializing component: CPU...");
            var cpu = new CPU(mmu);

            Console.WriteLine("Loading BIOS into memory...");
            //load bios here

            Console.WriteLine("Starting CPU...");
            cpu.Start(verbose);
        }
    }
}
