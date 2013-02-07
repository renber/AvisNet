using Avis.Common.Tests.Mocks;
using Avis.IO.Messages;
using Avis.IO.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Avis.Common.Tests.IO
{
    [TestClass]
    public class TestLiveness
    {
        [TestMethod]
        public void DetectDeadConnection()
        {
            ManualResetEvent evt = new ManualResetEvent(false);

            var mock = new Mock<IConnector>();            

            LivenessMonitor monitor = new LivenessMonitor(mock.Object, 500, 500);            
            monitor.ConnectionDied += (snd, e) =>
                {
                    evt.Set();
                };
            monitor.Enable();

            if (!evt.WaitOne(1000))
            {
                // ConnectionDied event was not fired
                Assert.Fail("Dead connection not detected.");
            }
        }

        [TestMethod]
        public void TestMonitoringMessages()
        {
            var mock = new Mock<IConnector>();
            mock.Setup(x => x.Connected).Returns(true);

            LivenessMonitor monitor = new LivenessMonitor(mock.Object, 500, 500);
            monitor.Enable();

            mock.Raise(x => x.MessageReceived += null, new MessageEvent(new DummyMessage()));
            Assert.IsTrue(monitor.IsUp);
            Thread.Sleep(250);
            mock.Raise(x => x.MessageReceived += null, new MessageEvent(new DummyMessage()));
            Assert.IsTrue(monitor.IsUp);
            Thread.Sleep(250);
            mock.Raise(x => x.MessageReceived += null, new MessageEvent(new DummyMessage()));
            Assert.IsTrue(monitor.IsUp);
            Thread.Sleep(2000); // provoke timeout
            Assert.IsFalse(monitor.IsUp);
        }

        [TestMethod]
        public void TestSendingKeepAliveMessage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);

            var mock = new Mock<IConnector>();
            mock.Setup(x => x.Send(It.IsAny<Message>())).Callback(new Action<Message>((msg) =>
            {
                if (msg.TypeId == TestConn.ID)
                {
                    evt.Set();
                }
            }));
            mock.Setup(x => x.Connected).Returns(true);

            LivenessMonitor monitor = new LivenessMonitor(mock.Object, 500, 500);
            monitor.Enable();

            if (!evt.WaitOne(1000))
            {
                Assert.Fail("TestConn not sent.");
            }
        }

        [TestMethod]
        public void TestReceiveIAmAliveMessage()
        {
            bool testConnSent = false;

            var mock = new Mock<IConnector>();           
            mock.Setup(x => x.Connected).Returns(true);

            LivenessMonitor monitor = new LivenessMonitor(mock.Object, 500, 500);
            monitor.Enable();

            // make sure that TestConn is sent
            mock.Setup(x => x.Send(It.IsAny<Message>())).Callback(new Action<Message>((msg) =>
            {
                testConnSent = true;
            }));

            Thread.Sleep(650);
            mock.Raise(x => x.MessageReceived += null, new MessageEvent(ConfConn.INSTANCE)); // send "i am alive"
            Assert.IsTrue(monitor.IsUp);
            Thread.Sleep(2000); // let the conenction die
            Assert.IsFalse(monitor.IsUp);
            Assert.IsTrue(testConnSent); // TestConn was sent
        }

    }
}
