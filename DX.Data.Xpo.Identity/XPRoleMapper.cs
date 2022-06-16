using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DX.Data.Xpo.Identity
{
#if (NETSTANDARD2_1 || NETCOREAPP)
	public class XPRoleMapper<TKey, TRole, TXPORole/*, TXPORoleClaim*/> : XPDataMapper<TKey, TRole, TXPORole>
		where TKey : IEquatable<TKey>
		where TRole : class, IXPRole<TKey>, new()
		where TXPORole : class, IXPSimpleObject, IXPRole<TKey>
		//where TXPORoleClaim : class, IXPSimpleObject, IXPRoleClaim<TKey>
#else
	public class XPRoleMapper<TKey, TRole, TXPORole> : XPDataMapper<TKey, TRole, TXPORole>
		where TKey : IEquatable<TKey>
		where TRole : class, IXPRole<TKey>, new()
		where TXPORole : class, IXPSimpleObject, IXPRole<TKey>
#endif
	{
		public override Func<TXPORole, TRole> CreateModel
			=> (source) => new TRole
			{
				Id = source.Id,
#if (NETSTANDARD2_1 || NETCOREAPP)
				NormalizedName = source.NormalizedName,
#endif
				Name = source.Name
			};

		public override TXPORole Assign(TRole source, TXPORole destination)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			//destination.Id = source.Id;
			destination.Name = source.Name;
#if (NETSTANDARD2_1 || NETCOREAPP)
			destination.NormalizedName = source.NormalizedName;
#endif
			return destination;
		}
		static Dictionary<string, string> _propertyMap = new Dictionary<string, string>
		{
			{"Id", "Id"},			
#if (NETSTANDARD2_1 || NETCOREAPP)
			{"NormalizedName", "NormalizedName"},
#endif
			{"Name", "Name"}
		};

		public override string Map(string sourceField)
		{
			return _propertyMap[sourceField];
		}
	}
}
