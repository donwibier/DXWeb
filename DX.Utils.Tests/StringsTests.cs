using System;
using System.Drawing;
using System.IO;
using Xunit;

namespace DX.Utils.Tests
{
    public class StripHtmlTagsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void NullOrEmpty_ReturnsEmpty(string input)
        {
            Assert.Equal(string.Empty, Strings.StripHTMLTags(input));
        }

        [Fact]
        public void PlainText_IsUnchanged()
        {
            Assert.Equal("Hello world", Strings.StripHTMLTags("Hello world"));
        }

        [Fact]
        public void SimpleTags_AreRemoved()
        {
            Assert.Equal("Hello world", Strings.StripHTMLTags("<b>Hello</b> <i>world</i>"));
        }

        [Fact]
        public void RepeatedSpaces_AreCollapsed()
        {
            Assert.Equal("a b", Strings.StripHTMLTags("a     b"));
        }

        [Fact]
        public void Tabs_AreRemoved()
        {
            Assert.Equal("ab", Strings.StripHTMLTags("a\t\tb"));
        }

        [Fact]
        public void Script_IsRemovedIncludingContent()
        {
            Assert.Equal("ab", Strings.StripHTMLTags("a<script type=\"text/javascript\">alert('x');</script>b"));
        }

        [Fact]
        public void MultiLineScript_IsRemovedIncludingContent()
        {
            Assert.Equal("ab", Strings.StripHTMLTags("a<SCRIPT>\nvar x = 1;\nalert(x);\n</SCRIPT>b"));
        }

        [Fact]
        public void Style_IsRemovedIncludingContent()
        {
            Assert.Equal("ab", Strings.StripHTMLTags("a<style media=\"all\">p { color: red; }</style>b"));
        }

        [Fact]
        public void SingleLineHead_IsRemovedIncludingTitle()
        {
            Assert.Equal("body", Strings.StripHTMLTags("<html><head><title>T</title></head><body>body</body></html>"));
        }

        [Fact]
        public void MultiLineHead_IsRemovedIncludingTitle()
        {
            Assert.Equal("body", Strings.StripHTMLTags("<html><head>\n<title>Secret title</title>\n</head><body>body</body></html>"));
        }

        [Fact]
        public void Br_BecomesCarriageReturn()
        {
            Assert.Equal("a\rb", Strings.StripHTMLTags("a<br/>b"));
            Assert.Equal("a\rb", Strings.StripHTMLTags("a<BR>b"));
        }

        [Fact]
        public void Paragraphs_BecomeDoubleBreaks_TrailingOnesTrimmed()
        {
            Assert.Equal("one\r\rtwo", Strings.StripHTMLTags("<p>one</p><p>two</p>"));
        }

        [Fact]
        public void ListItems_BecomeBullets()
        {
            var result = Strings.StripHTMLTags("<ul><li>a</li><li>b</li></ul>");
            Assert.Contains(" * a\r", result);
            Assert.Contains(" * b", result);
            Assert.DoesNotContain("<", result);
        }

        [Fact]
        public void TableCells_BecomeTabs()
        {
            Assert.Equal("\ta\tb", Strings.StripHTMLTags("<table><tr><td>a</td><td>b</td></tr></table>"));
        }

        [Fact]
        public void LineFeeds_AreNormalisedToCarriageReturns()
        {
            Assert.DoesNotContain("\n", Strings.StripHTMLTags("a\nb\r\nc"));
        }

        [Fact]
        public void ManyBreaks_AreCollapsedToTwo()
        {
            Assert.Equal("a\r\rb", Strings.StripHTMLTags("a<br><br><br><br><br><br>b"));
        }

        [Theory]
        [InlineData("&copy; 2024", "(c) 2024")]
        [InlineData("&reg;", "(r)")]
        [InlineData("&trade;", "(tm)")]
        [InlineData("a&frasl;b", "a/b")]
        [InlineData("&bull; x", " *  x")]
        [InlineData("a&#160;b", "a b")]
        public void KnownEntities_AreReplaced(string input, string expected)
        {
            Assert.Equal(expected, Strings.StripHTMLTags(input));
        }

        [Fact]
        public void Nbsp_BecomesSpace()
        {
            Assert.Equal("a b", Strings.StripHTMLTags("a&nbsp;b"));
        }

        [Fact]
        public void EscapedAngleBrackets_ArePreservedAsText()
        {
            Assert.Equal("5 < 6 and 7 > 3", Strings.StripHTMLTags("5 &lt; 6 and 7 &gt; 3"));
        }

