using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DX.Utils
{
    public static class Conversion
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts the value to an int with default 0 on conversion error. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">    The value. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static int ParseInt(object value)
        {
            return ParseInt(value, 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Converts an object to an int with default on conversion error with standard currency info.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">        The value to parse. </param>
        /// <param name="defaultValue"> The default value. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static int ParseInt(object value, int defaultValue)
        {
            int result = defaultValue;
            try
            {
                if (value != null)
                    result = Convert.ToInt32(value);
            }
            catch
            {
                result = defaultValue;
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Converts the string to double with default 0 in case of conversion error with standard
        /// currency info.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">    The value to convert. </param>
        ///
        /// <returns>   The double value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static double ParseDouble(object value)
        {
            return ParseDouble(value, 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Converts the string to double with default in case of conversion error with standard currency
        /// info.
        /// E.g.
        ///     ParseDouble("1,123,342.87", 0)  -> 1,123,342.87 ParseDouble("1.123.342,87", -1) -> -1.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">        The string value. </param>
        /// <param name="defaultValue"> The default value. </param>
        ///
        /// <returns>   The double value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static double ParseDouble(object value, double defaultValue)
        {
            return ParseDouble(value, defaultValue, ",", ".");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts a string to double with default in case of conversion error. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">                The object to convert. </param>
        /// <param name="defaultValue">         The default value in case of error. </param>
        /// <param name="thousandSeparator">    The thousand separator. </param>
        /// <param name="decimalSeparator">     The decimal separator. </param>
        ///
        /// <returns>   The double value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static double ParseDouble(object value, double defaultValue, string thousandSeparator, string decimalSeparator)
        {
            double result = defaultValue;
            if (value != null)
            {
                try
                {
                    NumberFormatInfo provider = new NumberFormatInfo
                    {
                        NumberDecimalSeparator = decimalSeparator,
                        NumberGroupSeparator = thousandSeparator,
                        NumberGroupSizes = new int[] { 3 }
                    };
                    result = Convert.ToDouble(value, provider);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Converts an object to decimal with default 0 in case of conversion error with standard
        /// currency info.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">    The string value. </param>
        ///
        /// <returns>   The decimal value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static decimal ParseDecimal(object value)
        {
            return ParseDecimal(value, 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Converts an object to decimal with default in case of conversion error with standard currency
        /// info.
        /// E.g.
        ///     ParseDecimal("1,123,342.87", 0)  -> 1,123,342.87 ParseDecimal("1.123.342,87", -1) -> -1.
        /// </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">        The string value. </param>
        /// <param name="defaultValue"> The default value. </param>
        ///
        /// <returns>   The decimal value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static decimal ParseDecimal(object value, decimal defaultValue)
        {
            return ParseDecimal(value, defaultValue, ",", ".");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts an object to decimal with default in case of conversion error. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">                The string value. </param>
        /// <param name="defaultValue">         The default value. </param>
        /// <param name="thousandSeparator">    The thousand separator. </param>
        /// <param name="decimalSeparator">     The decimal separator. </param>
        ///
        /// <returns>   The decimal value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static decimal ParseDecimal(object value, decimal defaultValue, string thousandSeparator, string decimalSeparator)
        {
            decimal result = defaultValue;
            if (value != null)
            {
                try
                {
                    NumberFormatInfo provider = new NumberFormatInfo
                    {
                        NumberDecimalSeparator = decimalSeparator,
                        NumberGroupSeparator = thousandSeparator,
                        NumberGroupSizes = new int[] { 3 }
                    };
                    result = Convert.ToDecimal(value, provider);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts a string to boolean ("T", "True", 1) -> true rest false. </summary>
        ///
        /// <remarks>   Don, 26-11-2012. </remarks>
        ///
        /// <param name="value">    The value. </param>
        ///
        /// <returns>   The converted boolean or false on an exception. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool ParseBool(object value)
        {
            if (value == null)
                return false;
            else if (value is string)
            {
                if (String.IsNullOrEmpty((string)value))
                    return false;

                string s = ((string)value).ToUpper();
                return (s.Equals("TRUE") || s.Equals("1") || (s.Equals("T")));
            }

            bool result = false;
            try
            {
                result = Convert.ToBoolean(value);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Parses an object into an Enum. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="TEnum">    Type of the Enum. </typeparam>
        /// <param name="value">    The object to parse (it will be converted to a string by <c>String.Format("{0}", value)</c>. </param>
        ///
        /// <returns> The Enum or in case of a conversion error the default Enum value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static TEnum ParseEnum<TEnum>(object value)
            where TEnum : struct, IConvertible
        {
            return ParseEnum<TEnum>(value, default(TEnum));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Parses an object into an Enum. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="TEnum">    Type of the enum. </typeparam>
        /// <param name="value">        The object to parse (it will be converted to a string by
        ///                             <c>String.Format("{0}", value)</c>. </param>
        /// <param name="defaultValue"> The default value. </param>
        ///
        /// <returns>   The Enum value or in case of a conversion error the <paramref name="defaultValue"/>. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static TEnum ParseEnum<TEnum>(object value, TEnum defaultValue)
            where TEnum : struct, IConvertible
        {
            TEnum result;
            if (!Enum.TryParse<TEnum>(String.Format("{0}", value), out result))
                result = defaultValue;
            return result;
        }

		public static bool GetConfigOption(string configString, string configOption, out string result)
		{
			result = String.Empty;
			Dictionary<string, string> connStringParts = configString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(t => t.Split(new char[] { '=' }, 2))
					.ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.InvariantCultureIgnoreCase);
			if (connStringParts.ContainsKey(configOption))
			{
				result = connStringParts[configOption];
				return true;
			}
			return false;
		}

		public static TEnum GetConfigOption<TEnum>(string configString, string configOption, TEnum defaultValue)
			where TEnum:struct, IConvertible
		{
			string resultString =  "";
			TEnum result = defaultValue;
			if (GetConfigOption(configString, configOption, out resultString))
			{
				Enum.TryParse<TEnum>(resultString, out result);
			}

			return result;
		}

		public static bool GetConfigOption(string configString, string configOption, bool defaultValue = false)
		{
			string resultString = "";
			bool result = defaultValue;
			if (GetConfigOption(configString, configOption, out resultString))
			{
				result = ParseBool(resultString);
			}
			return result;
		}
		#region Min/Max methods

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>   Returns the lowest (min.) value from the <c>values</c>. </summary>
		///
		/// <remarks>   Don, 30-11-2012. </remarks>
		///
		/// <typeparam name="T">    Generic type parameter. </typeparam>
		/// <param name="values">   The values. </param>
		///
		/// <returns>   The lowest value. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public static T MinValue<T>(IEnumerable<T> values)
        {
            return values.Min();
            
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the highest (max.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The highest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static T MaxValue<T>(IEnumerable<T> values)
        {
            return values.Max();

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the lowest (min.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The lowest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static T MinValue<T>(params T[] values)
        {
            return values.Min();

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the highest (max.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The highest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static T MaxValue<T>(params T[] values)
        {
            return values.Max();

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the lowest (min.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The lowest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static int MinValue(params int[] values)
        {
            return MinValue<int>(values);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the highest (max.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The highest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static int MaxValue(params int[] values)
        {
            return MaxValue<int>(values);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the lowest (min.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The lowest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static double MinValue(params double[] values)
        {
            return MinValue<double>(values);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the highest (max.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The highest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static double MaxValue(params double[] values)
        {
            return MaxValue<double>(values);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the lowest (min.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The lowest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static decimal MinValue(params decimal[] values)
        {
            return MinValue<decimal>(values);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the highest (max.) value from the <c>values</c>. </summary>
        ///
        /// <remarks>   Don, 30-11-2012. </remarks>
        ///
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="values">   The values. </param>
        ///
        /// <returns>   The highest value. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static decimal MaxValue(params decimal[] values)
        {
            return MaxValue<decimal>(values);
        }

        #endregion
    }
}
