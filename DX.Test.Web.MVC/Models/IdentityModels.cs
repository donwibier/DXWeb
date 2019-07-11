using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.AspNet.Identity;


namespace DX.Test.Web.MVC.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : XPIdentityUser<string, XpoApplicationUser>
    {
		public ApplicationUser()
		{
		}

		//public ApplicationUser(XpoApplicationUser source) : base(source)
		//{
		//}

		//public ApplicationUser(XpoApplicationUser source, int loadingFlags) : base(source, loadingFlags)
		//{
		//}
		//public override void Assign(object source, int loadingFlags)
		//{
		//	base.Assign(source, loadingFlags);
		//	//XpoApplicationUser src = source as XpoApplicationUser;
		//	//if (src != null)
		//	//{
		//	//	// additional properties here
		//	//	this.PropertyA = src.PropertyA;
		//	//	// etc.				
		//	//}
		//}

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

	// This class will be persisted in the database by XPO
	// It should have the same properties as the ApplicationUser
	[MapInheritance(MapInheritanceType.ParentTable)]
	public class XpoApplicationUser : XpoDxUser
	{
		public XpoApplicationUser(Session session) : base(session)
		{
		}
		//public override void Assign(object source, int loadingFlags)
		//{
		//	base.Assign(source, loadingFlags);
		//	//ApplicationUser src = source as ApplicationUser;
		//	//if (src != null)
		//	//{
		//	//	// additional properties here
		//	//	this.PropertyA = src.PropertyA;
		//	//	// etc.				
		//	//}
		//}
	}

	public class ApplicationUserMapper : XPUserMapper<ApplicationUser, XpoApplicationUser>
	{
		public override XpoApplicationUser Assign(ApplicationUser source, XpoApplicationUser destination)
		{
			var result = base.Assign(source, destination);
			return result;
		}

		public override string Map(string sourceField)
		{
			return base.Map(sourceField);
		}

		public override Func<XpoApplicationUser, ApplicationUser> CreateModel => base.CreateModel;
	}

	public class ApplicationDbContext
	{
		public static DX.Data.Xpo.XpoDatabase Create()
		{
			return new DX.Data.Xpo.XpoDatabase("DefaultConnection");
		}
	}
}