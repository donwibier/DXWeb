# DX.Utils

General-purpose C# helper classes and extension methods used throughout the DXWeb package family. No DevExpress dependency — this package is plain .NET and can be used standalone in any project.

**Target frameworks:** net462, net8.0, net9.0, net10.0

## Install

```
dotnet add package DX.Utils
```

## What's in the box

### Attributes.cs — reflection over attributes

```cs
public static class Attributes
{
    public static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit = false) where TAttribute : Attribute;
    public static TResult? GetAttributeData<TAttribute, TResult>(MemberInfo member, Func<TAttribute, TResult> selector) where TAttribute : Attribute;
}
```

`GetTypesWith<TAttribute>()` scans loaded assemblies for every type decorated with `TAttribute`. `GetAttributeData` pulls a single value off a member's attribute without you having to null-check `GetCustomAttribute` yourself.

Also included: `RequiredIfAttribute`, a `ValidationAttribute` that makes a property required only when another named property equals a given value (compiled only for non-.NET-Framework TFMs).

### Bits.cs — bit-flag helpers

```cs
public static class Bits
{
    public static int BitSet(int value, int bit);
    public static int BitClear(int value, int bit);
    public static int BitToggle(int value, int bit);
    public static bool BitHas(int value, int bit);
}
```

Plus the same four operations as extension methods on `int` (`BitsExtensions`), so `myFlags.BitHas(4)` reads naturally at call sites. Used internally by `DX.Data.Xpo.Identity` for its legacy loading-flags scheme.

### Collection.cs — typed collection base classes

`CollectionItem`, `BaseCollection`, and generic `Collection<TItemClass>` — an old-style strongly-typed collection base (pre-`List<T>`/`ObservableCollection<T>` era) kept for backward compatibility with consumers that still derive from it.

### Conversion.cs — safe parsing & config helpers

```cs
public static class Utils
{
    public static T Iif<T>(bool condition, T trueValue, T falseValue);
}
public static class ListExtensions
{
    public static void RemoveAll<T>(this IList<T> list, Action<T> beforeRemove);
}
public static class Conversion
{
    public static int ParseInt(object value, int defaultValue = 0);
    public static double ParseDouble(object value, double defaultValue = 0);
    public static decimal ParseDecimal(object value, decimal defaultValue = 0);
    public static bool ParseBool(object value, bool defaultValue = false);
    public static TEnum ParseEnum<TEnum>(object value, TEnum defaultValue) where TEnum : struct;
    public static string GetConfigOption(string key, string defaultValue = "");
    public static T MinValue<T>(T a, T b);
    public static T MaxValue<T>(T a, T b);
}
```

All the `Parse*` methods swallow format/overflow exceptions and fall back to `defaultValue` — handy for reading loosely-typed data (query strings, `DataRow` cells, config values) without wrapping every call in a `try/catch`.

### DateTimeExtensions.cs

```cs
public static DateTime DateMin(this DateTime a, DateTime b);
public static DateTime DateMax(this DateTime a, DateTime b);
```

### EnumExtensions.cs — `DisplayAttribute`-aware enum labels

```cs
public static string GetDisplay(this Enum value);
public static IEnumerable<(TEnumType Value, string Display)> GetDisplayValues<TEnumType>() where TEnumType : struct, Enum;
```

Reads `[Display(Name = "...", ResourceType = ...)]` off enum members (falling back to the raw member name), so you can bind enums to dropdowns/grids with human-readable, localizable labels in one call.

### ExceptionExtensions.cs

```cs
public static Exception GetInnerException(this Exception ex);
```

Walks `InnerException` all the way down and returns the innermost exception — useful when surfacing the *real* error message from a wrapped `AggregateException`/`TargetInvocationException`.

### GeoHaversine.cs — great-circle distance

```cs
public enum GeoScale { KM, Mtr, Mile }
public static double Haversine(double lat1, double lon1, double lat2, double lon2, GeoScale scale = GeoScale.KM);
public static double Distance(double lat1, double lon1, double lat2, double lon2);
public static double DistanceKM(double lat1, double lon1, double lat2, double lon2);
public static double DistanceMtr(double lat1, double lon1, double lat2, double lon2);
public static double DistanceMile(double lat1, double lon1, double lat2, double lon2);
```

