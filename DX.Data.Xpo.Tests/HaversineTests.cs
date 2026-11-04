using System;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.Xpo;
using DX.Utils;
using Xunit;

namespace DX.Data.Xpo.Tests
{
    public class HaversineTests
    {
        private const double R = 6367; // the radius GeoHaversine uses

        // Independent reference: great-circle distance via the atan2 form (numerically different from asin form)
        private static double Reference(double lat1, double lon1, double lat2, double lon2, double radius = R)
        {
            double rad(double d) => d * Math.PI / 180;
            double p1 = rad(lat1), p2 = rad(lat2), dl = rad(lon2 - lon1);
            double y = Math.Sqrt(Math.Pow(Math.Cos(p2) * Math.Sin(dl), 2) +
                                 Math.Pow(Math.Cos(p1) * Math.Sin(p2) - Math.Sin(p1) * Math.Cos(p2) * Math.Cos(dl), 2));
            double x = Math.Sin(p1) * Math.Sin(p2) + Math.Cos(p1) * Math.Cos(p2) * Math.Cos(dl);
            return radius * Math.Atan2(y, x);
        }

        public static TheoryData<double, double, double, double> Points => new TheoryData<double, double, double, double>
        {
            { 52.3676, 4.9041, 48.8566, 2.3522 },     // Amsterdam - Paris
            { 51.5074, -0.1278, 40.7128, -74.0060 },  // London - New York
            { -33.8688, 151.2093, 35.6762, 139.6503 },// Sydney - Tokyo
            { 0, 0, 0, 1 },
            { 10, 10, 10, 10 },
            { 89.9, 0, -89.9, 180 },
        };

        [Theory]
        [MemberData(nameof(Points))]
        public void DistanceKM_MatchesReference(double lat1, double lon1, double lat2, double lon2)
        {
            Assert.Equal(Reference(lat1, lon1, lat2, lon2), GeoHaversine.DistanceKM(lat1, lon1, lat2, lon2), 6);
        }

        [Fact]
        public void KnownDistances()
        {
            // published great-circle distances (mean radius 6371 km), within 0.2 % because the code uses 6367 km
            Assert.InRange(GeoHaversine.DistanceKM(52.3676, 4.9041, 48.8566, 2.3522), 430.0 * 0.998, 430.0 * 1.002);
            Assert.InRange(GeoHaversine.DistanceKM(51.5074, -0.1278, 40.7128, -74.0060), 5570.0 * 0.998, 5570.0 * 1.002);
            Assert.Equal(R * Math.PI / 180, GeoHaversine.DistanceKM(0, 0, 0, 1), 9);
            Assert.Equal(R * Math.PI, GeoHaversine.DistanceKM(0, 0, 0, 180), 6);
            Assert.Equal(0, GeoHaversine.DistanceKM(12.3, 45.6, 12.3, 45.6), 12);
        }

        [Fact]
        public void Units_AreConsistent()
        {
            var km = GeoHaversine.DistanceKM(52.3676, 4.9041, 48.8566, 2.3522);
            Assert.Equal(km * 1000, GeoHaversine.DistanceMtr(52.3676, 4.9041, 48.8566, 2.3522), 6);
            Assert.Equal(km / R * 3956, GeoHaversine.DistanceMile(52.3676, 4.9041, 48.8566, 2.3522), 6);
        }

        [Fact]
        public void IsSymmetric()
        {
            Assert.Equal(GeoHaversine.DistanceKM(51.5, -0.12, 40.7, -74), GeoHaversine.DistanceKM(40.7, -74, 51.5, -0.12), 9);
        }

        [Fact]
        public void Antipodal_DoesNotProduceNaN()
        {
            var d = GeoHaversine.DistanceKM(45, 30, -45, -150);
            Assert.False(double.IsNaN(d));
            Assert.Equal(R * Math.PI, d, 6);
        }

        // ---------- XPO criteria ----------

        public static TheoryData<double, double, double, double> ShortPoints => new TheoryData<double, double, double, double>
        {
            { 52.3676, 4.9041, 48.8566, 2.3522 },     // Amsterdam - Paris
            { 51.5074, -0.1278, 40.7128, -74.0060 },  // London - New York (central angle < 60 degrees)
            { 0, 0, 0, 1 },
            { 10, 10, 10, 10 },
        };

        [Theory]
        [MemberData(nameof(ShortPoints))]
        public void CriteriaDistanceKM_MatchesManagedImplementation(double lat1, double lon1, double lat2, double lon2)
        {
            var op = GeoHaversineOperators.GeoDistanceKM(lat1, lon1, new OperandProperty("Lat"), new OperandProperty("Lon"));
            var db = new InMemoryDb();
            using var uow = db.NewUnitOfWork();
            var p = new Product(uow) { Name = "x", Lat = lat2, Lon = lon2 };
            Assert.Equal(GeoHaversine.DistanceKM(lat1, lon1, lat2, lon2), EvaluateOnObject(uow, op, p), 6);
        }

