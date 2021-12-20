using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DX.Data.Xpo.Identity;
using DevExpress.Xpo;
using DX.Data.Xpo;
using DX.Data.Xpo.Identity.Persistent;

namespace DX.Test.Web.Core.Models
{
	// This class will be persisted in the database by XPO
	// It should have the same properties as the ApplicationUser
	[MapInheritance(MapInheritanceType.ParentTable)]
	public class XpoApplicationUser : XpoDxUser
	{
		public XpoApplicationUser(Session session) : base(session)
		{
		}

		// STEP 1: Add custom fields here
		string functionTitle;
		string department;
		string companyName;

		[Size(100)]
		public string CompanyName
		{
			get => companyName;
			set => SetPropertyValue(nameof(CompanyName), ref companyName, value);
		}

		[Size(SizeAttribute.DefaultStringMappingFieldSize)]
		public string Department
		{
			get => department;
			set => SetPropertyValue(nameof(Department), ref department, value);
		}


		[Size(100)]
		public string FunctionTitle
		{
			get => functionTitle;
			set => SetPropertyValue(nameof(FunctionTitle), ref functionTitle, value);
		}

    }

	// Add profile data for application users by adding properties to the ApplicationUser class
	public class ApplicationUser : XPIdentityUser
	{
		public ApplicationUser()
		{

		}
		// STEP 2: Add custom fields here
		public string CompanyName { get; set; }
		public string Department { get; set; }
		public string FunctionTitle { get; set; }
	}


	public class ApplicationUserMapper : XPUserMapper<ApplicationUser, XpoApplicationUser>
	{
		public override XpoApplicationUser Assign(ApplicationUser source, XpoApplicationUser destination)
		{
			var result = base.Assign(source, destination);
			
			// STEP 3: Implement Mapping here
			result.CompanyName = source.CompanyName;
			result.Department = source.Department;
			result.FunctionTitle = source.FunctionTitle;

			return result;
		}

		public override string Map(string sourceField)
		{
			return base.Map(sourceField);
		}

		// STEP 4: Implement Mapping here
		public override Func<XpoApplicationUser, ApplicationUser> CreateModel =>
			(source) =>
			{
				var result = base.CreateModel(source);

				result.CompanyName = source.CompanyName;
				result.Department = source.Department;
				result.FunctionTitle = source.FunctionTitle;

				return result;
			};
    }

	[MapInheritance(MapInheritanceType.ParentTable)]
	public class XpoApplicationRole : XpoDxRole
	{
		public XpoApplicationRole(Session session) : base(session)
		{
		}		
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

    


}
