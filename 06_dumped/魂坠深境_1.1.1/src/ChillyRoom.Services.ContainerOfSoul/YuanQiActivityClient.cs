using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChillyRoom.Services.Core.Errors;
using Generated.ChillyRoomSdkClient;
using Newtonsoft.Json;
using UnityEngine;

namespace ChillyRoom.Services.ContainerOfSoul;

public class YuanQiActivityClient
{
	private readonly struct ObjectResponseResult<T>(T responseObject, string responseText)
	{
		public T Object { get; } = responseObject;

		public string Text { get; } = responseText;
	}

	private string baseUrl = "";

	private readonly HttpClient httpClient;

	private readonly Lazy<JsonSerializerSettings> settings;

	public string BaseUrl
	{
		get
		{
			return baseUrl;
		}
		set
		{
			baseUrl = value;
		}
	}

	public bool EnableRequestCompression { get; set; }

	public YuanQiActivityClient(string baseUrl, HttpClient httpClient)
	{
		BaseUrl = baseUrl;
		this.httpClient = httpClient;
		settings = new Lazy<JsonSerializerSettings>(CreateSerializerSettings);
	}

	private void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
	{
		ChillyRoomService.AddEssentialConverters(settings);
	}

	public Task<GetYuanQiActivityStateResponseDto> GetActivityStateAsync(GetYuanQiActivityStateRequestDto body)
	{
		return GetActivityStateAsync(body, CancellationToken.None);
	}

	public Task<GetYuanQiActivityStateResponseDto> GetActivityStateAsync(GetYuanQiActivityStateRequestDto body, CancellationToken cancellationToken)
	{
		return SendAsync<GetYuanQiActivityStateResponseDto>("/YuanQiActivity/GetActivityState", body, cancellationToken);
	}

	public Task<ReportYuanQiActivityMissionReachedResponseDto> ReportMissionReachedAsync(ReportYuanQiActivityMissionReachedRequestDto body)
	{
		return ReportMissionReachedAsync(body, CancellationToken.None);
	}

	public Task<ReportYuanQiActivityMissionReachedResponseDto> ReportMissionReachedAsync(ReportYuanQiActivityMissionReachedRequestDto body, CancellationToken cancellationToken)
	{
		return SendAsync<ReportYuanQiActivityMissionReachedResponseDto>("/YuanQiActivity/ReportMissionReached", body, cancellationToken);
	}

	public Task<ClaimYuanQiActivityMissionRewardResponseDto> ClaimMissionRewardAsync(ClaimYuanQiActivityMissionRewardRequestDto body)
	{
		return ClaimMissionRewardAsync(body, CancellationToken.None);
	}

	public Task<ClaimYuanQiActivityMissionRewardResponseDto> ClaimMissionRewardAsync(ClaimYuanQiActivityMissionRewardRequestDto body, CancellationToken cancellationToken)
	{
		return SendAsync<ClaimYuanQiActivityMissionRewardResponseDto>("/YuanQiActivity/ClaimMissionReward", body, cancellationToken);
	}

	private JsonSerializerSettings CreateSerializerSettings()
	{
		JsonSerializerSettings result = new JsonSerializerSettings();
		UpdateJsonSerializerSettings(result);
		return result;
	}

