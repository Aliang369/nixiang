using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeGeneSlotDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGeneSlotDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkdlbmVTbG90RGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5FR2Vu" + "ZVNoYXBlLnByb3RvIlsKDkdlbmVTbG90RGV0YWlsEg0KBUluZGV4GAEgASgF" + "EikKCUdlbmVTaGFwZRgCIAEoDjIWLkVHZW5lU2hhcGUuRUdlbmVTaGFwZRIP" + "CgdHZW5lVGlkGAMgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEGeneShapeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GeneSlotDetail), (MessageParser)(object)GeneSlotDetail.Parser, new string[3] { "Index", "GeneShape", "GeneTid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
