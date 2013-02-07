using Avis.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Common.Tests.IO
{
    [TestClass]
    public class TestXdrCoding
    {
        [TestMethod]
        public void stringIO()
        {
            MemoryStream buff = new MemoryStream();

            XdrCoding.putString(buff, "");

            Assert.AreEqual(4, buff.Position);

            buff.Position = 0;

            XdrCoding.putString(buff, "hello");

            Assert.AreEqual(12, buff.Position);

            buff.Position = 0;
            Assert.AreEqual("hello", XdrCoding.getString(buff));
        }

        [TestMethod]
        public void utf8()
        {
            roundtrip("Hello A\u0308\uFB03ns this is some bogus text");
            roundtrip("Hi there \u00C4\uFB03ns more bogus text");

            // some UTF-8 data seen in the wild that caused problems...
            roundtrip
              (Encoding.UTF8.GetString(
                (new byte[] {(byte)0xc3, (byte)0x94, (byte)0xc3, (byte)0xb8, 
                      (byte)0xce, (byte)0xa9, 0x73, 0x20, 0x73, 0x75, 0x70,
                      0x70, 0x6f, 0x73, 0x65, 0x20})));
        }

        private void roundtrip(String str)
        {
            MemoryStream buff = new MemoryStream();

            XdrCoding.putString(buff, str);

            buff.Position = 0;

            Assert.AreEqual(str, XdrCoding.getString(buff));
        }

        [TestMethod]
        public void nameValueIO()
        {
            MemoryStream buff = new MemoryStream();
            Dictionary<String, Object> nameValues = new Dictionary<String, Object>();

            XdrCoding.putNameValues(buff, nameValues);
            Assert.AreEqual(4, buff.Position);

            buff.Position = 0;
            nameValues.Add("int", 42);
            nameValues.Add("opaque", new byte[] { 1, 2, 3 });

            XdrCoding.putNameValues(buff, nameValues);
            Assert.AreEqual(44, buff.Position);

            buff.Position = 0;

            // the default equal only compares the byte arrays for object equality which will fail
            // so check it manually
            var res = XdrCoding.getNameValues(buff);
            Assert.AreEqual(nameValues.Count, res.Count);
            Assert.AreEqual(nameValues["int"], res["int"]);
            Assert.IsTrue(((byte[])nameValues["opaque"]).SequenceEqual((byte[])res["opaque"]));                        
        }

        [TestMethod]
        public void objectsIO()
        {
            MemoryStream buff = new MemoryStream();
            Object[] objects = new Object[] { "hello", 42 };

            XdrCoding.putObjects(buff, objects);

            buff.Position = 0;

            Object[] objectsCopy = XdrCoding.getObjects(buff);

            Assert.IsTrue(objects.SequenceEqual(objectsCopy));
        }

        [TestMethod]
        public void padding()
        {
            Assert.AreEqual(0, XdrCoding.paddingFor(0));
            Assert.AreEqual(3, XdrCoding.paddingFor(1));
            Assert.AreEqual(2, XdrCoding.paddingFor(2));
            Assert.AreEqual(1, XdrCoding.paddingFor(3));
            Assert.AreEqual(0, XdrCoding.paddingFor(4));
            Assert.AreEqual(3, XdrCoding.paddingFor(5));
            Assert.AreEqual(3, XdrCoding.paddingFor(25));
            Assert.AreEqual(3, XdrCoding.paddingFor(4 * 1234 + 1));
        }

    }
}
