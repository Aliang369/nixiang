using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateScratchCardDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateScratchCardDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLk9wZXJhdGVTY3JhdGNoQ2FyZERldGFpbC5wcm90bxIFdHlwZXMa" + "F2VudW0uRUNvbW1vblN0YXRlLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnBy" + "b3RvIpwBChhPcGVyYXRlU2NyYXRjaENhcmREZXRhaWwSDQoFSW5kZXgYASAB" + "KAUSDQoFUHJpemUYAiABKAUSEAoIQ2FyZExpc3QYAyADKAUSKQoFU3RhdGUY" + "BCABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1vblN0YXRlEiUKCUF3YXJkTGlz" + "dBgFIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateScratchCardDetail), (MessageParser)(object)OperateScratchCardDetail.Parser, new string[5] { "Index", "Prize", "CardList", "State", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
