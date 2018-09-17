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
		void GetGridViewDataRowCount(GridViewCustomBindingGetDataRowCountArgs e);
		void GetGridViewUniqueHeaderFilterValues(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e);
		void GetGridViewGroupingInfo(GridViewCustomBindingGetGroupingInfoArgs e);
		void GetGridViewData(GridViewCustomBindingGetDataArgs e);
		void GetGridViewSummaryValues(GridViewCustomBindingGetSummaryValuesArgs e);

		void GetGridLookupRowValues(GridViewCustomBindingGetRowValuesArgs e);
	}
}
