using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Generated.ChillyRoomSdkClient;
using UnityEngine;

public class LocalizationManager : ScriptableObjectSingleton<LocalizationManager>
{
	public class LocalizationData
	{
		public int previousVersion;

		public string appId;

		public string appSecret;

		public string sheetToken;

		public string sheetID;

		public List<DescriptionGenerator> generator;

		public string excelFolderPath => $"{Directory.GetParent(Application.dataPath)}/Localize";

		public string previousFilePath => excelFolderPath + "/" + previousFileName;

		public string previousFileName => $"Localization_v{previousVersion}.xlsx";

		public int currentVersion => previousVersion + 1;

		public string currentFilePath => excelFolderPath + "/" + currentFileName;

		public string currentFileName => $"Localization_v{currentVersion}.xlsx";

		public string diffFilePath => excelFolderPath + "/" + diffFileName;

		public string diffFileName => "Diff.xlsx";
	}

	public enum LanguageCode
	{
		Unset,
		ChineseSimplified,
		ChineseTraditional,
		English,
		French,
		German,
		Italian,
		Japanese,
		Korean,
		BrazilianPortuguese,
		Russian,
		LatinAmericanSpanish,
		Thai,
		Turkish,
		Vietnamese
	}

	public abstract class DescriptionGenerator
	{
		public class Data
		{
			public LocalizeTextAttribute attribute;

			public string sourceText;

			public object parentObject;

			public string parentPath;

			public TraverseLocalizeTermState state;
		}

		public virtual bool CanGenerate(Data data)
		{
			return true;
		}

		public virtual string GenerateDescription(Data data)
		{
			return string.Empty;
		}
	}

	public class ParentUnityObjectDescriptionGenerator : DescriptionGenerator
	{
		public override bool CanGenerate(Data data)
		{
			return data.state.lastVisitUnityObject != null;
		}

		public override string GenerateDescription(Data data)
		{
			return "(来自 " + AssetTypeToString(data.state.lastVisitUnityObject) + "-" + data.state.lastVisitUnityObject.name + " 的文本)";
		}

		public string AssetTypeToString(UnityEngine.Object target)
		{
			Type type = target.GetType();
			if (type == typeof(ItemDefinition))
			{
				return "卡牌";
			}
			if (type == typeof(EnchantDef))
			{
				return "附魔";
			}
			if (type == typeof(RuneDef))
			{
				return "印记";
			}
			if (type == typeof(SkillDef))
			{
				return "护符";
			}
			if (type == typeof(SkillWordDef))
			{
				return "护符词条";
			}
			if (type == typeof(DraftMap))
			{
				return "地图";
			}
			if (type == typeof(CharacterAIComponent))
			{
				return "敌人";
			}
			if (type == typeof(EnemyInfoData))
			{
				return "敌人";
			}
			if (type == typeof(KeywordDef))
			{
				return "关键字";
			}
			if (type == typeof(PlayerCharacterDef))
			{
				return "角色";
			}
			if (type == typeof(CharacterClassDef))
			{
				return "职业";
			}
			if (type == typeof(InfluenceDef))
			{
				return "祝福";
			}
			if (type == typeof(ItemTag))
			{
				return "卡牌标签";
			}
			if (type == typeof(ContractDef))
			{
				return "挑战契约";
			}
			if (type == typeof(TalentDef))
			{
				return "天赋";
			}
			if (type == typeof(AvatarTalkData))
			{
				return "对话";
			}
			return "资产";
		}
	}

	public class LocalizeAttributeDescriptionGenerator : DescriptionGenerator
	{
		public override bool CanGenerate(Data data)
		{
			return data.attribute != null;
		}

		public override string GenerateDescription(Data data)
		{
			string text = "(" + data.attribute.description + ")";
			if (text.Contains("描述"))
			{
				text += "(在句末一定要补上缺失的句号)";
			}
			return text;
		}
	}

