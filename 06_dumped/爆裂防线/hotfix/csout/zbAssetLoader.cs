using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class zbAssetLoader
{
	private static zbAssetLoader sInstance;

	private string m_autoPatchPath = "";

	private string m_abDirectoryName = "assetbundle";

	private AssetBundle m_mainAB;

	private AssetBundleManifest m_mainifest;

	public Dictionary<string, AssetBundleInfo> m_abCache = new Dictionary<string, AssetBundleInfo>();

	private Dictionary<string, Sprite> m_combatSpriteCache = new Dictionary<string, Sprite>();

	public static zbAssetLoader Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbAssetLoader();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		m_autoPatchPath = PlayerPrefs.GetString("AutopatchPath");
		LoadMainAssetBundle();
	}

	private void LoadDependenciesAB(string strBundleName)
	{
		if ((Object)(object)m_mainifest == (Object)null)
		{
			return;
		}
		string[] allDependencies = m_mainifest.GetAllDependencies(strBundleName);
		for (int i = 0; i < allDependencies.Length; i++)
		{
			string text = allDependencies[i].ToLower();
			if (!m_abCache.TryGetValue(text, out var value))
			{
				LoadAssetBundle(text);
			}
			else
			{
				value.lastLoadTime = Time.time;
			}
		}
	}

	public AssetBundle LoadAssetBundle(string strBundleName)
	{
		LoadDependenciesAB(strBundleName);
		if (m_abCache.TryGetValue(strBundleName, out var value))
		{
			value.lastLoadTime = Time.time;
			return value.assetBundle;
		}
		string text = Path.Combine(Application.persistentDataPath, m_autoPatchPath, m_abDirectoryName, strBundleName);
		if (!File.Exists(text))
		{
			text = Path.Combine(Application.streamingAssetsPath, m_abDirectoryName, strBundleName);
		}
		AssetBundle val = AssetBundle.LoadFromFile(text, 0u, 16uL);
		if ((Object)(object)val != (Object)null)
		{
			m_abCache[strBundleName] = new AssetBundleInfo
			{
				assetBundle = val,
				lastLoadTime = Time.time
			};
		}
		return val;
	}

	public T LoadResouce<T>(string resName) where T : Object
	{
		if (string.IsNullOrEmpty(resName))
		{
			return default(T);
		}
		string bundleNameFromPath = GetBundleNameFromPath(resName);
		AssetBundle val = LoadAssetBundle(bundleNameFromPath);
		T val2 = (((Object)(object)val != (Object)null) ? val.LoadAsset<T>("Assets/" + resName) : default(T));
		if ((Object)(object)val2 == (Object)null)
		{
			val = LoadAssetBundle(bundleNameFromPath);
			val2 = (((Object)(object)val != (Object)null) ? val.LoadAsset<T>("Assets/" + resName) : default(T));
		}
		if ((Object)(object)val2 == (Object)null)
		{
			Debug.LogWarning((object)("LoadResouce faild: " + resName + " bundle=" + bundleNameFromPath));
		}
		return val2;
	}

	public void UnloadAssetBundle(string bundleName)
	{
		if (m_abCache.TryGetValue(bundleName, out var value))
		{
			value.assetBundle.Unload(false);
			m_abCache.Remove(bundleName);
		}
	}

	public string GetBundleNameFromPath(string resName)
	{
		string? directoryName = Path.GetDirectoryName(resName);
		string fileName = Path.GetFileName(directoryName);
		return (Path.Combine(directoryName, fileName) + ".ab").Replace("\\", "/").ToLower();
	}

	private void LoadMainAssetBundle()
	{
		if ((Object)(object)m_mainAB == (Object)null)
		{
			string path = Path.Combine(Application.persistentDataPath, m_autoPatchPath, "assetbundle");
			string text = "assetbundle";
			path = Path.Combine(path, text);
			if (!File.Exists(path))
			{
				path = Path.Combine(Application.streamingAssetsPath, "assetbundle", text);
			}
			m_mainAB = AssetBundle.LoadFromFile(path);
			if ((Object)(object)m_mainAB != (Object)null)
			{
				m_mainifest = m_mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			}
		}
	}

	public Sprite LoadSpriteAsset(string strAssetPath)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = this.LoadResouce<Texture2D>(strAssetPath);
		if (!((Object)(object)val != (Object)null))
		{
			return null;
		}
		return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f));
	}

	public Sprite LoadCombatSpriteAsset(string strAssetPath)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(strAssetPath))
		{
			return null;
		}
		if (m_combatSpriteCache.TryGetValue(strAssetPath, out var value))
		{
			return value;
		}
		Texture2D val = this.LoadResouce<Texture2D>(strAssetPath);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		Sprite val2 = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f));
		m_combatSpriteCache[strAssetPath] = val2;
		return val2;
	}

	public void LoadScene(string scenePath)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(scenePath);
		string bundleNameFromPath = GetBundleNameFromPath(scenePath);
		if ((Object)(object)LoadAssetBundle(bundleNameFromPath) != (Object)null)
		{
			SceneManager.LoadScene(fileNameWithoutExtension);
		}
	}

	public void ReleaseAssets()
	{
		ClearCombatSpriteCache();
		zbUtil.ClearSpineCache();
		Resources.UnloadUnusedAssets();
	}

	public void ClearCombatSpriteCache()
	{
		foreach (KeyValuePair<string, Sprite> item in m_combatSpriteCache)
		{
			Object.Destroy((Object)(object)item.Value);
		}
		m_combatSpriteCache.Clear();
	}
}
