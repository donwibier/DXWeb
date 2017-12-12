using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
#if(NETSTANDARD2_0)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{
#if(!NETSTANDARD2_0)
    public class XPRoleStore<TRole, TXPORole> : XPRoleStore<string, TRole, TXPORole>
		where TRole : XPIdentityRole<string, TXPORole>, IRole<string>
		where TXPORole : XPBaseObject, IDxRole<string>, IRole<string>
	{
		public XPRoleStore() : 
			base() { }
		public XPRoleStore(string connectionName) : 
			base(connectionName) { }
		public XPRoleStore(string connectionString, string connectionName) : 
			base(connectionString, connectionName) { }
		public XPRoleStore(XpoDatabase database) : 
			base(database) { }
	}

	public class XPRoleStore<TKey, TRole, TXPORole/*, TXPOUser*/> : XpoStore<TXPORole, TKey>,
		IQueryableRoleStore<TRole, TKey>
		where TKey : IEquatable<TKey>
		where TRole : XPIdentityRole<TKey, TXPORole>, IRole<TKey>
		where TXPORole : XPBaseObject, IDxRole<TKey>, IRole<TKey>
	{
		public XPRoleStore() :
			base()
		{
            
		}

		public XPRoleStore(string connectionName) :
			base(connectionName)
		{

		}
		public XPRoleStore(string connectionString, string connectionName) :
			base(connectionString, connectionName)
		{

		}

		public XPRoleStore(XpoDatabase database) :
			base(database)
		{

		}




		#region Generic Helper methods and members

		//protected static Type XPOUserType { get { return typeof(TXPOUser); } }
		protected static Type XPORoleType { get { return typeof(TXPORole); } }

		//protected static TXPOUser XPOCreateUser(Session s) { return Activator.CreateInstance(typeof(TXPOUser), s) as TXPOUser; }
		protected static TXPORole XPOCreateRole(Session s) { return Activator.CreateInstance(typeof(TXPORole), s) as TXPORole; }
		#endregion

		public IQueryable<TRole> Roles
		{
			get
			{
				XPQuery<TXPORole> q = new XPQuery<TXPORole>(GetSession());
				var result = from u in q
							 select Activator.CreateInstance(typeof(TRole), u as TXPORole) as TRole;
				return result;
			}
		}

		public Task CreateAsync(TRole role)
		{
			ThrowIfDisposed();
			if (role == null)
			{
				throw new ArgumentNullException("role");
			}

			return Task.FromResult(XPOExecute<object>((db, wrk) =>
			{
				var xpoRole = XPOCreateRole(wrk);
				xpoRole.Assign(role, 0);

				return null;
			}));
		}

		public Task DeleteAsync(TRole role)
		{
			ThrowIfDisposed();
			if (role == null)
			{
				throw new ArgumentNullException("role");
			}

			return Task.FromResult(XPOExecute<object>((db, wrk) =>
			{
				wrk.Delete(wrk.FindObject(XPORoleType, CriteriaOperator.Parse($"{KeyField} == ?", role.Id)));
				return null;
			}));
		}

		public Task<TRole> FindByIdAsync(TKey roleId)
		{
			ThrowIfDisposed();

			return Task.FromResult(XPOExecute((db, wrk) =>
			{
				var xpoRole = wrk.FindObject(XPORoleType, CriteriaOperator.Parse($"{KeyField} == ?", roleId));
				return xpoRole == null ? null : Activator.CreateInstance(typeof(TRole), xpoRole, 0) as TRole;
			}));

		}

		public Task<TRole> FindByIdAsync(string roleId)
		{
			ThrowIfDisposed();

			return Task.FromResult(XPOExecute((db, wrk) =>
			{
				var xpoRole = wrk.FindObject(XPORoleType, CriteriaOperator.Parse($"{KeyField} == ?", roleId));
				return xpoRole == null ? null : Activator.CreateInstance(typeof(TRole), xpoRole, 0) as TRole;
			}));
		}

		public Task<TRole> FindByNameAsync(string roleName)
		{
			ThrowIfDisposed();
			if (String.IsNullOrEmpty(roleName))
				throw new ArgumentException("roleName is null or empty");

			return Task.FromResult(XPOExecute((db, wrk) =>
			{
				var xpoRole = wrk.FindObject(XPORoleType, CriteriaOperator.Parse("NameUpper == ?", roleName.ToUpperInvariant()));
				return xpoRole == null ? null : Activator.CreateInstance(typeof(TRole), xpoRole, 0) as TRole;
			}));

		}

		public Task UpdateAsync(TRole role)
		{
			ThrowIfDisposed();
			if (role == null)
				throw new ArgumentNullException("roleName");

			return Task.FromResult(XPOExecute<object>((db, wrk) =>
			{
				TXPORole r = wrk.FindObject(XPORoleType, CriteriaOperator.Parse($"{KeyField} == ?", role.Id)) as TXPORole;
				if (r != null)
				{
					r.Assign(role, 0);
				}
				return null;
			}));
		}
	}
#endif
}
