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
            var initialPC = _cpuInstance.Reg_PC;

            //Act
            _cpuInstance.Tick();

            //Assert
            Assert.Equals(_cpuInstance.Reg_PC, initialPC + 1);
        }
    }
}
