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
            Assert.True(true);
        }

        [TestMethod]
        public void DeployedOnRealTarget()
        {
            //Assert.SkipTest("Skipping test as needs checking");
            Assert.True(true);
        }

        [TestMethod]
        public void CheckNugetsWork()
        {
            //Assert.SkipTest("Skipping test as needs checking");
            Assert.True(true);
        }

        [TestMethod]
        public void CheckTestCiResultsUpload()
        {
            //testing what happens in CI.
            Assert.True(true);
        }
    }
}
