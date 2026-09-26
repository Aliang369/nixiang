using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVipInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVipInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChJ0eXBlLlZpcEluZm8ucHJvdG8SBXR5cGVzGhV0eXBlLlZpcEV4cEluZm8u" + "cHJvdG8aGXR5cGUuVmlwRnJlZUV4cEluZm8ucHJvdG8ihAEKB1ZpcEluZm8S" + "JQoKVmlwRXhwSW5mbxgBIAEoCzIRLnR5cGVzLlZpcEV4cEluZm8SKQoKVmlw" + "RnJlZUV4cBgCIAEoCzIVLnR5cGVzLlZpcEZyZWVFeHBJbmZvEhQKDFZpcExl" + "dmVsUGFjaxgDIAMoBRIRCglEYWlseUZyZWUYBCABKAVCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeVipExpInfoReflection.Descriptor,
			TypeVipFreeExpInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VipInfo), (MessageParser)(object)VipInfo.Parser, new string[4] { "VipExpInfo", "VipFreeExp", "VipLevelPack", "DailyFree" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
