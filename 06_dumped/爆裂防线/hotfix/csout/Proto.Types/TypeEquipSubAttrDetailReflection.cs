using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSubAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSubAttrDetailReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkVxdWlwU3ViQXR0ckRldGFpbC5wcm90bxIFdHlwZXMaFGVudW0u" + "RUF0dHJUeXBlLnByb3RvGhVlbnVtLkVCb29sU3RhdGUucHJvdG8iugEKEkVx" + "dWlwU3ViQXR0ckRldGFpbBImCghBdHRyVHlwZRgBIAEoDjIULkVBdHRyVHlw" + "ZS5FQXR0clR5cGUSDgoGRnVuY0lkGAIgASgFEg4KBkF0dHJJZBgDIAEoBRIN" + "CgVWYWx1ZRgEIAEoAhIkCgRFeGNlGAUgASgOMhYuRUJvb2xTdGF0ZS5FQm9v" + "bFN0YXRlEicKB1Nob3dQY3QYBiABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3Rh" + "dGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEAttrTypeReflection.Descriptor,
			EnumEBoolStateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSubAttrDetail), (MessageParser)(object)EquipSubAttrDetail.Parser, new string[6] { "AttrType", "FuncId", "AttrId", "Value", "Exce", "ShowPct" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
