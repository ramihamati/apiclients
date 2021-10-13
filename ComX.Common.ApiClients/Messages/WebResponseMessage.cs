using ComX.Common.ApiClients.ExceptionHandler;
using ComX.Common.ApiClients.Readers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.Json;
using System.Threading.Tasks;

namespace ComX.Common.ApiClients.Messages
{
    public class WebResponseMessage
    {
        private readonly HttpResponseMessage _responseMessage;
        private readonly ExceptionDispatcher _exceptionDispatcher;
        private readonly HttpContentReader _readHandler;
        private readonly ReaderDefaultFormatters _defaultFormmaters;

        public WebResponseMessage(
            HttpResponseMessage responseMessage,
            ReaderDefaultFormatters defaultFormmaters,
            ExceptionDispatcher exceptionDispatcher)
        {
            _responseMessage = responseMessage;
            _exceptionDispatcher = exceptionDispatcher;

            if (responseMessage != null)
            {
                _readHandler = new HttpContentReader(responseMessage.Content, defaultFormmaters);
            }
            _defaultFormmaters = defaultFormmaters;
        }

        public void AddOrReplaceFormatter(string contentType, MediaTypeFormatter formatter)
        {
            _readHandler.AddOrReplaceFormatter(contentType, formatter);
        }

        /// <summary>
        /// Deserializes the response using one of the registered formatters.
        /// A formatter is selected based on the content type. If the content type is missing the formatter will throw an error.
        /// </summary>
        public async Task<TModel> AsModelAsync<TModel>() where TModel : class
        {
            _exceptionDispatcher.ThrowIfCaptured();

            //if the response is not succesfull an error will be thrown
            //this simplifies dealing with all the status codes 
            await EnsureSuccesOrThrow(_responseMessage).ConfigureAwait(false);

            return await _readHandler.AsModelAsync<TModel>().ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes the response using the JsonMediaTypeFormatter. Not all responses contain a content type appropiate for deserialization.
        /// </summary>
        public async Task<TModel> AsJsonModelAsync<TModel>() where TModel : class
        {
            _exceptionDispatcher.ThrowIfCaptured();

            //if the response is not succesfull an error will be thrown
            //this simplifies dealing with all the status codes 
            await EnsureSuccesOrThrow(_responseMessage).ConfigureAwait(false);

            return await _readHandler.AsJsonModelAsync<TModel>().ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the content as a string. If the status code is not succesfull it throws
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<string> AsStringAsync()
        {
            _exceptionDispatcher.ThrowIfCaptured();

            //if the response is not succesfull an error will be thrown
            //this simplifies dealing with all the status codes 
            await EnsureSuccesOrThrow(_responseMessage).ConfigureAwait(false);
            return await _readHandler.AsStringAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the content as a stream. If the status code is not succesfull it throws
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<Stream> AsStreamAsync()
        {
            _exceptionDispatcher.ThrowIfCaptured();

            //if the response is not succesfull an error will be thrown
            //this simplifies dealing with all the status codes 
            await EnsureSuccesOrThrow(_responseMessage).ConfigureAwait(false);

            return await _readHandler.AsStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the content as a bytearray. If the status code is not succesfull it throws
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<byte[]> AsByteArrayAsync()
        {
            _exceptionDispatcher.ThrowIfCaptured();

            //if the response is not succesfull an error will be thrown
            //this simplifies dealing with all the status codes 
            await EnsureSuccesOrThrow(_responseMessage).ConfigureAwait(false);

            return await _readHandler.AsByteArrayAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the content as form data. If the status code is not succesfull it throws
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<NameValueCollection> AsFormDataAsync()
        {
            _exceptionDispatcher.ThrowIfCaptured();

            //if the response is not succesfull an error will be thrown
            //this simplifies dealing with all the status codes 
            await EnsureSuccesOrThrow(_responseMessage).ConfigureAwait(false);

            return await _readHandler.AsFormDataAsync().ConfigureAwait(false);
        }

        public async Task<MultipartResponseReader> AsMultiPartAsync()
        {
            _exceptionDispatcher.ThrowIfCaptured();

            //if the response is not succesfull an error will be thrown
            //this simplifies dealing with all the status codes 
            await EnsureSuccesOrThrow(_responseMessage).ConfigureAwait(false);

            //pass handler formatters - because it may be the case where the user added extra formatters in the original ContenReader
            return new MultipartResponseReader(await _readHandler.AsMultiPartAsync(), _readHandler._formatters, _defaultFormmaters);
        }

        public IActionResult AsActionResult()
        {
            if (_exceptionDispatcher.IsFaulted)
            {
                return new ContentResult()
                {
                    Content = _exceptionDispatcher.Exception.Message,
                    ContentType = "text/plain",
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            return new HttpResponseMessageResult(_responseMessage);
        }

        /// <summary>
        /// Analyzes the response and determines wether the result is succesfull, faulted or failed.
        /// <para>A response is faulted then the status code is internal server error.
        /// If the response has a string content it is added in ApiResult. Otherwise the reason phrase is added
        /// </para>
        /// <para>A response is failed if the status is not in the 200 range. If the response has a string content it is added in the APiResult.
        /// Otherwise the reason phrase is added</para>
        /// <para>Error deserialization : List&lt;string&gt; ; string[] ; ModelStateDictionary will be deserialized as a list of errors in the Errors array;</para>
        /// <para>Error deserialization : Other models from an unsuccesfull status will be deserialized as string.</para>
        /// </summary>
        public async Task<ApiResult> AsApiResultAsync()
        {
            try
            {
                if (_exceptionDispatcher.IsFaulted)
                {
                    return _exceptionDispatcher.AsApiResult();
                }

                if (_responseMessage.IsSuccessStatusCode)
                {
                    return ApiResult.Success(_responseMessage.StatusCode);
                }
                else
                {
                    //if we have a model state - convert it to a serializable StateError

                    (bool handled, ModelStateDictionary stateCode) = await HandleModelState();

                    if (handled)
                    {
                        if (_responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return ApiResult.Fault(stateCode);
                        }
                        else
                        {
                            return ApiResult.Fail(_responseMessage.StatusCode, stateCode);
                        }
                    }
                    else
                    {
                        List<string> errorMessages = await HandleErrorMessages();

                        if (_responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return ApiResult.Fault(errorMessages.ToArray());
                        }
                        else
                        {
                            return ApiResult.Fail(_responseMessage.StatusCode, errorMessages.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResult.Fault(ex);
            }
        }

        /// <summary>
        /// Analyzes the response and determines wether the result is succesfull, faulted or failed.
        /// <para>If the response is succesfull the model is deserialized using the available formatters</para>
        /// <para>A response is faulted then the status code is internal server error.
        /// If the response has a string content it is added in ApiResult. Otherwise the reason phrase is added
        /// </para>
        /// <para>A response is failed if the status is not in the 200 range. If the response has a string content it is added in the APiResult.
        /// Otherwise the reason phrase is added</para>
        /// </summary>
        public async Task<ApiResult<TModel>> AsApiResultAsync<TModel>() where TModel : class
        {
            try
            {
                if (_exceptionDispatcher.IsFaulted)
                {
                    return _exceptionDispatcher.AsApiResult<TModel>();
                }

                if (_responseMessage.IsSuccessStatusCode)
                {
                    TModel model = await AsModelAsync<TModel>().ConfigureAwait(false);

                    return ApiResult<TModel>.Success(model, _responseMessage.StatusCode);
                }
                else
                {
                    //if we have a model state - convert it to a serializable StateError

                    (bool handled, ModelStateDictionary stateCode) = await HandleModelState();

                    if (handled)
                    {
                        if (_responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return ApiResult<TModel>.Fault(stateCode);
                        }
                        else
                        {
                            return ApiResult<TModel>.Fail(_responseMessage.StatusCode, stateCode);
                        }
                    }
                    else
                    {
                        List<string> errorMessages = await HandleErrorMessages();

                        if (_responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return ApiResult<TModel>.Fault(errorMessages.ToArray());
                        }
                        else
                        {
                            return ApiResult<TModel>.Fail(_responseMessage.StatusCode, errorMessages.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResult<TModel>.Fault(ex);
            }
        }

        private async Task<Tuple<bool, ModelStateDictionary>> HandleModelState()
        {
            if (_responseMessage.Content is ObjectContent objectContent)
            {
                if (typeof(ModelStateDictionary).IsAssignableFrom(objectContent.ObjectType))
                {
                    ModelStateDictionary modelStateDictionary = await _responseMessage.Content.ReadAsAsync<ModelStateDictionary>().ConfigureAwait(false);

                    return Tuple.Create(true, modelStateDictionary);
                }
            }

            return Tuple.Create<bool, ModelStateDictionary>(false, default);
        }

        private async Task<List<string>> HandleErrorMessages()
        {
            List<string> errorMessages = new List<string>();
            bool _skip = false;

            if (_responseMessage.Content is ObjectContent objectContent)
            {
                if (typeof(IEnumerable<string>).IsAssignableFrom(objectContent.ObjectType))
                {
                    string errArrayContent = await _responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(errArrayContent))
                    {
                        IEnumerable<string> errors = (IEnumerable<string>)JsonSerializer.Deserialize(errArrayContent, objectContent.ObjectType);
                        errorMessages = errorMessages.Concat(errors).ToList();
                        _skip = true;
                    }
                }
            }

            if (!_skip)
            {
                string content = await _responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                string errorMessage = (!string.IsNullOrWhiteSpace(content)) ? content : _responseMessage.ReasonPhrase;
                errorMessages.Add(errorMessage);
            }

            return errorMessages;
        }

        private async Task EnsureSuccesOrThrow(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.Content != null)
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    throw new Exception(content);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }

            throw new Exception($"An internal server error occured. Reason phrase = {response.ReasonPhrase}");
        }
    }
}
