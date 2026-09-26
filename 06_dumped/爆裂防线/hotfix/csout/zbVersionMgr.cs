using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class zbVersionMgr
{
	private static string m_strCurVersion = "v1.0";

	private static bool m_bCurVersionLoaded = false;

	private static Version m_verCurCache;

	private static string m_strCurVersionCacheKey;

	private static readonly HashSet<string> s_warnedBadVersions = new HashSet<string>();

	public static string GetCurVersion()
	{
		JObject record = zbDataConfig.Instance.gameBaseJsonTable.GetRecord();
		object obj;
		if (record == null)
		{
			obj = null;
		}
		else
		{
			JToken obj2 = record["curVersion"];
			if (obj2 == null)
			{
				obj = null;
			}
			else
			{
				JToken obj3 = obj2[(object)"curVersion"];
				obj = ((obj3 != null) ? Extensions.Value<string>((IEnumerable<JToken>)obj3) : null);
			}
		}
		if (obj == null)
		{
			obj = m_strCurVersion;
		}
		m_strCurVersion = (string)obj;
		m_bCurVersionLoaded = true;
		return m_strCurVersion;
	}

	public static bool IsReleased(string strTestVersion)
	{
		if (!m_bCurVersionLoaded)
		{
			GetCurVersion();
		}
		if (m_verCurCache == null || m_strCurVersionCacheKey != m_strCurVersion)
		{
			m_verCurCache = TryParseVersion(m_strCurVersion) ?? new Version(0, 0);
			m_strCurVersionCacheKey = m_strCurVersion;
		}
		Version version = TryParseVersion(strTestVersion);
		if (version == null)
		{
			if (s_warnedBadVersions.Add(strTestVersion ?? "<null>"))
			{
				Debug.LogWarning((object)("[zbVersionMgr] 配置表版本号非法: \"" + strTestVersion + "\"（当前版本 " + m_strCurVersion + "），该记录按未发布处理"));
			}
			return false;
		}
		return version <= m_verCurCache;
	}

	private static Version TryParseVersion(string strVersion)
	{
		if (string.IsNullOrEmpty(strVersion))
		{
			return null;
		}
		Version.TryParse(strVersion.Trim().TrimStart(new char[2] { 'v', 'V' }), out Version result);
		return result;
	}
}