        [Fact]
        public void BareAmpersand_InPlainText_IsPreserved()
        {
            Assert.Equal("Tom & Jerry; fun", Strings.StripHTMLTags("Tom & Jerry; fun"));
        }

        [Fact]
        public void KnownEntities_AreCaseInsensitive()
        {
            Assert.Equal("(c) a b", Strings.StripHTMLTags("&COPY; a&NBSP;b"));
        }

        [Fact]
        public void UnknownEntities_AreRemoved()
        {
            Assert.Equal("ab", Strings.StripHTMLTags("a&hellip;&#8217;&#x2019;b"));
        }

        [Fact]
        public void TagRemoval_DoesNotDependOnCurrentCulture()
        {
            using (new CultureScope("tr-TR"))
                Assert.Equal("ab", Strings.StripHTMLTags("a<SCRIPT>var i = 1;</SCRIPT>b"));
        }

        // Reference: the original (quadratic) collapsing loop, which the linear implementation must reproduce.
        static string LegacyCollapse(string result)
        {
            string breaks = "\r\r\r";
            string tabs = "\t\t\t\t\t";
            for (int index = 0; index < result.Length; index++)
            {
                result = result.Replace(breaks, "\r\r");
                result = result.Replace(tabs, "\t\t\t\t");
                breaks = breaks + "\r";
                tabs = tabs + "\t";
            }
            return result;
        }

        [Fact]
        public void BreakAndTabRuns_CollapseExactlyLikeTheLegacyLoop()
        {
            for (int n = 0; n <= 300; n++)
            {
                // "\n" -> "\r" and "<td>" -> "\t" happen before the collapsing step
                Assert.Equal(LegacyCollapse("a" + new string('\r', n) + "b"), Strings.StripHTMLTags("a" + new string('\n', n) + "b"));
                Assert.Equal(LegacyCollapse("a" + new string('\t', n) + "b"),
                    Strings.StripHTMLTags("a" + string.Concat(System.Linq.Enumerable.Repeat("<td>", n)) + "b"));
            }
        }

        [Fact]
        public void LongInput_IsEquivalentToConcatenatedParts()
        {
            // 200 KB of markup: completes quickly (the former loop was O(n^2)) and every
            // repeated block is stripped exactly like a single block.
            const string block = "<div><p>Lorem <b>ipsum</b> &copy; dolor<br>sit amet&nbsp;x</p>\n<ul><li>a</li></ul></div>";
            string single = Strings.StripHTMLTags(block);
            var html = string.Concat(System.Linq.Enumerable.Repeat(block, 2500));
            var result = Strings.StripHTMLTags(html);
            Assert.Equal(string.Join("\r\r", System.Linq.Enumerable.Repeat(single, 2500)), result);
        }

        [Fact]
        public void UnclosedAngleBracket_IsKept()
        {
            Assert.Equal("a < b", Strings.StripHTMLTags("a < b"));
        }

        [Fact]
        public void Unicode_IsPreserved()
        {
            Assert.Equal("Żółć 日本語 😀", Strings.StripHTMLTags("<span>Żółć</span> 日本語 😀"));
        }

