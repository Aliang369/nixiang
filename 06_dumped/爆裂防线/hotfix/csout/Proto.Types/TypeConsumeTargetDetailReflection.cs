using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeConsumeTargetDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeConsumeTargetDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkNvbnN1bWVUYXJnZXREZXRhaWwucHJvdG8SBXR5cGVzGhdlbnVt" + "LkVDb21tb25TdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90byKG" + "AQoTQ29uc3VtZVRhcmdldERldGFpbBINCgVJbmRleBgBIAEoBRIOCgZUYXJn" + "ZXQYAiABKAUSKQoFU3RhdGUYAyABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1v" + "blN0YXRlEiUKCUF3YXJkTGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWls" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ConsumeTargetDetail), (MessageParser)(object)ConsumeTargetDetail.Parser, new string[4] { "Index", "Target", "State", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
