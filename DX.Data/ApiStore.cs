using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;

using System.Threading.Tasks;
using static System.Net.WebRequestMethods;


namespace DX.Data
{
#if (NET5_0_OR_GREATER)
	using System.Net.Http.Json;
	public abstract class ApiStore<TKey, TModel> : IDataStore<TKey, TModel>
		   where TKey : IEquatable<TKey>
		   where TModel : class, new()

	{
		public HttpClient Http { get; }
		public ApiStore(HttpClient httpClient)
		{
			Http = httpClient;
		}

		public abstract string KeyField { get; }
		public abstract string ControllerBase { get; }
		public abstract TKey ModelKey(TModel model);
		public abstract void SetModelKey(TModel model, TKey key);

		public TModel GetByKey(TKey key)
		{
			var result = Http.GetFromJsonAsync<TModel>($"{ControllerBase}/{key}").GetAwaiter().GetResult();
			return result!;
		}

		public async Task<IDataResult<TKey, TModel>> CreateAsync(params TModel[] items)
		{
			var result = await Http.PostAsJsonAsync(ControllerBase, items);
			var response = await result.Content.ReadAsStringAsync();
			if (result.IsSuccessStatusCode)
			{
				var r = await result.Content.ReadFromJsonAsync<TModel[]>();
				if (r != null && r.Length == items.Length)
				{
					for (int i = 0; i < items.Length; i++)
						items[i] = r[i];
				}
			}
			else
				return new DataResult<TKey, TModel>(DataMode.Create, nameof(TModel), ValidationExceptionFromResponse(response));

			return new DataResult<TKey, TModel> { Mode = DataMode.Create, Success = true, Data = items };
		}

		public async Task<IDataResult<TKey, TModel>> UpdateAsync(params TModel[] items)
		{
			var result = await Http.PutAsJsonAsync(ControllerBase, items);
			var response = await result.Content.ReadAsStringAsync();
			if (result.IsSuccessStatusCode)
			{
				var r = await result.Content.ReadFromJsonAsync<TModel[]>();
				if (r != null && r.Length == items.Length)
				{
					for (int i = 0; i < items.Length; i++)
						items[i] = r[i];
				}
			}
			else
				return new DataResult<TKey, TModel>(DataMode.Update, nameof(TModel), ValidationExceptionFromResponse(response));

			return new DataResult<TKey, TModel> { Mode = DataMode.Update, Success = true, Data = items };
		}

		public async Task<IDataResult<TKey, TModel>> StoreAsync(params TModel[] items)
		{
			var result = await Http.PostAsJsonAsync($"{ControllerBase}/Store", items);
			var response = await result.Content.ReadAsStringAsync();
			if (result.IsSuccessStatusCode)
			{
				var r = await result.Content.ReadFromJsonAsync<TModel[]>();
				if (r != null && r.Length == items.Length)
				{
					for (int i = 0; i < items.Length; i++)
						items[i] = r[i];
				}
			}
			else
				return new DataResult<TKey, TModel>(DataMode.Store, nameof(TModel), ValidationExceptionFromResponse(response));

			return new DataResult<TKey, TModel> { Mode = DataMode.Store, Success = true, Data = items };
		}


		public async Task<IDataResult<TKey, TModel>> DeleteAsync(params TKey[] ids)
		{
			foreach (var id in ids)
			{
				var result = await Http.DeleteAsync($"{ControllerBase}/{id}");
				var response = await result.Content.ReadAsStringAsync();
				if (!result.IsSuccessStatusCode)
					return new DataResult<TKey, TModel>(DataMode.Delete, nameof(TModel), ValidationExceptionFromResponse(response));
			}

			return new DataResult<TKey, TModel> { Mode = DataMode.Delete, Success = true };
		}
		public async Task<IDataResult<TKey, TModel>> DeleteAsync(params TModel[] items)
		{
			return await DeleteAsync(items.Select(i => ModelKey(i)).ToArray());
		}

		protected virtual ValidationException ValidationExceptionFromResponse(string response)
		{
			if (response.StartsWith("\"") && response.EndsWith("\""))
				response = response.Substring(1, response.Length - 2);
			response = response.Replace("\\", "");

			ValidationException result = null!;
			try
			{
				var err = JsonConvert.DeserializeObject<ValidationFailure[]>(response)!;
				result = new ValidationException(err);
			}
			catch (Exception ex)
			{
				result = new ValidationException("There are errors!", new[] {
								new ValidationFailure("Validation error", $"Please check required fields (^{ex.Message}^)")
							});
			}
			return result;
		}

	}
#endif
}
