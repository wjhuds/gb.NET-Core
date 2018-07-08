using Sandbox.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Sandbox.Server
{
    public class Emulator
    {
        private readonly IHubContext<DevHub> _hubContext;

        public Emulator(IHubContext<DevHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public static void Start(string[] args)
        {
            _hubContext.Clients 
            if (args.Length == 0)
            {
                _hub.SendConsoleLog("USAGE: gbnet rom_path [verbose]");
                return;
            }

            var biosPath = "bios.gb";
            var romPath = args[0];

            var verbose = (args.Length > 1 && args[1] == "true") ? true : false;

            _hub.SendConsoleLog($"Verbose mode: <{(verbose ? "ON" : "OFF")}>");

            hub.SendConsoleLog("Bootstrapping emulator components...");

            hub.SendConsoleLog("Decoding BIOS into byte values...");
            byte[] bios = File.ReadAllBytes(biosPath);

            hub.SendConsoleLog("Initializing component: MMU...");
            var mmu = new MMU(bios);

            hub.SendConsoleLog("Initializing component: CPU...");
            var cpu = new CPU(mmu);

            hub.SendConsoleLog("Decoding ROM into byte values...");
            byte[] rom = File.ReadAllBytes(romPath);

            hub.SendConsoleLog("Loading ROM data into MMU...");
            mmu.LoadRom(rom);

            hub.SendConsoleLog("Starting CPU cycle...");
            cpu.Start(verbose);

            hub.SendConsoleLog("System terminated. Presss any key to exit...");
        }
    }
}
