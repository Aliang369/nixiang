using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAdsCommonTargetDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsCommonTargetDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkFkc0NvbW1vblRhcmdldERldGFpbC5wcm90bxIFdHlwZXMaF2Vu" + "dW0uRUNvbW1vblN0YXRlLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3Rv" + "Is8BChVBZHNDb21tb25UYXJnZXREZXRhaWwSDgoGVGFyZ2V0GAEgASgFEi0K" + "CUZyZWVTdGF0ZRgCIAEoDjIaLkVDb21tb25TdGF0ZS5FQ29tbW9uU3RhdGUS" + "JQoJRnJlZUF3YXJkGAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSKwoHQWRT" + "dGF0ZRgEIAEoDjIaLkVDb21tb25TdGF0ZS5FQ29tbW9uU3RhdGUSIwoHQWRB" + "d2FyZBgFIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsCommonTargetDetail), (MessageParser)(object)AdsCommonTargetDetail.Parser, new string[5] { "Target", "FreeState", "FreeAward", "AdState", "AdAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
