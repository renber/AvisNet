using Avis.IO;
using Avis.IO.Messages;
using Avis.IO.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Avis.Common.Tests.IO
{
    [TestClass]
    public class TestFrameCodec
    {
        [TestMethod]
        public void EncodeDecode()
        {
            ClientFrameCodec codec = ClientFrameCodec.Instance;

            var message = new NotifyDeliver();
            message.attributes.Add("string", BigString());
            message.attributes.Add("blob", new byte[1024 * 1024]);

            var stream = new MemoryStream();

            codec.Encode(message, stream);
            stream.Position = 0;
            NotifyDeliver resMsg = (NotifyDeliver)codec.Decode(stream);            

            Assert.AreEqual(message.attributes["string"], resMsg.attributes["string"]);
            Assert.AreEqual(((byte[])message.attributes["blob"]).Length,
                          ((byte[])resMsg.attributes["blob"]).Length);
        }


       [TestMethod]
        public void TestConnection()
        {
            AutoResetEvent evt = new AutoResetEvent(false);
            bool success = false;

            int port = 4000 + new Random().Next(500);
            ElvinListener con = new ElvinListener();

            var m = new NotifyDeliver();
            m.attributes.Add("string", BigString());
            m.attributes.Add("blob", new byte[1024 * 1024]);

            con.MessageReceived += (sender, e) =>
                {
                    NotifyDeliver rcvd = (NotifyDeliver) e.Message;

                    Assert.AreEqual(m.attributes["string"], rcvd.attributes["string"]);
                    Assert.AreEqual(((byte[])rcvd.attributes["blob"]).Length,
                                  ((byte[])m.attributes["blob"]).Length);

                    success = true;
                    evt.Set();
                };

            con.Listen(IPAddress.Parse("127.0.0.1"), port);

           // client
            ElvinConnector client = new ElvinConnector();
            client.Connect("127.0.0.1", port);
            client.Send(m);
            //con.Send(m, "127.0.0.1", port);

            // wait max. 3 seconds
            evt.WaitOne(3000);

            client.Close();
            con.Stop();            
            
            if (!success)
            {
                Assert.Fail("Connection was unsuccessful");
            }
        }

       private static String BigString()
       {
           StringBuilder str = new StringBuilder();

           for (int i = 0; i < 800 * 1024; i++)
               str.Append('A');

           return str.ToString();
       }

    }
}
