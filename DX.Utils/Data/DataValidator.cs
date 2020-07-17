using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DX.Utils.Data
{
	public enum DataValidationResultType
	{
		Success = 1,
		Warning = 2,
		Error = 3
	}
	public class DataValidationResult<TKey> : IDataValidationResult<TKey>
		where TKey : IEquatable<TKey>
	{
		public DataValidationResult()
		{
		}
		public DataValidationResult(DataValidationResultType resultType, TKey id, string fieldName, string message, int code)
		{
			ResultType = resultType;
			ID = id;
			FieldName = fieldName;
			Message = message;
			Code = code;

		}
		public DataValidationResultType ResultType { get; set; }
		public string FieldName { get; set; }
		public string Message { get; set; }
		public int Code { get; set; }
		public TKey ID { get; set; }

	}

	public class DataValidationResults<TKey> : IDataValidationResults<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly List<IDataValidationResult<TKey>> errors = new List<IDataValidationResult<TKey>>();

		public IEnumerable<IDataValidationResult<TKey>> Results { get => errors; }
		public void Add(IDataValidationResult<TKey> error)
		{
			errors.Add(error);

		}
		public void Add(DataValidationResultType resultType, TKey id, string fieldName, string message, int code)
		{
			errors.Add(new DataValidationResult<TKey>(resultType, id, fieldName, message, code));
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
			where TModel : IDataStoreModel<TKey>
	{
		//public abstract bool Inserting(TModel model);
		//public abstract bool Updating(TModel model);
		//public abstract bool Deleting(TModel model);
		public abstract IDataValidationResult<TKey> Deleting(TKey id, IDataValidationResults<TKey> validationResults, params object[] args);
		public abstract IDataValidationResult<TKey> Inserting(TModel model, IDataValidationResults<TKey> validationResults);
		public abstract IDataValidationResult<TKey> Updating(TModel model, IDataValidationResults<TKey> validationResults);
	}
	public abstract class DataValidator<TKey, TModel, TDBModel> : DataValidator<TKey, TModel>, IDataStoreValidator<TKey, TModel, TDBModel>
			where TKey : IEquatable<TKey>
			where TModel : IDataStoreModel<TKey>
			where TDBModel : class, IDataStoreModel<TKey>
	{
		//public abstract bool Deleted(TModel model, TDBModel dbModel);
		//public abstract bool Inserted(TModel model, TDBModel dbModel);
		//public abstract bool Updated(TModel model, TDBModel dbModel);
		public abstract IDataValidationResult<TKey> Deleted(TKey id, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
		public abstract IDataValidationResult<TKey> Inserted(TModel model, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
		public abstract IDataValidationResult<TKey> Updated(TModel model, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
	}
}
