using System;
using System.Collections.Generic;
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

public class DataOptimizationClient
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

	public DataOptimizationClient(string baseUrl, HttpClient httpClient)
	{
		BaseUrl = baseUrl;
		this.httpClient = httpClient;
		settings = new Lazy<JsonSerializerSettings>(CreateSerializerSettings);
	}

	private void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
	{
		ChillyRoomService.AddEssentialConverters(settings);
	}

	public Task<PlayerMigrationStatusDTO> CheckCrossRegionStatusAsync()
	{
		return CheckCrossRegionStatusAsync(CancellationToken.None);
	}

	public Task<PlayerMigrationStatusDTO> CheckCrossRegionStatusAsync(CancellationToken cancellationToken)
	{
		return SendAsync<PlayerMigrationStatusDTO>("/DataOptimization/CheckCrossRegionStatus", HttpMethod.Get, cancellationToken);
	}

	public Task<PlayerMigrationResultDTO> ConfirmMigrationAsync()
	{
		return ConfirmMigrationAsync(CancellationToken.None);
	}

	public Task<PlayerMigrationResultDTO> ConfirmMigrationAsync(CancellationToken cancellationToken)
	{
		return SendAsync<PlayerMigrationResultDTO>("/DataOptimization/ConfirmMigration", HttpMethod.Post, cancellationToken);
	}

	private async Task<T> SendAsync<T>(string path, HttpMethod method, CancellationToken cancellationToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((BaseUrl != null) ? BaseUrl.TrimEnd('/') : string.Empty).Append(path);
		using HttpRequestMessage request = new HttpRequestMessage();
		request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
		request.Method = method;
		request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
		request.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
		string uriString = stringBuilder.ToString();
		request.RequestUri = new Uri(uriString, UriKind.RelativeOrAbsolute);
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			throw new ApiException<ErrorResponse>("Network Unreachable", -100, null, null, null);
		}
		HttpResponseMessage httpResponseMessage;
		try
		{
			httpResponseMessage = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is WebException || ex is TaskCanceledException)
		{
			throw new ApiException<ErrorResponse>("Error making api request", -1, null, null, ex);
		}
		using (httpResponseMessage)
		{
			Dictionary<string, IEnumerable<string>> headers = CollectHeaders(httpResponseMessage);
			int status = (int)httpResponseMessage.StatusCode;
			try
			{
				switch (status)
				{
				case 200:
				{
					ObjectResponseResult<T> objectResponseResult2 = await ReadObjectResponseAsync<T>(httpResponseMessage, headers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (objectResponseResult2.Object == null)
					{
						throw new ApiException<ErrorResponse>("Response was null which was not expected.", status, objectResponseResult2.Text, headers, null);
					}
					return objectResponseResult2.Object;
				}
				case 400:
				case 401:
				{
					ObjectResponseResult<ErrorResponse> objectResponseResult = await ReadObjectResponseAsync<ErrorResponse>(httpResponseMessage, headers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					throw new ApiException<ErrorResponse>((status == 400) ? "Bad Request" : "Unauthorized", status, objectResponseResult.Text, headers, objectResponseResult.Object, null);
				}
				default:
				{
					string text = ((httpResponseMessage.Content != null) ? (await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)) : null);
					string response = text;
					throw new ApiException<ErrorResponse>("The HTTP status code of the response was not expected (" + status + ").", status, response, headers, null);
				}
				}
			}
			catch (ApiException<ErrorResponse>)
			{
				throw;
			}
		}
	}

	private JsonSerializerSettings CreateSerializerSettings()
	{
		JsonSerializerSettings result = new JsonSerializerSettings();
		UpdateJsonSerializerSettings(result);
		return result;
	}

	private async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
	{
		if (response?.Content == null)
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

	private static Dictionary<string, IEnumerable<string>> CollectHeaders(HttpResponseMessage response)
	{
		Dictionary<string, IEnumerable<string>> dictionary = new Dictionary<string, IEnumerable<string>>();
		foreach (KeyValuePair<string, IEnumerable<string>> item in (HttpHeaders)response.Headers)
		{
			dictionary[item.Key] = item.Value;
		}
		if (response.Content?.Headers == null)
		{
			return dictionary;
		}
		foreach (KeyValuePair<string, IEnumerable<string>> item2 in (HttpHeaders)response.Content.Headers)
		{
			dictionary[item2.Key] = item2.Value;
		}
		return dictionary;
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
}
