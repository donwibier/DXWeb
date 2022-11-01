using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace DX.Utils
{
    public enum GeoScale
    {
        gsMeter,
        gsKilometer,
        gsMile
    }

    //public struct GeoPoint
    //{
    //    public double Lat { get; set; }
    //    public double Lon { get; set; }

    //    public GeoPoint ToRadian() => new GeoPoint { Lat = this.Lat.toRadian(), Lon = this.Lon.toRadian() };
    //}

    public static class GeoHaversine
    {
        //static double convertToDouble(string coordStr)
        //{            
        //    //string[] parts = coordStr.Split(new char[] { '.' }); ??
        //    NumberFormatInfo nfi = new NumberFormatInfo();
        //    if (double.TryParse(coordStr, NumberStyles.Float, nfi, out double result))
        //        return result;            
        //    return default;
        //}

        public static double toRadian(this double degrees) => (degrees * Math.PI) / 180;

        public static double Haversine(double latOrigin, double lonOrigin, double latEnd, double lonEnd)
        {
            double latRadOrigin = latOrigin.toRadian();
            double lonRadOrigin = lonOrigin.toRadian();
            double latRadEnd = latEnd.toRadian();
            double lonRadEnd = lonEnd.toRadian();

            //Haversine Formula
            double dlat = latRadOrigin - latRadEnd;
            double dlon = lonRadOrigin - lonRadEnd;
            double result = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(latRadEnd) * Math.Cos(latRadOrigin) * Math.Pow(Math.Sin(dlon / 2), 2);
            return 2 * Math.Asin(Math.Min(1, Math.Sqrt(result)));
        }

        public static readonly IDictionary<GeoScale, double> RadiusScale = new Dictionary<GeoScale, double>
        {
            { GeoScale.gsMeter, 6367000 },  // The earth radius mtr
            { GeoScale.gsKilometer, 6367 }, // The earth radius km
            { GeoScale.gsMile, 3956 }       // The earth radius miles
        };

        public static double Distance(double latOrigin, double lonOrigin, double latEnd, double lonEnd, GeoScale scale)
        {
            return RadiusScale[scale] * Haversine(latOrigin, lonOrigin, latEnd, lonEnd);
        }

        public static double DistanceKM(double latOrigin, double lonOrigin, double latEnd, double lonEnd)
        {
            return Distance(latOrigin, lonOrigin, latEnd, lonEnd, GeoScale.gsKilometer);
        }

        public static double DistanceMtr(double latOrigin, double lonOrigin, double latEnd, double lonEnd)
        {
            return Distance(latOrigin, lonOrigin, latEnd, lonEnd, GeoScale.gsMeter);
        }

        public static double DistanceMile(double latOrigin, double lonOrigin, double latEnd, double lonEnd)
        {
            return Distance(latOrigin, lonOrigin, latEnd, lonEnd, GeoScale.gsMile);
        }
    }


}

