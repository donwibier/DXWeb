# DX.Data.Xpo.Mvc

Server-side filtering, sorting, grouping, and summary-calculation support for DevExpress ASP.NET MVC 5's `ASPxGridView`, built directly on top of `DX.Data.Xpo`'s `XPDataStore`. **Requires an active DevExpress ASP.NET license** — this package references `DevExpress.Web.Mvc5`.

**Target framework:** net462 only (classic ASP.NET MVC 5 / `System.Web`)
**DevExpress dependency:** `DevExpress.Xpo`, `DevExpress.Data`, `DevExpress.Web.Mvc5` (all `26.1.*`)

## Install

```
dotnet add package DX.Data.Xpo.Mvc
```

## XPPagedDataStore<TKey, TModel, TXPOClass> (XPPagedDataStore.cs)

```cs
public abstract class XPPagedDataStore<TKey, TModel, TXPOClass> : XPDataStore<TKey, TModel, TXPOClass>, IPagedDataStore
    where TXPOClass : XPBaseObject
{
    protected XPPagedDataStore(IDataLayer dataLayer, IValidator<TXPOClass> validator);

    // ASPxGridView custom-binding event handlers:
    public virtual void GetGridViewDataRowCount(GridViewCustomBindingGetDataRowCountArgs e);
    public virtual void GetGridViewUniqueHeaderFilterValues(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e);
    public virtual void GetGridViewGroupingInfo(GridViewCustomBindingGetGroupingInfoArgs e);
    public virtual void GetGridViewData(GridViewCustomBindingGetDataArgs e);
    public virtual void GetGridViewSummaryValues(GridViewCustomBindingGetSummaryValuesArgs e);
    public virtual void GetGridLookupRowValues(GridViewCustomBindingGetRowValuesArgs e);
}
```

Wire an `ASPxGridView`'s custom-data-binding events directly to these six methods and the grid gets server-side paging, sorting, filtering, grouping, and summary calculation for free — all translated into `CriteriaOperator`/XPO `IQueryable` operations against your existing `XPDataStore`, so query logic isn't duplicated between the grid and the rest of your data layer. A row-count cache (`GetGridViewDataRowCount`) is kept per filter expression in `HostingEnvironment.Cache`, keyed by `Counts_{GetType().FullName}`, so repeated identical-filter requests skip the `COUNT(*)` round-trip.

### Usage

```cs
public class CustomerGridStore : XPPagedDataStore<int, CustomerDto, XpoCustomer>
{
    public CustomerGridStore(IDataLayer dataLayer, IValidator<XpoCustomer> validator) : base(dataLayer, validator) { }
    // ToDBModel / ToModel / Query<T>() implemented as with any XPDataStore subclass (see DX.Data.Xpo's README)
}
```

```cs
public class InvoiceController : Controller
{
    readonly CustomerGridStore store;

    public ActionResult GridViewPartialView()
    {
        // wire the grid's CustomDataBinding events straight to the store:
        // grid.CustomDataBinding.GetDataRowCount += (s, e) => store.GetGridViewDataRowCount(e);
        // grid.CustomDataBinding.GetData += (s, e) => store.GetGridViewData(e);
        // grid.CustomDataBinding.GetGroupingInfo += (s, e) => store.GetGridViewGroupingInfo(e);
        // grid.CustomDataBinding.GetSummaryValues += (s, e) => store.GetGridViewSummaryValues(e);
        // grid.CustomDataBinding.GetUniqueHeaderFilterValues += (s, e) => store.GetGridViewUniqueHeaderFilterValues(e);
        return PartialView("GridViewPartialView");
    }
}
```

See the [DevExpress Demo Center's Advanced Custom Binding example](https://demos.devexpress.com/MVCxGridViewDemos/DataBinding/AdvancedCustomBinding) for the full `ASPxGridView` markup/routing pattern this class is designed to plug into.

## Utils/GridViewCustomOperationDataHelper.cs

Internal `IQueryable` extension methods (`ApplySorting`, `ApplyFilter`, `Select`, `UniqueValuesForField`, `GetGroupInfo`, `CalculateSummary`) that translate `ASPxGridView` state (`GridViewColumnState`, `GridViewGroupInfo`, `GridViewSummaryItemState`) into DevExpress `CriteriaOperator`/LINQ expression-tree operations against the underlying `IQueryable`. You don't call these directly — `XPPagedDataStore`'s methods use them internally.

## Utils/CriteriaValidator.cs

```cs
public class CriteriaValidator : EvaluatorCriteriaValidator
{
    public static bool IsCriteriaOperatorValid(CriteriaOperator criteria);
}
```

Rejects a parsed filter `CriteriaOperator` if it contains an `OperandValue` with a `null` value — a defensive check applied before any filter expression coming from the grid's UI is turned into a query, preventing malformed/incomplete filter clauses (e.g. mid-typing autofilter state) from reaching the database.

## Notes

- This package targets **net462 only** — `ASPxGridView`/MVC 5 is a classic ASP.NET Framework technology with no .NET (Core) equivalent in this repo.
- Requires an active [DevExpress ASP.NET license](https://www.devexpress.com/products/net/controls/asp/) — the underlying `DevExpress.Web.Mvc5` package will not function without one.
- Builds directly on `DX.Data.Xpo`'s `XPDataStore` — see that package's README for the base store API this class extends.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
