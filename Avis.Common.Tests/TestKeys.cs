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

                keys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
                keys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));
                keys.add(KeyScheme.SHA1_PRODUCER, new Key("producer key 1"));
                keys.add(KeyScheme.SHA1_CONSUMER, new Key("consumer key 1"));
                keys.add(KeyScheme.SHA1_CONSUMER, new Key("consumer key 2"));

                // test roundtrip encode/decode
                keys.encode(buff);

                buff.Seek(0, SeekOrigin.Begin); // reset to start

                Keys newKeys = Keys.decode(buff);

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
            keys1.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            keys1.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));

            keys1.add(KeyScheme.SHA1_PRODUCER, new Key("producer key 1"));
            keys1.add(KeyScheme.SHA1_CONSUMER, new Key("consumer key 1"));

            Keys keys2 = new Keys();
            keys2.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            keys2.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));

            keys2.add(KeyScheme.SHA1_PRODUCER, new Key("producer key 1"));
            keys2.add(KeyScheme.SHA1_CONSUMER, new Key("consumer key 1"));

            Assert.AreEqual(keys1.GetHashCode(), keys2.GetHashCode());
            Assert.AreEqual(keys1, keys2);

            keys2.remove(KeyScheme.SHA1_CONSUMER, new Key("consumer key 1"));

            Assert.IsFalse(keys1.Equals(keys2));
        }

        /// <summary>
        /// Test add/remove of single keys.
        /// </summary>
        [TestMethod]
        public void addRemove()
        {
            Keys keys = new Keys();

            keys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            keys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));

            keys.add(KeyScheme.SHA1_PRODUCER, new Key("producer key 1"));
            keys.add(KeyScheme.SHA1_CONSUMER, new Key("consumer key 1"));

            DualKeySet dualKeys = keys.keysetFor(KeyScheme.SHA1_DUAL);

            Assert.AreEqual(1, dualKeys.consumerKeys.Count);
            Assert.AreEqual(1, dualKeys.producerKeys.Count);

            keys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key 3"));
            Assert.AreEqual(2, keys.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);

            keys.remove(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key 3"));
            Assert.AreEqual(1, keys.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);

            keys.remove(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key 1"));
            Assert.AreEqual(0, keys.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);

            // remove a non-existent key, check nothing Bad happens
            keys.remove(KeyScheme.SHA1_CONSUMER, new Key("blah"));

            keys.remove(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("dual key 2"));
            Assert.AreEqual(0, keys.keysetFor(KeyScheme.SHA1_DUAL).consumerKeys.Count);

            SingleKeySet consumerKeys = keys.keysetFor(KeyScheme.SHA1_CONSUMER);
            Assert.AreEqual(1, consumerKeys.Count);
            keys.remove(KeyScheme.SHA1_CONSUMER, new Key("consumer key 1"));
            Assert.AreEqual(0, consumerKeys.Count);

            SingleKeySet producerKeys = keys.keysetFor(KeyScheme.SHA1_PRODUCER);
            Assert.AreEqual(1, producerKeys.Count);
            keys.remove(KeyScheme.SHA1_PRODUCER, new Key("producer key 1"));
            Assert.AreEqual(0, producerKeys.Count);

            Assert.IsTrue(keys.IsEmpty);

            // remove a non-existent key, check nothing Bad happens
            keys.remove(KeyScheme.SHA1_CONSUMER, new Key("blah"));
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

            keys1.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key prod 1"));
            keys1.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("dual key cons 1"));
            keys1.add(KeyScheme.SHA1_PRODUCER, new Key("producer key 1.1"));
            keys1.add(KeyScheme.SHA1_PRODUCER, new Key("producer key 1.2"));

            keys2.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("dual key prod 2"));
            keys2.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("dual key cons 2"));
            keys2.add(KeyScheme.SHA1_CONSUMER, new Key("consumer key 1"));

            // add keys in bulk to keys3
            keys3.add(keys1);

            Assert.AreEqual(1, keys3.keysetFor(KeyScheme.SHA1_DUAL).consumerKeys.Count);
            Assert.AreEqual(1, keys3.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);
            Assert.AreEqual(2, keys3.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(0, keys3.keysetFor(KeyScheme.SHA1_CONSUMER).Count);

            keys3.add(keys2);
            Assert.AreEqual(2, keys3.keysetFor(KeyScheme.SHA1_DUAL).consumerKeys.Count);
            Assert.AreEqual(2, keys3.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);
            Assert.AreEqual(2, keys3.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(1, keys3.keysetFor(KeyScheme.SHA1_CONSUMER).Count);

            keys3.remove(keys1);
            Assert.AreEqual(1, keys3.keysetFor(KeyScheme.SHA1_DUAL).consumerKeys.Count);
            Assert.AreEqual(1, keys3.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);
            Assert.AreEqual(0, keys3.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(1, keys3.keysetFor(KeyScheme.SHA1_CONSUMER).Count);

            keys3.remove(keys2);

            Assert.IsTrue(keys3.IsEmpty);
        }

        [TestMethod]
        public void delta()
        {
            Keys addedKeys = new Keys();
            Keys removedKeys = new Keys();
            Keys baseKeys = new Keys();

            addedKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("added/removed key"));
            addedKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("added key 1"));
            addedKeys.add(KeyScheme.SHA1_PRODUCER, new Key("added key 2"));

            removedKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("added/removed key"));
            removedKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("non existent key"));
            removedKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("removed key"));

            baseKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("kept key"));
            baseKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("removed key"));

            Keys delta = baseKeys.delta(addedKeys, removedKeys);

            Keys correctKeys = new Keys();
            correctKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("added key 1"));
            correctKeys.add(KeyScheme.SHA1_PRODUCER, new Key("added key 2"));
            correctKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, new Key("kept key"));

            Assert.AreEqual(correctKeys, delta);

            // check delta works with empty keys
            Keys keys4 = Keys.EMPTY_KEYS.delta(addedKeys, removedKeys);

            correctKeys = new Keys();
            correctKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, new Key("added key 1"));
            correctKeys.add(KeyScheme.SHA1_PRODUCER, new Key("added key 2"));

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

            Delta delta = keys1.deltaFrom(keys2);

            Assert.AreEqual(0, delta.added.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(0, delta.added.keysetFor(KeyScheme.SHA1_CONSUMER).Count);
            Assert.AreEqual(0, delta.added.keysetFor(KeyScheme.SHA1_DUAL).Count);

            Assert.AreEqual(0, delta.removed.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(0, delta.removed.keysetFor(KeyScheme.SHA1_CONSUMER).Count);
            Assert.AreEqual(0, delta.removed.keysetFor(KeyScheme.SHA1_DUAL).Count);

            // add a single producer key
            keys2.add(KeyScheme.SHA1_PRODUCER, key1);

            delta = keys1.deltaFrom(keys2);
            Assert.AreEqual(1, delta.added.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(0, delta.removed.keysetFor(KeyScheme.SHA1_PRODUCER).Count);

            checkApplyDelta(delta, keys1, keys2);

            // remove a single producer key
            keys1.add(KeyScheme.SHA1_PRODUCER, key2);

            delta = keys1.deltaFrom(keys2);
            Assert.AreEqual(1, delta.added.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(1, delta.removed.keysetFor(KeyScheme.SHA1_PRODUCER).Count);

            checkApplyDelta(delta, keys1, keys2);

            // key1 is now in both sets
            keys1.add(KeyScheme.SHA1_PRODUCER, key1);

            delta = keys1.deltaFrom(keys2);
            Assert.AreEqual(0, delta.added.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(1, delta.removed.keysetFor(KeyScheme.SHA1_PRODUCER).Count);

            // key2 is not in both
            keys2.add(KeyScheme.SHA1_PRODUCER, key2);

            delta = keys1.deltaFrom(keys2);
            Assert.AreEqual(0, delta.added.keysetFor(KeyScheme.SHA1_PRODUCER).Count);
            Assert.AreEqual(0, delta.removed.keysetFor(KeyScheme.SHA1_PRODUCER).Count);

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

            keys1.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, key1);
            keys1.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, key2);
            keys1.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, key3);

            keys2.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, key3);
            keys2.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, key3);

            Delta delta = keys1.deltaFrom(keys2);
            Assert.AreEqual(1, delta.added.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);
            Assert.AreEqual(1, delta.removed.keysetFor(KeyScheme.SHA1_DUAL).producerKeys.Count);

            Assert.AreEqual(0, delta.added.keysetFor(KeyScheme.SHA1_DUAL).consumerKeys.Count);
            Assert.AreEqual(1, delta.removed.keysetFor(KeyScheme.SHA1_DUAL).consumerKeys.Count);

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
            Assert.AreEqual(keys1.delta(delta.added, delta.removed), keys2);
        }

        [TestMethod]
        public void producer()
        {
            Key alicePrivate = new Key("alice private");
            Key alicePublic = alicePrivate.publicKeyFor(KeyScheme.SHA1_PRODUCER);

            Keys aliceNtfnKeys = new Keys();
            aliceNtfnKeys.add(KeyScheme.SHA1_PRODUCER, alicePrivate);

            Keys bobSubKeys = new Keys();
            bobSubKeys.add(KeyScheme.SHA1_PRODUCER, alicePublic);

            Keys eveSubKeys = new Keys();
            eveSubKeys.add(KeyScheme.SHA1_PRODUCER, new Key("Not alice's key").publicKeyFor(KeyScheme.SHA1_PRODUCER));

            Assert.IsTrue(bobSubKeys.match(aliceNtfnKeys));
            Assert.IsFalse(eveSubKeys.match(aliceNtfnKeys));
        }

        [TestMethod]
        public void consumer()
        {
            Key bobPrivate = new Key("bob private");
            Key bobPublic = bobPrivate.publicKeyFor(KeyScheme.SHA1_CONSUMER);

            Keys aliceNtfnKeys = new Keys();
            aliceNtfnKeys.add(KeyScheme.SHA1_CONSUMER, bobPublic);

            Keys bobSubKeys = new Keys();
            bobSubKeys.add(KeyScheme.SHA1_CONSUMER, bobPrivate);

            Keys eveSubKeys = new Keys();
            eveSubKeys.add(KeyScheme.SHA1_CONSUMER, new Key("Not bob's key"));

            Assert.IsTrue(bobSubKeys.match(aliceNtfnKeys));
            Assert.IsFalse(eveSubKeys.match(aliceNtfnKeys));
        }

        [TestMethod]
        public void dual()
        {
            Key alicePrivate = new Key("alice private");
            Key alicePublic = alicePrivate.publicKeyFor(KeyScheme.SHA1_DUAL);
            Key bobPrivate = new Key("bob private");
            Key bobPublic = bobPrivate.publicKeyFor(KeyScheme.SHA1_DUAL);

            Keys aliceNtfnKeys = new Keys();
            aliceNtfnKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, bobPublic);
            aliceNtfnKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, alicePrivate);

            Keys bobSubKeys = new Keys();
            bobSubKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, bobPrivate);
            bobSubKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, alicePublic);

            Keys eveSubKeys = new Keys();
            Key randomPrivate = new Key("Not bob's key");
            eveSubKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Consumer, randomPrivate);
            eveSubKeys.add(KeyScheme.SHA1_DUAL, DualKeyScheme.Subset.Producer, randomPrivate.publicKeyFor(KeyScheme.SHA1_DUAL));

            Assert.IsTrue(bobSubKeys.match(aliceNtfnKeys));
            Assert.IsFalse(aliceNtfnKeys.match(bobSubKeys));
            Assert.IsFalse(eveSubKeys.match(aliceNtfnKeys));
        }
    }
}
