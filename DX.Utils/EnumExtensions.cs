using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace DX.Utils
{
#if (NETSTANDARD2_1 || NET6_0)
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
        public static string GetDisplay(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            DisplayAttribute[] attributes = (DisplayAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Count() == 0)
                return string.Format("{0}", value);
            else if (attributes.Count() > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            string resourceId = string.IsNullOrEmpty(attributes[0].Name) ? value.ToString(): attributes[0].Name;
            Type type = attributes[0].ResourceType;
            PropertyInfo nameProperty = type.GetProperty(resourceId, BindingFlags.Static | BindingFlags.Public);
            
            if (nameProperty == null)
                return value.ToString();

            object result = nameProperty.GetValue(nameProperty.DeclaringType, null);
            return (result != null) ? result.ToString() : string.Empty;
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
