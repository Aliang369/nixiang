using System;
using UnityEngine;

public static class VectorExtends
{
	public class Ref<T>
	{
		private readonly Func<T> getter;

		private readonly Action<T> setter;

		public T Value
		{
			get
			{
				return getter();
			}
			set
			{
				setter(value);
			}
		}

		public Ref(Func<T> getter, Action<T> setter)
		{
			this.getter = getter;
			this.setter = setter;
		}
	}

	public class Vector3Ref : Ref<Vector3>
	{
		public float x
		{
			get
			{
				return base.Value.x;
			}
			set
			{
				Vector3 value2 = base.Value;
				value2.x = value;
				base.Value = value2;
			}
		}

		public float y
		{
			get
			{
				return base.Value.y;
			}
			set
			{
				Vector3 value2 = base.Value;
				value2.y = value;
				base.Value = value2;
			}
		}

		public float z
		{
			get
			{
				return base.Value.z;
			}
			set
			{
				Vector3 value2 = base.Value;
				value2.z = value;
				base.Value = value2;
			}
		}

		public Vector3Ref(Func<Vector3> getter, Action<Vector3> setter)
			: base(getter, setter)
		{
		}
	}

	public class Vector2Ref : Ref<Vector2>
	{
		public float x
		{
			get
			{
				return base.Value.x;
			}
			set
			{
				Vector2 value2 = base.Value;
				value2.x = value;
				base.Value = value2;
			}
		}

		public float y
		{
			get
			{
				return base.Value.y;
			}
			set
			{
				Vector2 value2 = base.Value;
				value2.y = value;
				base.Value = value2;
			}
		}

		public Vector2Ref(Func<Vector2> getter, Action<Vector2> setter)
			: base(getter, setter)
		{
		}
	}

	public class Vector2Pointer
	{
		private unsafe Vector2* pVector2;

		public Vector2Pointer(ref Vector2 vector2)
		{
		}
	}

	public static Vector2 AsVector2(this Vector3 vector3)
	{
		return new Vector2(vector3.x, vector3.y);
	}

	public static Vector3 AsVector3(this Vector2 vector2)
	{
		return new Vector3(vector2.x, vector2.y, 0f);
	}

	public static Vector3 GetWith(this Vector3 vector3, float? x = null, float? y = null, float? z = null)
	{
		if (x.HasValue)
		{
			vector3.x = x.Value;
		}
		if (y.HasValue)
		{
			vector3.y = y.Value;
		}
		if (z.HasValue)
		{
			vector3.z = z.Value;
		}
		return vector3;
	}

	public static Vector3? GetWith(this Vector3? vector3, float? x = null, float? y = null, float? z = null)
	{
		if (!vector3.HasValue)
		{
			return null;
		}
		Vector3 value = vector3.Value;
		if (x.HasValue)
		{
			value.x = x.Value;
		}
		if (y.HasValue)
		{
			value.y = y.Value;
		}
		if (z.HasValue)
		{
			value.z = z.Value;
		}
		return value;
	}

	public static void Set(this ref Vector3 vector3, float? x = null, float? y = null, float? z = null)
	{
		if (x.HasValue)
		{
			vector3.x = x.Value;
		}
		if (y.HasValue)
		{
			vector3.y = y.Value;
		}
		if (z.HasValue)
		{
			vector3.z = z.Value;
		}
	}

	public static void Set(this ref Vector2 vector3, float? x = null, float? y = null)
	{
		if (x.HasValue)
		{
			vector3.x = x.Value;
		}
		if (y.HasValue)
		{
			vector3.y = y.Value;
		}
	}

	public static Vector3 Multiply(this Vector3 v, Vector3 v2)
	{
		return new Vector3(v.x * v2.x, v.y * v2.y, v.z * v2.z);
	}

	public static Vector3 Divide(this Vector3 v, Vector3 v2)
	{
		return new Vector3(v.x / v2.x, v.y / v2.y, v.z / v2.z);
	}

	public static bool NullOrZero(this Vector3? v)
	{
		if (!v.HasValue || v == Vector3.zero)
		{
			return true;
		}
		return false;
	}
}
