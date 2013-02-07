/*
 * port of org.avis.common.JUTestElvinURI
 */

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Avis.Common.Tests
{
    [TestClass]
    public class TestElvinURI
    {
        [TestMethod]

        public void version()
        {
            ElvinURI uri = new ElvinURI("elvin://elvin_host");

            Assert.AreEqual(Common.ClientVersionMajor, uri.VersionMajor);
            Assert.AreEqual(Common.ClientVersionMinor, uri.VersionMinor);
            Assert.AreEqual("elvin_host", uri.Host);

            uri = new ElvinURI("elvin:5.1//elvin_host");

            Assert.AreEqual(5, uri.VersionMajor);
            Assert.AreEqual(1, uri.VersionMinor);
            Assert.AreEqual("elvin_host", uri.Host);

            uri = new ElvinURI("elvin:5//elvin_host");

            Assert.AreEqual(5, uri.VersionMajor);
            Assert.AreEqual(Common.ClientVersionMinor, uri.VersionMinor);
            Assert.AreEqual("elvin_host", uri.Host);

         /*   AssertInvalid("http:hello//elvin_host");
            AssertInvalid("elvin:hello//elvin_host");
            AssertInvalid("elvin:4.0.0//elvin_host");
            AssertInvalid("elvin:4.//elvin_host");
            AssertInvalid("elvin: //elvin_host");
            AssertInvalid("elvin:111111111111111.2222222222222222222//elvin_host"); */
        }

        [TestMethod]
        public void protocol()
        {
            ElvinURI uri = new ElvinURI("elvin://elvin_host");

            Assert.AreEqual(Protocols.DefaultProtocol, uri.Protocol);

            uri = new ElvinURI("elvin:/tcp,xdr,ssl/elvin_host");
            Assert.IsTrue(uri.Protocol.SequenceEqual(new List<String>() { "tcp", "xdr", "ssl" }));            
            Assert.AreEqual("elvin_host", uri.Host);

            uri = new ElvinURI("elvin:/secure/elvin_host");

            Assert.AreEqual(Protocols.SecureProtocol, uri.Protocol);

            AssertInvalid("elvin:/abc,xyz/elvin_host");
            AssertInvalid("elvin:/abc,xyz,dfg,qwe/elvin_host");
            AssertInvalid("elvin:/abc,/elvin_host");
            AssertInvalid("elvin:/,abc/elvin_host");
            AssertInvalid("elvin:/abc,,xyz/elvin_host");
            AssertInvalid("elvin:///elvin_host");
        }

        [TestMethod]
        public void endpoint()
        {
            ElvinURI uri = new ElvinURI("elvin://elvin_host");
            Assert.AreEqual("elvin_host", uri.Host);
            Assert.AreEqual(Common.DefaultPort, uri.Port);

            uri = new ElvinURI("elvin://elvin_host:12345");
            Assert.AreEqual("elvin_host", uri.Host);
            Assert.AreEqual(12345, uri.Port);

            AssertInvalid("elvin://");
            AssertInvalid("elvin://hello:there");
        }

        [TestMethod]
        public void options()
        {
            ElvinURI uri = new ElvinURI("elvin://elvin_host;name1=value1");

            var d = new Dictionary<String, String>();
            d.Add("name1", "value1");
            CollectionAssert.AreEquivalent(d, uri.Options);

            uri = new ElvinURI("elvin://elvin_host;name1=value1;name2=value2");
            
            d.Add("name2", "value2");
            CollectionAssert.AreEquivalent(d, uri.Options);

            AssertInvalid("elvin://elvin_host;name1;name2=value2");
            AssertInvalid("elvin://elvin_host;=name1;name2=value2");
            AssertInvalid("elvin://elvin_host;");
            AssertInvalid("elvin://;x=y");
            AssertInvalid("elvin://;x=");
        }

        [TestMethod]
        public void equality()
        {
            assertSameUri("elvin://elvin_host", "elvin://elvin_host:2917");
            assertSameUri("elvin://elvin_host", "elvin:/tcp,none,xdr/elvin_host");

            assertNotSameUri("elvin://elvin_host", "elvin:/tcp,ssl,xdr/elvin_host");
            assertNotSameUri("elvin://elvin_host", "elvin://elvin_host:29170");
            assertNotSameUri("elvin://elvin_host", "elvin://elvin_host;name=value");
        }

        [TestMethod]
        public void canonicalize()
        {
            ElvinURI uri = new ElvinURI("elvin://elvin_host");
            Assert.AreEqual("elvin://elvin_host", uri.ToString());

            Assert.AreEqual("elvin:4.0/tcp,none,xdr/elvin_host:2917",
                          uri.ToCanonicalString());

            uri = new ElvinURI("elvin://elvin_host;name1=value1");
            Assert.AreEqual("elvin:4.0/tcp,none,xdr/elvin_host:2917;name1=value1",
                          uri.ToCanonicalString());

            uri = new ElvinURI("elvin:/secure/elvin_host:29170;b=2;a=1");
            Assert.AreEqual("elvin:4.0/ssl,none,xdr/elvin_host:29170;a=1;b=2",
                          uri.ToCanonicalString());

            uri = new ElvinURI("elvin:5.1/secure/elvin_host:29170;b=2;a=1");
            Assert.AreEqual("elvin:5.1/ssl,none,xdr/elvin_host:29170;a=1;b=2",
                          uri.ToCanonicalString());
        }

        [TestMethod]
        public void constructors()
        {
            ElvinURI defaultUri = new ElvinURI("elvin:5.6/a,b,c/default_host:1234");

            ElvinURI uri = new ElvinURI("elvin://host", defaultUri);

            Assert.AreEqual(defaultUri.Scheme, "elvin");
            Assert.AreEqual(defaultUri.VersionMajor, uri.VersionMajor);
            Assert.AreEqual(defaultUri.VersionMinor, uri.VersionMinor);
            Assert.AreEqual(defaultUri.Protocol, uri.Protocol);
            Assert.AreEqual("host", uri.Host);
            Assert.AreEqual(defaultUri.Port, uri.Port);

            uri = new ElvinURI("elvin:7.0/x,y,z/host:5678", defaultUri);

            Assert.AreEqual(7, uri.VersionMajor);
            Assert.AreEqual(0, uri.VersionMinor);
            Assert.IsTrue(uri.Protocol.SequenceEqual(new List<String>() { "x", "y", "z" }));            
            Assert.AreEqual("host", uri.Host);
            Assert.AreEqual(5678, uri.Port);
        }

        [TestMethod]
        public void ipv6()
        {
            ElvinURI uri =
              new ElvinURI("elvin://[2001:0db8:85a3:08d3:1319:8a2e:0370:7344]:1234");

            Assert.AreEqual("[2001:0db8:85a3:08d3:1319:8a2e:0370:7344]", uri.Host);
            Assert.AreEqual(1234, uri.Port);

            uri = new ElvinURI("elvin:/tcp,xdr,ssl/[::1/128]:4567");

            Assert.IsTrue(uri.Protocol.SequenceEqual(new List<String>() { "tcp", "xdr", "ssl" }));            
            Assert.AreEqual("[::1/128]", uri.Host);
            Assert.AreEqual(4567, uri.Port);

            AssertInvalid("elvin://[::1/128");
            AssertInvalid("elvin://[[::1/128");
            AssertInvalid("elvin://[::1/128]]");
            AssertInvalid("elvin://[]");
            AssertInvalid("elvin://[");
            AssertInvalid("elvin://[::1/128];hello");
            AssertInvalid("elvin://[::1/128]:xyz");
            AssertInvalid("elvin://[::1/128];");
            AssertInvalid("elvin:///[::1/128]");
        }

        private static void assertSameUri(String uri1, String uri2)
        {
            Assert.AreEqual(new ElvinURI(uri1), new ElvinURI(uri2));
            Assert.AreEqual(new ElvinURI(uri1).GetHashCode(), new ElvinURI(uri2).GetHashCode());
        }

        private static void assertNotSameUri(String uri1, String uri2)
        {
            Assert.IsFalse(new ElvinURI(uri1).Equals(new ElvinURI(uri2)));
        }

        private static void AssertInvalid(String uriString)
        {
            try
            {
                new ElvinURI(uriString);

                Assert.Fail("Invalid URI \"" + uriString + "\" not detected");
            }
            catch (UriFormatException ex)
            {
                // ok, expected exception                
            }
        }
    }
}
