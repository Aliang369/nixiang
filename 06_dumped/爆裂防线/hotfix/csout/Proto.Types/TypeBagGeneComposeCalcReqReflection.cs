using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBagGeneComposeCalcReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBagGeneComposeCalcReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkJhZ0dlbmVDb21wb3NlQ2FsY1JlcS5wcm90bxIFdHlwZXMaFWVu" + "dW0uRUdlbmVTaGFwZS5wcm90bxoUZW51bS5FR2VuZVR5cGUucHJvdG8iagoV" + "QmFnR2VuZUNvbXBvc2VDYWxjUmVxEiYKCEdlbmVUeXBlGAEgASgOMhQuRUdl" + "bmVUeXBlLkVHZW5lVHlwZRIpCglHZW5lU2hhcGUYAiABKA4yFi5FR2VuZVNo" + "YXBlLkVHZW5lU2hhcGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEGeneShapeReflection.Descriptor,
			EnumEGeneTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BagGeneComposeCalcReq), (MessageParser)(object)BagGeneComposeCalcReq.Parser, new string[2] { "GeneType", "GeneShape" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
