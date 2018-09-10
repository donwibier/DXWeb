using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Data.Xpo.Mvc
{
    public interface IPagedDataStore
    {
		void GetGridCustomDataRowCount(GridViewCustomBindingGetDataRowCountArgs e);
		void GetGridCustomUniqueHeaderFilterValues(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e);
		void GetGridCustomGroupingInfo(GridViewCustomBindingGetGroupingInfoArgs e);
		void GetGridCustomData(GridViewCustomBindingGetDataArgs e);
		void GetGridCustomSummaryValues(GridViewCustomBindingGetSummaryValuesArgs e);
	}
}
