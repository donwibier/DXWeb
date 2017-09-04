using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DX.Utils
{
    public delegate TResult AttributeDataResult<TAttribute, TResult>(TAttribute attribute);

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An attributes. </summary>
    ///
    /// <remarks>   Don, 3-2-2016. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Attributes
    {
        private static readonly Dictionary<string, IEnumerable<Type>> typeDictionary = new Dictionary<string, IEnumerable<Type>>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the types withs in this collection. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <typeparam name="TAttribute">   Type of the attribute. </typeparam>
        /// <param name="inherit">  true to inherit. </param>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to process the types withs in this collection.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : System.Attribute
        {
            string key = String.Format("{0}${1}", typeof(TAttribute).AssemblyQualifiedName, inherit);

            if (!typeDictionary.ContainsKey(key))
            {
                typeDictionary[key] = from a in AppDomain.CurrentDomain.GetAssemblies()
                                      from t in a.GetTypes()
                                      where t.IsDefined(typeof(TAttribute), inherit)
                                      select t;
            }
            return typeDictionary[key];
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the attribute data in this collection. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <typeparam name="TAttribute">   Type of the attribute. </typeparam>
        /// <typeparam name="TResult">      Type of the result. </typeparam>
        /// <param name="type">     The type. </param>
        /// <param name="inherit">  true to inherit. </param>
        /// <param name="result">   The result. </param>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to process the attribute data in this collection.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static IEnumerable<TResult> GetAttributeData<TAttribute, TResult>(Type type, bool inherit, AttributeDataResult<TAttribute, TResult> result)
            where TAttribute : System.Attribute
        {
            var res = from a in Attribute.GetCustomAttributes(type, inherit)
                      where typeof(Attribute).IsAssignableFrom(typeof(TAttribute))                            
                      select (TResult)result((TAttribute)Attribute.GetCustomAttributes(type).FirstOrDefault<Attribute>());
            return res;
        }

        //private static bool IsMemberTested(MemberInfo member)
        //{
        //    foreach (object attribute in member.GetCustomAttributes(true))
        //    {
        //        if (attribute is IsTestedAttribute)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
