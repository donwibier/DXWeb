using System;
using System.Collections.Generic;
using System.Text;

namespace DX.Utils
{
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Returns the given date with 0 Timepart. For selecting something with given Date
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime DateMin(this DateTime date)
		{
			var result = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
			return result;
		}
		/// <summary>
		/// Returns the given date + 1 day with 0 timepart. For selecting with given Date
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime DateMax(this DateTime date)
		{
			var result = date.DateMin().AddDays(1);
			return result;
		}
	}
}
