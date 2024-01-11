using DevExpress.Data.Filtering;
using DevExpress.Xpo;
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
	[Obsolete("For legacy reasons only. Use DX.Data.AutoMapper or DX.Data.Mapster descendants")]
	public class XPRoleMapper<TKey, TRole, TXPORole/*, TXPORoleClaim*/> : XPDataMapper<TKey, TRole, TXPORole>
		where TKey : IEquatable<TKey>
		where TRole : class, IXPRole<TKey>, new()
		where TXPORole : class, IXPSimpleObject, IXPRole<TKey>
		//where TXPORoleClaim : class, IXPSimpleObject, IXPRoleClaim<TKey>
	{
		public override Func<TXPORole, TRole> CreateModel
			=> (source) => new TRole
			{
				Id = source.Id,
				NormalizedName = source.NormalizedName,
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
			destination.NormalizedName = source.NormalizedName;
			return destination;
		}
		static Dictionary<string, string> _propertyMap = new Dictionary<string, string>
		{
			{"Id", "Id"},			
			{"NormalizedName", "NormalizedName"},
			{"Name", "Name"}
		};

		public override string Map(string sourceField)
		{
			return _propertyMap[sourceField];
		}
	}
}
