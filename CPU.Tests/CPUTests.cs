using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sandbox.Core;

namespace Sandbox.Tests
{
    [TestClass]
    public class CPUTests
    {
        private CPU _cpuInstance;
        private Mock<MMU> _mockMMU;

        [TestMethod]
        public void TestInitialize()
        {
            //Arrange
            //..

            //Act
            _cpuInstance = new CPU(_mockMMU.Object);

            //Assert
            Assert.IsNotNull(_cpuInstance);
        }

        [TestMethod]
        public void TestTick()
        {
            //Arrange
            var initialPC = _cpuInstance.ProgramCounter;

            //Act
            _cpuInstance.Tick();

            //Assert
            Assert.Equals(_cpuInstance.ProgramCounter, initialPC + 1);
        }
    }
}