### Log.cs — minimal thread-static logger

```cs
public static class Log
{
    public static void Exception(Exception ex);
    public static void Write(string message);
    public static string GetLog();
    public static void ClearLog();
}
```

Backed by `UtilsConfig.LoggerType` (read from the `DXWeb/Utils` config section) so the logging target can be swapped without code changes.

### MimeDetection — content-based MIME type detection

```cs
public static class MimeTypes
{
    public const string DEFAULT = "application/octet-stream";

    public static string DetermineMimeType(string fileName, byte[] fileBytes);
    public static MimeType GetMimeTypeFromBytes(string fileName, byte[] fileBytes);
}

public sealed class MimeType
{
    public string Name { get; }         // e.g. "image/png"
    public string PrimaryType { get; }  // e.g. "image"
    public string SubType { get; }      // e.g. "png"
}
```

Determines a file's real MIME type by sniffing its content (magic bytes) rather than trusting its extension or a browser-supplied `Content-Type` — useful when validating uploads that could be mislabeled or renamed to spoof a type.

```cs
byte[] bytes = File.ReadAllBytes(path);
string mime = MimeTypes.DetermineMimeType(fileName, bytes); // "" if undetermined
```

`DetermineMimeType` first checks the byte content against a table of known magic-byte signatures; if nothing matches it falls back to the file's extension. The signature table (`mime-types.xml`, embedded as a resource) and detection approach are ported from the [Winista.Mime](https://github.com/lupomontero/winista.mime) project (itself derived from Apache Tika's `mime-info`). `sbyte[]`-based overloads (`GetMimeTypeFromSBytes`, etc.) are also available for callers already working with signed bytes.

### PropertyExtensions.cs — reflection-based object utilities

```cs
public static bool HasProperty(this object obj, string name);
public static T GetPropertyValue<T>(this object obj, string name);
public static void SetPropertyValue(this object obj, string name, object value);
public static bool IsCollectionProperty(this PropertyInfo property);
public static TDestination Assign<TSource, TDestination>(this TDestination destination, TSource source);
public static IDictionary<string, object> AsDictionary(this object source);
public static string AppendQueryParameters(this string url, object parameters);
public static string AsQueryParameters(this object source);
```

`AppendQueryParameters`/`AsQueryParameters` build a query string from an object's public properties via reflection — decorate a property with `[AppendQueryIgnoreAttribute]` to skip it, or `[AppendQueryFormatAttribute("...")]` to control how it's rendered. `Assign` is a shallow reflection-based property copier between two (possibly unrelated) types, used internally by several `XPDataStore`/`EFDataStore` implementations to refresh a model after a commit.

### RegExLibrary.cs — precompiled regexes

Static, precompiled `Regex` fields: `EmailParser`, `QueryStringParser`, `UrlParser`, `SEOSafe`, `SEOReplaceSpaces`, and a handful of legacy ASP.NET URL-rewrite parsers.

### Strings.cs — string helpers

```cs
public static class Strings
{
    public static string StripHTMLTags(string html);
    public static string Join(string separator, params string[] values); // skips null/empty entries
    public static string CopyMaxLength(string value, int maxLength);
    public static string InsureHtmlParagraph(string html);
    public static string HtmlColor(Color color);
    public static string FormatFileSize(long bytes);
    public static string RemoveDiacritics(string value);
    public static string MakeSEOSafe(string value);
    public static string MakeFileNameSEOSafe(string value);
}
```

### UtilsConfig.cs

```cs
public sealed class UtilsConfig
{
    public static UtilsConfig Current { get; }
    public string LoggerType { get; }
}
```

Singleton that reads the `DXWeb/Utils` custom config section (app/web.config on Framework, or the matching config provider on .NET) to configure `Log`'s backing logger type.

## Notes

- `Urls.cs` is compiled only under `#if INC_URLS` and is not part of the default build (legacy ASP.NET WebForms-era helpers).
- This package has no DevExpress dependency and is version-bumped alongside the rest of the DXWeb release wave purely for consistency — you can safely pin it independently.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
