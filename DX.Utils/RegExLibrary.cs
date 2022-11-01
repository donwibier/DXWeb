using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DX.Utils
{
    public static class RegExLibrary
    {
        public static string GetRegExGroupValue(GroupCollection groups, string groupName)
        {
            return GetRegExGroupValue(groups, groupName, "");
        }
        public static string GetRegExGroupValue(GroupCollection groups, string groupName, string defaultValue)
        {
            return (groups[groupName] != null) ? groups[groupName].Value : defaultValue;
        }

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  [Alias]: A named capture group. [([a-zA-Z0-9_\-\.]+)]
        ///      [1]: A numbered capture group. [[a-zA-Z0-9_\-\.]+]
        ///          Any character in this class: [a-zA-Z0-9_\-\.], one or more repetitions
        ///  @
        ///  [Domain]: A named capture group. [((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,9}|[0-9]{1,3})]
        ///      ((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,9}|[0-9]{1,3})
        ///          [2]: A numbered capture group. [(\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+)]
        ///              Select from 2 alternatives
        ///                  [3]: A numbered capture group. [\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.]
        ///                      \[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.
        ///                          Literal [
        ///                          Any character in this class: [0-9], between 1 and 3 repetitions
        ///                          Literal .
        ///                          Any character in this class: [0-9], between 1 and 3 repetitions
        ///                          Literal .
        ///                          Any character in this class: [0-9], between 1 and 3 repetitions
        ///                          Literal .
        ///                  [4]: A numbered capture group. [([a-zA-Z0-9\-]+\.)+]
        ///                      [5]: A numbered capture group. [[a-zA-Z0-9\-]+\.], one or more repetitions
        ///                          [a-zA-Z0-9\-]+\.
        ///                              Any character in this class: [a-zA-Z0-9\-], one or more repetitions
        ///                              Literal .
        ///          [6]: A numbered capture group. [[a-zA-Z]{2,9}|[0-9]{1,3}]
        ///              Select from 2 alternatives
        ///                  Any character in this class: [a-zA-Z], between 2 and 9 repetitions
        ///                  Any character in this class: [0-9], between 1 and 3 repetitions
        ///  
        ///
        /// </summary>
        public static Regex EmailParser = new Regex(
            @"(?<Alias>([a-zA-Z0-9_\-\.]+))@(?<Domain>((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,9}|[0-9]{1,3}))",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  Any character in this class: [\?&]
        ///  [name]: A named capture group. [[^&=]+]
        ///      Any character that is NOT in this class: [&=], one or more repetitions
        ///  =
        ///  [value]: A named capture group. [[^&=]+]
        ///      Any character that is NOT in this class: [&=], one or more repetitions
        ///  
        ///
        /// </summary>
        public static Regex QueryStringParser = new Regex(
              "[\\?&](?<name>[^&=]+)=(?<value>[^&=]+)",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  Beginning of line or string
        ///  [SchemeEx]: A named capture group. [(?<Scheme>[^:/\?#]+):], zero or one repetitions
        ///      (?<Scheme>[^:/\?#]+):
        ///          [Scheme]: A named capture group. [[^:/\?#]+]
        ///              Any character that is NOT in this class: [:/\?#], one or more repetitions
        ///          :
        ///  [AuthorityEx]: A named capture group. [//(?<Authority>[^/\?#]*)], zero or one repetitions
        ///      //(?<Authority>[^/\?#]*)
        ///          //
        ///          [Authority]: A named capture group. [[^/\?#]*]
        ///              Any character that is NOT in this class: [/\?#], any number of repetitions
        ///  [Path]: A named capture group. [[^\?#]*]
        ///      Any character that is NOT in this class: [\?#], any number of repetitions
        ///  [QueryEx]: A named capture group. [\?(?<Query>[^#]*)], zero or one repetitions
        ///      \?(?<Query>[^#]*)
        ///          Literal ?
        ///          [Query]: A named capture group. [[^#]*]
        ///              Any character that is NOT in this class: [#], any number of repetitions
        ///  [FragmentEx]: A named capture group. [#(?<Fragment>.*)], zero or one repetitions
        ///      #(?<Fragment>.*)
        ///          #
        ///          [Fragment]: A named capture group. [.*]
        ///              Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        public static Regex UrlParser = new Regex(
              "^(?<SchemeEx>(?<Scheme>[^:/\\?#]+):)?(?<AuthorityEx>//(?<Aut" +
              "hority>[^/\\?#]*))?(?<Path>[^\\?#]*)(?<QueryEx>\\?(?<Query>[" +
              "^#]*))?(?<FragmentEx>#(?<Fragment>.*))?", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        ///  Beginning of line or string
        ///  [Url]: A named capture group. [~/[A-Za-z0-9\s-_\+/]*((\.aspx)|(/))]
        ///      ~/[A-Za-z0-9\s-_\+/]*((\.aspx)|(/))
        ///          ~/
        ///          Any character in this class: [A-Za-z0-9\s-_\+/], any number of repetitions
        ///          [1]: A numbered capture group. [(\.aspx)|(/)]
        ///              Select from 2 alternatives
        ///                  [2]: A numbered capture group. [\.aspx]
        ///                      \.aspx
        ///                          Literal .
        ///                          aspx
        ///                  [3]: A numbered capture group. [/]
        ///                      /
        ///  [4]: A numbered capture group. [\?(?<Params>.*)], zero or one repetitions
        ///      \?(?<Params>.*)
        ///          Literal ?
        ///          [Params]: A named capture group. [.*]
        ///              Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        public static Regex SplitScriptParamsUrl = new Regex(
              "^(?<Url>((https?://[A-Za-z0-9\\s-_.]*)|(~?/))[A-Za-z0-9\\s-_\\+/]*((\\.aspx)|(/)))(\\?(?<Params>.*))?",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        public const string SEOSafeReplace = "[^A-Za-z0-9\\s-_\\+]";
        public const string SEOSafeMatch = "[A-Za-z0-9\\s-_\\+]";
        /// <summary>
        /// regexSEOSave
        /// </summary>
        public static Regex SEOSafe = new Regex(SEOSafeReplace,
                                                RegexOptions.IgnoreCase
                                                | RegexOptions.CultureInvariant
                                                | RegexOptions.IgnorePatternWhitespace
                                                | RegexOptions.Compiled
                                                );
        /// <summary>
        /// regexSEOReplaceSpaces
        /// </summary>
        public static Regex SEOReplaceSpaces = new Regex("[\\s]",
                                                RegexOptions.CultureInvariant
                                                | RegexOptions.Compiled
                                                );
        /// <summary>
        /// regexSEOReplaceSpaces
        /// </summary>
        public static Regex SEOReplacePlusses = new Regex("[\\+]",
                                                RegexOptions.CultureInvariant
                                                | RegexOptions.Compiled
                                                );

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  ^~/Content/
        ///      Beginning of line or string
        ///      ~/Content/
        ///  [Country]: A named capture group. [.{5}]
        ///      Any character, exactly 5 repetitions
        ///  /
        ///  [Group]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  /
        ///  [SEOName]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        //
        public static Regex PageRewriteParser = new Regex(
              String.Format(@"^~/Content/(?<Country>{0}{{5}})/(?<Group>{0}*)/(?<SEOName>{0}*)(?:(\.aspx)?)(\?(?<Params>.*))?", SEOSafeMatch),
              RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///  Regular expression built for C# on: vr, dec 4, 2009, 09:08:50 
        ///  Using Expresso Version: 3.0.3276, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  ^~/Data/
        ///      Beginning of line or string
        ///      ~/Data/
        ///  [Country]: A named capture group. [.{5}]
        ///      Any character, exactly 5 repetitions
        ///  /
        ///  [Datasource]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  /
        ///  [KeyID]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  /
        ///  [SEOName]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        public static Regex ListDataRewriteParser = new Regex(
            String.Format(@"^~/Data/(?<Country>{0}{{5}})/(?<Datasource>{0}*)/(?<Group>{0}*)/(?<SEOName>{0}*)(?:(\.aspx)?)(\?(?<Params>.*))?", SEOSafeMatch),
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  @"^~/Publication/
        ///      @"
        ///      Beginning of line or string
        ///      ~/Publication/
        ///  [Country]: A named capture group. [.{5}]
        ///      Any character, exactly 5 repetitions
        ///  /
        ///  [Datasource]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  /
        ///  [PubYear]: A named capture group. [[0-9]{4}]
        ///      Any character in this class: [0-9], exactly 4 repetitions
        ///  /
        ///  [PubMonth]: A named capture group. [[0-9]{2}]
        ///      Any character in this class: [0-9], exactly 2 repetitions
        ///  /
        ///  [PubDay]: A named capture group. [[0-9]{2}]
        ///      Any character in this class: [0-9], exactly 2 repetitions
        ///  /
        ///  [SEOName]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        public static Regex BlogRewriteParser = new Regex(
            @"^~/Publication/(?<Country>.{5})/(?<Datasource>.*)/(?<PubYear>[0-9]{4})/(?<PubMonth>[0-9]{2})/(?<PubDay>[0-9]{2})/(?<SEOName>.*)",
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  "^~/Publications/
        ///      "
        ///      Beginning of line or string
        ///      ~/Publications/
        ///  [Country]: A named capture group. [.{5}]
        ///      Any character, exactly 5 repetitions
        ///  /
        ///  [Datasource]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  /
        ///  [PubYear]: A named capture group. [[0-9]{4}]
        ///      Any character in this class: [0-9], exactly 4 repetitions
        ///  /
        ///  [PubMonth]: A named capture group. [[0-9]{2}]
        ///      Any character in this class: [0-9], exactly 2 repetitions
        ///  [1]: A numbered capture group. [/(?&lt;PubDay&gt;[0-9]{2})], zero or one repetitions
        ///      /(?&lt;PubDay&gt;[0-9]{2})
        ///          /
        ///          [PubDay]: A named capture group. [[0-9]{2}]
        ///              Any character in this class: [0-9], exactly 2 repetitions
        ///  
        ///
        /// </summary>
        public static Regex BlogOverviewRewriteParser = new Regex(
            @"^~/Publications/(?<Country>.{5})/(?<Datasource>.*)/(?<PubYear>[0-9]{4})/(?<PubMonth>[0-9]{2})(/(?<PubDay>[0-9]{2}))?",
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  ^~/
        ///      Beginning of line or string
        ///      ~/
        ///  [Country]: A named capture group. [.{5}]
        ///      Any character, exactly 5 repetitions
        ///  /
        ///  [MenuPath]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        public static Regex SitemapRewriteParser = new Regex(
              String.Format(@"^~/(?<Country>{0}{{5}})/(?<MenuPath>{1}*)(?:(\.aspx)?)(\?(?<Params>.*))?", SEOSafeMatch, SEOSafeMatch.Replace("]", "/]")),
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  ^~/
        ///      Beginning of line or string
        ///      ~/
        ///  [1]: A numbered capture group. [(?<Script>default.aspx)|((?<Country>.{5})(?:((\.aspx)|(/))?))]
        ///      Select from 2 alternatives
        ///          [Script]: A named capture group. [default.aspx]
        ///              default.aspx
        ///                  default
        ///                  Any character
        ///                  aspx
        ///          [2]: A numbered capture group. [(?<Country>.{5})(?:((\.aspx)|(/))?)]
        ///              (?<Country>.{5})(?:((\.aspx)|(/))?)
        ///                  [Country]: A named capture group. [.{5}]
        ///                      Any character, exactly 5 repetitions
        ///                  Match expression but don't capture it. [((\.aspx)|(/))?]
        ///                      [3]: A numbered capture group. [(\.aspx)|(/)], zero or one repetitions
        ///                          Select from 2 alternatives
        ///                              [4]: A numbered capture group. [\.aspx]
        ///                                  \.aspx
        ///                                      Literal .
        ///                                      aspx
        ///                              [5]: A numbered capture group. [/]
        ///                                  /
        ///  [6]: A numbered capture group. [\?(?<HomepageParams>.*)], zero or one repetitions
        ///      \?(?<HomepageParams>.*)
        ///          Literal ?
        ///          [HomepageParams]: A named capture group. [.*]
        ///              Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        public static Regex HomepageRewriteParser = new Regex(
              String.Format("^~/((?<Script>default.aspx)|((?<Country>{0}{{5}})(?:((\\.aspx)|(/))?)))(\\?(?<HomepageParams>.*))?", SEOSafeMatch),
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  ^~/
        ///      Beginning of line or string
        ///      ~/
        ///  [Country]: A named capture group. [.{5}]
        ///      Any character, exactly 5 repetitions
        ///  
        ///
        /// </summary>
        public static Regex XmlSitemap = new Regex(
              String.Format(@"^~/Sitemap/(?<Country>{0}{{5}})/?$", SEOSafeMatch),
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  ^~/
        ///      Beginning of line or string
        ///      ~/
        ///  [1]: A numbered capture group. [.*]
        ///      Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        public static Regex CmsCustomUrlRewriteParser = new Regex(
              @"^~/(.*)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }


}
