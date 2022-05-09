using nanoFramework.TestFramework;
using System;

namespace IntegrationTests
{
    [TestClass]
    public class Tests
    {
        [Setup]
        public void SetupConnectToWifi()
        {
            // Comment next line to run the tests on a real hardware
            // Adjust your SSID and password in the constants
            // currently fails with exception if the following is uncommented.
            //Assert.SkipTest("Skipping setup as needs secret");
        }

        [TestMethod]
        public void DeployedOnRealTarget()
        {
            //Assert.SkipTest("Skipping test as needs checking");
        }

        [TestMethod]
        public void CheckNugetsWork()
        {
            //Assert.SkipTest("Skipping test as needs checking");
        }

        [TestMethod]
        public void CheckTestCiResultsUpload()
        {
            //testing what happens in CI.
            Assert.True(true);
            // Currently nothing is o/p to folder TestResults!!!
        }
    }
}
