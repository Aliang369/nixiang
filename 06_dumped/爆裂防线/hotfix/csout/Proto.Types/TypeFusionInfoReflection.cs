using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFusionInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFusionInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkZ1c2lvbkluZm8ucHJvdG8SBXR5cGVzGhl0eXBlLkZ1c2lvbkJh" + "c2VJbmZvLnByb3RvGhp0eXBlLk9wZXJhdGVCYXNlSW5mby5wcm90byJhCgpG" + "dXNpb25JbmZvEigKCEJhc2VJbmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJh" + "c2VJbmZvEikKCkZ1c2lvbkJhc2UYAiABKAsyFS50eXBlcy5GdXNpb25CYXNl" + "SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeFusionBaseInfoReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FusionInfo), (MessageParser)(object)FusionInfo.Parser, new string[2] { "BaseInfo", "FusionBase" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
