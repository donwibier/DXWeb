using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DX.Utils
{
    public static class StringsExtensions
    {
        public static string StripHTMLTags(this string s)
        {
            return Strings.StripHTMLTags(s);
        }

        public static string CopyMaxLength(this string input, int maxLen)
        {
            return Strings.CopyMaxLength(input, maxLen);
        }

        public static string CopyMaxLength(this string input, int maxLen, bool wholeWords)
        {
            return Strings.CopyMaxLength(input, maxLen, wholeWords);
        }
        public static string CopyMaxLength(this string input, int maxLen, bool wholeWords, string moreSuffix)
        {
            return Strings.CopyMaxLength(input, maxLen, wholeWords, moreSuffix);
        }

        public static string CopyMaxLength(this string input, int maxLen, bool wholeWords, char[] wordEndTokens, string moreSuffix)
        {
            return Strings.CopyMaxLength(input, maxLen, wholeWords, wordEndTokens, moreSuffix);
        }
    }
    public static class Strings
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Removes HTML tags from the <c>source</c>. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="source">   The HTML input. </param>
        ///
        /// <returns>   the stripped result incl. line breaks etc. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        // Precompiled patterns for StripHTMLTags (static Regex.Replace calls would thrash the
        // 15-entry Regex cache). CultureInvariant makes IgnoreCase independent of the current culture.
        const RegexOptions RxOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        const RegexOptions RxOptsIC = RxOpts | RegexOptions.IgnoreCase;

        static readonly Regex rxRepeatingSpaces = new Regex(@"( )+", RxOpts);
        static readonly Regex rxHeadOpen = new Regex(@"<( )*head([^>])*>", RxOptsIC);
        static readonly Regex rxHeadClose = new Regex(@"(<( )*(/)( )*head( )*>)", RxOptsIC);
        static readonly Regex rxHeadBlock = new Regex(@"(?s)(<head>).*?(</head>)", RxOptsIC);
        static readonly Regex rxScriptOpen = new Regex(@"<( )*script([^>])*>", RxOptsIC);
        static readonly Regex rxScriptClose = new Regex(@"(<( )*(/)( )*script( )*>)", RxOptsIC);
        static readonly Regex rxScriptBlock = new Regex(@"(?s)<script.*?(/>|</script>)", RxOptsIC);
        static readonly Regex rxStyleOpen = new Regex(@"<( )*style([^>])*>", RxOptsIC);
        static readonly Regex rxStyleClose = new Regex(@"(<( )*(/)( )*style( )*>)", RxOptsIC);
        static readonly Regex rxStyleBlock = new Regex(@"(?s)<style.*?(/>|</style>)", RxOptsIC);
        static readonly Regex rxTd = new Regex(@"<( )*td([^>])*>", RxOptsIC);
        static readonly Regex rxBr = new Regex(@"<br[^>]*>", RxOptsIC);
        static readonly Regex rxDivOpen = new Regex(@"<( )*div([^>])*>", RxOptsIC);
        static readonly Regex rxTrOpen = new Regex(@"<( )*tr([^>])*>", RxOptsIC);
        static readonly Regex rxPOpen = new Regex(@"<( )*p([^>])*>", RxOptsIC);
        static readonly Regex rxDivClose = new Regex(@"</div[^>]*>", RxOptsIC);
        static readonly Regex rxTrClose = new Regex(@"</tr[^>]*>", RxOptsIC);
        static readonly Regex rxPClose = new Regex(@"</p[^>]*>", RxOptsIC);
        static readonly Regex rxLiOpen = new Regex(@"<( )*li([^>])*>", RxOptsIC);
        static readonly Regex rxLiClose = new Regex(@"<( )*/li([^>])*>", RxOptsIC);
        static readonly Regex rxUlClose = new Regex(@"<( )*/ul([^>])*>", RxOptsIC);
        static readonly Regex rxAnyTag = new Regex(@"<[^>]*>", RxOpts);
        // Known entities, one capture group per entity; replacement i belongs to group i + 1.
        // (Matches cannot overlap and no replacement creates a new match, so a single pass is
        // equivalent to replacing them one after another.)
        static readonly Regex rxKnownEntities = new Regex(
            @"(&nbsp;)|(¿)|(&iquest;)|(&bull;)|(&lsaquo;)|(&rsaquo;)|(&trade;)|(&frasl;)|(&lt;)|(&gt;)|(&copy;)|(&reg;)|(&#160;)", RxOptsIC);
        static readonly string[] knownEntityReplacements =
            { " ", " * ", " * ", " * ", "<", ">", "(tm)", "/", "<", ">", "(c)", "(r)", " " };
        static readonly MatchEvaluator knownEntityEvaluator = m =>
        {
            for (int i = 1; i < m.Groups.Count; i++)
                if (m.Groups[i].Success)
                    return knownEntityReplacements[i - 1];
            return m.Value;
        };
        // Remaining (unknown) character entities: '&name;', '&#123;' or '&#x1F;'
        static readonly Regex rxOtherEntities = new Regex(@"&#?[a-z0-9]{2,6};", RxOptsIC);
        static readonly Regex rxBreakSpacesBreak = new Regex("(\r)( )+(\r)", RxOpts);
        static readonly Regex rxTabSpacesTab = new Regex("(\t)( )+(\t)", RxOpts);
        static readonly Regex rxTabSpacesBreak = new Regex("(\t)( )+(\r)", RxOpts);
        static readonly Regex rxBreakSpacesTab = new Regex("(\r)( )+(\t)", RxOpts);
        static readonly Regex rxBreakTabsBreak = new Regex("(\r)(\t)+(\r)", RxOpts);
        static readonly Regex rxBreakTabs = new Regex("(\r)(\t)+", RxOpts);
        static readonly Regex rxBreakRun = new Regex("\r{3,}", RxOpts);
        static readonly Regex rxTabRun = new Regex("\t{5,}", RxOpts);
        static readonly MatchEvaluator breakRunEvaluator = m => new string('\r', CollapsedRunLength(m.Length, 3, 2));
        static readonly MatchEvaluator tabRunEvaluator = m => new string('\t', CollapsedRunLength(m.Length, 5, 4));

        // Length a run of n identical characters ends up with after the historic collapsing loop
        // ("for each k = 0, 1, ...: replace every (firstPattern + k) chars with 'replacement' chars",
        // String.Replace being a non-overlapping left-to-right scan). Kept for identical output.
        static int CollapsedRunLength(int n, int firstPattern, int replacement)
        {
            for (int p = firstPattern; p <= n; p++)
                n = (n / p) * replacement + (n % p);
            return n;
        }

        public static string StripHTMLTags(string source)
        {
            if (String.IsNullOrEmpty(source))
                return String.Empty;
            // NOTE: &lt; and &gt; are decoded after the tags have been removed (below), so escaped
            // text like "5 &lt; 6" is not mistaken for a tag.
            string result = source;
            try
            {

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                //DON: result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                //DON: result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = rxRepeatingSpaces.Replace(result, " ");

                // Remove the header (prepare first by clearing attributes)
                result = rxHeadOpen.Replace(result, "<head>");
                result = rxHeadClose.Replace(result, "</head>");
                result = rxHeadBlock.Replace(result, string.Empty);

                // remove all scripts (prepare first by clearing attributes)
                result = rxScriptOpen.Replace(result, "<script>");
                result = rxScriptClose.Replace(result, "</script>");
                result = rxScriptBlock.Replace(result, string.Empty);

                // remove all styles (prepare first by clearing attributes)
                result = rxStyleOpen.Replace(result, "<style>");
                result = rxStyleClose.Replace(result, "</style>");
                result = rxStyleBlock.Replace(result, string.Empty);

                // insert tabs in spaces of <td> tags
                result = rxTd.Replace(result, "\t");

                // insert line breaks in places of <BR> and <LI> tags
                result = rxBr.Replace(result, "\r");

                // remove <P>, <DIV> and <TR> tags
                result = rxDivOpen.Replace(result, String.Empty);
                result = rxTrOpen.Replace(result, String.Empty);
                result = rxPOpen.Replace(result, String.Empty);

                // insert line paragraphs (double line breaks) in place
                // if </P>, </DIV> and </TR> tags
                result = rxDivClose.Replace(result, "\r\r");
                result = rxTrClose.Replace(result, "\r\r");
                result = rxPClose.Replace(result, "\r\r");

                // replace <LI> tags with bullets
                result = rxLiOpen.Replace(result, " * ");
                // replace </li> and </ul> end tags with single/double line breaks for formatting issues.
                result = rxLiClose.Replace(result, "\r");
                result = rxUlClose.Replace(result, "\r");

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = rxAnyTag.Replace(result, string.Empty);

                // replace special characters (incl. hard html spaces, before removing all other html codes)
                result = rxKnownEntities.Replace(result, knownEntityEvaluator);

                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = rxOtherEntities.Replace(result, string.Empty);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = rxBreakSpacesBreak.Replace(result, "\r\r");
                result = rxTabSpacesTab.Replace(result, "\t\t");
                result = rxTabSpacesBreak.Replace(result, "\t\r");
                result = rxBreakSpacesTab.Replace(result, "\r\t");
                // Remove redundant tabs
                result = rxBreakTabsBreak.Replace(result, "\r\r");
                // Remove multiple tabs following a line break with just one tab
                result = rxBreakTabs.Replace(result, "\r\t");
                // Collapse runs of line breaks / tabs (linear replacement of the former O(n^2) loop)
                result = rxBreakRun.Replace(result, breakRunEvaluator);
                result = rxTabRun.Replace(result, tabRunEvaluator);
                if (result.EndsWith("\r\r"))
                    result = result.Substring(0, result.Length - 2);
            }
            catch (Exception err)
            {
                Log.Write("Error in Strings.StripHTMLTagsEx", true, Log.LogType.Error, err);
                result = source;
            }

            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// This method does the same as String.Join(...) but leaves out the null objects or empty
        /// strings.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="separator">    The separator. </param>
        /// <param name="args">         The args. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Join(string separator, params object[] args)
        {
            List<object> list = new List<object>();
            foreach (object o in args)
            {
                string s = String.Format("{0}", o);
                if (!String.IsNullOrEmpty(s))
                    list.Add(s);
            }
            return String.Join(separator, list.ToArray());
        }

        public static string CssAppend(params object[] classes)
        {
            return Join(" ", classes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies a string with the given maxLength. If the length of the input is less or equal to
        /// maxLen, then input is returned, otherwise a substring is returned.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="input">    The input string . </param>
        /// <param name="maxLen">   The maximum length which should be copied. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string CopyMaxLength(string input, int maxLen)
        {
            return CopyMaxLength(input, maxLen, false, null, String.Empty);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies a string with the given maxLength. If the length of the input is less or equal to
        /// maxLen, then input is returned, otherwise a substring is returned.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="input">        The input string . </param>
        /// <param name="maxLen">       The maximum length which should be copied. </param>
        /// <param name="wholeWords">   true to whole words. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string CopyMaxLength(string input, int maxLen, bool wholeWords)
        {
            return CopyMaxLength(input, maxLen, wholeWords, "...");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies a string with the given maxLength. If the length of the input is less or equal to
        /// maxLen, then input is returned, otherwise a substring is returned.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="input">        The input string . </param>
        /// <param name="maxLen">       The maximum length which should be copied. </param>
        /// <param name="wholeWords">   true to whole words. </param>
        /// <param name="moreSuffix">   The more suffix. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string CopyMaxLength(string input, int maxLen, bool wholeWords, string moreSuffix)
        {
            return CopyMaxLength(input, maxLen, wholeWords, new char[] { ' ', ',', '.', ';', ':', '"', '!', '?' }, moreSuffix);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies a string with the given maxLength. If the length of the input is less or equal to
        /// maxLen, then input is returned, otherwise a substring is returned.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="input">            The input string . </param>
        /// <param name="maxLen">           The maximum length which should be copied. </param>
        /// <param name="wholeWords">       true to whole words. </param>
        /// <param name="wordEndTokens">    The word end tokens. </param>
        /// <param name="moreSuffix">       The more suffix. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string CopyMaxLength(string input, int maxLen, bool wholeWords, char[] wordEndTokens, string moreSuffix)
        {
            if (String.IsNullOrEmpty(input) || (maxLen == 0) || (input.Length <= maxLen))
                return input;

            string result = input.Substring(0, maxLen);
            if (wholeWords)
            {
                int i = result.LastIndexOfAny(wordEndTokens);
                if (i > 0)
                    result = result.Substring(0, i);
            }

            return result + moreSuffix;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Reads a textfile. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="path"> The path of the file. </param>
        ///
        /// <returns>   The script as a string. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string readScript(string path)
        {
            string result = "";
            
            using (TextReader tr = new  StreamReader(path))
                result = tr.ReadToEnd();
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Insures the HTML paragraph. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="htmlSource">    The A HTML. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string InsureHtmlParagraph(string htmlSource)
        {
            if (htmlSource == null)
                return "";
            htmlSource = htmlSource.Trim();
            if (htmlSource.Length == 0)
                return "<p></p>";

            // Walk the <p> / </p> tags and only open/close a paragraph where one is missing, so
            // already well-formed input is left untouched.
            StringBuilder sb = new StringBuilder(htmlSource.Length + 16);
            bool open = false;
            int pos = 0;
            while (pos < htmlSource.Length)
            {
                int nextOpen = htmlSource.IndexOf("<p>", pos, StringComparison.OrdinalIgnoreCase);
                int nextClose = htmlSource.IndexOf("</p>", pos, StringComparison.OrdinalIgnoreCase);
                int next = (nextOpen == -1) ? nextClose : (nextClose == -1 ? nextOpen : Math.Min(nextOpen, nextClose));
                int textEnd = (next == -1) ? htmlSource.Length : next;
                if (textEnd > pos)
                {
                    string text = htmlSource.Substring(pos, textEnd - pos);
                    if (!open && !String.IsNullOrWhiteSpace(text))
                    {
                        sb.Append("<p>");
                        open = true;
                    }
                    sb.Append(text);
                }
                if (next == -1)
                    break;
                if (next == nextOpen)
                {
                    if (open)
                        sb.Append("</p>");
                    sb.Append(htmlSource, next, 3);
                    open = true;
                    pos = next + 3;
                }
                else
                {
                    sb.Append(htmlSource, next, 4);
                    open = false;
                    pos = next + 4;
                }
            }
            if (open)
                sb.Append("</p>");
            return sb.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   HTMLs the color. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="AColor">   Color of the A. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string HtmlColor(System.Drawing.Color AColor)
        {
            string s = string.Format("#{0:X2}{1:X2}{2:X2}", AColor.R, AColor.G, AColor.B);
            return s;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Formats the size of the file. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="fileSize"> Size of the file. </param>
        ///
        /// <returns>   The formatted file size. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string FormatFileSize(long fileSize)
        {
            if (fileSize == 0)
                return "";

            // 1.000.000.000.000 = 1 Tb
            //     1.000.000.000 = 1 Gb
            //         1.000.000 = 1 Mb
            //             1.000 = 1 Kb 
            ulong intSize = 0;
            string strSuffix = "";
            bool neg = fileSize < 0;
            // magnitude as ulong, so long.MinValue does not overflow
            ulong size = neg ? (ulong)(-(fileSize + 1)) + 1UL : (ulong)fileSize;

            if (size >= 1000000000000)
            {
                intSize = (size / 1000000000000);
                strSuffix = "Tb";
            }
            else if (size >= 1000000000)
            {
                intSize = (size / 1000000000);
                strSuffix = "Gb";
            }
            else if (size >= 1000000)
            {
                intSize = (size / 1000000);
                strSuffix = "Mb";
            }
            else if (size >= 1000)
            {
                intSize = (size / 1000);
                strSuffix = "Kb";
            }
            else
            {
                intSize = size;
                strSuffix = "bytes";
            }

            return $"{(((neg) ? "-" : ""))}{intSize:#,##0} {strSuffix}";
        }

        public static string RemoveDiacritics(string s)
        {
            string normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
        public static string MakeSEOSafe(string value, string spaceReplacement = "-")
        {
            string str = RemoveDiacritics(value);
            str = RegExLibrary.SEOSafe.Replace(str, "");
            str = RegExLibrary.SEOReplaceSpaces.Replace(str, spaceReplacement);
            str = RegExLibrary.SEOReplacePlusses.Replace(str, spaceReplacement);
            return str;
        }
        public static string MakeFileNameSEOSafe(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;

            string file = MakeSEOSafe(Path.GetFileNameWithoutExtension(fileName));
            string ext = MakeSEOSafe(Path.GetExtension(fileName));
            if (String.IsNullOrEmpty(ext))
                return file;

            return String.Format("{0}.{1}", file, ext);
        }

    }
}