	private static readonly Assembly CurrentAssembly = Assembly.GetAssembly(typeof(LocalizationManager));

	private static readonly Dictionary<Type, List<TraverseFieldMeta>> TraverseFieldMetaCache = new Dictionary<Type, List<TraverseFieldMeta>>();

	[HideInInspector]
	public static Action<LanguageCode> OnLanguageChange;

	[SerializeField]
	private LocalizationData localizationData;

	public bool enableEditorLocalization;

	public LanguageCode currentLanguage;

	public List<LanguageCode> localLanguages;

	public static Dictionary<SystemLanguage, LanguageCode> systemLanguageCodesDic = new Dictionary<SystemLanguage, LanguageCode>
	{
		{
			SystemLanguage.ChineseSimplified,
			LanguageCode.ChineseSimplified
		},
		{
			SystemLanguage.ChineseTraditional,
			LanguageCode.ChineseTraditional
		},
		{
			SystemLanguage.English,
			LanguageCode.English
		},
		{
			SystemLanguage.Portuguese,
			LanguageCode.BrazilianPortuguese
		},
		{
			SystemLanguage.Spanish,
			LanguageCode.LatinAmericanSpanish
		},
		{
			SystemLanguage.French,
			LanguageCode.French
		},
		{
			SystemLanguage.German,
			LanguageCode.German
		},
		{
			SystemLanguage.Italian,
			LanguageCode.Italian
		},
		{
			SystemLanguage.Japanese,
			LanguageCode.Japanese
		},
		{
			SystemLanguage.Russian,
			LanguageCode.Russian
		},
		{
			SystemLanguage.Thai,
			LanguageCode.Thai
		},
		{
			SystemLanguage.Turkish,
			LanguageCode.Turkish
		},
		{
			SystemLanguage.Vietnamese,
			LanguageCode.Vietnamese
		}
	};

	public static Dictionary<LanguageCode, string> cultureDic = new Dictionary<LanguageCode, string>
	{
		{
			LanguageCode.ChineseSimplified,
			"zh"
		},
		{
			LanguageCode.ChineseTraditional,
			"zh-TW"
		},
		{
			LanguageCode.English,
			"en"
		},
		{
			LanguageCode.BrazilianPortuguese,
			"pt-BR"
		},
		{
			LanguageCode.LatinAmericanSpanish,
			"es-LA"
		},
		{
			LanguageCode.French,
			"fr"
		},
		{
			LanguageCode.German,
			"de"
		},
		{
			LanguageCode.Italian,
			"it"
		},
		{
			LanguageCode.Japanese,
			"ja"
		},
		{
			LanguageCode.Korean,
			"ko"
		},
		{
			LanguageCode.Russian,
			"ru"
		},
		{
			LanguageCode.Thai,
			"th"
		},
		{
			LanguageCode.Turkish,
			"tr"
		},
		{
			LanguageCode.Vietnamese,
			"vi"
		}
	};

	public List<LocalizeTerm> localizationTerms;

	[NonSerialized]
	public Dictionary<string, LocalizeTerm> localizationTermCache;

	public static Dictionary<Type, TypeInfoCache> typeInfoCaches = new Dictionary<Type, TypeInfoCache>();

	public static bool assetCatalogDirty;

	public static TraverseLocalizeTermState assetCatalogState;

	public bool EnableLocalization
	{
		get
		{
			if (!Application.isEditor || enableEditorLocalization)
			{
				return GlobalConfigManager.instance.enableLocalization;
			}
			return false;
		}
	}

	public static SystemLanguage LanguageCodeToSystemLanguage(LanguageCode code)
	{
		if (code != LanguageCode.Korean)
		{
			return systemLanguageCodesDic.FirstOrDefault((KeyValuePair<SystemLanguage, LanguageCode> t) => t.Value == code).Key;
		}
		return SystemLanguage.Korean;
	}

