using System;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using DX.Data.Xpo.Identity.Persistent;

namespace DX.Data.Xpo.Identity.Tests
{
    /// <summary>
    /// A fresh in-memory XPO database per test, with XpoDefault.Session null as XpoDatabase leaves it,
    /// so code that falls back on the default session fails here as it does in an application
    /// </summary>
    public sealed class InMemoryDb
    {
        public InMemoryDb()
        {
            XpoDefault.Session = null;
            var dictionary = new ReflectionDictionary();
            dictionary.GetDataStoreSchema(typeof(XpoDxUser).Assembly);
            DataLayer = new SimpleDataLayer(dictionary, new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema));
        }

        public IDataLayer DataLayer { get; }

        public UnitOfWork NewUnitOfWork() => new UnitOfWork(DataLayer);

        /// <summary>
        /// Runs <paramref name="work"/> in a new unit of work and commits it
        /// </summary>
        public T Commit<T>(Func<UnitOfWork, T> work)
        {
            using var uow = NewUnitOfWork();
            var result = work(uow);
            uow.CommitChanges();
            return result;
        }

        public int Count<T>() where T : class
        {
            using var uow = NewUnitOfWork();
            return new XPCollection<T>(uow).Count;
        }

        public static XpoDxUser NewUser(Session session, string name)
            => new XpoDxUser(session) { UserName = name, Email = name, NormalizedUserName = name.ToUpperInvariant(), NormalizedEmail = name.ToUpperInvariant() };

        public static XpoDxRole NewRole(Session session, string name)
            => new XpoDxRole(session) { Name = name, NormalizedName = name.ToUpperInvariant() };
    }
}
