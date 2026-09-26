using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdsSignDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsSignDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkFkc1NpZ25EZXRhaWwucHJvdG8SBXR5cGVzGiB0eXBlLkFkc0Nv" + "bW1vblRhcmdldERldGFpbC5wcm90bxoadHlwZS5PcGVyYXRlQmFzZUluZm8u" + "cHJvdG8iegoNQWRzU2lnbkRldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5" + "cGVzLk9wZXJhdGVCYXNlSW5mbxIwCgpUYXJnZXRMaXN0GAIgAygLMhwudHlw" + "ZXMuQWRzQ29tbW9uVGFyZ2V0RGV0YWlsEg0KBUNvdW50GAMgASgFQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAdsCommonTargetDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsSignDetail), (MessageParser)(object)AdsSignDetail.Parser, new string[3] { "BaseInfo", "TargetList", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
