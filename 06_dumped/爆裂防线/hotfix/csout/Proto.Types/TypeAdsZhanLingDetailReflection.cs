using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdsZhanLingDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsZhanLingDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkFkc1poYW5MaW5nRGV0YWlsLnByb3RvEgV0eXBlcxogdHlwZS5B" + "ZHNDb21tb25UYXJnZXREZXRhaWwucHJvdG8aGnR5cGUuT3BlcmF0ZUJhc2VJ" + "bmZvLnByb3RvIo0BChFBZHNaaGFuTGluZ0RldGFpbBIoCghCYXNlSW5mbxgB" + "IAEoCzIWLnR5cGVzLk9wZXJhdGVCYXNlSW5mbxINCgVMZXZlbBgCIAEoBRIN" + "CgVSZWFjaBgDIAEoBRIwCgpUYXJnZXRMaXN0GAQgAygLMhwudHlwZXMuQWRz" + "Q29tbW9uVGFyZ2V0RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAdsCommonTargetDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsZhanLingDetail), (MessageParser)(object)AdsZhanLingDetail.Parser, new string[4] { "BaseInfo", "Level", "Reach", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