	private async Task<TResponse> SendAsync<TResponse>(string path, object body, CancellationToken cancellationToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((BaseUrl != null) ? BaseUrl.TrimEnd('/') : "").Append(path);
		using HttpRequestMessage request = new HttpRequestMessage();
		string text = JsonConvert.SerializeObject(body, settings.Value);
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
		request.Content = httpContent;
		request.Method = HttpMethod.Post;
		request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
		request.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
		string uriString = stringBuilder.ToString();
		request.RequestUri = new Uri(uriString, UriKind.RelativeOrAbsolute);
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			throw new ApiException<ErrorResponse>("Network Unreachable", -100, null, null, null);
		}
		HttpResponseMessage response;
		try
		{
			response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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
			Dictionary<string, IEnumerable<string>> headers = CollectHeaders(response);
			int status = (int)response.StatusCode;
			switch (status)
			{
			case 200:
			{
				ObjectResponseResult<TResponse> objectResponseResult2 = await ReadObjectResponseAsync<TResponse>(response, headers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (objectResponseResult2.Object == null)
				{
					throw new ApiException<ErrorResponse>("Response was null which was not expected.", status, objectResponseResult2.Text, headers, null);
				}
				return objectResponseResult2.Object;
			}
			case 400:
			case 401:
			{
				ObjectResponseResult<ErrorResponse> objectResponseResult = await ReadObjectResponseAsync<ErrorResponse>(response, headers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (objectResponseResult.Object == null)
				{
					throw new ApiException<ErrorResponse>("Response was null which was not expected.", status, objectResponseResult.Text, headers, null);
				}
				throw new ApiException<ErrorResponse>((status == 400) ? "Bad Request" : "Unauthorized", status, objectResponseResult.Text, headers, objectResponseResult.Object, null);
			}
			case 500:
			{
				string text2 = ((response.Content != null) ? (await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : string.Empty);
				string response3 = text2;
				throw new ApiException<ErrorResponse>("Server Error", status, response3, headers, null);
			}
			default:
			{
				string text2 = ((response.Content != null) ? (await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : null);
				string response2 = text2;
				throw new ApiException<ErrorResponse>($"The HTTP status code of the response was not expected ({status}).", status, response2, headers, null);
			}
			}
		}
		catch (ApiException<ErrorResponse>)
		{
			throw;
		}
		finally
		{
			response?.Dispose();
		}
	}

	private static Dictionary<string, IEnumerable<string>> CollectHeaders(HttpResponseMessage response)
	{
		Dictionary<string, IEnumerable<string>> dictionary = new Dictionary<string, IEnumerable<string>>();
		foreach (KeyValuePair<string, IEnumerable<string>> item in (HttpHeaders)response.Headers)
		{
			dictionary[item.Key] = item.Value;
		}
		if (response.Content == null || response.Content.Headers == null)
		{
			return dictionary;
		}
		foreach (KeyValuePair<string, IEnumerable<string>> item2 in (HttpHeaders)response.Content.Headers)
		{
			dictionary[item2.Key] = item2.Value;
		}
		return dictionary;
	}

	private async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
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
			T responseObject = JsonSerializer.Create(settings.Value).Deserialize<T>(reader2);
			return new ObjectResponseResult<T>(responseObject, string.Empty);
		}
		catch (JsonException innerException)
		{
			throw new ApiException<ErrorResponse>("Could not deserialize the response body stream as " + typeof(T).FullName + ".", (int)response.StatusCode, string.Empty, headers, innerException);
		}
	}

	private static byte[] Compress(string uncompressedString)
	{
		using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString));
		using MemoryStream memoryStream2 = new MemoryStream();
		using (GZipStream destination = new GZipStream(memoryStream2, CompressionMode.Compress))
		{
			memoryStream.CopyTo(destination);
		}
		return memoryStream2.ToArray();
	}

	private static bool ContentIsGzipEncoded(IReadOnlyDictionary<string, IEnumerable<string>> headers)
	{
		if (headers.TryGetValue("Content-Encoding", out var value) || headers.TryGetValue("content-encoding", out value))
		{
			foreach (string item in value)
			{
				if (item == "gzip")
				{
					return true;
				}
			}
		}
		return false;
	}

	private static string ConvertToString(object value, CultureInfo cultureInfo)
	{
		string text;
		if (value != null)
		{
			text = Convert.ToString(value, cultureInfo);
			if (text == null)
			{
				return string.Empty;
			}
		}
		else
		{
			text = string.Empty;
		}
		return text;
	}
}
