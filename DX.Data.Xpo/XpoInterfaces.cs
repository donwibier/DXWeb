using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Data.Xpo
{
	public interface IXPOKey<TKey>
			 where TKey : IEquatable<TKey>
	{
		TKey Key { get; }
	}

	public interface IAssignable
	{
		void Assign(object source, int loadingFlags);
	}
}