	public static LanguageCode SystemLanguageToLanguageCode(SystemLanguage language)
	{
		return systemLanguageCodesDic.GetValueOrDefault(language, LanguageCode.ChineseSimplified);
	}

	public void SetLanguage(SystemLanguage language)
	{
		SetLanguage(SystemLanguageToLanguageCode(language));
	}

	public void SetLanguage(LanguageCode language)
	{
		if (!ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization)
		{
			TraceLog.Error("当前未启用本地化功能");
			return;
		}
		assetCatalogDirty = true;
		currentLanguage = language;
		SystemLanguage language2 = LanguageCodeToSystemLanguage(currentLanguage);
		SetRawLanguage(language);
		OnLanguageChange?.Invoke(language);
		if (GlobalConfigManager.instance.enableNetwork)
		{
			ChillyRoomService.GetSdk().SwitchCoreLanguage(language2);
		}
		ApplicationStateInfo applicationStateInfo = Bootstrap.instance?.CurrentApplicationState;
		if (applicationStateInfo != null)
		{
			applicationStateInfo.fortressState?.uiManager?.SetDirty();
			applicationStateInfo.runningGameState?.UIManager?.SetDirty();
		}
	}

	public static void SetRawLanguage(LanguageCode language)
	{
		SystemLanguage value = LanguageCodeToSystemLanguage(language);
		CommonConfig.Instance.currentLanguage = value;
		if (!cultureDic.TryGetValue(language, out var value2))
		{
			value2 = cultureDic[LanguageCode.English];
			TraceLog.Error($"cultureDic 未配置语言 {language}，回退到<英文>");
		}
		CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(value2);
	}

	public static TypeInfoCache GetTypeInfoCache(Type type)
	{
		typeInfoCaches.TryGetValue(type, out var value);
		if (value == null)
		{
			value = new TypeInfoCache(type);
		}
		typeInfoCaches[type] = value;
		return value;
	}

	public static string GetResourceRootId(ICTAsset target)
	{
		if (target == null)
		{
			return "";
		}
		if (target is CTScriptableObject cTScriptableObject)
		{
			return "#CTSO#" + cTScriptableObject.FullID;
		}
		if (target is CTPrefabAsset cTPrefabAsset)
		{
			return "#Prefab#" + cTPrefabAsset.FullId;
		}
		if (target is CTUIBaseAsset cTUIBaseAsset)
		{
			return "#CTUI#" + cTUIBaseAsset.FullId;
		}
		TypeInfoCache typeInfoCache = GetTypeInfoCache(target.GetType());
		if (typeInfoCache.isDerivedFromGenericParent)
		{
			int num = typeInfoCache.GetIndex(target);
			return $"#AssetPreData#{target.GetType().Name}#{num}";
		}
		if (target.GetType().BaseType.IsDerivedFromGenericParent(typeof(ScriptableObjectSingleton<>)))
		{
			return "#singleton#" + typeof(ScriptableObjectSingleton<>).Name + "#" + target.GetType().Name;
		}
		return "";
	}

	public static string GetString(string key)
	{
		return ScriptableObjectSingleton<LocalizationManager>.instance.localizationTermCache?.TryGetValue(key).BaseText ?? "";
	}

	public static string GetLanguageNameText(LanguageCode languageCode)
	{
		return languageCode switch
		{
			LanguageCode.ChineseSimplified => "简体中文", 
			LanguageCode.ChineseTraditional => "繁體中文", 
			LanguageCode.English => "English", 
			LanguageCode.French => "Français", 
			LanguageCode.German => "Deutsch", 
			LanguageCode.Italian => "Italiano", 
			LanguageCode.Japanese => "日本語", 
			LanguageCode.Korean => "한국어", 
			LanguageCode.BrazilianPortuguese => "Português", 
			LanguageCode.Russian => "Русский", 
			LanguageCode.LatinAmericanSpanish => "Español", 
			LanguageCode.Thai => "ไทย", 
			LanguageCode.Turkish => "Türkçe", 
			LanguageCode.Vietnamese => "Tiếng Việt", 
			_ => "", 
		};
	}

