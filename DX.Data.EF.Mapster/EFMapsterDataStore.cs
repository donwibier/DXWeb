using FluentValidation;
using Mapster;
using MapsterMapper;
using System;
using System.Linq;
#if (NETCOREAPP)
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace DX.Data.EF.Mapster
{
	public abstract class EFMapsterDataStore<TEFContext, TKey, TModel, TDBModel> : EFDataStore<TEFContext, TKey, TModel, TDBModel>
		where TEFContext : DbContext, new()
		where TKey : IEquatable<TKey>
		where TModel : class, new()
		where TDBModel : class, new()

	{
		public EFMapsterDataStore(TEFContext context, IMapper mapper, IValidator<TDBModel> validator) : base(context, validator)
		{
			Mapper = mapper;
		}

		public IMapper Mapper { get; }
		protected override TDBModel ToDBModel(TModel source, TDBModel target) => Mapper.Map(source, target);
		protected override TModel ToModel(TDBModel source, TModel target) => Mapper.Map(source, target);
		public override IQueryable<T> Query<T>() => EFQuery().ProjectToType<T>();
		public override TModel GetByKey(TKey key) => Mapper.Map(EFGetByKey(key), CreateModel());
	}
}
