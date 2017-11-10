
#if (!NETSTANDARD2_0)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace DX.Utils
{
    public class Urls
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets the fully qualified domainname of the application root like
        /// "http://www.appdomain.com/SomeFolder/...." ATTENTION: This will not work in a background
        /// thread since the HttpContext.Current is NULL !!!!!!!
        /// </summary>
        ///
        /// <value> The absolute application URL. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string AbsoluteApplicationUrl
        {
            get
            {
                string result = AppRelativeToFullDomainUrl(HostingEnvironment.ApplicationVirtualPath);

                //// make sure there is a slash at the end
                if (!result.EndsWith("/"))
                    result += "/";
                return result;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Transforms a "~/SomeFolder/file.aspx" to "http://www.appdomain.com/SomeFolder/file.aspx"
        /// ATTENTION: This will not work in a background thread since the HttpContext.Current is NULL
        /// !!!!!!!
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="relativePath"> ~/SomeFolder/... </param>
        ///
        /// <returns>   http://www.appdomain.com/SomeFolder/.... </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string AppRelativeToFullDomainUrl(string relativePath)
        {
            return AppRelativeToFullDomainUrl(new HttpContextWrapper(HttpContext.Current), relativePath);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Transforms a "~/SomeFolder/file.aspx" to "http://www.appdomain.com/SomeFolder/file.aspx".
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values. </exception>
        ///
        /// <param name="ctx">          The current <c>HttpContext</c>. </param>
        /// <param name="relativePath"> The relative path. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string AppRelativeToFullDomainUrl(HttpContextBase ctx, string relativePath)
        {
            if (IsFullyQualifiedUrl(relativePath))
                return relativePath;

            if (ctx == null)
                throw new ArgumentException("ctx == null");

            UriBuilder uri = new UriBuilder(ctx.Request.Url) { Path = VirtualPathUtility.IsAppRelative(relativePath) ? VirtualPathUtility.ToAbsolute(relativePath) : relativePath };
            return uri.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Determines whether the given url is a fully qualified url incl. domain and protocol etc.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="url">  The URL. </param>
        ///
        /// <returns>
        /// <c>true</c> if [is fully qualified URL] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool IsFullyQualifiedUrl(string url)
        {
            bool result = false;
            try
            {
                Uri uri = new Uri(url);
                result = true;
            }
            catch (UriFormatException uriErr)
            {
                Log.Exception(uriErr, Log.LogType.None);
                result = false;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Valid URL. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="url">  The A URL. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string ValidUrl(string url)
        {            
            string theUrl = url;
            if (!String.IsNullOrEmpty(theUrl))
            {
                theUrl = theUrl.Trim(); 
                if (VirtualPathUtility.IsAppRelative(theUrl))
                    return VirtualPathUtility.ToAbsolute(theUrl);
            }

            return theUrl;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts an app-relative path (with ~/) to a server-rooted path. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="relativeUrl">  The relative app-path. </param>
        ///
        /// <returns>   The full rooted path. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string AppRelativeToAbsolute(string relativeUrl)
        {
            if (String.IsNullOrEmpty(relativeUrl) || IsFullyQualifiedUrl(relativeUrl))
                return relativeUrl;

            if (VirtualPathUtility.IsAppRelative(relativeUrl))
                return VirtualPathUtility.ToAbsolute(relativeUrl);

            return relativeUrl;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds the querystring parameter(s) to the given url. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="url">                  The url to append it to. </param>
        /// <param name="queryStringAddition">  The querystring addition. </param>
        ///
        /// <returns> The url including the parameters. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string AddQueryParamToUrl(string url, string queryStringAddition)
        {
            if (String.IsNullOrEmpty(queryStringAddition))
                return url;

            return String.Format("{0}{1}{2}", url, url.Contains("?") ? "&" : "?", queryStringAddition);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds the querystring parameter to the given url. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="url">          The url to append it to. </param>
        /// <param name="paramName">    The querystring parameter name. </param>
        /// <param name="paramValue">   the value of the querystring parameter (will be urlencoded). </param>
        ///
        /// <returns> The url including the parameter. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string AddQueryParamToUrl(string url, string paramName, string paramValue)
        {
            if (String.IsNullOrEmpty(paramName) || String.IsNullOrEmpty(paramValue))
                return url;

            return AddQueryParamToUrl(url, String.Format("{0}={1}", paramName, HttpUtility.UrlEncode(paramValue)));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Excludes the query params from URL. Example:
        ///     Utils.ExcludeQueryParamsFromUrl("http://www.somedomain.nl/default.aspx?x=1&amp;y=2&amp;
        ///     z=3", new string[] { "x", "y", "page"});
        /// 
        ///     Output: http://www.somedomain.nl/default.aspx?z=3.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="url">              The URL. </param>
        /// <param name="excludedParams">   The excluded params. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string ExcludeQueryParamsFromUrl(string url, string[] excludedParams)
        {
            if (String.IsNullOrEmpty(url) || (excludedParams == null) || (excludedParams.Length == 0))
                return url;


            string[] urlParts = url.Split(new char[] { '?' }, 2);
            if (urlParts.Length < 2)
                return url;

            List<string> query = new List<string>();
            Array.ForEach(urlParts[1].Split(new char[] { '&', '?' }),
                set =>
                {
                    var item = set.Split(new char[] { '=' }, 2);
                    if (!excludedParams.Contains(item[0]))
                        query.Add(set);
                });
            return String.Join("?", new string[] { urlParts[0], String.Join("&", query.ToArray()) });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a app relative URL from a local path. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="localPath">    The local path. </param>
        ///
        /// <returns>   Empty if no valid local path. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string LocalPathToAppRelativeUrl(string localPath)
        {
            string appFolder = HostingEnvironment.MapPath("~/");

            if (localPath.ToLower().StartsWith(appFolder.ToLower()))
            {
                string result = localPath.Substring(appFolder.Length).Replace("\\", "/");
                result = String.Format("~{0}{1}", (result.StartsWith("/") ? "" : "/"), result);
                return result;
            }
            return "";
        }

    }
}
#endif
