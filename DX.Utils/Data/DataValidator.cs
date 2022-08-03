using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DX.Utils.Data
{
	public enum DataValidationResultType
	{
		Success = 1,
		Warning = 2,
		Info = 3,
		Error = 4
	}
	public enum DataValidationEventType
	{
		Unknown,
		Inserting,
		Inserted,
		Updating,
		Updated,
		Deleting,
		Deleted
	}
	public class DataValidationResult<TKey> : IDataValidationResult<TKey>
		where TKey : IEquatable<TKey>
	{
		public DataValidationResult()
		{
		}
		public DataValidationResult(DataValidationResultType resultType, TKey id, string fieldName,
			string message, int code, DataValidationEventType eventType)
		{
			ResultType = resultType;
			ID = id;
			FieldName = fieldName;
			Message = message;
			Code = code;
			EventType = eventType;
		}
		public DataValidationResultType ResultType { get; set; }
		public string FieldName { get; set; }
		public string Message { get; set; }
		public int Code { get; set; }
		public TKey ID { get; set; }
		public DataValidationEventType EventType { get; set; }
	}

	public class DataValidationResults<TKey> : IDataValidationResults<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly List<IDataValidationResult<TKey>> errors = new List<IDataValidationResult<TKey>>();
		public DataValidationResults()
		{
			errors = new List<IDataValidationResult<TKey>>();
		}
		public DataValidationResults(params DataValidationResult<TKey>[] results)
		{
			errors.AddRange(results);
		}

		public IEnumerable<IDataValidationResult<TKey>> Results { get => errors; }
		public void Add(IDataValidationResult<TKey> error)
		{
			errors.Add(error);

		}
		public void Add(DataValidationResultType resultType, TKey id, string fieldName, string message, int code,
			DataValidationEventType eventType)
		{
			errors.Add(new DataValidationResult<TKey>(resultType, id, fieldName, message, code, eventType));
		}

		public string[] Messages(params DataValidationResultType[] resultsTypes)
		{
			string[] results = new string[] { };
			if (errors == null || errors.Count() == 0)
				return results;

			if (resultsTypes == null || resultsTypes.Length == 0)
				results = errors.Select(r => r.Message).ToArray();
			else
				results = errors.Where(r => resultsTypes.Contains(r.ResultType))
					.Select(r => r.Message)
					.ToArray();
			return results;
		}

		public void AddRange(IEnumerable<IDataValidationResult<TKey>> range)
		{
			errors.AddRange(range);
		}

		public void AddRange(IDataValidationResults<TKey> source)
		{
			if (source != null)
				errors.AddRange(source.Results);
		}

		public bool Success { get => (errors.Count == 0) || (errors.Count == errors.FindAll(x => x.ResultType == DataValidationResultType.Success).Count); }
	}

	public class DataValidationException<TKey> : Exception
		where TKey : IEquatable<TKey>
	{
		public DataValidationException(IDataValidationResults<TKey> validationResults)
			: base()
		{
			ValidationResults = validationResults;
		}

		public IDataValidationResults<TKey> ValidationResults { get; protected set; }
		public override IDictionary Data
		{
			get
			{
				var results = ValidationResults.Results.ToDictionary(r => r.ID, r => r.Message);
				return results;
			}
		}
		public override string Message
		{
			get { return String.Join("\n", ValidationResults.Messages(DataValidationResultType.Error, DataValidationResultType.Warning)); }
		}
	}

	public abstract class DataValidator<TKey, TModel> : IDataStoreValidator<TKey, TModel>
			where TKey : IEquatable<TKey>
			where TModel : class, new()
	{
		public abstract IDataValidationResults<TKey> Deleting(TKey id, IDataValidationResults<TKey> validationResults, params object[] args);
		public abstract IDataValidationResults<TKey> Inserting(TModel model, IDataValidationResults<TKey> validationResults);
		public abstract IDataValidationResults<TKey> Updating(TKey id, TModel model, IDataValidationResults<TKey> validationResults);
	}
	public abstract class DataValidator<TKey, TModel, TDBModel> : DataValidator<TKey, TModel>, IDataStoreValidator<TKey, TModel, TDBModel>
			where TKey : IEquatable<TKey>
			where TModel : class, new()
			where TDBModel : class
	{
		public abstract IDataValidationResults<TKey> Deleted(TKey id, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
		public abstract IDataValidationResults<TKey> Inserted(TKey id, TModel model, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
		public abstract IDataValidationResults<TKey> Updated(TKey id, TModel model, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
	}
}
