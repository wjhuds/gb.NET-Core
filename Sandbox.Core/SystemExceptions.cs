using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Core
{
    class MemoryReadException : Exception
    {
        public MemoryReadException()
        {
        }

        public MemoryReadException(string message) : base(message)
        {
        }

        public MemoryReadException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    class MemoryWriteException : Exception
    {
        public MemoryWriteException()
        {
        }

        public MemoryWriteException(string message) : base(message)
        {
        }

        public MemoryWriteException(string message, Exception inner) : base(message, inner)
        {
        }
    }

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
