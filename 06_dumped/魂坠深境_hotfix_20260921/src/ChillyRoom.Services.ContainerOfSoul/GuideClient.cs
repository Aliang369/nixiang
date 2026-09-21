using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChillyRoom.Services.Core.Errors;
using Generated.ChillyRoomSdkClient;
using Newtonsoft.Json;
using UnityEngine;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NSwag", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
public class GuideClient
{
	protected struct ObjectResponseResult<T>(T responseObject, string responseText)
	{
		public T Object { get; } = responseObject;

		public string Text { get; } = responseText;
	}

	private string _baseUrl = "";

	private HttpClient _httpClient;

	private Lazy<JsonSerializerSettings> _settings;

	public string BaseUrl
	{
		get
		{
			return _baseUrl;
		}
		set
		{
			_baseUrl = value;
		}
	}

	protected JsonSerializerSettings JsonSerializerSettings => _settings.Value;

	public bool EnableRequestCompression { get; set; }

	public GuideClient(string baseUrl, HttpClient httpClient)
	{
		BaseUrl = baseUrl;
		_httpClient = httpClient;
		_settings = new Lazy<JsonSerializerSettings>(CreateSerializerSettings);
	}

	private JsonSerializerSettings CreateSerializerSettings()
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		UpdateJsonSerializerSettings(jsonSerializerSettings);
		return jsonSerializerSettings;
	}

	private void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
	{
		ChillyRoomService.AddEssentialConverters(settings);
	}

	public virtual Task FinishIntroAsync()
	{
		return FinishIntroAsync(CancellationToken.None);
	}

	public virtual async Task FinishIntroAsync(CancellationToken cancellationToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((BaseUrl != null) ? BaseUrl.TrimEnd('/') : "").Append("/Guide/FinishIntro");
		HttpClient client_ = _httpClient;
		bool disposeClient_ = false;
		try
		{
			using HttpRequestMessage request_ = new HttpRequestMessage();
			request_.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
			request_.Method = new HttpMethod("POST");
			request_.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
			string uriString = stringBuilder.ToString();
			request_.RequestUri = new Uri(uriString, UriKind.RelativeOrAbsolute);
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				throw new ApiException<ErrorResponse>("Network Unreachable", -100, null, null, null);
			}
			bool disposeResponse_ = true;
			HttpResponseMessage response_;
			try
			{
				response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (HttpRequestException innerException)
			{
				throw new ApiException<ErrorResponse>("Error making api request", -1, null, null, (Exception)(object)innerException);
			}
			catch (WebException innerException2)
			{
				throw new ApiException<ErrorResponse>("Error making api request", -1, null, null, innerException2);
			}
			catch (TaskCanceledException innerException3)
			{
				throw new ApiException<ErrorResponse>("Error making api request", -1, null, null, innerException3);
			}
			try
			{
				Dictionary<string, IEnumerable<string>> headers_ = ((IEnumerable<KeyValuePair<string, IEnumerable<string>>>)(object)response_.Headers).ToDictionary((KeyValuePair<string, IEnumerable<string>> h_) => h_.Key, (KeyValuePair<string, IEnumerable<string>> h_) => h_.Value);
				if (response_.Content != null && response_.Content.Headers != null)
				{
					foreach (KeyValuePair<string, IEnumerable<string>> item in (HttpHeaders)response_.Content.Headers)
					{
						headers_[item.Key] = item.Value;
					}
				}
				int status_ = (int)response_.StatusCode;
				switch (status_)
				{
				case 200:
					break;
				case 400:
				{
					ObjectResponseResult<ErrorResponse> objectResponseResult2 = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (objectResponseResult2.Object == null)
					{
						throw new ApiException<ErrorResponse>("Response was null which was not expected.", status_, objectResponseResult2.Text, headers_, null);
					}
					throw new ApiException<ErrorResponse>("Bad Request", status_, objectResponseResult2.Text, headers_, objectResponseResult2.Object, null);
				}
				case 401:
				{
					ObjectResponseResult<ErrorResponse> objectResponseResult = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (objectResponseResult.Object == null)
					{
						throw new ApiException<ErrorResponse>("Response was null which was not expected.", status_, objectResponseResult.Text, headers_, null);
					}
					throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponseResult.Text, headers_, objectResponseResult.Object, null);
				}
				case 500:
				{
					string text = ((response_.Content != null) ? (await response_.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : string.Empty);
					string response2 = text;
					throw new ApiException<ErrorResponse>("Server Error", status_, response2, headers_, null);
				}
				default:
				{
					string text = ((response_.Content != null) ? (await response_.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : null);
					string response = text;
					throw new ApiException<ErrorResponse>("The HTTP status code of the response was not expected (" + status_ + ").", status_, response, headers_, null);
				}
				}
			}
			catch (ApiException<ErrorResponse>)
			{
				throw;
			}
			finally
			{
				if (disposeResponse_)
				{
					response_.Dispose();
				}
			}
		}
		finally
		{
			if (disposeClient_)
			{
				client_.Dispose();
			}
		}
	}

	public virtual Task FinishGuideModuleAsync(FinishGuideModuleRequest body)
	{
		return FinishGuideModuleAsync(body, CancellationToken.None);
	}

	public virtual async Task FinishGuideModuleAsync(FinishGuideModuleRequest body, CancellationToken cancellationToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((BaseUrl != null) ? BaseUrl.TrimEnd('/') : "").Append("/Guide/FinishGuideModule");
		HttpClient client_ = _httpClient;
		bool disposeClient_ = false;
		try
		{
			using HttpRequestMessage request_ = new HttpRequestMessage();
			string text = JsonConvert.SerializeObject(body, _settings.Value);
			HttpContent httpContent;
			if (!EnableRequestCompression || text.Length < 800)
			{
				httpContent = new StringContent(text);
			}
			else
			{
				httpContent = new ByteArrayContent(Compress(text));
				httpContent.Headers.ContentEncoding.Add("gzip");
			}
			httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
			request_.Content = httpContent;
			request_.Method = new HttpMethod("POST");
			request_.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
			string uriString = stringBuilder.ToString();
			request_.RequestUri = new Uri(uriString, UriKind.RelativeOrAbsolute);
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				throw new ApiException<ErrorResponse>("Network Unreachable", -100, null, null, null);
			}
			bool disposeResponse_ = true;
			HttpResponseMessage response_;
			try
			{
				response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (HttpRequestException innerException)
			{
				throw new ApiException<ErrorResponse>("Error making api request", -1, null, null, (Exception)(object)innerException);
			}
			catch (WebException innerException2)
			{
				throw new ApiException<ErrorResponse>("Error making api request", -1, null, null, innerException2);
			}
			catch (TaskCanceledException innerException3)
			{
				throw new ApiException<ErrorResponse>("Error making api request", -1, null, null, innerException3);
			}
			try
			{
				Dictionary<string, IEnumerable<string>> headers_ = ((IEnumerable<KeyValuePair<string, IEnumerable<string>>>)(object)response_.Headers).ToDictionary((KeyValuePair<string, IEnumerable<string>> h_) => h_.Key, (KeyValuePair<string, IEnumerable<string>> h_) => h_.Value);
				if (response_.Content != null && response_.Content.Headers != null)
				{
					foreach (KeyValuePair<string, IEnumerable<string>> item in (HttpHeaders)response_.Content.Headers)
					{
						headers_[item.Key] = item.Value;
					}
				}
				int status_ = (int)response_.StatusCode;
				switch (status_)
				{
				case 200:
					break;
				case 400:
				{
					ObjectResponseResult<ErrorResponse> objectResponseResult2 = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (objectResponseResult2.Object == null)
					{
						throw new ApiException<ErrorResponse>("Response was null which was not expected.", status_, objectResponseResult2.Text, headers_, null);
					}
					throw new ApiException<ErrorResponse>("Bad Request", status_, objectResponseResult2.Text, headers_, objectResponseResult2.Object, null);
				}
				case 401:
				{
					ObjectResponseResult<ErrorResponse> objectResponseResult = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (objectResponseResult.Object == null)
					{
						throw new ApiException<ErrorResponse>("Response was null which was not expected.", status_, objectResponseResult.Text, headers_, null);
					}
					throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponseResult.Text, headers_, objectResponseResult.Object, null);
				}
				case 500:
				{
					string text2 = ((response_.Content != null) ? (await response_.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : string.Empty);
					string response2 = text2;
					throw new ApiException<ErrorResponse>("Server Error", status_, response2, headers_, null);
				}
				default:
				{
					string text2 = ((response_.Content != null) ? (await response_.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : null);
					string response = text2;
					throw new ApiException<ErrorResponse>("The HTTP status code of the response was not expected (" + status_ + ").", status_, response, headers_, null);
				}
				}
			}
			catch (ApiException<ErrorResponse>)
			{
				throw;
			}
			finally
			{
				if (disposeResponse_)
				{
					response_.Dispose();
				}
			}
		}
		finally
		{
			if (disposeClient_)
			{
				client_.Dispose();
			}
		}
	}

	protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
	{
		if (response == null || response.Content == null)
		{
			return new ObjectResponseResult<T>(default(T), string.Empty);
		}
		try
		{
			using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
			Stream stream2 = stream;
			if (ContentIsGzipEncoded(headers))
			{
				stream2 = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
			}
			using StreamReader reader = new StreamReader(stream2);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			T responseObject = JsonSerializer.Create(JsonSerializerSettings).Deserialize<T>(reader2);
			return new ObjectResponseResult<T>(responseObject, string.Empty);
		}
		catch (JsonException innerException)
		{
			throw new ApiException<ErrorResponse>("Could not deserialize the response body stream as " + typeof(T).FullName + ".", (int)response.StatusCode, string.Empty, headers, innerException);
		}
	}

	private string ConvertToString(object value, CultureInfo cultureInfo)
	{
		if (value == null)
		{
			return "";
		}
		if (value is Enum)
		{
			string name = Enum.GetName(value.GetType(), value);
			if (name != null)
			{
				FieldInfo declaredField = value.GetType().GetTypeInfo().GetDeclaredField(name);
				if (declaredField != null && declaredField.GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute enumMemberAttribute)
				{
					if (enumMemberAttribute.Value == null)
					{
						return name;
					}
					return enumMemberAttribute.Value;
				}
				string text = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
				if (text != null)
				{
					return text;
				}
				return string.Empty;
			}
		}
		else
		{
			if (value is bool)
			{
				return Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
			}
			if (value is byte[])
			{
				return Convert.ToBase64String((byte[])value);
			}
			if (value.GetType().IsArray)
			{
				IEnumerable<object> source = ((Array)value).OfType<object>();
				return string.Join(",", source.Select((object o) => ConvertToString(o, cultureInfo)));
			}
		}
		string text2 = Convert.ToString(value, cultureInfo);
		if (text2 != null)
		{
			return text2;
		}
		return "";
	}

	private byte[] Compress(string uncompressedString)
	{
		using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString));
		using MemoryStream memoryStream2 = new MemoryStream();
		using (GZipStream destination = new GZipStream(memoryStream2, CompressionMode.Compress))
		{
			memoryStream.CopyTo(destination);
		}
		return memoryStream2.ToArray();
	}

	private bool ContentIsGzipEncoded(IReadOnlyDictionary<string, IEnumerable<string>> headers)
	{
		bool result = false;
		if (headers.TryGetValue("Content-Encoding", out var value) || headers.TryGetValue("content-encoding", out value))
		{
			foreach (string item in value)
			{
				result = item == "gzip";
			}
		}
		return result;
	}
}
