using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

namespace ComX.Common.ApiClients
{
    public class ApiResult<TModel> : ApiResult
    {
        [JsonPropertyName("Model")]
        public TModel Model { get; set; }

        public static ApiResult<TModel> Success(TModel model, HttpStatusCode httpStatusCode)
        {
            if ((int)httpStatusCode < 200 || (int)httpStatusCode >= 300)
            {
                throw new Exception($"Success method cannot receive a non-succesfull status code : {httpStatusCode.ToString()}." +
                    $"\n100 group : Items in progress. " +
                    $"\n200 group : Succesfull responses." +
                    $"\n300 group : Redirects" +
                    $"\n400 group : Browser errors. Client errors." +
                    $"\n500 group : Server errors.");
            }

            return new ApiResult<TModel>
            {
                HttpStatus = httpStatusCode,
                ErrorMessage = new string[] { },
                Model = model
            };
        }

        public new static ApiResult<TModel> Fail(HttpStatusCode httpStatusCode, ModelStateDictionary stateCodeResult)
        {
            if ((int)httpStatusCode >= 200 && (int)httpStatusCode < 300)
            {
                throw new Exception($"Fail method cannot receive a success status code : {httpStatusCode.ToString()}. " +
                    $"\n100 group : Items in progress. " +
                    $"\n200 group : Succesfull responses." +
                    $"\n300 group : Redirects" +
                    $"\n400 group : Browser errors. Client errors." +
                    $"\n500 group : Server errors.");
            }

            return new ApiResult<TModel>
            {
                HttpStatus = httpStatusCode,
                StateModel = stateCodeResult,
                Model = default,
                ErrorMessage = new string[0]
            };
        }

        public new static ApiResult<TModel> Fail(HttpStatusCode httpStatusCode, string[] reasons)
        {
            if ((int)httpStatusCode >= 200 && (int)httpStatusCode < 300)
            {
                throw new Exception($"Fail method cannot receive a success status code : {httpStatusCode.ToString()}. " +
                    $"\n100 group : Items in progress. " +
                    $"\n200 group : Succesfull responses." +
                    $"\n300 group : Redirects" +
                    $"\n400 group : Browser errors. Client errors." +
                    $"\n500 group : Server errors.");
            }

            return new ApiResult<TModel>
            {
                HttpStatus = httpStatusCode,
                ErrorMessage = reasons,
                Model = default
            };
        }

        public new static ApiResult<TModel> Fail(HttpStatusCode httpStatusCode, string reason)
        {
            if ((int)httpStatusCode >= 200 && (int)httpStatusCode < 300)
            {
                throw new Exception($"Fail method cannot receive a success status code : {httpStatusCode.ToString()}. " +
                    $"\n100 group : Items in progress. " +
                    $"\n200 group : Succesfull responses." +
                    $"\n300 group : Redirects" +
                    $"\n400 group : Browser errors. Client errors." +
                    $"\n500 group : Server errors.");
            }

            return new ApiResult<TModel>
            {
                HttpStatus = httpStatusCode,
                ErrorMessage = new string[] { reason },
                Model = default
            };
        }

        public new static ApiResult<TModel> Fault(ModelStateDictionary stateCodeResult)
        {
            return new ApiResult<TModel>
            {
                Model = default,
                StateModel = stateCodeResult,
                HttpStatus = HttpStatusCode.InternalServerError,
                ErrorMessage = new string[0]
            };
        }

        public new static ApiResult<TModel> Fault(string errorMessage)
        {
            return new ApiResult<TModel>
            {
                Model = default,
                ErrorMessage = new string[] { errorMessage },
                HttpStatus = HttpStatusCode.InternalServerError
            };
        }

        public new static ApiResult<TModel> Fault(string[] errorMessages)
        {
            return new ApiResult<TModel>
            {
                Model = default,
                ErrorMessage = errorMessages,
                HttpStatus = HttpStatusCode.InternalServerError
            };
        }

        public new static ApiResult<TModel> Fault(Exception exception)
        {
            List<string> errorMessages;

            if (exception.InnerException != null)
            {
                errorMessages = (new string[] { "msg : " + exception.Message, "stack : " + exception.StackTrace }).Concat(Fault(exception.InnerException).ErrorMessage).ToList();
            }
            else
            {
                errorMessages = (new string[] { "msg : " + exception.Message, "stack : " + exception.StackTrace }).ToList();
            }

            return new ApiResult<TModel>
            {
                Model = default,
                ErrorMessage = errorMessages.ToArray(),
                HttpStatus = HttpStatusCode.InternalServerError
            };
        }
    }
}