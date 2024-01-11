using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;



namespace DX.Data.EF.AutoMapper
{
    public abstract class EFAutoMapperDataStore<TEFContext, TKey, TModel, TDBModel> : EFDataStore<TEFContext, TKey, TModel, TDBModel>
        where TEFContext : DbContext, new()
        where TKey : IEquatable<TKey>
        where TModel : class, new()
        where TDBModel : class, new()

    {
        public EFAutoMapperDataStore(TEFContext context, IMapper mapper, IValidator<TDBModel> validator) : base(context, validator)
        {
            Mapper = mapper;
        }

        public IMapper Mapper { get; }
        protected override TDBModel ToDBModel(TModel source, TDBModel target) => Mapper.Map(source, target);
		protected override TModel ToModel(TDBModel source, TModel target) => Mapper.Map(source, target);
		public override IQueryable<T> Query<T>() => EFQuery().ProjectTo<T>(Mapper.ConfigurationProvider);
        public override TModel GetByKey(TKey key) => Mapper.Map(EFGetByKey(key), CreateModel());
	}
}
