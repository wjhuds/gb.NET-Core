using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Sandbox.Server
{
    public class DevHub : Hub
    {
        public async Task StartEmulation(string[] args)
        {
            Task.Factory.StartNew(() => {
            var emu = new Emulator();
            emu.Start(args));
        }
        public void SendConsoleLog(string message)
        {
            Clients.All.SendAsync("receiveConsoleLog", message);
        }
    }
}