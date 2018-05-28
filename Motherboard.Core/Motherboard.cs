using System;

namespace Motherboard.Core
{
    public class Motherboard
    {
        private Motherboard instance;

        public CPU Cpu { get; set; }

        private Motherboard()
        {
            
        }

        

        public Motherboard GetInstance()
        {
            if (instance == null)
            {
                instance = new Motherboard();
                return instance;
            }
            return instance;
        }
    }
}
