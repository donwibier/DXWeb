using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace DX.Utils.Tests
{
    public static class TestResources
    {
        public static string StateOpen => "Open (localised)";
        public static string Closed => "Closed (localised)";
    }

    public enum TripState
    {
        [Display(Name = "StateOpen", ResourceType = typeof(TestResources))]
        Open,
        [Display(ResourceType = typeof(TestResources))]
        Closed,
        [Display(Name = "Missing", ResourceType = typeof(TestResources))]
        Pending,
        Plain,
        [Display(Name = "Nicely named")]
        Literal
    }

    [Flags]
    public enum Opts { None = 0, A = 1, B = 2 }

    public class EnumExtensionsTests
    {
        [Fact]
        public void GetDisplay_FromResource()
        {
            Assert.Equal("Open (localised)", TripState.Open.GetDisplay());
        }

        [Fact]
        public void GetDisplay_NoName_UsesValueNameAsResourceKey()
        {
            Assert.Equal("Closed (localised)", TripState.Closed.GetDisplay());
        }

        [Fact]
        public void GetDisplay_MissingResource_FallsBackToValueName()
        {
            Assert.Equal("Pending", TripState.Pending.GetDisplay());
        }

        [Fact]
        public void GetDisplay_NoAttribute_ReturnsName()
        {
            Assert.Equal("Plain", TripState.Plain.GetDisplay());
        }

        [Fact]
        public void GetDisplay_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((Enum)null).GetDisplay());
        }

        [Fact(Skip = "BUG: [Display(Name=\"...\")] without ResourceType (the common case) throws NullReferenceException instead of returning the Name")]
        public void GetDisplay_DisplayNameWithoutResourceType_ReturnsName()
        {
            Assert.Equal("Nicely named", TripState.Literal.GetDisplay());
        }

        [Fact(Skip = "BUG: GetDisplay throws NullReferenceException for undefined values or combined [Flags] values (GetField(value.ToString()) returns null)")]
        public void GetDisplay_UndefinedOrCombinedValue_ReturnsToString()
        {
            Assert.Equal("A, B", (Opts.A | Opts.B).GetDisplay());
            Assert.Equal("42", ((Opts)42).GetDisplay());
        }

        [Fact]
        public void GetDisplayValues_ForEnumWithoutLiteralDisplay()
        {
            var values = typeof(Opts).GetDisplayValues<Opts>();
            Assert.Equal(new[] { Opts.None, Opts.A, Opts.B }, values.Keys.ToArray());
            Assert.Equal("A", values[Opts.A]);
        }
    }

    [Description("described")]
    [Category("cat")]
    public class Decorated { }

    [Category("inherited")]
    public class DecoratedBase { }
    public class DecoratedChild : DecoratedBase { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DXUtilsTestMarkerAttribute : Attribute { }

    [DXUtilsTestMarker]
    public class Marked1 { }
    [DXUtilsTestMarker]
    public class Marked2 { }

    public class AttributesTests
    {
        [Fact]
        public void GetTypesWith_FindsDecoratedTypes()
        {
            var types = Attributes.GetTypesWith<DXUtilsTestMarkerAttribute>(false).ToList();
            Assert.Contains(typeof(Marked1), types);
            Assert.Contains(typeof(Marked2), types);
            Assert.DoesNotContain(typeof(Decorated), types);
        }

        [Fact]
        public void GetTypesWith_Inherit_IncludesDerivedTypes()
        {
            var types = Attributes.GetTypesWith<CategoryAttribute>(true).ToList();
            Assert.Contains(typeof(DecoratedChild), types);
            var direct = Attributes.GetTypesWith<CategoryAttribute>(false).ToList();
            Assert.DoesNotContain(typeof(DecoratedChild), direct);
        }

        [Fact(Skip = "BUG: GetAttributeData ignores TAttribute: it casts the first attribute of any type for every attribute found -> InvalidCastException / wrong data")]
        public void GetAttributeData_ReturnsDataOfRequestedAttributeOnly()
        {
            var cats = Attributes.GetAttributeData<CategoryAttribute, string>(typeof(Decorated), false, a => a.Category).ToList();
            Assert.Equal(new[] { "cat" }, cats);
        }

        [Fact(Skip = "BUG: Attributes.typeDictionary is a plain static Dictionary written without a lock; concurrent first calls throw InvalidOperationException (corrupted state) / KeyNotFoundException")]
        public void GetTypesWith_ConcurrentFirstCalls_DoNotThrow()
        {
            var field = typeof(Attributes).GetField("typeDictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var dict = (System.Collections.IDictionary)field.GetValue(null);
            int errors = 0;
            for (int round = 0; round < 50; round++)
            {
                dict.Clear();
                System.Threading.Tasks.Parallel.For(0, 64, i =>
                {
                    try
                    {
                        switch (i % 4)
                        {
                            case 0: Attributes.GetTypesWith<ObsoleteAttribute>(i % 8 == 0); break;
                            case 1: Attributes.GetTypesWith<DescriptionAttribute>(i % 8 == 1); break;
                            case 2: Attributes.GetTypesWith<SerializableAttribute>(i % 8 == 2); break;
                            default: Attributes.GetTypesWith<FlagsAttribute>(i % 8 == 3); break;
                        }
                    }
                    catch (Exception)
                    {
                        System.Threading.Interlocked.Increment(ref errors);
                    }
                });
            }
            dict.Clear();
            Assert.Equal(0, errors);
        }

        [Fact]
        public void GetAttributeData_NoAttributes_ReturnsEmpty()
        {
            Assert.Empty(Attributes.GetAttributeData<CategoryAttribute, string>(typeof(AttributesTests), false, a => a.Category));
        }
    }

    public class RequiredIfModel
    {
        public string Kind { get; set; }

        [RequiredIf(nameof(Kind), RequiredIfComparison.IsEqualTo, "Company")]
        public string CompanyName { get; set; }

        [RequiredIf(nameof(Kind), RequiredIfComparison.IsNotEqualTo, "Company")]
        public string FirstName { get; set; }

        [RequiredIf("Parent_Kind", RequiredIfComparison.IsEqualTo, "Company")]
        public string VatNumber { get; set; }
    }

    public class RequiredIfAttributeTests
    {
        // RequiredIf returns results without MemberNames (see bug test below), so match on the message text.
        static bool About(ValidationResult r, string member) => r.ErrorMessage.Contains(member);

        static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, true);
            return results;
        }

        [Fact]
        public void Ctor_NullOtherProperty_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RequiredIfAttribute(null, RequiredIfComparison.IsEqualTo, 1));
        }

        [Fact]
        public void Validate_Comparisons()
        {
            var eq = new RequiredIfAttribute("X", RequiredIfComparison.IsEqualTo, "a");
            Assert.True(eq.Validate("a"));
            Assert.False(eq.Validate("b"));
            Assert.False(eq.Validate(null));
            var ne = new RequiredIfAttribute("X", RequiredIfComparison.IsNotEqualTo, "a");
            Assert.False(ne.Validate("a"));
            Assert.True(ne.Validate("b"));
            Assert.True(ne.Validate(null));
            Assert.True(new RequiredIfAttribute("X", RequiredIfComparison.IsEqualTo, null).Validate(null));
        }

        [Fact]
        public void RequiredWhenConditionMet()
        {
            var results = Validate(new RequiredIfModel { Kind = "Company", FirstName = "x" });
            Assert.Contains(results, r => About(r, nameof(RequiredIfModel.CompanyName)));
        }

        [Fact]
        public void NotRequiredWhenConditionNotMet()
        {
            var results = Validate(new RequiredIfModel { Kind = "Person", FirstName = "x" });
            Assert.DoesNotContain(results, r => About(r, nameof(RequiredIfModel.CompanyName)));
        }

        [Fact]
        public void IsNotEqualTo()
        {
            Assert.Contains(Validate(new RequiredIfModel { Kind = "Person" }), r => About(r, nameof(RequiredIfModel.FirstName)));
            Assert.DoesNotContain(Validate(new RequiredIfModel { Kind = "Company", CompanyName = "c" }), r => About(r, nameof(RequiredIfModel.FirstName)));
        }

        [Fact]
        public void ValuePresent_IsAlwaysValid()
        {
            Assert.DoesNotContain(Validate(new RequiredIfModel { Kind = "Company", CompanyName = "c", VatNumber = "v" }),
                r => About(r, nameof(RequiredIfModel.CompanyName)));
        }

        [Fact(Skip = "BUG: RequiredIfAttribute never passes DefaultErrorMessageFormatString to the base ctor, so the message is 'The field X is invalid.' instead of 'The X field is required.'")]
        public void DefaultErrorMessage_SaysRequired()
        {
            var r = Validate(new RequiredIfModel { Kind = "Company", FirstName = "x" }).First(x => About(x, nameof(RequiredIfModel.CompanyName)));
            Assert.Equal("The CompanyName field is required.", r.ErrorMessage);
        }

        [Fact(Skip = "BUG: RequiredIfAttribute.IsValid returns ValidationResult without memberNames, so EditForm/Blazor validators can't attach the error to the field")]
        public void ValidationResult_CarriesMemberName()
        {
            var r = Validate(new RequiredIfModel { Kind = "Company", FirstName = "x" }).Single();
            Assert.Equal(new[] { nameof(RequiredIfModel.CompanyName) }, r.MemberNames);
        }

        [Fact(Skip = "BUG: the '_'-prefixed fallback path in IsValid inverts the check ('!Validate'), so 'Parent_Kind' reports errors exactly when the condition is NOT met")]
        public void UnderscoreFallback_BehavesLikeDirectProperty()
        {
            Assert.Contains(Validate(new RequiredIfModel { Kind = "Company", CompanyName = "c" }), r => About(r, nameof(RequiredIfModel.VatNumber)));
            Assert.DoesNotContain(Validate(new RequiredIfModel { Kind = "Person", FirstName = "f" }), r => About(r, nameof(RequiredIfModel.VatNumber)));
        }
    }
}
