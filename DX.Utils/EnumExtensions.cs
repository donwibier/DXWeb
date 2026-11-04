using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace DX.Utils
{
#if (NETSTANDARD2_1 || NET6_0_OR_GREATER)
    public static class EnumExtensions
    {
        /*
        public enum BusinessTripStates
        {
            [Display(Name = "BusinessTripStateCleared", ResourceType = typeof(Resources))]
            Cleared,
            [Display(Name = "BusinessTripStateBillingRejected", ResourceType = typeof(Resources))]
            BillingRejected,
            [Display(Name = "BusinessTripStateBillingReviewed", ResourceType = typeof(Resources))]
            BillingReviewed,
            [Display(Name = "BusinessTripStateBillingReviewInDue", ResourceType = typeof(Resources))]
            BillingReviewInDue,
            [Display(Name = "BusinessTripStateClosed", ResourceType = typeof(Resources))]
            Closed
        }
        //DataSource = typeof(BusinessTripStates).GetDisplayValues();
        //ValueField = "Key";
        //TextField = "Value";
        */
        // Per enum value: either a literal display text, or the static resource property to read
        // on every call (resource values depend on the current UI culture, so only the lookup is cached).
        sealed class DisplayInfo
        {
            public string Text;
            public PropertyInfo ResourceProperty;
        }

        static readonly ConcurrentDictionary<Enum, DisplayInfo> displayCache = new ConcurrentDictionary<Enum, DisplayInfo>();

        public static string GetDisplay(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!displayCache.TryGetValue(value, out DisplayInfo info))
            {
                FieldInfo field = value.GetType().GetField(value.ToString());
                if (field == null)
                    return value.ToString(); // undefined or combined [Flags] value: not cached (unbounded set)
                info = displayCache.GetOrAdd(value, ResolveDisplay(value, field));
            }
            if (info.ResourceProperty == null)
                return info.Text;
            object result = info.ResourceProperty.GetValue(info.ResourceProperty.DeclaringType, null);
            return (result != null) ? result.ToString() : string.Empty;
        }

        static DisplayInfo ResolveDisplay(Enum value, FieldInfo field)
        {
            DisplayAttribute[] attributes = (DisplayAttribute[])field.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length == 0)
                return new DisplayInfo { Text = string.Format("{0}", value) };
            else if (attributes.Length > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            string resourceId = string.IsNullOrEmpty(attributes[0].Name) ? value.ToString() : attributes[0].Name;
            Type type = attributes[0].ResourceType;
            if (type == null)
                return new DisplayInfo { Text = resourceId }; // plain [Display(Name = "...")]

            PropertyInfo nameProperty = type.GetProperty(resourceId, BindingFlags.Static | BindingFlags.Public);
            if (nameProperty == null)
                return new DisplayInfo { Text = value.ToString() };
            return new DisplayInfo { ResourceProperty = nameProperty };
        }

        //public static string GetDisplayName(this Enum enumValue)
        //{
        //    return enumValue.GetType()
        //                    .GetMember(enumValue.ToString())
        //                    .First()
        //                    .GetCustomAttribute<DisplayAttribute>()
        //                    .GetName();
        //}

        public static Dictionary<TEnumType, string> GetDisplayValues<TEnumType>(this Type enumType)
            where TEnumType : Enum
        {
            var enumValues = new Dictionary<TEnumType, string>();
            var test = Enum.GetValues(enumType);
            foreach (Enum value in Enum.GetValues(enumType))
            {
                enumValues.Add((TEnumType)value, value.GetDisplay());
            }
            return enumValues;
        }        
    }
#endif
}
