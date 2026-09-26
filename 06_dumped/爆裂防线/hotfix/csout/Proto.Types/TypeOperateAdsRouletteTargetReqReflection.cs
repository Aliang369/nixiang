using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsRouletteTargetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsRouletteTargetReqReflection()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVBZHNSb3VsZXR0ZVRhcmdldFJlcS5wcm90bxIFdHlw" + "ZXMiKwobT3BlcmF0ZUFkc1JvdWxldHRlVGFyZ2V0UmVxEgwKBFVVSUQYASAB" + "KAlCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsRouletteTargetReq), (MessageParser)(object)OperateAdsRouletteTargetReq.Parser, new string[1] { "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
