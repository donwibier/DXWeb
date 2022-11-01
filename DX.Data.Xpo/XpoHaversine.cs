using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Helpers;
using DX.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DX.Data.Xpo
{

    public class GeoHaversineOperators
    {
        #region Haversine methods in XPO CriteriaOperators only :-)

        public static CriteriaOperator GeoInRangeKM(CriteriaOperator latOrg, CriteriaOperator lonOrg,
            CriteriaOperator latDest, CriteriaOperator lonDest, double rangeKM)
        {
            return new BetweenOperator(GeoDistanceKM(latOrg, lonOrg, latDest, lonDest), 0, rangeKM);
        }

        public static CriteriaOperator GeoDistanceKM(CriteriaOperator latOrg, CriteriaOperator lonOrg,
            CriteriaOperator latDest, CriteriaOperator lonDest)
        {
            return HaversineOperator(latOrg, lonOrg, latDest, lonDest) * GeoHaversine.RadiusScale[GeoScale.gsKilometer];
        }

        public static CriteriaOperator GeoInRangeMiles(CriteriaOperator latOrg, CriteriaOperator lonOrg,
            CriteriaOperator latDest, CriteriaOperator lonDest, double rangeMiles)
        {
            return new BetweenOperator(GeoDistanceMiles(latOrg, lonOrg, latDest, lonDest), 0, rangeMiles);
        }

        public static CriteriaOperator GeoDistanceMiles(CriteriaOperator latOrg, CriteriaOperator lonOrg,
            CriteriaOperator latDest, CriteriaOperator lonDest)
        {
            return HaversineOperator(latOrg, lonOrg, latDest, lonDest) * GeoHaversine.RadiusScale[GeoScale.gsMile];
        }

        private static CriteriaOperator toRadian(CriteriaOperator degrees)
        {
            return new BinaryOperator(new BinaryOperator(degrees, Math.PI, BinaryOperatorType.Multiply), 180, BinaryOperatorType.Divide);
        }

        public static CriteriaOperator HaversineOperator(CriteriaOperator latOrigin, CriteriaOperator lonOrigin, CriteriaOperator latEnd, CriteriaOperator lonEnd)
        {
            CriteriaOperator latRadOrigin = toRadian(latOrigin);
            CriteriaOperator lonRadOrigin = toRadian(lonOrigin);
            CriteriaOperator latRadEnd = toRadian(latEnd);
            CriteriaOperator lonRadEnd = toRadian(lonEnd);

            //Haversine Formula
            CriteriaOperator dlat = new BinaryOperator(latRadOrigin, latRadEnd, BinaryOperatorType.Minus);
            CriteriaOperator dlon = new BinaryOperator(lonRadOrigin, lonRadEnd, BinaryOperatorType.Minus);

            CriteriaOperator result =
                new FunctionOperator(FunctionOperatorType.Power, new FunctionOperator(FunctionOperatorType.Sin, dlat / 2), 2) +
                new FunctionOperator(FunctionOperatorType.Cos, latRadEnd) *
                new FunctionOperator(FunctionOperatorType.Cos, latRadOrigin) *
                new FunctionOperator(FunctionOperatorType.Power, new FunctionOperator(FunctionOperatorType.Sin, dlon / 2), 2);

            result = new FunctionOperator(FunctionOperatorType.Min, 1, new FunctionOperator(FunctionOperatorType.Sqr, result));
            result = 2 * new FunctionOperator(FunctionOperatorType.Asin, result);

            return result;
        }
        #endregion

    }
    public class GeoHaversineFunction : ICustomFunctionOperatorBrowsable, ICustomFunctionOperatorFormattable,
        ICustomFunctionOperatorQueryable, ICustomCriteriaOperatorQueryable
    {
        const string FunctionName = "GeoHaversine";
        static readonly GeoHaversineFunction instance = new GeoHaversineFunction();

        public static void Register()
        {
            CriteriaOperator.RegisterCustomFunction(instance);
            CustomCriteriaManager.RegisterCriterion(instance);
        }

        public static bool Unregister()
        {
            bool unreg = CustomCriteriaManager.UnregisterCriterion(instance);
            return CriteriaOperator.UnregisterCustomFunction(instance) && unreg;
        }

        #region ICustomFunctionOperatorBrowsable Members
        public FunctionCategory Category => FunctionCategory.Math;
        public string Description => "Used to calculate the distance between 2 geo points";

        public bool IsValidOperandCount(int count) => count == 4; // Need 4 parameters

        public bool IsValidOperandType(int operandIndex, int operandCount, Type type)
        {
            return type == typeof(string) || type == typeof(double);
        }

        public int MaxOperandCount => 4; // 4 operants must be there. 
        public int MinOperandCount => 4; // 4 operants must be there. 
        #endregion

        private static CultureInfo ci = new CultureInfo("en-US");

        private static double getDouble(object val)
        {

            if (val == null) return 0;
            if (val.GetType() == typeof(double)) return (double)val;

            if (val.GetType() == typeof(string))
            {
                if (double.TryParse(string.Format("{0}", val), NumberStyles.Any, ci, out double res))
                    return res;
            }
            try { return Convert.ToDouble(val); } catch { return 0; }
        }

        #region ICustomFunctionOperator Members

        // Evaluates the function on the client. 
        public object Evaluate(params object[] operands)
        {
            double latOrg = getDouble(operands[0]);
            double lonOrg = getDouble(operands[1]);
            double latDest = getDouble(operands[2]);
            double lonDest = getDouble(operands[3]);

            return Utils.GeoHaversine.DistanceKM(latOrg, lonOrg, latDest, lonDest);
        }

        public string Name => FunctionName;

        public Type ResultType(params Type[] operands) => typeof(double);

        #endregion

        #region ICustomFunctionOperatorFormattable Members

        // The function's expression to be evaluated on the server. 

        public string Format(Type providerType, params string[] operands)
        {
            var result = GeoHaversineOperators.GeoDistanceKM(
                CriteriaOperator.Parse(operands[0]),
                CriteriaOperator.Parse(operands[1]),
                CriteriaOperator.Parse(operands[2]),
                CriteriaOperator.Parse(operands[3])).LegacyToString();
            return result;
            //return String.Format("[dbo].[{0}]({1})", FunctionName, String.Join(",", operands));
        }
        #endregion

        #region GeoHaversine SQL
        /*
			SET ANSI_NULLS ON
			GO

			SET QUOTED_IDENTIFIER ON
			GO


							-- =====================================================
							-- Author:		Don Wibier
							-- Create date: 10 FEB 2010
							-- Description:	Calculates the distance between 
							--              2 Geo points using Haversine algorithm
							-- =====================================================
							CREATE FUNCTION [dbo].[GeoHaversine]
							(
								@latitude1 FLOAT, @longitude1 FLOAT,
								@latitude2 FLOAT, @longitude2 FLOAT
							)
							RETURNS FLOAT
							AS
							BEGIN
								DECLARE
									@radiusKM FLOAT,
									@lat1 FLOAT, @lon1 FLOAT,
									@lat2 FLOAT, @lon2 FLOAT,
									@dlat FLOAT, @dlon FLOAT,
									@a FLOAT, @result FLOAT, @min FLOAT;
								-- Convert arguments in degrees to radians
								SELECT
									@radiusKM = 6367,
									@lat1 = (@latitude1 * PI() ) / 180, 
									@lon1 = (@longitude1 * PI() ) / 180,
									@lat2 = (@latitude2 * PI() ) / 180, 
									@lon2 = (@longitude2 * PI() ) / 180;
								-- Calculate differences
								SELECT
									@dlat = @lat2 - @lat1, 
									@dlon = @lon2 - @lon1;
								-- Haversine Formula 
								SET @a = POWER(SIN(@dlat/2), 2) + COS(@lat1) * COS(@lat2) * POWER(SIN(@dlon/2), 2);
								SET @min = SQRT(@a);
								IF @min > 1
									SET @min = 1;
								SET @result = 2 * ASIN(@min);
								-- End of Haversine Formula 
								RETURN (@result * @radiusKM);
							END
			GO
		 */
        #endregion

        // The method name must match the function name (specified via FunctionName). 
        public static double GeoHaversine(string latitudeOrigin, string longituteOrigin, string latituteDestination, string longitudeDestination, string unit)
        {

            return (double)instance.Evaluate(latitudeOrigin, longituteOrigin, latituteDestination, longitudeDestination, unit);
        }
        #region ICustomFunctionOperatorQueryable Members

        public System.Reflection.MethodInfo GetMethodInfo()
        {
            return typeof(GeoHaversineFunction).GetMethod(FunctionName);
        }

        public CriteriaOperator GetCriteria(params CriteriaOperator[] operands)
        {
            if (operands == null || operands.Length != 1 || !(operands[0] is MemberInitOperator))
                throw new ArgumentException(FunctionName);

            var members = ((MemberInitOperator)operands[0]).Members;
            if (members.Count != 4)
                throw new ArgumentException("Invalid number of parameters");

            return GeoHaversineOperators.GeoDistanceKM(members[0].Property, members[1].Property, 
                members[2].Property, members[3].Property);
            //CriteriaOperatorCollection operandCollection = new CriteriaOperatorCollection();
            
            //// Iterate operands[0].Members for function parameters 
            //// and populate a criteria operator collection.
            //foreach (XPMemberAssignment operand in ((MemberInitOperator)operands[0]).Members)
            //{
            //    operandCollection.Add(operand.Property);
            //}
            //// MyConcat is returned as a CriteriaOperator object (here, FunctionOperator).
            //return new FunctionOperator(FunctionName, operandCollection);
        }
        #endregion
    }
}