	private static List<TraverseFieldMeta> GetTraverseFieldMetas(object target)
	{
		if (target == null)
		{
			return new List<TraverseFieldMeta>();
		}
		Type type = target.GetType();
		if (TraverseFieldMetaCache.TryGetValue(type, out var value))
		{
			return value;
		}
		value = new List<TraverseFieldMeta>();
		foreach (FieldInfo allSerializeField in UnityHelper.GetAllSerializeFields(target))
		{
			Type fieldType = allSerializeField.FieldType;
			value.Add(new TraverseFieldMeta
			{
				field = allSerializeField,
				localizeAttr = allSerializeField.GetCustomAttribute<LocalizeTextAttribute>(),
				isString = (fieldType == typeof(string)),
				isPrimitive = fieldType.IsPrimitive,
				isCurrentAssemblyType = (fieldType.Assembly == CurrentAssembly),
				isList = fieldType.ImplementOrIs(typeof(IList)),
				isDictionary = fieldType.ImplementOrIs(typeof(IDictionary)),
				isCollection = fieldType.ImplementOrIs(typeof(ICollection)),
				isStringList = (fieldType.ImplementOrIs(typeof(IList)) && fieldType.IsGenericType && fieldType.GetGenericArguments()[0] == typeof(string))
			});
		}
		TraverseFieldMetaCache[type] = value;
		return value;
	}

	public static void LocalizeAssetCatalog()
	{
		if (!assetCatalogDirty)
		{
			return;
		}
		AssetCatalog assetCatalog = Bootstrap.instance?.assetCatalog;
		if (assetCatalog == null)
		{
			return;
		}
		TraverseLocalizeTermState state = new TraverseLocalizeTermState(TraverseLocalizeTermState.EType.Translate);
		foreach (ICTAsset allResourceAsset in assetCatalog.GetAllResourceAssets())
		{
			LocalizeTarget(allResourceAsset, state);
		}
		assetCatalogDirty = false;
	}

