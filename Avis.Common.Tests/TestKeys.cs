using Avis.Immigrated;
using Avis.Security;
using Avis.Security.Special;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Common.Tests
{
    [TestClass]
    public class TestKeys
    {
        [TestMethod]
        public void IO()
        {
            using (MemoryStream buff = new MemoryStream())
            {
                Keys keys = new Keys();

                keys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
                keys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));
                keys.Add(KeyScheme.Sha1Producer, new Key("producer key 1"));
                keys.Add(KeyScheme.Sha1Consumer, new Key("consumer key 1"));
                keys.Add(KeyScheme.Sha1Consumer, new Key("consumer key 2"));

                // test roundtrip encode/decode
                keys.Encode(buff);

                buff.Seek(0, SeekOrigin.Begin); // reset to start

                Keys newKeys = Keys.Decode(buff);

                Assert.IsTrue(keys.Equals(newKeys));
            }
        }

        [TestMethod]
        public void equality()
        {
            Assert.IsTrue(new Key("a test key number 1").Equals(new Key("a test key number 1")));
            Assert.IsFalse(new Key("a test key number 1").Equals(new Key("a test key number 2")));

            Assert.AreEqual(new Key("a test key number 1").GetHashCode(),
                          new Key("a test key number 1").GetHashCode());

            // test Keys.equals ()
            Keys keys1 = new Keys();
            keys1.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            keys1.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));

            keys1.Add(KeyScheme.Sha1Producer, new Key("producer key 1"));
            keys1.Add(KeyScheme.Sha1Consumer, new Key("consumer key 1"));

            Keys keys2 = new Keys();
            keys2.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            keys2.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));

            keys2.Add(KeyScheme.Sha1Producer, new Key("producer key 1"));
            keys2.Add(KeyScheme.Sha1Consumer, new Key("consumer key 1"));

            Assert.AreEqual(keys1.GetHashCode(), keys2.GetHashCode());
            Assert.AreEqual(keys1, keys2);

            keys2.Remove(KeyScheme.Sha1Consumer, new Key("consumer key 1"));

            Assert.IsFalse(keys1.Equals(keys2));
        }

        /// <summary>
        /// Test add/remove of single keys.
        /// </summary>
        [TestMethod]
        public void addRemove()
        {
            Keys keys = new Keys();

            keys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            keys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));

            keys.Add(KeyScheme.Sha1Producer, new Key("producer key 1"));
            keys.Add(KeyScheme.Sha1Consumer, new Key("consumer key 1"));

            DualKeySet dualKeys = keys.KeysetFor(KeyScheme.Sha1Dual);

            Assert.AreEqual(1, dualKeys.ConsumerKeys.Count);
            Assert.AreEqual(1, dualKeys.ProducerKeys.Count);

            keys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key 3"));
            Assert.AreEqual(2, keys.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);

            keys.Remove(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key 3"));
            Assert.AreEqual(1, keys.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);

            keys.Remove(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            Assert.AreEqual(0, keys.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);

            // remove a non-existent key, check nothing Bad happens
            keys.Remove(KeyScheme.Sha1Consumer, new Key("blah"));

            keys.Remove(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));
            Assert.AreEqual(0, keys.KeysetFor(KeyScheme.Sha1Dual).ConsumerKeys.Count);

            SingleKeySet consumerKeys = keys.KeysetFor(KeyScheme.Sha1Consumer);
            Assert.AreEqual(1, consumerKeys.Count);
            keys.Remove(KeyScheme.Sha1Consumer, new Key("consumer key 1"));
            Assert.AreEqual(0, consumerKeys.Count);

            SingleKeySet producerKeys = keys.KeysetFor(KeyScheme.Sha1Producer);
            Assert.AreEqual(1, producerKeys.Count);
            keys.Remove(KeyScheme.Sha1Producer, new Key("producer key 1"));
            Assert.AreEqual(0, producerKeys.Count);

            Assert.IsTrue(keys.IsEmpty);

            // remove a non-existent key, check nothing Bad happens
            keys.Remove(KeyScheme.Sha1Consumer, new Key("blah"));
        }

        /// <summary>
        /// Test add/remove of entire keysets.
        /// </summary>
        [TestMethod]
        public void addRemoveSets()
        {
            Keys keys1 = new Keys();
            Keys keys2 = new Keys();
            Keys keys3 = new Keys();

            keys1.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key prod 1"));
            keys1.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("dual key cons 1"));
            keys1.Add(KeyScheme.Sha1Producer, new Key("producer key 1.1"));
            keys1.Add(KeyScheme.Sha1Producer, new Key("producer key 1.2"));

            keys2.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("dual key prod 2"));
            keys2.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("dual key cons 2"));
            keys2.Add(KeyScheme.Sha1Consumer, new Key("consumer key 1"));

            // add keys in bulk to keys3
            keys3.Add(keys1);

            Assert.AreEqual(1, keys3.KeysetFor(KeyScheme.Sha1Dual).ConsumerKeys.Count);
            Assert.AreEqual(1, keys3.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);
            Assert.AreEqual(2, keys3.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(0, keys3.KeysetFor(KeyScheme.Sha1Consumer).Count);

            keys3.Add(keys2);
            Assert.AreEqual(2, keys3.KeysetFor(KeyScheme.Sha1Dual).ConsumerKeys.Count);
            Assert.AreEqual(2, keys3.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);
            Assert.AreEqual(2, keys3.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(1, keys3.KeysetFor(KeyScheme.Sha1Consumer).Count);

            keys3.remove(keys1);
            Assert.AreEqual(1, keys3.KeysetFor(KeyScheme.Sha1Dual).ConsumerKeys.Count);
            Assert.AreEqual(1, keys3.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);
            Assert.AreEqual(0, keys3.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(1, keys3.KeysetFor(KeyScheme.Sha1Consumer).Count);

            keys3.remove(keys2);

            Assert.IsTrue(keys3.IsEmpty);
        }

        [TestMethod]
        public void delta()
        {
            Keys addedKeys = new Keys();
            Keys removedKeys = new Keys();
            Keys baseKeys = new Keys();

            addedKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("added/removed key"));
            addedKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("added key 1"));
            addedKeys.Add(KeyScheme.Sha1Producer, new Key("added key 2"));

            removedKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("added/removed key"));
            removedKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("non existent key"));
            removedKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("removed key"));

            baseKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("kept key"));
            baseKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("removed key"));

            Keys delta = baseKeys.Alter(addedKeys, removedKeys);

            Keys correctKeys = new Keys();
            correctKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("added key 1"));
            correctKeys.Add(KeyScheme.Sha1Producer, new Key("added key 2"));
            correctKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, new Key("kept key"));

            Assert.AreEqual(correctKeys, delta);

            // check delta works with empty keys
            Keys keys4 = Keys.EmptyKeys.Alter(addedKeys, removedKeys);

            correctKeys = new Keys();
            correctKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, new Key("added key 1"));
            correctKeys.Add(KeyScheme.Sha1Producer, new Key("added key 2"));

            Assert.AreEqual(correctKeys, keys4);
        }

        /// <summary>
        /// Basic test of computeDelta ()
        /// </summary>
        [TestMethod]
        public void computeDeltaSingleScheme()
        {
            Keys keys1 = new Keys();
            Keys keys2 = new Keys();

            Key key1 = new Key("key 1");
            Key key2 = new Key("key 2");

            Delta delta = keys1.DeltaFrom(keys2);

            Assert.AreEqual(0, delta.Added.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(0, delta.Added.KeysetFor(KeyScheme.Sha1Consumer).Count);
            Assert.AreEqual(0, delta.Added.KeysetFor(KeyScheme.Sha1Dual).Count);

            Assert.AreEqual(0, delta.Removed.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(0, delta.Removed.KeysetFor(KeyScheme.Sha1Consumer).Count);
            Assert.AreEqual(0, delta.Removed.KeysetFor(KeyScheme.Sha1Dual).Count);

            // add a single producer key
            keys2.Add(KeyScheme.Sha1Producer, key1);

            delta = keys1.DeltaFrom(keys2);
            Assert.AreEqual(1, delta.Added.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(0, delta.Removed.KeysetFor(KeyScheme.Sha1Producer).Count);

            checkApplyDelta(delta, keys1, keys2);

            // remove a single producer key
            keys1.Add(KeyScheme.Sha1Producer, key2);

            delta = keys1.DeltaFrom(keys2);
            Assert.AreEqual(1, delta.Added.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(1, delta.Removed.KeysetFor(KeyScheme.Sha1Producer).Count);

            checkApplyDelta(delta, keys1, keys2);

            // key1 is now in both sets
            keys1.Add(KeyScheme.Sha1Producer, key1);

            delta = keys1.DeltaFrom(keys2);
            Assert.AreEqual(0, delta.Added.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(1, delta.Removed.KeysetFor(KeyScheme.Sha1Producer).Count);

            // key2 is not in both
            keys2.Add(KeyScheme.Sha1Producer, key2);

            delta = keys1.DeltaFrom(keys2);
            Assert.AreEqual(0, delta.Added.KeysetFor(KeyScheme.Sha1Producer).Count);
            Assert.AreEqual(0, delta.Removed.KeysetFor(KeyScheme.Sha1Producer).Count);

            checkApplyDelta(delta, keys1, keys2);
        }

        /// <summary>
        /// Test computeDelta () with a dual key set.
        /// </summary>
        [TestMethod]
        public void computeDeltaDual()
        {
            Keys keys1 = new Keys();
            Keys keys2 = new Keys();

            Key key1 = new Key("key 1");
            Key key2 = new Key("key 2");
            Key key3 = new Key("key 3");

            keys1.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, key1);
            keys1.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, key2);
            keys1.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, key3);

            keys2.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, key3);
            keys2.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, key3);

            Delta delta = keys1.DeltaFrom(keys2);
            Assert.AreEqual(1, delta.Added.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);
            Assert.AreEqual(1, delta.Removed.KeysetFor(KeyScheme.Sha1Dual).ProducerKeys.Count);

            Assert.AreEqual(0, delta.Added.KeysetFor(KeyScheme.Sha1Dual).ConsumerKeys.Count);
            Assert.AreEqual(1, delta.Removed.KeysetFor(KeyScheme.Sha1Dual).ConsumerKeys.Count);

            checkApplyDelta(delta, keys1, keys2);
        }

      /// <summary>
        /// Check applying delta to keys1 gives keys2
      /// </summary>
      /// <param name="delta"></param>
      /// <param name="keys1"></param>
      /// <param name="keys2"></param>
        private static void checkApplyDelta(Delta delta,
                                             Keys keys1, Keys keys2)
        {
            Assert.AreEqual(keys1.Alter(delta.Added, delta.Removed), keys2);
        }

        [TestMethod]
        public void producer()
        {
            Key alicePrivate = new Key("alice private");
            Key alicePublic = alicePrivate.PublicKeyFor(KeyScheme.Sha1Producer);

            Keys aliceNtfnKeys = new Keys();
            aliceNtfnKeys.Add(KeyScheme.Sha1Producer, alicePrivate);

            Keys bobSubKeys = new Keys();
            bobSubKeys.Add(KeyScheme.Sha1Producer, alicePublic);

            Keys eveSubKeys = new Keys();
            eveSubKeys.Add(KeyScheme.Sha1Producer, new Key("Not alice's key").PublicKeyFor(KeyScheme.Sha1Producer));

            Assert.IsTrue(bobSubKeys.Match(aliceNtfnKeys));
            Assert.IsFalse(eveSubKeys.Match(aliceNtfnKeys));
        }

        [TestMethod]
        public void consumer()
        {
            Key bobPrivate = new Key("bob private");
            Key bobPublic = bobPrivate.PublicKeyFor(KeyScheme.Sha1Consumer);

            Keys aliceNtfnKeys = new Keys();
            aliceNtfnKeys.Add(KeyScheme.Sha1Consumer, bobPublic);

            Keys bobSubKeys = new Keys();
            bobSubKeys.Add(KeyScheme.Sha1Consumer, bobPrivate);

            Keys eveSubKeys = new Keys();
            eveSubKeys.Add(KeyScheme.Sha1Consumer, new Key("Not bob's key"));

            Assert.IsTrue(bobSubKeys.Match(aliceNtfnKeys));
            Assert.IsFalse(eveSubKeys.Match(aliceNtfnKeys));
        }

        [TestMethod]
        public void dual()
        {
            Key alicePrivate = new Key("alice private");
            Key alicePublic = alicePrivate.PublicKeyFor(KeyScheme.Sha1Dual);
            Key bobPrivate = new Key("bob private");
            Key bobPublic = bobPrivate.PublicKeyFor(KeyScheme.Sha1Dual);

            Keys aliceNtfnKeys = new Keys();
            aliceNtfnKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, bobPublic);
            aliceNtfnKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, alicePrivate);

            Keys bobSubKeys = new Keys();
            bobSubKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, bobPrivate);
            bobSubKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, alicePublic);

            Keys eveSubKeys = new Keys();
            Key randomPrivate = new Key("Not bob's key");
            eveSubKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Consumer, randomPrivate);
            eveSubKeys.Add(KeyScheme.Sha1Dual, DualKeyScheme.Subset.Producer, randomPrivate.PublicKeyFor(KeyScheme.Sha1Dual));

            Assert.IsTrue(bobSubKeys.Match(aliceNtfnKeys));
            Assert.IsFalse(aliceNtfnKeys.Match(bobSubKeys));
            Assert.IsFalse(eveSubKeys.Match(aliceNtfnKeys));
        }
    }
}
