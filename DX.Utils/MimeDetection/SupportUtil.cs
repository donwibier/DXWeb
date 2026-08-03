
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace DX.Utils.MimeDetection
{
    public class SupportUtil
    {
		public static sbyte[] ToSByteArray(byte[] byteArray) => byteArray?.Select(b => (sbyte)b).ToArray()!;
		public static byte[] ToByteArray(string sourceString) => UTF8Encoding.UTF8.GetBytes(sourceString);
		public static byte[] ToByteArray(object[] tempObjectArray) => tempObjectArray?.Select(o => (byte)o).ToArray()!;

		/// <summary>
		/// Obtains an array containing all the elements of the collection.
		/// </summary>
		/// <param name="objects">The array into which the elements of the collection will be stored.</param>
		/// <returns>The array containing all the elements of the collection.</returns>
		//public static object[] ToArray(System.Collections.ICollection c, System.Object[] objects)
		//      {
		//          int index = 0;

		//          System.Type type = objects.GetType().GetElementType();
		//          System.Object[] objs = (System.Object[])Array.CreateInstance(type, c.Count);

		//          System.Collections.IEnumerator e = c.GetEnumerator();

		//          while (e.MoveNext())
		//              objs[index++] = e.Current;

		//          //If objects is smaller than c then do not return the new array in the parameter
		//          if (objects.Length >= c.Count)
		//              objs.CopyTo(objects, 0);

		//          return objs;
		//      }
		public static object[] ToArray(System.Collections.ICollection c, System.Object[] objects)
		{
			var objs = new object[c.Count];
			c.CopyTo(objs, 0);
			return objs;
		}
		public static byte[] ToByteArray(sbyte[] tempObjectArray)
		{
			return tempObjectArray?.Select(b => (byte)b).ToArray()!;
		}
	}
}