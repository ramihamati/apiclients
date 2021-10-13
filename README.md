**Get**

```
 GetWebRequestBuilder getRequest = factory.GetRequest();
getRequest.SetUri(builder =>
{
   builder.SetPath("/users/nocontent");
   builder.AddQueryParameter("name", "john");
});
WebRequestMessage requestMessage = getRequest.BuildRequest();
WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);
ApiResult result = await response.AsApiResultAsync().ConfigureAwait(false);
```

**Get as String**

```
GetWebRequestBuilder getRequest = factory.GetRequest();   
getRequest.SetUri(builder =>{ 
   builder.SetPath("/users/nocontent");  
   builder.AddQueryParameter("name", "john");
});
WebRequestMessage requestMessage = getRequest.BuildRequest();
WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

Assert.CatchAsync(async () => await response.AsStringAsync());

```

**WebResponse Responses**
```
public async Task<TModel> AsModelAsync<TModel>()
public async Task<TModel> AsJsonModelAsync<TModel>()
public async Task<string> AsStringAsync()
public async Task<Stream> AsStreamAsync()
public async Task<byte[]> AsByteArrayAsync()
public async Task<NameValueCollection> AsFormDataAsync()
public async Task<MultipartResponseReader> AsMultiPartAsync()
 public IActionResult AsActionResult()
public async Task<ApiResult> AsApiResultAsync()
 public async Task<ApiResult<TModel>> AsApiResultAsync<TModel>()
```

**ApiResult**

```
public class ApiResult<TModel> : ApiResult
{
        [JsonPropertyName("Model")]
        public TModel Model { get; set; }
}

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
}
```

**POST**

```
  PostWebRequestBuilder postRequest = factory.PostRequest();

            postRequest.SetUri(builder =>
            {
                builder.SetPath("/users/jsoncontent");
            });

            postRequest.AddHeader(builder =>
            {
                builder.AddHeader("X-Person", "John");
                builder.AddContentHeader("X-ContentPerson", "Doe");
            });
            Person person = new Person { Name = "John", Surname = "Doe" };
            postRequest.SetContent(builder => builder.ContentJson<Person>(person));

            WebRequestMessage requestMessage = postRequest.BuildRequest();
            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            ApiResult result = await response.AsApiResultAsync();
```

**URI BUILDER**

```
WebRequestUriBuilder requestBuilder = new WebRequestUriBuilder();
postRequest.SetUri(builder =>
{ 
   builder.SetPath("a", "/b/c/");
   builder.AddQueryParameter("name", "jon");
   builder.SetFragment("#fragment");
});

```
**REQUEST BUILDER***

```
 PostWebRequestBuilder postRequest = factory.PostRequest();

            postRequest.SetUri(builder =>
            {
                builder.SetPath("/users/jsoncontent");
            });

            postRequest.AddHeader(builder =>
            {
                builder.AddHeader("X-Person", "John");
                builder.AddContentHeader("X-ContentPerson", "Doe");
            });
            Person person = new Person { Name = "John", Surname = "Doe" };
            postRequest.SetContent(builder => builder.ContentJson<Person>(person));

            WebRequestMessage requestMessage = postRequest.BuildRequest();
            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            ApiResult result = await response.AsApiResultAsync();
```