        [Fact]
        public void LargeInput_CompletesAndStripsTags()
        {
            var html = string.Concat(System.Linq.Enumerable.Repeat("<p>Lorem ipsum <b>dolor</b> sit amet</p>", 200));
            var result = Strings.StripHTMLTags(html);
            Assert.DoesNotContain("<", result);
            Assert.Contains("Lorem ipsum dolor sit amet", result);
        }
    }

    public class JoinTests
    {
        [Fact]
        public void SkipsNullAndEmpty()
        {
            Assert.Equal("a,b,1", Strings.Join(",", "a", null, "", "b", 1));
        }

        [Fact]
        public void NoArgs_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, Strings.Join(","));
        }

        [Fact]
        public void WhitespaceIsNotSkipped()
        {
            Assert.Equal("a, ,b", Strings.Join(",", "a", " ", "b"));
        }

        [Fact]
        public void CssAppend_JoinsWithSpaceAndSkipsEmpty()
        {
            Assert.Equal("btn btn-primary", Strings.CssAppend("btn", null, "", "btn-primary"));
        }

        [Fact]
        public void FormatsUsingCurrentCulture()
        {
            using (new CultureScope("nl-NL"))
                Assert.Equal("1,5", Strings.Join(",", 1.5));
        }
    }

    public class CopyMaxLengthTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void NullOrEmpty_ReturnsInput(string input)
        {
            Assert.Equal(input, Strings.CopyMaxLength(input, 5));
        }

        [Fact]
        public void ShorterThanMax_ReturnsInput()
        {
            Assert.Equal("abc", Strings.CopyMaxLength("abc", 10));
            Assert.Equal("abc", Strings.CopyMaxLength("abc", 10, true));
        }

        [Fact]
        public void MaxZero_ReturnsInputUnchanged()
        {
            Assert.Equal("abcdef", Strings.CopyMaxLength("abcdef", 0, true));
        }

        [Fact]
        public void LongerThanMax_IsTruncated_NoSuffixByDefault()
        {
            Assert.Equal("abc", Strings.CopyMaxLength("abcdef", 3));
        }

        [Fact]
        public void WholeWords_CutsAtLastWordEnd_AndAddsEllipsis()
        {
            Assert.Equal("The quick...", Strings.CopyMaxLength("The quick brown fox", 12, true));
        }

        [Fact]
        public void WholeWords_NoSeparatorFound_KeepsHardCut()
        {
            Assert.Equal("abcd...", Strings.CopyMaxLength("abcdefgh", 4, true));
        }

        [Fact]
        public void CustomSuffixAndTokens()
        {
            Assert.Equal("a-b [more]", Strings.CopyMaxLength("a-b-c-d", 4, true, new[] { '-' }, " [more]"));
            Assert.Equal("a-b [more]", "a-b-c-d".CopyMaxLength(5, true, new[] { '-' }, " [more]"));
        }

        [Fact]
        public void ExtensionMethods_DelegateToStrings()
        {
            Assert.Equal("abc", "abcdef".CopyMaxLength(3));
            Assert.Equal("The...", "The quick".CopyMaxLength(5, true));
            Assert.Equal("The~", "The quick".CopyMaxLength(5, true, "~"));
            Assert.Equal("a", "<b>a</b>".StripHTMLTags());
        }

        [Fact]
        public void ExactlyMaxLength_ReturnsInputWithoutSuffix()
        {
            Assert.Equal("abc", Strings.CopyMaxLength("abc", 3, false, "..."));
        }

        [Fact]
        public void NegativeMaxLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Strings.CopyMaxLength("abc", -1));
        }
    }

    public class InsureHtmlParagraphTests
    {
        [Fact]
        public void Null_ReturnsEmpty()
        {
            Assert.Equal("", Strings.InsureHtmlParagraph(null));
        }

        [Fact]
        public void PlainText_IsWrapped()
        {
            Assert.Equal("<p>hello</p>", Strings.InsureHtmlParagraph("  hello  "));
        }

        [Fact]
        public void AlreadyWrapped_IsUnchanged()
        {
            Assert.Equal("<p>hello</p>", Strings.InsureHtmlParagraph("<p>hello</p>"));
            Assert.Equal("<P>hello</P>", Strings.InsureHtmlParagraph("<P>hello</P>"));
        }

        [Fact]
        public void TwoWrappedParagraphs_AreUnchanged()
        {
            Assert.Equal("<p>a</p><p>b</p>", Strings.InsureHtmlParagraph("<p>a</p><p>b</p>"));
        }

        [Fact]
        public void TextAfterClosedParagraph_IsWrapped()
        {
            Assert.Equal("<p>a</p><p>b</p>", Strings.InsureHtmlParagraph("<p>a</p>b"));
        }

        [Fact]
        public void UnclosedParagraphs_AreClosedWithoutDuplicates()
        {
            Assert.Equal("<p>a</p><p>b</p>", Strings.InsureHtmlParagraph("a<p>b"));
        }

        [Fact]
        public void Empty_BecomesEmptyParagraph()
        {
            Assert.Equal("<p></p>", Strings.InsureHtmlParagraph(""));
        }
    }

    public class HtmlColorTests
    {
        [Fact]
        public void FormatsAsUpperHexRgb()
        {
            Assert.Equal("#FF8000", Strings.HtmlColor(Color.FromArgb(255, 128, 0)));
            Assert.Equal("#000000", Strings.HtmlColor(Color.Black));
            Assert.Equal("#0A0B0C", Strings.HtmlColor(Color.FromArgb(10, 11, 12)));
        }

        [Fact]
        public void IgnoresAlpha()
        {
            Assert.Equal("#FFFFFF", Strings.HtmlColor(Color.FromArgb(0, 255, 255, 255)));
        }
    }

    public class FormatFileSizeTests
    {
        [Fact]
        public void Zero_ReturnsEmptyString()
        {
            Assert.Equal("", Strings.FormatFileSize(0));
        }

        [Theory]
        [InlineData(1L, "1 bytes")]
        [InlineData(999L, "999 bytes")]
        [InlineData(1500L, "1 Kb")]
        [InlineData(2_500_000L, "2 Mb")]
        [InlineData(3_000_000_001L, "3 Gb")]
        [InlineData(4_000_000_000_001L, "4 Tb")]
        [InlineData(-1500L, "-1 Kb")]
        public void FormatsWithDecimalUnits(long size, string expected)
        {
            using (new CultureScope("en-US"))
                Assert.Equal(expected, Strings.FormatFileSize(size));
        }

        [Fact]
        public void LargeValues_UseCurrentCultureGroupSeparator()
        {
            using (new CultureScope("en-US"))
                Assert.Equal("5,000 Tb", Strings.FormatFileSize(5_000_000_000_000_000));
            using (new CultureScope("nl-NL"))
                Assert.Equal("5.000 Tb", Strings.FormatFileSize(5_000_000_000_000_000));
        }

        [Theory]
        [InlineData(1000L, "1 Kb")]
        [InlineData(1_000_000L, "1 Mb")]
        [InlineData(1_000_000_000L, "1 Gb")]
        public void ExactUnitBoundaries_RollOverToNextUnit(long size, string expected)
        {
            using (new CultureScope("en-US"))
                Assert.Equal(expected, Strings.FormatFileSize(size));
        }

        [Fact]
        public void MinValue_DoesNotProduceDoubleMinus()
        {
            using (new CultureScope("en-US"))
                Assert.DoesNotContain("--", Strings.FormatFileSize(long.MinValue));
        }
    }

    public class SeoTests
    {
        [Theory]
        [InlineData("café", "cafe")]
        [InlineData("Ångström", "Angstrom")]
        [InlineData("naïve façade", "naive facade")]
        [InlineData("plain", "plain")]
        [InlineData("", "")]
        public void RemoveDiacritics_StripsCombiningMarks(string input, string expected)
        {
            Assert.Equal(expected, Strings.RemoveDiacritics(input));
        }

        [Fact]
        public void RemoveDiacritics_Null_Throws()
        {
            Assert.Throws<NullReferenceException>(() => Strings.RemoveDiacritics(null));
        }

        [Fact]
        public void RemoveDiacritics_IsCultureIndependent()
        {
            using (new CultureScope("tr-TR"))
                Assert.Equal("Istanbul", Strings.RemoveDiacritics("İstanbul"));
        }

        [Theory]
        [InlineData("Hello World!", "Hello-World")]
        [InlineData("Crème brûlée & co.", "Creme-brulee--co")]
        [InlineData("a+b", "a-b")]
        [InlineData("keep_under-score", "keep_under-score")]
        [InlineData("日本語", "")]
        public void MakeSEOSafe_RemovesUnsafeCharacters(string input, string expected)
        {
            Assert.Equal(expected, Strings.MakeSEOSafe(input));
        }

        [Fact]
        public void MakeSEOSafe_CustomSpaceReplacement()
        {
            Assert.Equal("a_b_c", Strings.MakeSEOSafe("a b+c", "_"));
        }

        [Fact]
        public void MakeSEOSafe_UnderTurkishCulture_SameResult()
        {
            using (new CultureScope("tr-TR"))
                Assert.Equal("TITLE-iI", Strings.MakeSEOSafe("TITLE iI"));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("My Résumé.pdf", "My-Resume.pdf")]
        [InlineData("photo (1).JPG", "photo-1.JPG")]
        public void MakeFileNameSEOSafe_CleansNameAndExtension(string input, string expected)
        {
            Assert.Equal(expected, Strings.MakeFileNameSEOSafe(input));
        }

        [Fact]
        public void MakeFileNameSEOSafe_NoExtension_IsStillCleaned()
        {
            Assert.Equal("My-Resume", Strings.MakeFileNameSEOSafe("My Résumé"));
        }
    }

    public class ReadScriptTests
    {
        [Fact]
        public void ReadsWholeFile()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "line1\nline2 ü");
                Assert.Equal("line1\nline2 ü", Strings.readScript(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void MissingFile_Throws()
        {
            Assert.ThrowsAny<IOException>(() => Strings.readScript(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        }
    }
}
