using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Core
{
    class MemoryReadException : Exception
    {
        public MemoryReadException() { }
        public MemoryReadException(string message) : base(message) { }
        public MemoryReadException(string message, Exception inner) : base(message, inner) { }
    }

    class MemoryWriteException : Exception
    {
        public MemoryWriteException() { }
        public MemoryWriteException(string message) : base(message) { }
        public MemoryWriteException(string message, Exception inner) : base(message, inner) { }
    }

    class InstructionOutOfRangeException : Exception
    {
        public InstructionOutOfRangeException() { }
        public InstructionOutOfRangeException(string message) : base(message) { }
        public InstructionOutOfRangeException(string message, Exception inner) : base(message, inner) { }
    }

    class InstructionNotImplementedException : Exception
    {
        public InstructionNotImplementedException() { }
        public InstructionNotImplementedException(string message) : base(message) { }
        public InstructionNotImplementedException(string message, Exception inner) : base(message, inner) { }
    }
}
