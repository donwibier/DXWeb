using System;
using System.Collections.Generic;
using System.Text;

namespace DX.Utils
{
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Return the lowest InnerException from an exception
		/// </summary>
		/// <param name="e">Exception</param>
		/// <returns>InnerException (if any)</returns>
		public static Exception GetInnerException(this Exception e)
		{
			var result = e;
			while (result.InnerException != null)
			{
				result = result.InnerException;
			}
			return result;
		}
	}
}
