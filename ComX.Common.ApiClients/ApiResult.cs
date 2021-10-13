using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

namespace ComX.Common.ApiClients
{
    public class ApiResult
    {
        [JsonPropertyName("ErrorMessage")]
        public string[] ErrorMessage { get; protected set; }

        [JsonPropertyName("HttpStatus")]
        public HttpStatusCode HttpStatus { get; protected set; }

        [JsonPropertyName("IsSuccessfull")]
        public bool IsSuccessfull => (int)HttpStatus >= 200 && (int)HttpStatus < 300;

        [JsonPropertyName("IsFaulted")]
        public bool IsFaulted => HttpStatus == HttpStatusCode.InternalServerError;
        public ModelStateDictionary StateModel { get; set; } = null;

        public static ApiResult Success(HttpStatusCode httpStatusCode)
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

            return new ApiResult
            {
                ErrorMessage = new string[0],
                HttpStatus = httpStatusCode
            };
        }

        public static ApiResult Fail(HttpStatusCode httpStatusCode, ModelStateDictionary stateCodeResult)
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

            return new ApiResult
            {
                HttpStatus = httpStatusCode,
                ErrorMessage = new string[0],
                StateModel = stateCodeResult
            };
        }

        public static ApiResult Fail(HttpStatusCode httpStatusCode, string[] reasons)
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

            return new ApiResult
            {
                HttpStatus = httpStatusCode,
                ErrorMessage = reasons,
            };
        }

        public static ApiResult Fail(HttpStatusCode httpStatusCode, string reason)
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

            return new ApiResult
            {
                HttpStatus = httpStatusCode,
                ErrorMessage = new string[] { reason }
            };
        }

        public static ApiResult Fault(ModelStateDictionary stateCodeResult)
        {
            return new ApiResult
            {
                StateModel = stateCodeResult,
                HttpStatus = HttpStatusCode.InternalServerError,
                ErrorMessage = new string[0]
            };
        }

        public static ApiResult Fault(string errorMessage)
        {
            return new ApiResult
            {
                ErrorMessage = new string[] { errorMessage },
                HttpStatus = HttpStatusCode.InternalServerError
            };
        }

        public static ApiResult Fault(string[] errorMessages)
        {
            return new ApiResult
            {
                ErrorMessage = errorMessages,
                HttpStatus = HttpStatusCode.InternalServerError
            };
        }

        public static ApiResult Fault(Exception exception)
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

            return new ApiResult
            {
                ErrorMessage = errorMessages.ToArray(),
                HttpStatus = HttpStatusCode.InternalServerError
            };
        }
    }
}