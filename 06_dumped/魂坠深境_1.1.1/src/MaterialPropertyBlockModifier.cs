using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class MaterialPropertyBlockModifier : SerializedMonoBehaviour
{
	public enum PropertyType
	{
		Buffer = 1,
		Color,
		ConstantBuffer,
		Float,
		FloatArray,
		Int,
		Matrix,
		MatrixArray,
		Texture,
		Vector,
		VectorArray
	}

	public abstract class PropertyBlockData
	{
		public string name;

		public abstract PropertyType type { get; }

		public abstract void SetData(MaterialPropertyBlock block);
	}

	public class BufferPropertyBlockData : PropertyBlockData
	{
		public ComputeBuffer computeBuffer;

		public override PropertyType type => PropertyType.Buffer;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetBuffer(name, computeBuffer);
		}
	}

	public class ColorPropertyBlockData : PropertyBlockData
	{
		public Color color;

		public override PropertyType type => PropertyType.Color;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetColor(name, color);
		}
	}

	public class ConstantBufferPropertyBlockData : PropertyBlockData
	{
		public ComputeBuffer computeBuffer;

		public int offset;

		public int size;

		public override PropertyType type => PropertyType.ConstantBuffer;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetConstantBuffer(name, computeBuffer, offset, size);
		}
	}

	public class FloatPropertyBlockData : PropertyBlockData
	{
		public float value;

		public override PropertyType type => PropertyType.Float;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetFloat(name, value);
		}
	}

	public class FloatArrayPropertyBlockData : PropertyBlockData
	{
		public List<float> values;

		public override PropertyType type => PropertyType.FloatArray;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetFloatArray(name, values);
		}
	}

	public class IntPropertyBlockData : PropertyBlockData
	{
		public int value;

		public override PropertyType type => PropertyType.Int;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetInt(name, value);
		}
	}

	public class MatrixPropertyBlockData : PropertyBlockData
	{
		public Matrix4x4 value;

		public override PropertyType type => PropertyType.Matrix;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetMatrix(name, value);
		}
	}

	public class MatrixArrayPropertyBlockData : PropertyBlockData
	{
		public List<Matrix4x4> values;

		public override PropertyType type => PropertyType.MatrixArray;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetMatrixArray(name, values);
		}
	}

	public class TexturePropertyBlockData : PropertyBlockData
	{
		public Texture value;

		public override PropertyType type => PropertyType.Texture;

		public override void SetData(MaterialPropertyBlock block)
		{
			if (value == null)
			{
				Debug.LogError(value);
			}
			else
			{
				block.SetTexture(name, value);
			}
		}
	}

	public class VectorPropertyBlockData : PropertyBlockData
	{
		public Vector4 value;

		public override PropertyType type => PropertyType.Vector;

		public override void SetData(MaterialPropertyBlock block)
		{
			block.SetVector(name, value);
		}
	}

	public class VectorArrayPropertyBlockData : PropertyBlockData
	{
		public List<Vector4> values;

		public override PropertyType type => PropertyType.VectorArray;

		public override void SetData(MaterialPropertyBlock block)
		{
			try
			{
				block.SetVectorArray(name, values);
			}
			catch
			{
			}
		}
	}

	public bool useOriginMaterialBlock = true;

	[SerializeField]
	public List<PropertyBlockData> propertyBlockDatas;

	private void Awake()
	{
		SetMaterialPropertyBlock();
	}

	private void SetMaterialPropertyBlock()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		Renderer component = GetComponent<Renderer>();
		if (useOriginMaterialBlock)
		{
			component.GetPropertyBlock(materialPropertyBlock);
		}
		if (!(component != null) || propertyBlockDatas == null)
		{
			return;
		}
		foreach (PropertyBlockData propertyBlockData in propertyBlockDatas)
		{
			propertyBlockData.SetData(materialPropertyBlock);
		}
		component.SetPropertyBlock(materialPropertyBlock);
	}
}
