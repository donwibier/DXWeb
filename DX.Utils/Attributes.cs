using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        // Materialised results per (attribute type, inherit); thread safe. Each entry remembers which assemblies it has
        // scanned, so an assembly loaded later (plugins, lazy-loaded Blazor assemblies) is scanned on the next call
        private static readonly ConcurrentDictionary<string, TypeScan> typeDictionary = new ConcurrentDictionary<string, TypeScan>();

        // bumped on every assembly load in this AppDomain: entries with an older version look for new assemblies
        private static int assemblyVersion;

        static Attributes()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (s, e) => Interlocked.Increment(ref assemblyVersion);
        }

        private sealed class TypeScan
        {
            public readonly object Lock = new object();
            public readonly HashSet<Assembly> Scanned = new HashSet<Assembly>();
            public volatile IEnumerable<Type> Types = Array.AsReadOnly(new Type[0]);
            public volatile int Version = -1;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the types withs in this collection. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. Results are cached; assemblies loaded after an earlier call are picked up. </remarks>
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
            var scan = typeDictionary.GetOrAdd(key, k => new TypeScan());

            // read the version before listing assemblies: a load during the scan bumps it, so the next call rescans
            int version = Volatile.Read(ref assemblyVersion);
            if (scan.Version == version)
                return scan.Types;

            lock (scan.Lock)
            {
                if (scan.Version != version)
                {
                    var found = new List<Type>(scan.Types);
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (!scan.Scanned.Add(assembly))
                            continue;
                        found.AddRange(GetLoadableTypes(assembly).Where(t => t.IsDefined(typeof(TAttribute), inherit)));
                    }
                    scan.Types = found.AsReadOnly();
                    scan.Version = version;
                }
                return scan.Types;
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
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
            var res = from a in Attribute.GetCustomAttributes(type, typeof(TAttribute), inherit)
                      select result((TAttribute)a);
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

#if (!NETFRAMEWORK)
    public enum RequiredIfComparison
    {
        IsEqualTo,
        IsNotEqualTo
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessageFormatString = "The {0} field is required.";
        public string OtherProperty { get; private set; }
        public RequiredIfComparison Comparison { get; private set; }
        public object Value { get; private set; }
        public RequiredIfAttribute(string otherProperty, RequiredIfComparison comparison, object value)
            : base(DefaultErrorMessageFormatString)
        {
            if (string.IsNullOrEmpty(otherProperty))
            {
                throw new ArgumentNullException("otherProperty");
            }

            OtherProperty = otherProperty;
            Comparison = comparison;
            Value = value;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, OtherProperty);
        }

        public bool Validate(object actualPropertyValue)
        {
            switch (Comparison)
            {
                case RequiredIfComparison.IsNotEqualTo:
                    return actualPropertyValue == null ? Value != null : !actualPropertyValue.Equals(Value);
                default:
                    return actualPropertyValue == null ? Value == null : actualPropertyValue.Equals(Value);
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                try
                {
                    var property = validationContext.ObjectInstance.GetType().GetProperty(OtherProperty);
                    var propertyValue = property.GetValue(validationContext.ObjectInstance, null);

                    if (Validate(propertyValue))
                    {
                        return CreateFailure(validationContext);
                    }
                }
                catch (Exception)
                {
                    if (OtherProperty.Contains('_'))
                    {
                        var prop = OtherProperty.Split('_');

                        var property = validationContext.ObjectInstance.GetType().GetProperty(prop.Last());
                        var propertyValue = property.GetValue(validationContext.ObjectInstance, null);

                        if (Validate(propertyValue))
                        {
                            return CreateFailure(validationContext);
                        }
                    }
                }
            }
            return ValidationResult.Success;
        }

        private ValidationResult CreateFailure(ValidationContext validationContext)
        {
            string message = FormatErrorMessage(validationContext.DisplayName);
            return string.IsNullOrEmpty(validationContext.MemberName)
                ? new ValidationResult(message)
                : new ValidationResult(message, new[] { validationContext.MemberName });
        }
    }
#endif

}
