using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace DX.Utils.Tests
{
    public class Person
    {
        public int Id { get; set; }
        public long Big { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
        public double Score { get; set; }
        public DateTime Born { get; set; }
        public int? Optional { get; set; }
        public List<string> Tags { get; set; }
        public int[] Numbers { get; set; }
        private string Secret { get; set; } = "s3cr3t";
        public string GetSecret() => Secret;
    }

    public class QueryModel
    {
        public string Q { get; set; }
        public int Page { get; set; }
        [AppendQueryIgnore]
        public string Hidden { get; set; }
        [AppendQueryFormat("{0:yyyy-MM-dd}")]
        public DateTime? Date { get; set; }
        public string Empty { get; set; }
    }

    public class WithIndexer
    {
        public string Name { get; set; } = "x";
        public string this[int i] => i.ToString();
    }

    public class ReadOnlyName
    {
        public string Name { get; } = "fixed";
    }

    public class PropertyExtensionsTests
    {
        [Fact]
        public void HasProperty()
        {
            var p = new Person();
            Assert.True(p.HasProperty("Name"));
            Assert.False(p.HasProperty("name"));
            Assert.False(p.HasProperty("Nope"));
        }

        [Fact]
        public void GetPropertyValue()
        {
            var p = new Person { Name = "Don", Id = 5 };
            Assert.Equal("Don", p.GetPropertyValue<string>("Name"));
            Assert.Equal(5, p.GetPropertyValue<int>("Id"));
            Assert.Null(p.GetPropertyValue<string>("Nope"));
        }

        [Fact]
        public void SetPropertyValue_ConvertsSpecialTypes()
        {
            var p = new Person();
            p.SetPropertyValue("Salary", 12.5);
            p.SetPropertyValue("Score", 3);
            p.SetPropertyValue("Id", 7L);
            p.SetPropertyValue("Big", 9);
            p.SetPropertyValue("Name", 42);
            p.SetPropertyValue("Born", "2024-01-02");
            Assert.Equal(12.5m, p.Salary);
            Assert.Equal(3.0, p.Score);
            Assert.Equal(7, p.Id);
            Assert.Equal(9L, p.Big);
            Assert.Equal("42", p.Name);
            Assert.Equal(new DateTime(2024, 1, 2), p.Born);
        }

        [Fact]
        public void SetPropertyValue_Generic()
        {
            var p = new Person();
            p.SetPropertyValue<string>("Name", "x");
            Assert.Equal("x", p.Name);
        }

        [Fact]
        public void SetPropertyValue_UnknownProperty_Throws()
        {
            Assert.ThrowsAny<Exception>(() => new Person().SetPropertyValue("Nope", 1));
        }

        [Fact]
        public void AssignToObject_GenericDictionary()
        {
            IDictionary<string, object> d = new Dictionary<string, object> { ["Name"] = "A", ["Id"] = 3L, ["Unknown"] = 1 };
            var p = d.AssignToObject(new Person());
            Assert.Equal("A", p.Name);
            Assert.Equal(3, p.Id);
        }

        [Fact]
        public void AssignToObject_NonGenericDictionary()
        {
            IDictionary d = new Hashtable { ["Name"] = "B", ["Score"] = 1.5f };
            var p = d.AssignToObject(new Person());
            Assert.Equal("B", p.Name);
            Assert.Equal(1.5, p.Score);
        }

        [Fact]
        public void Assign_CopiesMatchingProperties_WithExclusions()
        {
            var src = new Person { Id = 1, Name = "src", Score = 2 };
            var dst = src.Assign(new Person(), "Score");
            Assert.Equal(1, dst.Id);
            Assert.Equal("src", dst.Name);
            Assert.Equal(0, dst.Score);
        }

        [Fact(Skip = "BUG: Assign/AsDictionary with excludeProperties == null copy nothing; the 'skipExcl' flag is inverted ('!skipExcl && ...')")]
        public void Assign_NullExclusions_CopiesEverything()
        {
            var src = new Person { Id = 1, Name = "src" };
            var dst = src.Assign(new Person(), null);
            Assert.Equal("src", dst.Name);
            Assert.Equal("src", src.AsDictionary(null)["Name"]);
        }

        [Fact]
        public void AsDictionary_IncludesPublicProperties()
        {
            var d = new Person { Name = "n" }.AsDictionary();
            Assert.Equal("n", d["Name"]);
            Assert.True(d.ContainsKey("Id"));
        }

        [Fact(Skip = "BUG: GetInfo uses BindingFlags.NonPublic, so private properties leak into AsDictionary / AsQueryParameters (and HasProperty)")]
        public void AsDictionary_ExcludesPrivateProperties()
        {
            var p = new Person();
            Assert.False(p.AsDictionary().ContainsKey("Secret"));
            Assert.False(p.HasProperty("Secret"));
        }

        [Fact(Skip = "BUG: indexer properties are not filtered out; AsDictionary calls GetValue on 'Item' and throws TargetParameterCountException")]
        public void AsDictionary_TypeWithIndexer_DoesNotThrow()
        {
            var d = new WithIndexer().AsDictionary();
            Assert.Equal("x", d["Name"]);
        }

        [Fact(Skip = "BUG: Assign/SetPropertyValue do not check CanWrite; a get-only (computed) property on the destination throws ArgumentException 'Property set method not found'")]
        public void Assign_ToReadOnlyProperty_IsSkipped()
        {
            var dst = new ReadOnlyName().Assign(new ReadOnlyName());
            Assert.Equal("fixed", dst.Name);
        }

        [Fact]
        public void IsCollectionProperty_TrueForCollections()
        {
            Assert.True(typeof(Person).GetProperty("Tags").IsCollectionProperty());
            Assert.True(typeof(Person).GetProperty("Numbers").IsCollectionProperty());
            Assert.False(typeof(Person).GetProperty("Id").IsCollectionProperty());
        }

        [Fact(Skip = "BUG: string implements IEnumerable<char>, so the IEnumerable<> check returns true for string before the explicit string exclusion is reached")]
        public void IsCollectionProperty_FalseForString()
        {
            Assert.False(typeof(Person).GetProperty("Name").IsCollectionProperty());
        }

        [Fact]
        public void AsQueryParameters_Null_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, ((object)null).AsQueryParameters());
        }

        [Fact]
        public void AsQueryParameters_SkipsIgnoredAndEmpty_AppliesFormat()
        {
            var q = new QueryModel { Q = "abc", Page = 2, Hidden = "h", Date = new DateTime(2024, 3, 5), Empty = " " }.AsQueryParameters();
            var parsed = HttpUtility.ParseQueryString(q);
            Assert.Equal("abc", parsed["Q"]);
            Assert.Equal("2", parsed["Page"]);
            Assert.Equal("2024-03-05", parsed["Date"]);
            Assert.Null(parsed["Hidden"]);
            Assert.Null(parsed["Empty"]);
        }

        [Fact(Skip = "BUG: AsQueryParameters URL-encodes each value and then NameValueCollection.ToString() encodes again, so 'a b&c' round-trips as 'a+b%26c'")]
        public void AsQueryParameters_ValuesAreEncodedOnce()
        {
            var q = new QueryModel { Q = "a b&c" }.AsQueryParameters();
            Assert.Equal("a b&c", HttpUtility.ParseQueryString(q)["Q"]);
        }

        [Fact]
        public void AppendQueryParameters_ProducesQuery()
        {
            var u = new Uri("https://example.com/search").AppendQueryParameters(new QueryModel { Q = "x", Page = 1 });
            Assert.Equal("example.com", u.Host);
            Assert.Equal("/search", u.AbsolutePath);
            Assert.Equal("x", HttpUtility.ParseQueryString(u.Query)["Q"]);
        }

        [Fact(Skip = "BUG: AppendQueryParameters replaces the existing query string instead of appending to it ('?keep=1' is lost)")]
        public void AppendQueryParameters_KeepsExistingQuery()
        {
            var u = new Uri("https://example.com/search?keep=1").AppendQueryParameters(new QueryModel { Q = "x" });
            var parsed = HttpUtility.ParseQueryString(u.Query);
            Assert.Equal("1", parsed["keep"]);
            Assert.Equal("x", parsed["Q"]);
        }

        [Fact]
        public void ConcurrentAccess_IsSafe()
        {
            Parallel.For(0, 2000, i =>
            {
                var p = new Person();
                p.SetPropertyValue("Id", i);
                Assert.Equal(i, p.GetPropertyValue<int>("Id"));
            });
        }
    }
}
