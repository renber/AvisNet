using Avis.IO.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Common.Tests.IO
{
    [TestClass]
    public class TestNack
    {
        [TestMethod]
        public void formattedMessage()
        {
            Nack nack = new Nack();

            nack.args = new Object[] { "foo", "bar" };
            nack.message = "There was a %1 in the %2 (%1, %2) %3 %";

            String formattedMessage = nack.FormattedMessage;

            Assert.AreEqual("There was a foo in the bar (foo, bar) %3 %",
                          formattedMessage);
        }
    }
}
