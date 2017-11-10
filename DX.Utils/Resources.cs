////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	resources.cs
//
// summary:	Implements the resources class
////////////////////////////////////////////////////////////////////////////////////////////////////
#if (!NETSTANDARD2_0)

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace DX.Utils
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Resources helper class. </summary>
    ///
    /// <remarks>   Don, 3-2-2016. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class Resources
    {
        const string rootUrl = "~/App_LocalResources";

#region Resource String Extension methods

        public static string GetResourced(this string message)
        {
            return GetText(message, (CultureInfo)null);
        }

        public static string GetResourced(this string message, bool scriptEncode)
        {
            return GetText(message, (CultureInfo)null, scriptEncode);
        }

        public static string GetResourced(this string message, string cultureName = "", string localResourceRootUrl = rootUrl)
        {
            return GetText(message, cultureName, false, localResourceRootUrl);
        }
#endregion

        public static string GetResourced(string classKey, string resourceKey, string defaultValue, string cultureName = "", bool scriptEncode = false, string localResourceRootUrl = rootUrl)
        {
            return GetText(String.Format("$Resource('{0}', '{1}', '{2}')", classKey, resourceKey, defaultValue), cultureName, scriptEncode, localResourceRootUrl);
        }

        public static string GetText(string message, string cultureName= "", bool scriptEncode = false, string localResourceRootUrl = rootUrl)
        {
            CultureInfo ci = String.IsNullOrEmpty(cultureName) ? null : new CultureInfo(cultureName);
            return GetText(message, ci, scriptEncode, localResourceRootUrl);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a text. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="message">  The message. </param>
        /// <param name="culture">  The culture. </param>
        ///
        /// <returns>   The text. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetText(string message, System.Globalization.CultureInfo culture, bool scriptEncode = false, string localResourceRootUrl = rootUrl)
        {
            if (String.IsNullOrEmpty(message))
                return "";
            //check for "$Resource("<file>", "<key>"[, "<DefaultText>"])
            string pattern = @"\$Resource\x28(?:[\s]*)\'(?'classKey'[a-zA-Z0-9,.:;_-]*)\'(?:[\s]*)\,(?:[\s]*)\'(?'resourceKey'[a-zA-Z0-9,.:;_-]*)\'(?:[\s]*)((\,(?:[\s]*)\'(?'defaultText'.*)\'(?:[\s]*))?)\x29";
            if (Regex.IsMatch(message, pattern))
            {
                return Regex.Replace(message, pattern, match =>
                {
                    //get the resource item (named group of the regex)
                    string classKey = match.Groups["classKey"].Value;
                    string resourceKey = match.Groups["resourceKey"].Value;
                    string defaultText = (match.Groups["defaultText"] != null) ? defaultText = match.Groups["defaultText"].Value : "";
                    string result = match.Value;
                    if ((!String.IsNullOrEmpty(classKey)) && (!String.IsNullOrEmpty(resourceKey)))
                    {
                        object obj = null;

                        string localResourceUrl = Urls.AppRelativeToAbsolute(String.Format("{0}/{1}.resx", localResourceRootUrl, classKey));
                        if (System.IO.File.Exists(HostingEnvironment.MapPath(localResourceUrl)))
                        {
                            try
                            {
                                obj = culture == null ?
                                    HttpContext.GetLocalResourceObject(classKey, resourceKey) :
                                    HttpContext.GetLocalResourceObject(classKey, resourceKey, culture);
                            }
                            catch
                            {
                                obj = null;
                            }
                        }

                        try
                        {
                            obj = culture == null ?
                                HttpContext.GetGlobalResourceObject(classKey, resourceKey) :
                                HttpContext.GetGlobalResourceObject(classKey, resourceKey, culture);
                        }
                        catch
                        {
                            obj = null;
                        }
                        if (obj != null)
                            result = obj.ToString();
                        else
                        {
                            if (match.Groups["defaultText"] != null)
                                result = defaultText;
                        }
                    }
                    return message;
                }, RegexOptions.ExplicitCapture);
            }
            //check for "$AppSetting("<key>"[, "<default>"])
            pattern = @"\$AppSetting\x28(?:[\s]*)\'(?'key'[a-zA-Z0-9,.:;_-]*)\'(?:[\s]*)((\,(?:[\s]*)\'(?'defaultText'.*)\'(?:[\s]*))?)\x29";
            if (Regex.IsMatch(message, pattern))
            {
                return Regex.Replace(message, pattern, match =>
                {
                    //get the AppSetting name
                    string appSettingName = match.Groups["key"].Value;
                    string result = match.Value;
                    if (!String.IsNullOrEmpty(appSettingName))
                        try
                        {
                            result = ConfigurationManager.AppSettings[appSettingName];
                        }
                        catch (Exception err)
                        {
                            result = String.Format("{0} (ERR:{1})", match.Value, err.Message);
                        }
                    return message;
                }, RegexOptions.ExplicitCapture);
            }

            return scriptEncode ? HttpUtility.JavaScriptStringEncode(message) : message;
        }
    }


}
#endif