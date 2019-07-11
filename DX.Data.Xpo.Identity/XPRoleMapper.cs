using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using DX.Utils.Data;
#if (NETSTANDARD2_0)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{

	public class XPRoleMapper<TKey, TRole, TXPORole> : XPDataMapper<TKey, TRole, TXPORole>
		where TKey : IEquatable<TKey>
		where TRole : class, IDxRole<TKey>, new()
		where TXPORole : XPBaseObject, IDxRole<TKey>
	{
		public override Func<TXPORole, TRole> CreateModel
			=> (source) => new TRole
			{
				ID = source.ID,
#if (NETSTANDARD2_0)
				NormalizedName = source.NormalizedName,
#endif
				Name = source.Name
			};

		public override TXPORole Assign(TRole source, TXPORole destination)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");

			destination.ID = source.ID;
			destination.Name = source.Name;
#if (NETSTANDARD2_0)
			destination.NormalizedName = source.NormalizedName;
#endif
			return destination;
		}
		static Dictionary<string, string> _propertyMap = new Dictionary<string, string>()
		{
			{"Id", "Id"},			
#if (NETSTANDARD2_0)
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
