using System;
using NUnit.Framework;
using UGTS.Encoder.WPF;

namespace UGTS.Encoder.UnitTest
{
    [TestFixture]
    public class ObservableTests
    {
        [Test]
        public void TestValuesAndImplicitCasting()
        {
            var n = new Observable<int>(3);
            int i = n;
            Assert.AreEqual(3, n.Value);
            Assert.AreEqual(3, i);

            var c = new Computed<int>(() => 7);
            int i2 = c;
            Assert.AreEqual(7, c.Value);
            Assert.AreEqual(7, i2);
        }

        [Test]
        public void TestBasicEvents()
        {
            var eventCount = 0;
            var oldValue = -1;
            var newValue = -1;

            var n = new Observable<int>(33);
            n.ValueChanged += (source, changes) => { eventCount += 1; oldValue = changes.OldValue; newValue = changes.NewValue; };
            n.Value = 74;
            Assert.AreEqual(1, eventCount);
            Assert.AreEqual(33, oldValue);
            Assert.AreEqual(74, newValue);

            var m = new Observable<string>("init");
            m.ValueChanged += (o, e) => n.Value = -1;

            m.Value = "init";
            Assert.AreEqual(74, n.Value);
            Assert.AreEqual(1, eventCount);

            m.Value = "newValue";
            Assert.AreEqual(-1, n.Value);
            Assert.AreEqual(2, eventCount);
            Assert.AreEqual(74, oldValue);
            Assert.AreEqual(-1, newValue);

            n.Value = 0;
            Assert.AreEqual(3, eventCount);
            Assert.AreEqual(-1, oldValue);
            Assert.AreEqual(0, newValue);

            m.Value = "newValue"; // value did not change, so no event should be fired, n should not be changed to -1, and eventCount should stay the same
            Assert.AreEqual(0, n);
            Assert.AreEqual(3, eventCount);
        }

        [Test]
        public void TestEqualityComparer()
        {
            var eventCount = 0;
            var t = new Observable<float>(3.001f) {EqualityComparer = (a, b) => Math.Abs(a - b) <= 0.00001};
            t.ValueChanged += (source, changes) => { eventCount += 1; };
            var newValue = 3.0009995f;
            t.Value = newValue;
            Assert.AreEqual(0, eventCount);
            Assert.AreNotEqual(newValue, t.Value);

            newValue = 3.002f;
            t.Value = newValue;
            Assert.AreEqual(1, eventCount);
            Assert.AreEqual(newValue, t.Value);

            t.EqualityComparer = null;
            newValue = 3.0020055f;
            t.Value = newValue;
            Assert.AreEqual(2, eventCount);
            Assert.AreEqual(newValue, t.Value);
        }

        [Test]
        public void TestComputedAndDependencies()
        {
            var oldFull = "";
            var newFull = "";

            var first = new Observable<string>("Ben");
            var last = new Observable<string>("Siron");
            var full = new Computed<string>(() => first + " " + last);
            full.ValueChanged += (source, changes) => { oldFull = changes.OldValue; newFull = changes.NewValue; };
            Assert.AreEqual("Ben Siron", full.Value);
            last.Value = "Ziron";
            Assert.AreEqual("Ben Ziron", full.Value);
            Assert.AreEqual("Ben Siron", oldFull);
            Assert.AreEqual("Ben Ziron", newFull);

            var eventCount = 0;
            var n = new Observable<int>(3);
            var m = new Observable<int>(20);
            var c = new Computed<int>(() => n + m*2 + 1);
            c.ValueChanged += (source, changes) => eventCount += 1;
            Assert.AreEqual(44, c.Value);
            n.Value = 1;
            Assert.AreEqual(1, eventCount);
            m.Value = 11;
            Assert.AreEqual(2, eventCount);
            Assert.AreEqual(24, c.Value);
        }

        [Test]
        public void TestShortCircuitLimitations()
        {
            var eventCount = 0;
            var a = new Observable<int>(3);
            var b = new Observable<string>("hey!");
            var c = new Computed<int>(() => (a > 0) ? a : b.Value.Length);
            c.ValueChanged += (source, changes) => { eventCount += 1; };
            a.Value = 4;
            Assert.AreEqual(1, eventCount); // c depends on a

            b.Value = "nay!";
            Assert.AreEqual(1, eventCount); // but not yet on b because of the short-circuit; c is not yet aware that it sometimes depends on b.

            a.Value = -1;
            Assert.AreEqual(2, eventCount);

            b.Value = "hey hey!";
            Assert.AreEqual(3, eventCount); // but now c knows that it depends on b.

            a.Value = 1;
            Assert.AreEqual(4, eventCount);

            b.Value = "hey hey hey!";
            Assert.AreEqual(5, eventCount); // and it still depends on b, because once dependent, always dependent
        }

        [Test]
        public void TestNestedDependencies()
        {
            var eventCount = 0;
            var a = new Observable<int>(7);
            var b = new Observable<int>(4);
            var c1 = new Computed<int>(() => a + b);
            var c2 = new Computed<int>(() => (c1 * 2) + b); // c2 has an indirect dependency on a
            c2.ValueChanged += (source, changes) => { eventCount += 1; };

            Assert.AreEqual(26, c2.Value);
            Assert.AreEqual(0, eventCount);
            
            a.Value = 9;
            Assert.AreEqual(30, c2.Value);
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void TestPropertyChanged()
        {
            var eventCount = 0;
            var computedCount = 0;
            var n = new Observable<int>(7);
            n.PropertyChanged += (source, args) => { eventCount += 1; };
            n.Value = 9;
            Assert.AreEqual(1, eventCount);

            var c = new Computed<int>(() => n * 2);
            c.PropertyChanged += (source, args) => { computedCount += 1; };
            n.Value = 11;

            Assert.AreEqual(2, eventCount);
            Assert.AreEqual(1, computedCount);
        }
    }
}
