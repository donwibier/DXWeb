using System;
using System.Collections.Generic;
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

        public static string StripHTMLTags(string source)
        {
            if (String.IsNullOrEmpty(source))
                return String.Empty;
            string result = source.Replace("&lt;", "<").Replace("&gt;", ">");
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
                result = Regex.Replace(result, @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*head([^>])*>", "<head>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(<( )*(/)( )*head( )*>)", "</head>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(<head>).*(</head>)", string.Empty, RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*script([^>])*>", "<script>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(<( )*(/)( )*script( )*>)", "</script>", RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(?s)<script.*?(/>|</script>)", string.Empty, RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*style([^>])*>", "<style>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(<( )*(/)( )*style( )*>)", "</style>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(?s)<style.*?(/>|</style>)", string.Empty, RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = Regex.Replace(result, @"<( )*td([^>])*>", "\t", RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                //result = Regex.Replace(result, @"<( )*br( )*>", "\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"<br[^>]*>", "\r", RegexOptions.IgnoreCase);
                //result = Regex.Replace(result, @"<( )*li( )*>", "\r", RegexOptions.IgnoreCase);

                // remove <P>, <DIV> and <TR> tags
                result = Regex.Replace(result, @"<( )*div([^>])*>", String.Empty, RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"<( )*tr([^>])*>", String.Empty, RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"<( )*p([^>])*>", String.Empty, RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if </P>, </DIV> and </TR> tags
                result = Regex.Replace(result, @"</div[^>]*>", "\r\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"</tr[^>]*>", "\r\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"</p[^>]*>", "\r\r", RegexOptions.IgnoreCase);

                // replace <UL and <LI tags with styles with <ul> and <li>
                //result = Regex.Replace(result, @"<( )*ul([^>])*>", "<ul>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"<( )*li([^>])*>", " * ", RegexOptions.IgnoreCase);
                // replace </li> and </ul> end tags with single/double line breaks for formatting issues.
                result = Regex.Replace(result, @"<( )*/li([^>])*>", "\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"<( )*/ul([^>])*>", "\r", RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < > but ignore UL LI lists!!!
                result = Regex.Replace(result, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);
                //result = Regex.Replace(result, @"<(?!lu|li|/lu|/li)[^>]*>", string.Empty, RegexOptions.IgnoreCase);

                // replace special characters:
                result = Regex.Replace(result, @" ", " ", RegexOptions.IgnoreCase);

                result = Regex.Replace(result, @"¿", " * ", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&iquest;", " * ", RegexOptions.IgnoreCase);

                result = Regex.Replace(result, @"&bull;", " * ", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&lsaquo;", "<", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&rsaquo;", ">", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&trade;", "(tm)", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&frasl;", "/", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&lt;", "<", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&gt;", ">", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&copy;", "(c)", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&reg;", "(r)", RegexOptions.IgnoreCase);

                // FIX: replace hard html spaces with soft spaces before removing all other hmtl codes!
                result = Regex.Replace(result, @"&#160;", " ", RegexOptions.IgnoreCase);

                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = Regex.Replace(result, @"&(.{2,6});", string.Empty, RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = Regex.Replace(result, "(\r)( )+(\r)", "\r\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\t)( )+(\t)", "\t\t", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\t)( )+(\r)", "\t\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\r)( )+(\t)", "\r\t", RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = Regex.Replace(result, "(\r)(\t)+(\r)", "\r\r", RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = Regex.Replace(result, "(\r)(\t)+", "\r\t", RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }
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

        public static string StringJoin(string separator, params object[] args)
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
            return StringJoin(" ", classes);
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
            if (String.IsNullOrEmpty(input) || (maxLen == 0) || (input.Length < maxLen))
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
            else
                htmlSource = htmlSource.Trim();

            if ((!htmlSource.StartsWith("<p>")) && (!htmlSource.StartsWith("<P>")))
                htmlSource = String.Format("<p>{0}", htmlSource);

            int nextStartParagraph = htmlSource.IndexOf("<p>", 3) != -1 ? htmlSource.IndexOf("<p>", 3) : htmlSource.IndexOf("<P>", 3);
            while (nextStartParagraph != -1)
            {
                htmlSource = htmlSource.Insert(nextStartParagraph, "</p>");
                nextStartParagraph = htmlSource.IndexOf("<p>", nextStartParagraph + 7) != -1 ? htmlSource.IndexOf("<p>", nextStartParagraph + 7) : htmlSource.IndexOf("<P>", nextStartParagraph + 7);
            }

            int nextEndParagraph = htmlSource.LastIndexOf("</p>") != -1 ? htmlSource.LastIndexOf("</p>") : htmlSource.LastIndexOf("</P>");
            while (nextEndParagraph != -1 && nextEndParagraph != htmlSource.Length - 4)
            {
                htmlSource = htmlSource.Insert(nextEndParagraph + 4, "<p>");
                nextEndParagraph = htmlSource.IndexOf("</p>", nextEndParagraph + 4) != -1 ? htmlSource.IndexOf("</p>", nextEndParagraph + 4) : htmlSource.IndexOf("</P>", nextEndParagraph + 4);
            }

            if ((!htmlSource.EndsWith("</p>")) && (!htmlSource.EndsWith("</P>")))
                htmlSource += "</p>";

            return htmlSource;
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
            string s = String.Format("#{0:X2}{1:X2}{2:X2}", AColor.R, AColor.G, AColor.B);
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
            long intSize = 0;
            string strSuffix = "";
            bool neg = false;
            if (fileSize < 0)
            {
                neg = true;
                fileSize = fileSize * -1; //make positive
            }

            if (fileSize > 1000000000000)
            {
                intSize = (fileSize / 1000000000000);
                strSuffix = "Tb";
            }
            else if (fileSize > 1000000000)
            {
                intSize = (fileSize / 1000000000);
                strSuffix = "Gb";
            }
            else if (fileSize > 1000000)
            {
                intSize = (fileSize / 1000000);
                strSuffix = "Mb";
            }
            else if (fileSize > 1000)
            {
                intSize = (fileSize / 1000);
                strSuffix = "Kb";
            }
            else
            {
                intSize = fileSize;
                strSuffix = "bytes";
            }

            return String.Format("{0}{1:#,##0} {2}", ((neg) ? "-" : ""), intSize, strSuffix);
        }
    }
}