	public static void LocalizeTarget(ICTAsset target, TraverseLocalizeTermState state)
	{
		try
		{
			int count = state.terms.Count;
			TraverseLocalizedTerm(target, GetResourceRootId(target), state);
			for (int i = count; i < state.terms.Count; i++)
			{
				string text = state.terms[i].GetText(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
				state.objectFields[i]?.SetValue(text);
			}
		}
		catch (Exception ex)
		{
			TraceLog.Error($"[LocalizationManager] LocalizeTarget({target}) {ex.Message}] ");
		}
	}

	public static void TraverseLocalizedTerm(object target, string parentPath, TraverseLocalizeTermState state)
	{
		try
		{
			DoTraverse();
		}
		catch (Exception ex)
		{
			TraceLog.Error($"[LocalizationManager] TraverseLocalizedTerm({target}) - {parentPath} - {ex.Message}] ");
		}
		void ConfigListTerms(TraverseFieldMeta meta)
		{
			FieldInfo field = meta.field;
			LocalizeTextAttribute localizeAttr = meta.localizeAttr;
			if (localizeAttr != null && field.GetValue(target) is IList list)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is string text && !text.IsNullOrEmpty())
					{
						string key = parentPath + "." + field.Name + "[" + i + "]";
						LocalizeTerm localizeTerm = GetLocalizeTerm(key);
						if (localizeTerm == null)
						{
							localizeTerm = new LocalizeTerm
							{
								key = key,
								category = state.currentCategory
							};
						}
						if (state.type == TraverseLocalizeTermState.EType.Export)
						{
							string termDescription = GetTermDescription(new DescriptionGenerator.Data
							{
								attribute = localizeAttr,
								sourceText = text,
								parentObject = target,
								parentPath = parentPath,
								state = state
							});
							localizeTerm.description = termDescription;
						}
						if (state.type == TraverseLocalizeTermState.EType.Import || state.type == TraverseLocalizeTermState.EType.Export)
						{
							localizeTerm.SetText(LanguageCode.ChineseSimplified, text);
						}
						state.terms.Add(localizeTerm);
						state.objectFields.Add(new TraverseLocalizeTermBinding(list, i));
					}
				}
			}
		}
		void ConfigTerms(TraverseFieldMeta meta)
		{
			FieldInfo field = meta.field;
			LocalizeTextAttribute localizeAttr = meta.localizeAttr;
			if (localizeAttr != null)
			{
				string text = field.GetValue(target) as string;
				if (!text.IsNullOrEmpty())
				{
					string key = parentPath + "." + field.Name;
					LocalizeTerm localizeTerm = GetLocalizeTerm(key);
					if (localizeTerm == null)
					{
						localizeTerm = new LocalizeTerm
						{
							key = key,
							category = state.currentCategory
						};
					}
					if (state.type == TraverseLocalizeTermState.EType.Export)
					{
						string termDescription = GetTermDescription(new DescriptionGenerator.Data
						{
							attribute = localizeAttr,
							sourceText = text,
							parentObject = target,
							parentPath = parentPath,
							state = state
						});
						localizeTerm.description = termDescription;
					}
					if (state.type == TraverseLocalizeTermState.EType.Import || state.type == TraverseLocalizeTermState.EType.Export)
					{
						localizeTerm.SetText(LanguageCode.ChineseSimplified, text);
					}
					state.terms.Add(localizeTerm);
					state.objectFields.Add(new TraverseLocalizeTermBinding(target, field));
				}
			}
		}
		void DoTraverse()
		{
			if (!state.visitedObjects.Add(target))
			{
				return;
			}
			if (target is UnityEngine.Object lastVisitUnityObject)
			{
				state.lastVisitUnityObject = lastVisitUnityObject;
			}
			if (target is Cutscene cutscene)
			{
				int num = 0;
				foreach (UnityEngine.Object item2 in cutscene.GetAllAsset())
				{
					TraverseLocalizedTerm(item2, parentPath + $".assets.[{num}]", state);
					num++;
				}
			}
			foreach (TraverseFieldMeta traverseFieldMeta in GetTraverseFieldMetas(target))
			{
				FieldInfo field = traverseFieldMeta.field;
				if (traverseFieldMeta.isString)
				{
					if (traverseFieldMeta.localizeAttr != null)
					{
						if (state.type == TraverseLocalizeTermState.EType.Import || state.type == TraverseLocalizeTermState.EType.Export || state.type == TraverseLocalizeTermState.EType.Others)
						{
							ConfigTerms(traverseFieldMeta);
						}
						else
						{
							GetTerms(traverseFieldMeta);
						}
					}
				}
				else if (traverseFieldMeta.isStringList)
				{
					if (traverseFieldMeta.localizeAttr != null)
					{
						if (state.type == TraverseLocalizeTermState.EType.Import || state.type == TraverseLocalizeTermState.EType.Export || state.type == TraverseLocalizeTermState.EType.Others)
						{
							ConfigListTerms(traverseFieldMeta);
						}
						else
						{
							GetListTerms(traverseFieldMeta);
						}
					}
				}
				else
				{
					if (!traverseFieldMeta.isPrimitive && traverseFieldMeta.isCurrentAssemblyType)
					{
						object value = field.GetValue(target);
						if (IsValidSubObject(value))
						{
							TraverseLocalizedTerm(value, parentPath + "." + field.Name, state);
						}
					}
					if (traverseFieldMeta.isList && field.GetValue(target) is IList list)
					{
						for (int i = 0; i < list.Count; i++)
						{
							if (IsValidSubObject(list[i]))
							{
								TraverseLocalizedTerm(list[i], parentPath + "." + field.Name + "[" + i + "]", state);
							}
						}
					}
					else if (traverseFieldMeta.isDictionary && field.GetValue(target) is IDictionary dictionary)
					{
						int num2 = 0;
						foreach (object value2 in dictionary.Values)
						{
							if (IsValidSubObject(value2))
							{
								TraverseLocalizedTerm(value2, parentPath + "." + field.Name + "[" + num2 + "]", state);
							}
							num2++;
						}
					}
					else if (traverseFieldMeta.isCollection && field.GetValue(target) is ICollection collection)
					{
						int num3 = 0;
						foreach (object item3 in collection)
						{
							if (IsValidSubObject(item3))
							{
								TraverseLocalizedTerm(item3, parentPath + "." + field.Name + "[" + num3 + "]", state);
							}
							num3++;
						}
					}
				}
			}
		}
		void GetListTerms(TraverseFieldMeta meta)
		{
			FieldInfo field = meta.field;
			if (meta.localizeAttr != null && field.GetValue(target) is IList list)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is string str && !str.IsNullOrEmpty())
					{
						LocalizeTerm localizeTerm = GetLocalizeTerm(parentPath + "." + field.Name + "[" + i + "]");
						if (localizeTerm != null)
						{
							state.terms.Add(localizeTerm);
							state.objectFields.Add(new TraverseLocalizeTermBinding(list, i));
						}
					}
				}
			}
		}
		void GetTerms(TraverseFieldMeta meta)
		{
			FieldInfo field = meta.field;
			if (meta.localizeAttr != null && !(field.GetValue(target) as string).IsNullOrEmpty())
			{
				LocalizeTerm localizeTerm = GetLocalizeTerm(parentPath + "." + field.Name);
				if (localizeTerm != null)
				{
					state.terms.Add(localizeTerm);
					state.objectFields.Add(new TraverseLocalizeTermBinding(target, field));
				}
			}
		}
		bool IsValidSubObject(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType().Assembly != CurrentAssembly)
			{
				return false;
			}
			if (!(obj is UnityEngine.Object))
			{
				return true;
			}
			if (obj is MonoBehaviour item && target is MonoBehaviour monoBehaviour && monoBehaviour.GetComponentsInChildren<MonoBehaviour>(includeInactive: true).Contains(item))
			{
				return true;
			}
			return false;
		}
	}

	public static void InitCache()
	{
		LocalizationManager localizationManager = ScriptableObjectSingleton<LocalizationManager>.instance;
		Debug.Log($"[LocalizationManager] InitCache begin - asset={localizationManager.name}, resourceId={localizationManager.resourceId}, terms={localizationManager.localizationTerms?.Count ?? (-1)}, cacheBefore={localizationManager.localizationTermCache?.Count ?? (-1)}");
		localizationManager.localizationTermCache = new Dictionary<string, LocalizeTerm>();
		foreach (LocalizeTerm localizationTerm in localizationManager.localizationTerms)
		{
			localizationManager.localizationTermCache.TryAdd(localizationTerm.key, localizationTerm);
		}
		Debug.Log($"[LocalizationManager] InitCache end - asset={localizationManager.name}, resourceId={localizationManager.resourceId}, terms={localizationManager.localizationTerms.Count}, cacheAfter={localizationManager.localizationTermCache.Count}");
	}

	public static LocalizeTerm GetLocalizeTerm(string key)
	{
		if (ScriptableObjectSingleton<LocalizationManager>.instance?.localizationTermCache == null)
		{
			InitCache();
		}
		return ScriptableObjectSingleton<LocalizationManager>.instance.localizationTermCache?.TryGetValue(key);
	}

	public static string GetTermDescription(DescriptionGenerator.Data data)
	{
		string text = "";
		foreach (DescriptionGenerator item in ScriptableObjectSingleton<LocalizationManager>.instance.localizationData.generator)
		{
			if (item.CanGenerate(data))
			{
				text = text + "  " + item.GenerateDescription(data);
			}
		}
		return text;
	}
}
