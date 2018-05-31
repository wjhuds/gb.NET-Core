using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Core
{
    class OpCodeException : Exception
    {
        public OpCodeException()
        {
        }

        public OpCodeException(string message) : base(message)
        {
        }

        public OpCodeException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
