using AutoMapper;
using AutoMapper.QueryableExtensions;
using DevExpress.Xpo;
using FluentValidation;

namespace DX.Data.Xpo.AutoMapper
{
    public abstract class XPAutoMapperStore<TKey, TModel, TDBModel> : XPDataStore<TKey, TModel, TDBModel>
        where TKey : IEquatable<TKey>
        where TModel : class, new()
        where TDBModel : class, IXPSimpleObject    
    {
        public XPAutoMapperStore(IDataLayer dataLayer, IMapper mapper, IValidator<TDBModel> validator) : base(dataLayer, validator)
        {
            Mapper = mapper;
        }

        public IMapper Mapper { get; }
        protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);
        public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider);

        public override TModel GetByKey(TKey key)
        {
            using (var wrk = new Session(DataLayer))
            {
                TModel result = CreateModel();
                return Mapper.Map(XPOGetByKey(key, wrk), result);
            }
        }
    }
}
