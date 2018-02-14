using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DX.Data.Xpo.Identity;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;

namespace DX.Test.Web.Core.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : XPIdentityUser<XpoApplicationUser>
    {
        public ApplicationUser(XpoApplicationUser source) : base(source)
        {}

        public ApplicationUser(XpoApplicationUser source, int loadingFlags) : base(source, loadingFlags)
        {}

        public ApplicationUser()
        {}

        public override void Assign(object source, int loadingFlags)
        {
            base.Assign(source, loadingFlags);
            //XpoApplicationUser src = source as XpoApplicationUser;
            //if (src != null)
            //{
            //	// additional properties here
            //	this.PropertyA = src.PropertyA;
            //	// etc.				
            //}
        }
    }

    public class ApplicationRole : XPIdentityRole<XpoApplicationRole>
    {
        public ApplicationRole(XpoApplicationRole source, int loadingFlags) : base(source, loadingFlags)
        {}

        public ApplicationRole(XpoApplicationRole source) : base(source)
        {}

        public ApplicationRole()
        {}
        public override void Assign(object source, int loadingFlags)
        {
            base.Assign(source, loadingFlags);
            //XpoApplicationRole src = source as XpoApplicationRole;
            //if (src != null)
            //{
            //	// additional properties here
            //	this.PropertyA = src.PropertyA;
            //	// etc.				
            //}
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
        public override void Assign(object source, int loadingFlags)
        {
            base.Assign(source, loadingFlags);
            //ApplicationUser src = source as ApplicationUser;
            //if (src != null)
            //{
            //	// additional properties here
            //	this.PropertyA = src.PropertyA;
            //	// etc.				
            //}
        }
    }

    [MapInheritance(MapInheritanceType.ParentTable)]
    public class XpoApplicationRole : XpoDxRole
    {
        public XpoApplicationRole(Session session) : base(session)
        {
        }
        public override void Assign(object source, int loadingFlags)
        {
            base.Assign(source, loadingFlags);
            //ApplicationUser src = source as ApplicationUser;
            //if (src != null)
            //{
            //	// additional properties here
            //	this.PropertyA = src.PropertyA;
            //	// etc.				
            //}
        }
    }


}
