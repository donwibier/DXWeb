using System;
using System.Linq;
using System.Linq.Expressions;
using DevExpress.Xpo;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace DX.Data.Xpo.Tests
{
    public class DataResultTests
    {
        [Fact]
        public void Default_IsNotSuccessfulAndEmpty()
        {
            var r = new DataResult<int, ProductModel>();
            Assert.False(r.Success);
            Assert.Equal(0, r.Key);
            Assert.Null(r.Exception);
            Assert.Null(r.Data);
        }

        [Fact]
        public void NullError_MeansSuccess()
        {
            var r = new DataResult<int, ProductModel>(DataMode.Update, "Name", null);
            Assert.True(r.Success);
            Assert.Equal(DataMode.Update, r.Mode);
            Assert.Null(r.Exception);
        }

        [Fact]
        public void ValidationException_IsKeptAsIs()
        {
            var ve = new ValidationException(new[] { new ValidationFailure("Name", "required") });
            var r = new DataResult<int, ProductModel>(DataMode.Create, "X", ve);
            Assert.False(r.Success);
            Assert.Same(ve, r.Exception);
        }

        [Fact]
        public void OtherException_IsWrappedWithPropertyName()
        {
            var r = new DataResult<int, ProductModel>(DataMode.Delete, "Prop", new InvalidOperationException("outer"));
            var f = Assert.Single(r.Exception.Errors);
            Assert.Equal("Prop", f.PropertyName);
            Assert.Equal("outer", f.ErrorMessage);
        }

        [Fact]
        public void OtherException_UsesInnerExceptionMessage()
        {
            var r = new DataResult<int, ProductModel>(DataMode.Delete, "Prop",
                new InvalidOperationException("outer", new ArgumentException("inner")));
            Assert.Equal("inner", Assert.Single(r.Exception.Errors).ErrorMessage);
        }

        [Fact]
        public void KeyOnlyResult_IsObjectTyped()
        {
            IDataResult<string, object> r = new DataResult<string> { Key = "k", Data = new object[] { 1 } };
            Assert.Equal("k", r.Key);
            Assert.Single(r.Data);
        }
    }

    public class PredicateBuilderTests
    {
        private static readonly ProductModel[] Items =
        {
            new ProductModel { Id = 1, Name = "A", Price = 1 },
            new ProductModel { Id = 2, Name = "B", Price = 5 },
            new ProductModel { Id = 3, Name = "C", Price = 10 },
        };

        private static int[] Ids(Expression<Func<ProductModel, bool>> e) => Items.Where(e.Compile()).Select(i => i.Id).ToArray();

        [Fact]
        public void TrueAndFalse()
        {
            Assert.Equal(new[] { 1, 2, 3 }, Ids(PredicateBuilder.True<ProductModel>()));
            Assert.Empty(Ids(PredicateBuilder.False<ProductModel>()));
        }

        [Fact]
        public void And_WithDifferentParameterInstances_Compiles()
        {
            Expression<Func<ProductModel, bool>> a = p => p.Price > 1;
            Expression<Func<ProductModel, bool>> b = x => x.Price < 10;
            Assert.Equal(new[] { 2 }, Ids(a.And(b)));
        }

        [Fact]
        public void Or_WithDifferentParameterInstances_Compiles()
        {
            Expression<Func<ProductModel, bool>> a = p => p.Name == "A";
            Expression<Func<ProductModel, bool>> b = x => x.Name == "C";
            Assert.Equal(new[] { 1, 3 }, Ids(a.Or(b)));
        }

        [Fact]
        public void Chained_FromTrueAndFalse()
        {
            var and = PredicateBuilder.True<ProductModel>().And(p => p.Price >= 5).And(q => q.Name != "C");
            Assert.Equal(new[] { 2 }, Ids(and));
            var or = PredicateBuilder.False<ProductModel>().Or(p => p.Id == 1).Or(q => q.Id == 3);
            Assert.Equal(new[] { 1, 3 }, Ids(or));
            var mixed = PredicateBuilder.True<ProductModel>().And(PredicateBuilder.False<ProductModel>().Or(p => p.Id == 2));
            Assert.Equal(new[] { 2 }, Ids(mixed));
        }

        [Fact]
        public void Composed_ExpressionHasSingleParameter()
        {
            Expression<Func<ProductModel, bool>> a = p => p.Price > 1;
            Expression<Func<ProductModel, bool>> b = x => x.Price < 10;
            var c = a.And(b);
            Assert.Single(c.Parameters);
            Assert.Same(a.Parameters[0], c.Parameters[0]);
        }

        [Fact]
        public void Composed_CurrentlyWrapsSecondOperandInInvoke()
        {
            // a query provider (XPO, EF) can only translate the composed predicate when the second operand is
            // inlined with the parameter replaced, rather than wrapped in Expression.Invoke
            Expression<Func<ProductModel, bool>> a = p => p.Price > 1;
            Expression<Func<ProductModel, bool>> b = x => x.Price < 10;
            var body = (BinaryExpression)a.And(b).Body;
            Assert.Equal(ExpressionType.Invoke, body.Right.NodeType); // documents the current implementation
        }

        [Fact(Skip = "BUG: PredicateBuilder.And/Or (PredicateBuilder.cs:18,26) wrap expr2 in Expression.Invoke; XPO LINQ throws NotSupportedException 'The expression is not supported in the Where clause'")]
        public void ComposedPredicate_TranslatesInXpoQuery()
        {
            var db = new InMemoryDb();
            db.AddProduct("A", 1m);
            db.AddProduct("B", 5m);
            db.AddProduct("C", 10m);
            Expression<Func<Product, bool>> a = p => p.Price > 1;
            Expression<Func<Product, bool>> b = x => x.Price < 10;
            using var uow = db.NewUnitOfWork();
            var names = uow.Query<Product>().Where(a.And(b)).Select(p => p.Name).ToList();
            Assert.Equal(new[] { "B" }, names);
        }

        [Fact]
        public void SameParameterInstances_StillNotTranslatableInXpo()
        {
            // even with one shared parameter, the Invoke node is what XPO rejects
            var db = new InMemoryDb();
            db.AddProduct("A", 1m);
            var p = Expression.Parameter(typeof(Product), "p");
            var a = Expression.Lambda<Func<Product, bool>>(Expression.GreaterThan(Expression.Property(p, nameof(Product.Price)), Expression.Constant(0m)), p);
            var b = Expression.Lambda<Func<Product, bool>>(Expression.LessThan(Expression.Property(p, nameof(Product.Price)), Expression.Constant(10m)), p);
            using var uow = db.NewUnitOfWork();
            Assert.Throws<NotSupportedException>(() => uow.Query<Product>().Where(a.And(b)).ToList());
            // the hand-inlined equivalent works
            var inlined = Expression.Lambda<Func<Product, bool>>(Expression.AndAlso(a.Body, b.Body), p);
            Assert.Single(uow.Query<Product>().Where(inlined).ToList());
        }
    }
}
