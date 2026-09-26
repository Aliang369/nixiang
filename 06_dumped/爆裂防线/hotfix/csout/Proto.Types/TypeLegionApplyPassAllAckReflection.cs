using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionApplyPassAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionApplyPassAllAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbkFwcGx5UGFzc0FsbEFjay5wcm90bxIFdHlwZXMaHHR5" + "cGUuTGVnaW9uQXBwbHlEZXRhaWwucHJvdG8aGXR5cGUuTGVnaW9uQmFzZUlu" + "Zm8ucHJvdG8icwoVTGVnaW9uQXBwbHlQYXNzQWxsQWNrEisKCUFwcGx5TGlz" + "dBgBIAMoCzIYLnR5cGVzLkxlZ2lvbkFwcGx5RGV0YWlsEi0KDkxlZ2lvbkJh" + "c2VJbmZvGAIgASgLMhUudHlwZXMuTGVnaW9uQmFzZUluZm9CLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionApplyDetailReflection.Descriptor,
			TypeLegionBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionApplyPassAllAck), (MessageParser)(object)LegionApplyPassAllAck.Parser, new string[2] { "ApplyList", "LegionBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
