using MediatR;
using System.Diagnostics;

namespace LightInject.MemoryLeakTest
{
    [TestClass]
    public sealed class TestMemoryLeak : TestsBase
    {
        /// <summary>
        /// How to detect memory increase?
        /// - debug the test
        /// - open diagnostics window and verify the increasing memory usage
        /// </summary>
        public static IEnumerable<object[]> Iterations => Enumerable.Range(1, 10000).Select(i => new object[] { i });
        [DynamicData(nameof(Iterations))]
        [DataTestMethod]
        public void MemoryLeakTest(int iteration)
        {
            //Arrange

            //Act
            var mediatr = GetInstance<IMediator>();

            //Assert
            Trace.WriteLine($"Iteration {iteration}");

            Process currentProc = Process.GetCurrentProcess();
            var bytesInUse = currentProc.PrivateMemorySize64;
            Trace.WriteLine("Private bytes: " + bytesInUse);

            //For simplicity check process is not using more then 500MB of memory
            Assert.IsTrue(bytesInUse < 500_000_000);
        }
    }
}
