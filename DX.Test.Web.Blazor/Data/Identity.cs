using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity;
using DX.Data.Xpo.Identity.Persistent;
using System;


namespace DX.Test.Web.Blazor.Data
{
	public class ApplicationUserMapper : XPUserMapper<ApplicationUser, XpoApplicationUser>
	{
        public override Func<XpoApplicationUser, ApplicationUser> CreateModel => (source) => {
            var r = base.CreateModel(source);
            r.BirthDate = source.BirthDate;
            r.Street = source.Street;
            r.HouseNo = source.HouseNo;
            r.HouseNoSuffix = source.HouseNoSuffix;
            r.ZipCode = source.ZipCode;
            r.City = source.City;
            r.State = source.State;
            r.Country = source.Country;
            return r;
        };
        public override XpoApplicationUser Assign(ApplicationUser source, XpoApplicationUser destination)
        {
            XpoApplicationUser result = base.Assign(source, destination);
            result.BirthDate = source.BirthDate;
            result.Street = source.Street;
            result.HouseNo = source.HouseNo;
            result.HouseNoSuffix = source.HouseNoSuffix;
            result.ZipCode = source.ZipCode;
            result.City = source.City;
            result.State = source.State;
            result.Country = source.Country;

            return result;
        }
        public override string Map(string sourceField)
		{
			return base.Map(sourceField);
		}		
	}

	// Add profile data for application users by adding properties to the ApplicationUser class
	public class ApplicationUser : XPIdentityUser
	{
		public ApplicationUser()
		{

		}

		public DateTime BirthDate { get; set; }
		public string Street { get; set; }
		public string HouseNo { get; set; }
		public string HouseNoSuffix { get; set; }
		public string ZipCode { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }

	}

    // This class will be persisted in the database by XPO
    // It should have the same properties as the ApplicationUser
    [MapInheritance(MapInheritanceType.ParentTable)]
    public class XpoApplicationUser : XpoDxUser
    {
        public XpoApplicationUser(Session session) : base(session)
        {
        }

        DateTime birthDate;
        string country;
        string state;
        string city;
        string zipCode;
        string houseNoSuffix;
        string houseNo;
        string street;


        public DateTime BirthDate
        {
            get => birthDate;
            set => SetPropertyValue(nameof(BirthDate), ref birthDate, value);
        }
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Street
        {
            get => street;
            set => SetPropertyValue(nameof(Street), ref street, value);
        }

        [Size(10)]
        public string HouseNo
        {
            get => houseNo;
            set => SetPropertyValue(nameof(HouseNo), ref houseNo, value);
        }

        [Size(10)]
        public string HouseNoSuffix
        {
            get => houseNoSuffix;
            set => SetPropertyValue(nameof(HouseNoSuffix), ref houseNoSuffix, value);
        }

        [Size(10)]
        public string ZipCode
        {
            get => zipCode;
            set => SetPropertyValue(nameof(ZipCode), ref zipCode, value);
        }

        [Size(50)]
        public string City
        {
            get => city;
            set => SetPropertyValue(nameof(City), ref city, value);
        }

        [Size(50)]
        public string State
        {
            get => state;
            set => SetPropertyValue(nameof(State), ref state, value);
        }

        [Size(50)]
        public string Country
        {
            get => country;
            set => SetPropertyValue(nameof(country), ref country, value);
        }

        // Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 6/16/2022 10:19 AM
        public new class FieldsClass : XpoDxUser.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public const string BirthDateFieldName = "BirthDate";

            public OperandProperty BirthDate => new OperandProperty(GetNestedName(BirthDateFieldName));

            public const string StreetFieldName = "Street";

            public OperandProperty Street => new OperandProperty(GetNestedName(StreetFieldName));

            public const string HouseNoFieldName = "HouseNo";

            public OperandProperty HouseNo => new OperandProperty(GetNestedName(HouseNoFieldName));

            public const string HouseNoSuffixFieldName = "HouseNoSuffix";

            public OperandProperty HouseNoSuffix => new OperandProperty(GetNestedName(HouseNoSuffixFieldName));

            public const string ZipCodeFieldName = "ZipCode";

            public OperandProperty ZipCode => new OperandProperty(GetNestedName(ZipCodeFieldName));

            public const string CityFieldName = "City";

            public OperandProperty City => new OperandProperty(GetNestedName(CityFieldName));

            public const string StateFieldName = "State";

            public OperandProperty State => new OperandProperty(GetNestedName(StateFieldName));

            public const string CountryFieldName = "Country";

            public OperandProperty Country => new OperandProperty(GetNestedName(CountryFieldName));
        }

        public new static FieldsClass Fields
        {
            get
            {
                if (ReferenceEquals(_Fields, null))
                {
                    _Fields = new FieldsClass();
                }

                return _Fields;
            }
        }

        static FieldsClass _Fields;
    }


    public class ApplicationRole : XPIdentityRole
	{
		public ApplicationRole()
		{ }
	}
	public class ApplicationRoleMapper : XPRoleMapper<string, ApplicationRole, XpoApplicationRole>
	{
		public override Func<XpoApplicationRole, ApplicationRole> CreateModel => base.CreateModel;

		public override XpoApplicationRole Assign(ApplicationRole source, XpoApplicationRole destination)
		{
			XpoApplicationRole result = base.Assign(source, destination);
			return result;
		}

		public override string Map(string sourceField)
		{
			return base.Map(sourceField);
		}
	}


	[MapInheritance(MapInheritanceType.ParentTable)]
	public class XpoApplicationRole : XpoDxRole
	{
		public XpoApplicationRole(Session session) : base(session)
		{
		}

        // Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 6/16/2022 10:19 AM
        public new class FieldsClass : XpoDxRole.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }
        }

        public new static FieldsClass Fields
        {
            get
            {
                if (ReferenceEquals(_Fields, null))
                {
                    _Fields = new FieldsClass();
                }

                return _Fields;
            }
        }

        static FieldsClass _Fields;
    }
}
