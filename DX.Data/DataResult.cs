﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;


namespace DX.Data
{
    public class DataResult<TKey> : DataResult<TKey, object>
        where TKey : IEquatable<TKey>
    {

    }

    public class DataResult<TKey, TModel> : IDataResult<TKey, TModel> 
        where TKey : IEquatable<TKey>
        where TModel : class
    {
        public DataResult()
        {
            Key = default!;
            Exception = default!;

        }
        public DataResult(DataMode mode, string propertyName, Exception err) : this()
        {
            Mode = mode;
            Success = (err == null);
            if (!Success)
            {
                Exception = (err as ValidationException)??
                    new ValidationException(new[] { new ValidationFailure(propertyName, err.InnerException == null ? err.Message : err.InnerException.Message) });
            }
        }
        public bool Success { get; set; }
        public DataMode Mode { get; set; }
        public TKey Key { get; set; }
        public TModel[] Data { get; set; } = default!;
        public ValidationException Exception { get; set; }
    }

}
