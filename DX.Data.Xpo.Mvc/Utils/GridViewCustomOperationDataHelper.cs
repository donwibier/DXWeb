using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.Data.Linq;
using DevExpress.Data.Linq.Helpers;
using DevExpress.Web.Mvc;
using DX.Data.Xpo.Mvc.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DX.Data.Xpo.Mvc.Utils
{
	public static class GridViewCustomOperationDataHelper
	{
		static ICriteriaToExpressionConverter Converter { get { return new CriteriaToExpressionConverter(); } }

		public static IQueryable Select(this IQueryable query, string fieldName)
		{
			return query.MakeSelect(Converter, new OperandProperty(fieldName));
		}

		public static IQueryable ApplySorting(this IQueryable query, IEnumerable<GridViewColumnState> sortedColumns)
		{
			ServerModeOrderDescriptor[] orderDescriptors = sortedColumns
				.Select(c => new ServerModeOrderDescriptor(new OperandProperty(c.FieldName), c.SortOrder == ColumnSortOrder.Descending))
				.ToArray();
			return query.MakeOrderBy(Converter, orderDescriptors);
		}

		public static IQueryable ApplyFilter(this IQueryable query, IList<GridViewGroupInfo> groupInfoList)
		{
			var criteria = GroupOperator.And(
				groupInfoList.Select(i => new BinaryOperator(i.FieldName, i.KeyValue, BinaryOperatorType.Equal))
			);
			return query.ApplyFilter(CriteriaOperator.ToString(criteria));
		}
		public static IQueryable ApplyFilter(this IQueryable query, string filterExpression)
		{
			CriteriaOperator criteria = CriteriaOperator.Parse(filterExpression);
			if (CriteriaValidator.IsCriteriaOperatorValid(criteria))
				query = query.AppendWhere(Converter, criteria);
			return query;
		}

		public static IQueryable UniqueValuesForField(this IQueryable query, string fieldName)
		{
			query = query.Select(fieldName);
			var expression = Expression.Call(typeof(Queryable), "Distinct", new Type[] { query.ElementType }, query.Expression);
			return query.Provider.CreateQuery(expression);
		}

		public static IEnumerable<GridViewGroupInfo> GetGroupInfo(this IQueryable query, string fieldName, ColumnSortOrder order)
		{
			var rowType = query.ElementType;
			query = query.MakeGroupBy(Converter, new OperandProperty(fieldName));
			query = query.MakeOrderBy(Converter, new ServerModeOrderDescriptor(new OperandProperty("Key"), order == ColumnSortOrder.Descending));

			var param = Expression.Parameter(query.ElementType, string.Empty);
			var keyValues = query.ApplyExpression(Expression.Property(param, "Key"), param).ToArray();
			var dataRowCounts = query.ApplyExpression(Expression.Call(typeof(Enumerable), "Count", new Type[] { rowType }, param), param).ToArray();

			var list = new List<GridViewGroupInfo>();
			for (int i = 0; i < keyValues.Length; i++)
				list.Add(new GridViewGroupInfo() { KeyValue = keyValues[i], DataRowCount = (int)dataRowCounts[i] });
			return list;
		}

		public static object[] CalculateSummary(this IQueryable query, List<GridViewSummaryItemState> summaryItems)
		{
			var elementType = query.ElementType;
			query = query.MakeGroupBy(Converter, new OperandValue(0));
			var groupParam = Expression.Parameter(query.ElementType, string.Empty);

			var expressions = GetAggregateExpressions(elementType, summaryItems, groupParam);
			var groupValue = new List<object>();
			foreach (var expression in expressions)
			{
				var result = query.ApplyExpression(expression, groupParam).ToArray();
				if (result.Length > 0)
					groupValue.Add(result[0]);
			}
			return groupValue.Count > 0 ? groupValue.ToArray() : new object[summaryItems.Count];
		}

		static List<Expression> GetAggregateExpressions(Type elementType, List<GridViewSummaryItemState> summaryItems, ParameterExpression groupParam)
		{
			var list = new List<Expression>();
			var elementParam = Expression.Parameter(elementType, "elem");
			foreach (var item in summaryItems)
			{
				Expression e;
				LambdaExpression elementExpr = null;
				if (!string.IsNullOrEmpty(item.FieldName))
					elementExpr = Expression.Lambda(Converter.Convert(elementParam, new OperandProperty(item.FieldName)), elementParam);

				switch (item.SummaryType)
				{
					case SummaryItemType.Count:
						e = Expression.Call(typeof(Enumerable), "Count", new Type[] { elementType }, groupParam);
						break;
					case SummaryItemType.Sum:
						e = Expression.Call(typeof(Enumerable), "Sum", new Type[] { elementType }, groupParam, elementExpr);
						break;
					case SummaryItemType.Min:
						e = Expression.Call(typeof(Enumerable), "Min", new Type[] { elementType }, groupParam, elementExpr);
						break;
					case SummaryItemType.Max:
						e = Expression.Call(typeof(Enumerable), "Max", new Type[] { elementType }, groupParam, elementExpr);
						break;
					case SummaryItemType.Average:
						e = Expression.Call(typeof(Enumerable), "Average", new Type[] { elementType }, groupParam, elementExpr);
						break;
					default:
						throw new NotSupportedException(item.SummaryType.ToString());
				}
				list.Add(e);
			}
			return list;
		}

		static IQueryable ApplyExpression(this IQueryable query, Expression expression, ParameterExpression param)
		{
			var lambda = Expression.Lambda(expression, param);
			var callExpr = Expression.Call(typeof(Queryable), "Select", new Type[] { query.ElementType, lambda.Body.Type }, query.Expression, Expression.Quote(lambda));
			return query.Provider.CreateQuery(callExpr);
		}

		static object[] ToArray(this IQueryable query)
		{
			var list = new ArrayList();
			foreach (var item in query)
				list.Add(item);
			return list.ToArray();
		}
	}
}