        private static double EvaluateOnObject(Session s, CriteriaOperator op, Product p)
            => Convert.ToDouble(new ExpressionEvaluator(s.GetClassInfo<Product>().GetEvaluatorContextDescriptor(), op).Evaluate(p));

        [Fact]
        public void CriteriaDistanceKM_LongDistances()
        {
            var db = new InMemoryDb();
            using var uow = db.NewUnitOfWork();
            var tokyo = new Product(uow) { Lat = 35.6762, Lon = 139.6503 };
            var op = GeoHaversineOperators.GeoDistanceKM(-33.8688, 151.2093, new OperandProperty("Lat"), new OperandProperty("Lon"));
            // Sydney - Tokyo: ~7820 km
            Assert.Equal(GeoHaversine.DistanceKM(-33.8688, 151.2093, 35.6762, 139.6503), EvaluateOnObject(uow, op, tokyo), 6);
        }

        [Fact]
        public void GeoInRangeKM_FindsFarAwayPointsInRange()
        {
            var db = new InMemoryDb();
            db.AddProduct("Tokyo", lat: 35.6762, lon: 139.6503);     // ~9300 km from Amsterdam
            using var uow = db.NewUnitOfWork();
            var near = new XPCollection<Product>(uow,
                GeoHaversineOperators.GeoInRangeKM(52.3676, 4.9041, new OperandProperty("Lat"), new OperandProperty("Lon"), 10000));
            Assert.Equal("Tokyo", Assert.Single(near).Name);
        }

        [Fact]
        public void CriteriaDistanceMiles_MatchesManagedImplementation()
        {
            var db = new InMemoryDb();
            using var uow = db.NewUnitOfWork();
            var p = new Product(uow) { Lat = 40.7128, Lon = -74.0060 };
            var op = GeoHaversineOperators.GeoDistanceMiles(51.5074, -0.1278, new OperandProperty("Lat"), new OperandProperty("Lon"));
            Assert.Equal(GeoHaversine.DistanceMile(51.5074, -0.1278, 40.7128, -74.0060), EvaluateOnObject(uow, op, p), 6);
        }

        [Fact]
        public void GeoInRangeKM_FiltersInDatabaseQuery()
        {
            var db = new InMemoryDb();
            db.AddProduct("Paris", lat: 48.8566, lon: 2.3522);       // ~430 km from Amsterdam
            db.AddProduct("London", lat: 51.5074, lon: -0.1278);     // ~357 km
            db.AddProduct("New York", lat: 40.7128, lon: -74.0060);  // ~5860 km
            db.AddProduct("Amsterdam", lat: 52.3676, lon: 4.9041);   // 0

            using var uow = db.NewUnitOfWork();
            var near = new XPCollection<Product>(uow,
                GeoHaversineOperators.GeoInRangeKM(52.3676, 4.9041, new OperandProperty("Lat"), new OperandProperty("Lon"), 400));
            Assert.Equal(new[] { "Amsterdam", "London" }, near.Select(p => p.Name).OrderBy(n => n));

            var withinMiles = new XPCollection<Product>(uow,
                GeoHaversineOperators.GeoInRangeMiles(52.3676, 4.9041, new OperandProperty("Lat"), new OperandProperty("Lon"), 300));
            Assert.Equal(new[] { "Amsterdam", "London", "Paris" }, withinMiles.Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void CustomFunction_EvaluatesOnClient()
        {
            var f = new GeoHaversineFunction();
            Assert.Equal(GeoHaversine.DistanceKM(52.3676, 4.9041, 48.8566, 2.3522), (double)f.Evaluate(52.3676, 4.9041, 48.8566, 2.3522), 9);
            Assert.Equal(GeoHaversine.DistanceKM(52.3676, 4.9041, 48.8566, 2.3522), (double)f.Evaluate("52.3676", "4.9041", "48.8566", "2.3522"), 9);
            Assert.Equal(GeoHaversine.DistanceKM(52.3676, 4.9041, 48.8566, 2.3522),
                GeoHaversineFunction.GeoHaversine("52.3676", "4.9041", "48.8566", "2.3522", "km"), 9);
            Assert.Equal(GeoHaversine.DistanceKM(0, 0, 0, 0), (double)f.Evaluate(null, null, null, null));
            Assert.True(f.IsValidOperandCount(4));
            Assert.False(f.IsValidOperandCount(5));
            Assert.Equal(typeof(double), f.ResultType());
        }

        [Fact]
        public void CustomFunction_InParsedCriteria_FiltersInMemoryQuery()
        {
            GeoHaversineFunction.Register();
            var db = new InMemoryDb();
            db.AddProduct("London", lat: 51.5074, lon: -0.1278);
            db.AddProduct("New York", lat: 40.7128, lon: -74.0060);
            using var uow = db.NewUnitOfWork();
            var near = new XPCollection<Product>(uow, CriteriaOperator.Parse("GeoHaversine(52.3676, 4.9041, Lat, Lon) < 1000"));
            Assert.Equal("London", Assert.Single(near).Name);
        }
    }
}
