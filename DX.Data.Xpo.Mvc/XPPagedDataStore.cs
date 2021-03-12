using DevExpress.Data;
using DevExpress.Data.Linq.Helpers;
using DevExpress.Web.Mvc;
using DevExpress.Xpo;
using DX.Data.Xpo;
using DX.Data.Xpo.Mvc.Utils;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace DX.Data.Xpo.Mvc
{
	public abstract class XPPagedDataStore<TKey, TModel, TXPOClass> : XPDataStore<TKey, TModel, TXPOClass>, IPagedDataStore
		where TKey : IEquatable<TKey>
		where TModel : class, IDataStoreModel<TKey>, new()
		where TXPOClass : XPBaseObject, IDataStoreModel<TKey>
	{
		public XPPagedDataStore(XpoDatabase db,
					IXPDataMapper<TKey, TModel, TXPOClass> mapper,
					IXPDataStoreValidator<TKey, TModel, TXPOClass> validator)
			: base(db, mapper, validator)
		{

		}


		#region Preparations for advanced gridview paging and sorting

		static Regex regexFilter = new Regex("\\[.*?\\]", RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static Regex regexBrackets = new Regex("[\\[\\]]", RegexOptions.CultureInvariant | RegexOptions.Compiled);

		//protected abstract Dictionary<string, string> PropertyMap { get; }
		protected virtual string PrepareProperty(string dtoProperty/*, Dictionary<string, string> map = null*/)
		{
			if (String.IsNullOrEmpty(dtoProperty))
				return dtoProperty;

			//map = map ?? PropertyMap;
			var key = regexBrackets.Replace(dtoProperty, string.Empty);
			//return map.ContainsKey(key) ? regexBrackets.Replace(map[key], "") : key;
			return regexBrackets.Replace(Mapper.Map(key), string.Empty);
		}

		protected virtual string PrepareFilterExpression(string filterExpression/*, Dictionary<string, string> propertyMap = null*/)
		{
			if (String.IsNullOrEmpty(filterExpression))
				return filterExpression;

			string result = regexFilter.Replace(filterExpression, m =>
			{
				string f = m.ToString();
				return PrepareProperty(f/*, propertyMap*/);
			});
			return result;
		}

		protected virtual List<GridViewSummaryItemState> PrepareSummaryItems(List<GridViewSummaryItemState> summaryItems)
		{
			foreach (var summary in summaryItems)
				summary.FieldName = PrepareProperty(summary.FieldName);
			return summaryItems;
		}

		protected virtual IList<GridViewGroupInfo> PrepareGroupInfoList(IList<GridViewGroupInfo> groupInfoList)
		{
			foreach (var group in groupInfoList)
				group.FieldName = PrepareProperty(group.FieldName);
			return groupInfoList;
		}
		protected virtual IEnumerable<GridViewColumnState> PrepareSorting(IEnumerable<GridViewColumnState> sortedColumns)
		{
			foreach (var column in sortedColumns)
				column.FieldName = PrepareProperty(column.FieldName);
			return sortedColumns;
		}


		#endregion

		#region GridView Caching
		protected virtual Dictionary<string, int> CacheCount
		{
			get
			{
				var result = HostingEnvironment.Cache[$"Counts_{GetType().FullName}"] as Dictionary<string, int>;
				if (result == null)
				{
					result = new Dictionary<string, int>();
					HostingEnvironment.Cache[$"Counts_{GetType().FullName}"] = result;
				}
				return result;
			}
		}
		protected virtual bool CacheTryGetCount(string key, out int count)
		{
			count = 0;
			if (!CacheCount.ContainsKey(key))
				return false;
			count = CacheCount[key];
			return true;
		}
		protected virtual void CacheSaveCount(string key, int count)
		{
			CacheCount[key] = count;
		}
		#endregion

		#region GridView CustomBinding Methods
		public virtual void GetGridViewDataRowCount(GridViewCustomBindingGetDataRowCountArgs e)
		{
			int rowCount;
			if (CacheTryGetCount(e.FilterExpression, out rowCount))
				e.DataRowCount = rowCount;
			else
				e.DataRowCount = DB.Execute((db, w) => Query(w).ApplyFilter(PrepareFilterExpression(e.FilterExpression)).Count());
		}

		public virtual void GetGridViewUniqueHeaderFilterValues(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e)
		{
			var result = DB.Execute((db, w) =>
			{
				var r = Query(w)
					.ApplyFilter(PrepareFilterExpression(e.FilterExpression))
					/*.UniqueValuesForField(PrepareProperty(e.FieldName)) */
					.UniqueValuesForField(PrepareProperty(e.FieldName)) as IQueryable<TXPOClass>;

				return (from n in r select n).ToList();
				//return r;
			});
			e.Data = result;
		}
		public virtual void GetGridViewGroupingInfo(GridViewCustomBindingGetGroupingInfoArgs e)
		{
			var result = DB.Execute((db, w) =>
			{
				var r = Query(w)
					.ApplyFilter(PrepareFilterExpression(e.FilterExpression))
					.ApplyFilter(PrepareGroupInfoList(e.GroupInfoList))
					.GetGroupInfo(PrepareProperty(e.FieldName), e.SortOrder);
				return r.ToList();
			});
			e.Data = result;
		}

		public virtual void GetGridViewData(GridViewCustomBindingGetDataArgs e)
		{
			var result = DB.Execute((db, w) =>
			{
				var items = Query(w)
					.ApplyFilter(PrepareFilterExpression(e.FilterExpression))
					.ApplyFilter(PrepareGroupInfoList(e.GroupInfoList))
					.ApplySorting(PrepareSorting(e.State.SortedColumns))
					.Skip(e.StartDataRowIndex)
					.Take(e.DataRowCount);

				var r = ((IQueryable<TXPOClass>)items).Select(CreateModelInstance);
				return r.ToList();
			});
			e.Data = result;
		}

		public virtual void GetGridViewSummaryValues(GridViewCustomBindingGetSummaryValuesArgs e)
		{
			var result = DB.Execute((db, w) =>
			{

				var query = Query(w)
					.ApplyFilter(PrepareFilterExpression(e.FilterExpression))
					.ApplyFilter(PrepareGroupInfoList(e.GroupInfoList));

				List<GridViewSummaryItemState> summaryItems = e.SummaryItems;
				var summaryValues = query.CalculateSummary(PrepareSummaryItems(summaryItems));

				var countSummaryItem = summaryItems.FirstOrDefault(i => i.SummaryType == SummaryItemType.Count);
				if (e.GroupInfoList.Count == 0 && countSummaryItem != null)
				{
					var itemIndex = summaryItems.IndexOf(countSummaryItem);
					var count = summaryValues[itemIndex] != null ? Convert.ToInt32(summaryValues[itemIndex]) : 0;
					CacheSaveCount(e.FilterExpression, count);
				}

				return summaryValues;
			});
			e.Data = result;
		}

		#endregion

		#region GridLookup Custom Binding Methods

		public virtual void GetGridLookupRowValues(GridViewCustomBindingGetRowValuesArgs e)
		{
			var n = default(TKey);
			if (e.KeyValues.Count() == 0)
			{
				e.RowValues = new TModel[] { GetByKey(n) };
			}
			else
			{
				e.RowValues = DB.Execute((db, w) =>
				{
					var r = Query(w).Where(c => e.KeyValues.Contains(c.Id)).Select(CreateModelInstance);
					return r.ToList();
				});
			}
		}

		#endregion

	}
}
