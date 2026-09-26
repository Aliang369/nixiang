using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessMineInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMineInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkVuZGxlc3NNaW5lSW5mby5wcm90bxIFdHlwZXMaHnR5cGUuRW5k" + "bGVzc01pbmVCYXNlSW5mby5wcm90bxoidHlwZS5FbmRsZXNzTWluZU1hcFJv" + "d0RldGFpbC5wcm90byKCAQoPRW5kbGVzc01pbmVJbmZvEjcKE0VuZGxlc3NN" + "aW5lQmFzZUluZm8YASABKAsyGi50eXBlcy5FbmRsZXNzTWluZUJhc2VJbmZv" + "EjYKDkVuZGxlc3NNaW5lTWFwGAIgAygLMh4udHlwZXMuRW5kbGVzc01pbmVN" + "YXBSb3dEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeEndlessMineBaseInfoReflection.Descriptor,
			TypeEndlessMineMapRowDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMineInfo), (MessageParser)(object)EndlessMineInfo.Parser, new string[2] { "EndlessMineBaseInfo", "EndlessMineMap" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
