using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace DX.Utils.Tests
{
    public class RegExLibraryTests
    {
        [Fact]
        public void GetRegExGroupValue_ReturnsMatchedGroup()
        {
            var m = Regex.Match("key=value", "(?<k>\\w+)=(?<v>\\w+)");
            Assert.Equal("value", RegExLibrary.GetRegExGroupValue(m.Groups, "v"));
            Assert.Equal("key", RegExLibrary.GetRegExGroupValue(m.Groups, "k", "default"));
        }

        [Fact]
        public void GetRegExGroupValue_MissingGroup_ReturnsDefault()
        {
            var m = Regex.Match("key=", "(?<k>\\w+)=(?<v>\\w+)?");
            Assert.Equal("def", RegExLibrary.GetRegExGroupValue(m.Groups, "v", "def"));
            Assert.Equal("def", RegExLibrary.GetRegExGroupValue(m.Groups, "doesNotExist", "def"));
        }

        [Theory]
        [InlineData("john.doe@example.com", "john.doe", "example.com")]
        [InlineData("a_b-c@sub.domain.co.uk", "a_b-c", "sub.domain.co.uk")]
        [InlineData("x@[192.168.0.1]", "x", "[192.168.0.1")]
        public void EmailParser_ExtractsAliasAndDomain(string input, string alias, string domain)
        {
            var m = RegExLibrary.EmailParser.Match(input);
            Assert.True(m.Success);
            Assert.Equal(alias, m.Groups["Alias"].Value);
            Assert.Equal(domain, m.Groups["Domain"].Value);
        }

        [Theory]
        [InlineData("not an email")]
        [InlineData("@example.com")]
        [InlineData("user@localhost")]
        public void EmailParser_RejectsNonEmails(string input)
        {
            Assert.False(RegExLibrary.EmailParser.IsMatch(input));
        }

        [Fact]
        public void QueryStringParser_ExtractsPairs()
        {
            var pairs = RegExLibrary.QueryStringParser.Matches("/page.aspx?a=1&b=two&c=3")
                .Cast<Match>().Select(m => m.Groups["name"].Value + ":" + m.Groups["value"].Value).ToArray();
            Assert.Equal(new[] { "a:1", "b:two", "c:3" }, pairs);
        }

        [Fact]
        public void QueryStringParser_SkipsEmptyValues()
        {
            var names = RegExLibrary.QueryStringParser.Matches("?a=&b=2")
                .Cast<Match>().Select(m => m.Groups["name"].Value).ToArray();
            Assert.Equal(new[] { "b" }, names);
        }

        [Fact]
        public void UrlParser_SplitsAllParts()
        {
            var m = RegExLibrary.UrlParser.Match("https://user@host.com:8080/path/to/x.aspx?q=1&r=2#frag");
            Assert.True(m.Success);
            Assert.Equal("https", m.Groups["Scheme"].Value);
            Assert.Equal("user@host.com:8080", m.Groups["Authority"].Value);
            Assert.Equal("/path/to/x.aspx", m.Groups["Path"].Value);
            Assert.Equal("q=1&r=2", m.Groups["Query"].Value);
            Assert.Equal("frag", m.Groups["Fragment"].Value);
        }

        [Fact]
        public void UrlParser_RelativePath()
        {
            var m = RegExLibrary.UrlParser.Match("~/folder/page.aspx?x=1");
            Assert.False(m.Groups["Scheme"].Success);
            Assert.Equal("~/folder/page.aspx", m.Groups["Path"].Value);
            Assert.Equal("x=1", m.Groups["Query"].Value);
        }

        [Fact]
        public void SplitScriptParamsUrl_SplitsUrlAndParams()
        {
            var m = RegExLibrary.SplitScriptParamsUrl.Match("~/folder/page.aspx?id=5&x=y");
            Assert.True(m.Success);
            Assert.Equal("~/folder/page.aspx", m.Groups["Url"].Value);
            Assert.Equal("id=5&x=y", m.Groups["Params"].Value);
        }

        [Theory]
        [InlineData("abc", false)]
        [InlineData("a b", false)]
        [InlineData("a-b_c+d", false)]
        [InlineData("a!b", true)]
        [InlineData("a/b", true)]
        [InlineData("é", true)]
        public void SEOSafe_MatchesUnsafeCharacters(string input, bool hasUnsafe)
        {
            Assert.Equal(hasUnsafe, RegExLibrary.SEOSafe.IsMatch(input));
        }

        [Fact]
        public void MakeSEOSafe_KelvinSign_IsNormalisedToAsciiK()
        {
            // The raw SEOSafe regex (IgnoreCase) lets U+212A KELVIN SIGN through, but MakeSEOSafe normalises it to 'K' first.
            Assert.Equal("K", Strings.MakeSEOSafe("\u212A"));
        }

        [Fact]
        public void PageRewriteParser()
        {
            var m = RegExLibrary.PageRewriteParser.Match("~/Content/nl-NL/News/My-Article.aspx?p=1");
            Assert.True(m.Success);
            Assert.Equal("nl-NL", m.Groups["Country"].Value);
            Assert.Equal("News", m.Groups["Group"].Value);
            Assert.Equal("My-Article", m.Groups["SEOName"].Value);
            Assert.Equal("p=1", m.Groups["Params"].Value);
        }

        [Fact]
        public void ListDataRewriteParser()
        {
            var m = RegExLibrary.ListDataRewriteParser.Match("~/Data/en-US/Products/Bikes/Fast-One");
            Assert.True(m.Success);
            Assert.Equal("en-US", m.Groups["Country"].Value);
            Assert.Equal("Products", m.Groups["Datasource"].Value);
            Assert.Equal("Bikes", m.Groups["Group"].Value);
            Assert.Equal("Fast-One", m.Groups["SEOName"].Value);
        }

        [Fact]
        public void BlogRewriteParsers()
        {
            var m = RegExLibrary.BlogRewriteParser.Match("~/Publication/nl-NL/blog/2024/03/15/hello-world");
            Assert.True(m.Success);
            Assert.Equal("2024", m.Groups["PubYear"].Value);
            Assert.Equal("03", m.Groups["PubMonth"].Value);
            Assert.Equal("15", m.Groups["PubDay"].Value);
            Assert.Equal("hello-world", m.Groups["SEOName"].Value);

            var o = RegExLibrary.BlogOverviewRewriteParser.Match("~/Publications/nl-NL/blog/2024/03");
            Assert.True(o.Success);
            Assert.Equal("03", o.Groups["PubMonth"].Value);
            Assert.False(o.Groups["PubDay"].Success);
        }

        [Fact]
        public void SitemapAndHomepageParsers()
        {
            var s = RegExLibrary.SitemapRewriteParser.Match("~/nl-NL/about/team.aspx");
            Assert.True(s.Success);
            Assert.Equal("nl-NL", s.Groups["Country"].Value);
            Assert.Equal("about/team", s.Groups["MenuPath"].Value);

            Assert.True(RegExLibrary.HomepageRewriteParser.Match("~/default.aspx").Groups["Script"].Success);
            Assert.Equal("en-US", RegExLibrary.HomepageRewriteParser.Match("~/en-US/").Groups["Country"].Value);

            Assert.True(RegExLibrary.XmlSitemap.IsMatch("~/Sitemap/nl-NL"));
            Assert.True(RegExLibrary.XmlSitemap.IsMatch("~/Sitemap/nl-NL/"));
            Assert.False(RegExLibrary.XmlSitemap.IsMatch("~/Sitemap/nl-NL/x"));

            Assert.Equal("any/thing", RegExLibrary.CmsCustomUrlRewriteParser.Match("~/any/thing").Groups[1].Value);
        }

        [Fact]
        public void StaticRegexes_AreSafeForConcurrentUse()
        {
            Parallel.For(0, 2000, i =>
            {
                Assert.Equal("a-" + i, Strings.MakeSEOSafe("a " + i + "!"));
                Assert.True(RegExLibrary.EmailParser.IsMatch("u" + i + "@x.com"));
            });
        }
    }
}
