using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Data.Xpo
{
	[Obsolete("For legacy reasons only. Use DX.Data.AutoMapper or DX.Data.Mapster descendants")]
	public abstract class XPDataMapper<TKey, TModel, TXPOClass> : DataMapper<TKey, TModel, TXPOClass>,
																	  IXPDataMapper<TKey, TModel, TXPOClass>
			where TKey : IEquatable<TKey>
			where TModel : class, new()
			where TXPOClass : class, IXPSimpleObject
	{

	}
}
