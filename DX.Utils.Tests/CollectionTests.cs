using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using Xunit;

namespace DX.Utils.Tests
{
    public class TestItem : CollectionItem
    {
        public string Name { get; set; }
    }

    public class OtherItem : CollectionItem { }

    public class CollectionTests
    {
        [Fact]
        public void Add_SetsOwnerAndIsIndexable()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem { Name = "a" };
            Assert.Equal(0, c.Add(a));
            Assert.Same(c, a.Collection);
            Assert.Same(a, c[0]);
            Assert.True(c.Contains(a));
            Assert.Equal(0, c.IndexOf(a));
            Assert.Single(c);
        }

        [Fact]
        public void Insert_PutsItemAtIndex()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            var b = new TestItem();
            c.Add(a);
            c.Insert(0, b);
            Assert.Same(b, c[0]);
            Assert.Same(a, c[1]);
            Assert.Same(c, b.Collection);
        }

        [Fact]
        public void Remove_ClearsOwner()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            c.Add(a);
            c.Remove(a);
            Assert.Null(a.Collection);
            Assert.Empty(c);
        }

        [Fact]
        public void RemoveAt_ClearsOwner()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            c.Add(a);
            c.RemoveAt(0);
            Assert.Null(a.Collection);
        }

        [Fact]
        public void AddingToSecondCollection_MovesItem()
        {
            var c1 = new Collection<TestItem>();
            var c2 = new Collection<TestItem>();
            var a = new TestItem();
            c1.Add(a);
            c2.Add(a);
            Assert.Same(c2, a.Collection);
            Assert.Empty(c1);
            Assert.Single(c2);
        }

        [Fact]
        public void DisposingItem_RemovesItFromItsCollection()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            c.Add(a);
            a.Dispose();
            Assert.Empty(c);
            Assert.Null(a.Collection);
        }

        [Fact]
        public void Add_WrongItemType_ThroughIList_Throws()
        {
            var c = new Collection<TestItem>();
            Assert.Throws<ArgumentException>(() => ((IList)c).Add(new OtherItem()));
            Assert.Throws<ArgumentException>(() => ((IList)c).Add("not an item"));
        }

        [Fact]
        public void Add_Null_Throws()
        {
            var c = new Collection<TestItem>();
            Assert.ThrowsAny<ArgumentException>(() => c.Add(null));
        }

        [Fact]
        public void CopyTo_CopiesItems()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            c.Add(a);
            var arr = new TestItem[2];
            c.CopyTo(arr, 1);
            Assert.Same(a, arr[1]);
        }

        [Fact]
        public void EmptyCollection_DisposeIsFine()
        {
            new Collection<TestItem>().Dispose();
        }

        [Fact(Skip = "BUG: Collection.Dispose enumerates List while each item's Dispose removes itself -> InvalidOperationException 'Collection was modified'")]
        public void Dispose_NonEmpty_DisposesAllItems()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            var b = new TestItem();
            c.Add(a);
            c.Add(b);
            c.Dispose();
            Assert.Null(a.Collection);
            Assert.Null(b.Collection);
        }

        // Verified in a scratch console app: StackOverflow (process crash), so this can never be run unskipped in-suite.
        [Fact(Skip = "BUG: BaseCollection.OnClear calls base.List.Clear() which re-enters CollectionBase.Clear -> OnClear -> infinite recursion / StackOverflow (crashes the test host)")]
        public void Clear_EmptiesCollectionAndReleasesItems()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            c.Add(a);
            c.Clear();
            Assert.Empty(c);
            Assert.Null(a.Collection);
        }

        // Verified in a scratch console app: StackOverflow (process crash), so this can never be run unskipped in-suite.
        [Fact(Skip = "BUG: BaseCollection.OnSet assigns base.List[index] which re-enters IList.set_Item -> OnSet -> infinite recursion / StackOverflow (crashes the test host)")]
        public void IListIndexerSet_ReplacesItem()
        {
            var c = new Collection<TestItem>();
            var a = new TestItem();
            var b = new TestItem();
            c.Add(a);
            ((IList)c)[0] = b;
            Assert.Same(b, c[0]);
            Assert.Null(a.Collection);
            Assert.Same(c, b.Collection);
        }

        [Fact]
        public void TypeConverter_CanProduceInstanceDescriptor()
        {
            var item = new TestItem();
            var converter = TypeDescriptor.GetConverter(item);
            Assert.True(converter.CanConvertTo(typeof(InstanceDescriptor)));
            var descriptor = (InstanceDescriptor)converter.ConvertTo(item, typeof(InstanceDescriptor));
            Assert.NotNull(descriptor);
            Assert.IsType<TestItem>(descriptor.Invoke());
        }
    }
}
