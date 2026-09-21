using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UnityHelper
{
	public static string FullPath(this Transform transform)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (transform == null)
		{
			return stringBuilder.ToString();
		}
		stringBuilder.Append(transform.name);
		Transform parent = transform.parent;
		while (parent != null)
		{
			stringBuilder.Insert(0, "/");
			stringBuilder.Insert(0, parent.name);
			parent = parent.parent;
		}
		return stringBuilder.ToString();
	}

	public static string FullPath(this GameObject gameObject)
	{
		if (gameObject == null)
		{
			return "";
		}
		return gameObject.transform.FullPath();
	}

	public static T[] GetComponentsInChildrenWithoutSelf<T>(this Component cmp, bool includeInactive = false) where T : Component
	{
		return Array.FindAll(cmp.GetComponentsInChildren<T>(includeInactive), (T t) => t.transform != cmp.transform);
	}

	public static double RoundEx(double num, int para)
	{
		if (para < 0)
		{
			for (int i = 0; i < -para; i++)
			{
				num /= 10.0;
			}
			num = Math.Round(num, 0);
		}
		else
		{
			num = Math.Round(num, para);
		}
		if (para < 0)
		{
			for (int j = 0; j < -para; j++)
			{
				num *= 10.0;
			}
		}
		return num;
	}

	public static void MoveTargetRectToNeighborRectInAreaRect(RectTransform targetRect, RectTransform neighborRect, RectTransform areaRect)
	{
		if (!(targetRect == null) && !(neighborRect == null) && !(areaRect == null))
		{
			Canvas componentInParent = targetRect.GetComponentInParent<Canvas>();
			Canvas componentInParent2 = neighborRect.GetComponentInParent<Canvas>();
			Canvas componentInParent3 = areaRect.GetComponentInParent<Canvas>();
			Vector3[] array = new Vector3[4];
			areaRect.GetWorldCorners(array);
			for (int i = 0; i < 4; i++)
			{
				array[i] = array[i].WorldPositionFromCanvasToCanvas(componentInParent3, componentInParent);
			}
			Vector3 vector = array[0];
			Vector3 vector2 = array[2];
			Vector3[] array2 = new Vector3[4];
			neighborRect.GetWorldCorners(array2);
			Vector3 vector3 = neighborRect.position.WorldPositionFromCanvasToCanvas(componentInParent2, componentInParent);
			for (int j = 0; j < 4; j++)
			{
				array2[j] = array2[j].WorldPositionFromCanvasToCanvas(componentInParent2, componentInParent);
			}
			float num = (array2[0].x + array2[2].x) * 0.5f;
			float num2 = (vector.x + vector2.x) * 0.5f;
			float width = targetRect.GetWorldRect().width;
			Vector3 position = vector3;
			if (num < num2)
			{
				position.x = array2[2].x + width / 2f;
			}
			else
			{
				position.x = array2[0].x - width / 2f;
			}
			float height = targetRect.GetWorldRect().height;
			position.y = Mathf.Clamp(position.y, vector.y + height / 2f, vector2.y - height / 2f);
			targetRect.position = position;
		}
	}

	public static void TargetRectInAreaRect(RectTransform targetRect, RectTransform areaRect)
	{
		Vector3[] array = new Vector3[4];
		areaRect.GetWorldCorners(array);
		for (int i = 0; i < 4; i++)
		{
			array[i] = targetRect.WorldPositionToAnchored(array[i]);
		}
		Vector3 vector = array[0];
		Vector3 vector2 = array[2];
		Vector2 anchoredPosition = targetRect.anchoredPosition;
		float width = targetRect.GetWorldRect().width;
		Vector2 anchoredPosition2 = anchoredPosition;
		if (anchoredPosition2.x + width / 2f > vector2.x)
		{
			anchoredPosition2.x = vector2.x - width / 2f;
		}
		if (anchoredPosition2.x - width / 2f < vector.x)
		{
			anchoredPosition2.x = vector.x + width / 2f;
		}
		float height = targetRect.GetWorldRect().height;
		anchoredPosition2.y = Mathf.Clamp(anchoredPosition2.y, vector.y + height / 2f, vector2.y - height / 2f);
		targetRect.anchoredPosition = anchoredPosition2;
	}

	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static void ProperDestroy(UnityEngine.Object target)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(target);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(target);
		}
	}

	public static bool CheckSameSignature(MethodInfo a, MethodInfo b)
	{
		if (a.ReturnType != b.ReturnType)
		{
			return false;
		}
		ParameterInfo[] parameters = a.GetParameters();
		ParameterInfo[] parameters2 = b.GetParameters();
		if (parameters.Length != parameters2.Length)
		{
			return false;
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].ParameterType != parameters2[i].ParameterType)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}

	public static void PassPointerClick(this MonoBehaviour monoBehaviour, PointerEventData eventData)
	{
		Transform parent = monoBehaviour.transform.parent;
		while (parent != null)
		{
			IPointerClickHandler component = parent.GetComponent<IPointerClickHandler>();
			if (component != null)
			{
				component.OnPointerClick(eventData);
				break;
			}
			parent = parent.parent;
		}
	}

	public static T MinBy<T>(this IEnumerable<T> elements, Func<T, int> selector)
	{
		T val = default(T);
		bool flag = true;
		foreach (T element in elements)
		{
			if (flag || selector(element) < selector(val))
			{
				val = element;
				flag = false;
			}
		}
		return val;
	}

	public static T MaxBy<T>(this IEnumerable<T> elements, Func<T, int> selector)
	{
		T val = default(T);
		bool flag = true;
		foreach (T element in elements)
		{
			if (flag || selector(element) > selector(val))
			{
				val = element;
				flag = false;
			}
		}
		return val;
	}

	public static T DequeueOrDefault<T>(this Queue<T> queue)
	{
		if (queue.Count == 0)
		{
			return default(T);
		}
		return queue.Dequeue();
	}

	public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
	{
		return source ?? Enumerable.Empty<T>();
	}

	public static T FindWithIndex<T>(this IEnumerable<T> list, Predicate<T> match, int index)
	{
		IEnumerable<T> source = list.Where((T a) => match(a));
		if (index < 0 || index >= source.Count())
		{
			return default(T);
		}
		return source.ElementAt(index);
	}

	public static T IndexOrNull<T>(this IList<T> source, int index)
	{
		if (source == null || index < 0 || index >= source.Count)
		{
			return default(T);
		}
		return source[index];
	}

	public static T CyclicIndex<T>(this IList<T> source, int index)
	{
		if (source.Count == 0)
		{
			return default(T);
		}
		if (index < 0)
		{
			index = source.Count + index % source.Count;
		}
		return source[index % source.Count];
	}

	public static int HamiltonDistanceTo(this Vector2Int v1, Vector2Int other)
	{
		return v1.VerticalDistanceTo(other) + v1.HorizantalDistanceTo(other);
	}

	public static int VerticalDistanceTo(this Vector2Int v1, Vector2Int other)
	{
		return Mathf.Abs(v1.y - other.y);
	}

	public static int HorizantalDistanceTo(this Vector2Int v1, Vector2Int other)
	{
		return Mathf.Abs(v1.x - other.x);
	}

	public static IEnumerable<Type> GetAllSubclassInAssembly<T>()
	{
		Type type = typeof(T);
		return from p in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly s) => s.GetTypes())
			where type.IsAssignableFrom(p) && p != type
			select p;
	}

	public static void SetRange<T>(this List<T> list, int startIndex, int endIndex, T item)
	{
		if (list != null && startIndex >= 0 && startIndex < list.Count && endIndex >= 0 && endIndex <= list.Count && endIndex >= startIndex)
		{
			for (int i = startIndex; i < endIndex; i++)
			{
				list[i] = item;
			}
		}
	}

	public static T GetLast<T>(this List<T> list)
	{
		if (list == null)
		{
			return default(T);
		}
		return list[list.Count - 1];
	}

	public static T FindOrDefault<T>(this List<T> list, Predicate<T> match, T defaultValue)
	{
		int num = list.FindIndex(match);
		if (num == -1)
		{
			return defaultValue;
		}
		return list[num];
	}

	public static void AddWithoutDuplicate<T>(this List<T> list, T ele)
	{
		if (!list.Contains(ele))
		{
			list.Add(ele);
		}
	}

	public static void GetFieldWithAttributeNested(object target, Type attributeType, HashSet<object> map)
	{
		if (target == null)
		{
			return;
		}
		map.Add(target);
		FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetCustomAttribute(attributeType) != null)
			{
				object value = fieldInfo.GetValue(target);
				if (!map.Contains(value))
				{
					GetFieldWithAttributeNested(value, attributeType, map);
				}
			}
		}
	}

	public static void AutoFitLayoutXSpacing(this GridLayoutGroup layout, int maxLine, float maxSpacingX = 0f)
	{
		float num = 0f;
		int num2 = 0;
		foreach (RectTransform layoutActiveChild in layout.GetLayoutActiveChildren())
		{
			_ = layoutActiveChild;
			num2++;
			num += layout.cellSize.x + maxSpacingX;
		}
		int num3 = layout.padding.left + layout.padding.right;
		float num4 = (layout.GetComponent<RectTransform>().GetWidth() - (float)num3) * (float)maxLine;
		if (num > num4 && num2 > maxLine)
		{
			layout.spacing = new Vector2(maxSpacingX - (num - num4) / (float)(num2 - maxLine), layout.spacing.y);
		}
		else
		{
			layout.spacing = new Vector2(maxSpacingX, layout.spacing.x);
		}
	}

	public static void AutoFitLayoutSpacing(this VerticalLayoutGroup layout, float maxSpacing = 0f)
	{
		float num = 0f;
		int num2 = 0;
		foreach (RectTransform layoutActiveChild in layout.GetLayoutActiveChildren())
		{
			num2++;
			num += layoutActiveChild.GetHeight() + maxSpacing;
		}
		int num3 = layout.padding.top + layout.padding.bottom;
		float num4 = layout.GetComponent<RectTransform>().GetHeight() - (float)num3;
		if (num > num4 && num2 > 1)
		{
			layout.spacing = maxSpacing - (num - num4) / (float)(num2 - 1);
		}
		else
		{
			layout.spacing = maxSpacing;
		}
	}

	public static void AutoFitLayoutSpacing(this HorizontalLayoutGroup layout, float maxSpacing = 0f)
	{
		float num = 0f;
		int num2 = 0;
		foreach (RectTransform layoutActiveChild in layout.GetLayoutActiveChildren())
		{
			num2++;
			num += layoutActiveChild.GetWidth() + maxSpacing;
		}
		int num3 = layout.padding.left + layout.padding.right;
		float num4 = layout.GetComponent<RectTransform>().GetWidth() - (float)num3;
		if (num > num4 && num2 > 1)
		{
			layout.spacing = maxSpacing - (num - num4) / (float)(num2 - 1);
		}
		else
		{
			layout.spacing = maxSpacing;
		}
	}

	private static List<RectTransform> GetLayoutActiveChildren(this LayoutGroup layout)
	{
		List<RectTransform> list = new List<RectTransform>();
		for (int i = 0; i < layout.transform.childCount; i++)
		{
			RectTransform component = layout.transform.GetChild(i).GetComponent<RectTransform>();
			if (component != null && component.gameObject.activeInHierarchy && !IsIgnoreLayout(component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	private static bool IsIgnoreLayout(RectTransform rect)
	{
		return rect.GetComponent<ILayoutIgnorer>()?.ignoreLayout ?? false;
	}

	public static IEnumerable<FieldInfo> GetAllSerializeFields(object target, Func<FieldInfo, bool> filter = null, Type currentType = null)
	{
		if (currentType == null)
		{
			currentType = target.GetType();
		}
		FieldInfo[] fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if ((filter == null || filter(fieldInfo)) && (fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null))
			{
				yield return fieldInfo;
			}
		}
		if (!(currentType.BaseType != null))
		{
			yield break;
		}
		foreach (FieldInfo allSerializeField in GetAllSerializeFields(target, filter, currentType.BaseType))
		{
			yield return allSerializeField;
		}
	}

	public static List<T> RiffleShuffle<T>(List<T> a, List<T> b)
	{
		List<T> list = new List<T>();
		int num = 0;
		int num2 = 0;
		while (num < a.Count && num2 < b.Count)
		{
			if (num < a.Count)
			{
				list.Add(a[num]);
				num++;
			}
			if (num2 < a.Count)
			{
				list.Add(b[num2]);
				num2++;
			}
		}
		return list;
	}

	public static string FormatWith(this string str, params object[] args)
	{
		return string.Format(str, args);
	}

	public static Color HexToColor(this string hex)
	{
		hex = hex.TrimStart('#');
		if (hex.Length != 6)
		{
			throw new ArgumentException("Hex color must be 6 characters long.");
		}
		int num = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
		int num2 = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		return new Color(b: (float)int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / 255f, r: (float)num / 255f, g: (float)num2 / 255f);
	}

	public static Color Mul(this Color color, float c)
	{
		return color * c;
	}

	public static Color a(this Color color, float a)
	{
		color.a = a;
		return color;
	}

	public static void BuildToList<T, O>(this ICollection<O> updateInfo, ref List<T> origin, Func<O, T, bool> existedChecker, Func<O, T> creater)
	{
		if (origin == null)
		{
			origin = new List<T>();
		}
		foreach (T item in origin.ToList())
		{
			if (item is IValidate validate && !validate.IsValidate())
			{
				origin.Remove(item);
			}
		}
		foreach (O o in updateInfo)
		{
			if (origin.Find((T t) => existedChecker(o, t)) == null)
			{
				origin.Add(creater(o));
			}
		}
	}
}
