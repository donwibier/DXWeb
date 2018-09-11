# XPPagedDataSource

The XPPagedDataSource implements logic to support an the DevExpress MVC GridView with Advanced Custom databinding like the example at the [Demo Center](https://demos.devexpress.com/MVCxGridViewDemos/DataBinding/AdvancedCustomBinding)
This class is using [DevExpress XPO](https://www.devexpress.com/products/net/orm/) as data access layer.

The idea behind this is to have DTOModel which incoorporate data from one or more XPOEntities.
Also see Utils.Data.DataStore and Data.Xpo.XPDataStore.

```C#
    public class XPInvoiceStore : XPPagedDataStore<int, DTOModel, XPOEntity>
	{
		public XPInvoiceStore(XpoDatabase db, XPDataValidator<int, DTOModel, XPOEntity> validator) 
		    : base(db, validator)
		{

		}

		protected override DTOModel Assign(XPOEntity source, DTOModel destination)
		{
			var result = base.Assign(source, destination);

			return result;
		}

		protected override CRM_FRM Assign(DTOModel source, XPOEntity destination)
		{
			var result = base.Assign(source, destination);

			return result;
		}
		static Dictionary<string, string> _propertyMap = new Dictionary<string, string>()
		{
			{"ID", "ID"},
			{"OrderDate", "OrderDate"},
			{"CustomerID", "Customer.ID"},
			{"CustomerName", "Customer.Name"}
			// ...
		};
		protected override Dictionary<string, string> PropertyMap => _propertyMap;

		protected override Func<XPOEntity, DTOModel> CreateModelInstance => 
		    (x) => new DTOModel
		{
			ID = x.ID,
			OrderDate = x.OrderDate,
			CustomerID = x.Customer.ID,
			CustomerName = x.Customer.Name
		};

		protected override IQueryable<XPOEntity> Query(Session s)
		{
			var r = from n in s.Query<XPOEntity>()
			        /* if needed, preset filter */
					where n.OrderDate > new DateTime(DateTime.Now.Year, 1, 1) 
					/* At least set default order */
					orderby n.ID 
					select n;
			return r;
		}

		protected override IEnumerable<DTOModel> Query()
		{
			var results = DB.Execute((db, w) =>
					{
						var r = Query(w).Select(CreateModelInstance);
						return r.ToList();
					});

			return results;
		}
	}
```

Then in the Controller which operates the GridView, you will need to add the following action methods:

```C#
    public class InvoiceController : Controller
    {		
		var DB = new XPPagedDataStore<int, DTOModel, XPOEntity>(new XPDatabase("DefaultConnection"), new InvoiceValidator())
        //...
        
        
		public InvoiceViewModel CreateViewModel()
		{
			var model = new InvoiceViewModel
			{
				ControllerName = "Invoices",
				//..more properties whatever you need
			};
			var gm = GridViewExtension.GetViewModel($"InvoiceGrid");
			if (gm == null)
			{
				gm = new GridViewModel { KeyFieldName = "ID" };
				gm.Columns.Add("OrderDate");
				gm.Columns.Add("CustomerID");
				gm.Columns.Add("CustomerName");
                //... all columns configured in grid
			}
			model.GridViewModel = gm;
			return model;
		}
		public ActionResult Index()
		{
			var model = CreateViewModel();
			return View(model);
		}

		public ActionResult GridViewPartialView()
		{
			var viewModel = CreateViewModel();			
			return GridViewCoreBinder(viewModel);				
		}

		//==
		// Paging
		public ActionResult GridViewPagingAction(GridViewPagerState pager)
		{
			var model = CreateViewModel(null);
			model.GridViewModel.ApplyPagingState(pager);
			return GridViewCoreBinder(model);
		}
		// Filtering
		public ActionResult GridViewFilteringAction(GridViewFilteringState filteringState)
		{
			var model = CreateViewModel(null);
			model.GridViewModel.ApplyFilteringState(filteringState);
			return GridViewCoreBinder(model);
		}
		// Sorting
		public ActionResult GridViewSortingAction(GridViewColumnState column, bool reset)
		{
			var model = CreateViewModel(null);
			model.GridViewModel.ApplySortingState(column, reset);
			return GridViewCoreBinder(model);
		}
		// Grouping
		public ActionResult GridViewGroupingAction(GridViewColumnState column)
		{
			var model = CreateViewModel(null);
			model.GridViewModel.ApplyGroupingState(column);
			return GridViewCoreBinder(model);
		}

		PartialViewResult GridViewCoreBinder(InvoiceLineViewModel model)
		{
			model.GridViewModel.ProcessCustomBinding(
				DB.GetGridCustomDataRowCount,
				DB.GetGridCustomData,
				DB.GetGridCustomSummaryValues,
				DB.GetGridCustomGroupingInfo,
				DB.GetGridCustomUniqueHeaderFilterValues
			);
			return PartialView("GridViewPartialView", model);
		}
		//...
	}
```

Last, configure the Grid as follows:

```c#
@{
	var grid = Html.DevExpress().GridView(
		settings =>
		{
			settings.Name = "InvoiceLinesGrid";
			settings.KeyFieldName = "ID";
			settings.CallbackRouteValues = new { Controller = "InvoiceLines", Action = "GridViewPartialView" };

			settings.CustomBindingRouteValuesCollection.Add(GridViewOperationType.Paging, new { Controller = "InvoiceLines", Action = "GridViewPagingAction" });
			settings.CustomBindingRouteValuesCollection.Add(GridViewOperationType.Sorting, new { Controller = "InvoiceLines", Action = "GridViewSortingAction" });
			settings.CustomBindingRouteValuesCollection.Add(GridViewOperationType.Filtering, new { Controller = "InvoiceLines", Action = "GridViewFilteringAction" });
			settings.CustomBindingRouteValuesCollection.Add(GridViewOperationType.Grouping, new { Controller = "InvoiceLines", Action = "GridViewGroupingAction" });

			settings.Columns.Add("OrderDate", "Order Date");
			settings.Columns.Add("CustomerID", "Cust. ID");
			settings.Columns.Add("CustomerName", "Cust. Name");
            //... more configuration of grid
		});

	if (ViewData["EditError"] != null)
	{
		grid.SetEditErrorText((string)ViewData["EditError"]);
	}
}
@grid.BindToCustomData(Model.GridViewModel).SetEditErrorText(ViewBag.EditError).GetHtml()
```
