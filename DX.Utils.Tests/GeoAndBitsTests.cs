using System;
using Xunit;

namespace DX.Utils.Tests
{
    public class GeoHaversineTests
    {
        // Amsterdam Centraal -> Paris Gare du Nord, roughly 430 km great-circle.
        const double AmsLat = 52.3791, AmsLon = 4.9003, ParLat = 48.8809, ParLon = 2.3553;

        [Fact]
        public void ToRadian()
        {
            Assert.Equal(Math.PI, 180.0.toRadian(), 12);
            Assert.Equal(-Math.PI / 2, (-90.0).toRadian(), 12);
            Assert.Equal(0, 0.0.toRadian());
        }

        [Fact]
        public void SamePoint_IsZero()
        {
            Assert.Equal(0, GeoHaversine.DistanceKM(AmsLat, AmsLon, AmsLat, AmsLon), 9);
        }

        [Fact]
        public void AmsterdamParis_IsAbout430Km()
        {
            var km = GeoHaversine.DistanceKM(AmsLat, AmsLon, ParLat, ParLon);
            Assert.InRange(km, 425, 435);
        }

        [Fact]
        public void IsSymmetric()
        {
            Assert.Equal(GeoHaversine.DistanceKM(AmsLat, AmsLon, ParLat, ParLon),
                         GeoHaversine.DistanceKM(ParLat, ParLon, AmsLat, AmsLon), 9);
        }

        [Fact]
        public void Scales_AreConsistent()
        {
            var km = GeoHaversine.DistanceKM(AmsLat, AmsLon, ParLat, ParLon);
            Assert.Equal(km * 1000, GeoHaversine.DistanceMtr(AmsLat, AmsLon, ParLat, ParLon), 6);
            Assert.Equal(km / 6367 * 3956, GeoHaversine.DistanceMile(AmsLat, AmsLon, ParLat, ParLon), 6);
            Assert.Equal(km, GeoHaversine.Distance(AmsLat, AmsLon, ParLat, ParLon, GeoScale.gsKilometer));
        }

        [Fact]
        public void Antipodes_IsHalfCircumference()
        {
            Assert.Equal(Math.PI * 6367, GeoHaversine.DistanceKM(0, 0, 0, 180), 6);
            Assert.Equal(Math.PI * 6367, GeoHaversine.DistanceKM(90, 0, -90, 0), 6);
        }

        [Fact]
        public void CrossingDateLine_IsShortWay()
        {
            Assert.InRange(GeoHaversine.DistanceKM(0, 179.5, 0, -179.5), 110, 112);
        }

        [Fact]
        public void Haversine_ReturnsCentralAngleInRadians()
        {
            Assert.Equal(Math.PI / 2, GeoHaversine.Haversine(0, 0, 0, 90), 12);
        }

        [Fact]
        public void NaN_PropagatesAsNaN()
        {
            Assert.True(double.IsNaN(GeoHaversine.DistanceKM(double.NaN, 0, 0, 0)));
        }

        [Fact]
        public void UnknownScale_Throws()
        {
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => GeoHaversine.Distance(0, 0, 1, 1, (GeoScale)42));
        }
    }

    public class BitsTests
    {
        [Fact]
        public void Set()
        {
            Assert.Equal(0b0101, Bits.Set(0b0001, 0b0100));
            Assert.Equal(0b0101, 0b0001.BitSet(0b0100));
            Assert.Equal(0b0101, Bits.Set(0b0101, 0b0100));
        }

        [Fact]
        public void Toggle()
        {
            Assert.Equal(0b0001, Bits.Toggle(0b0101, 0b0100));
            Assert.Equal(0b0101, 0b0001.BitToggle(0b0100));
        }

        [Fact]
        public void Has()
        {
            Assert.True(Bits.Has(0b0111, 0b0101));
            Assert.False(Bits.Has(0b0011, 0b0101));
            Assert.True(0b0111.BitHas(0b0010));
            Assert.True(Bits.Has(-1, int.MinValue));
        }

        [Fact]
        public void Clear_OnlyClearsTheFlag()
        {
            Assert.Equal(0b0011, Bits.Clear(0b0111, 0b0100));
            Assert.Equal(0b0011, 0b0111.BitClear(0b0100));
        }

        [Fact]
        public void HasRef_WithClear_ClearsOnlyThatFlag_AndWithoutClear_LeavesValue()
        {
            int v = 0b0111;
            Assert.True(Bits.Has(ref v, 0b0100, true));
            Assert.Equal(0b0011, v);

            int w = 0b0111;
            Assert.True(Bits.Has(ref w, 0b0100, false));
            Assert.Equal(0b0111, w);
        }

        [Fact]
        public void HasRef_FlagNotSet_LeavesValue()
        {
            int v = 0b0011;
            Assert.False(Bits.Has(ref v, 0b0100, true));
            Assert.Equal(0b0011, v);
        }
    }
}
