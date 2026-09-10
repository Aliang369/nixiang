using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Containerofsoul
{
	private const string PatchAssetDirectoryName = "Asset";

	private const string PatchCatalogFileName = "catalog_1.json";

	private const string PatchCatalogHashFileName = "catalog_1.hash";

	public static async Task Main()
	{
		Debug.Log("Main Method");
		await MainAsync();
	}

	private static async Task MainAsync()
	{
		await UpdateAddressableCatalog();
		Addressables.LoadSceneAsync("Bootstrap");
	}

	private static string GetPatchAssetPath(string fileName)
	{
		return Path.Combine(Application.persistentDataPath, "Asset", fileName);
	}

	private static async Task<bool> UpdateAddressableCatalog()
	{
		AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(autoReleaseHandle: false);
		await checkHandle.Task;
		if (checkHandle.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogError("检查Catalog更新失败");
			return false;
		}
		int count = checkHandle.Result.Count;
		Debug.Log($"Addressable catalog update count: {count}");
		Addressables.Release(checkHandle);
		if (count > 0)
		{
			Debug.Log("检测到Addressable.Catalog更新");
			string path = Caching.currentCacheForWriting.path;
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(autoCleanBundleCache: true, null, autoReleaseHandle: false);
			await updateHandle.Task;
			if (updateHandle.Status != AsyncOperationStatus.Succeeded)
			{
				Addressables.Release(updateHandle);
				Debug.Log("更新Addressable.Catalog失败");
				return false;
			}
			Debug.Log("更新Addressable.Catalog成功");
			Addressables.Release(updateHandle);
		}
		else
		{
			Debug.Log("未能检测到Addressable.Catalog更新");
		}
		return true;
	}
}
