using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDailySignTargetAwardDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDailySignTargetAwardDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkRhaWx5U2lnblRhcmdldEF3YXJkRGV0YWlsLnByb3RvEgV0eXBl" + "cxoXZW51bS5FQ29tbW9uU3RhdGUucHJvdG8iYgoaRGFpbHlTaWduVGFyZ2V0" + "QXdhcmREZXRhaWwSEwoLVGFyZ2V0SW5kZXgYASABKAUSLwoLVGFyZ2V0U3Rh" + "dGUYAiABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1vblN0YXRlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECommonStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DailySignTargetAwardDetail), (MessageParser)(object)DailySignTargetAwardDetail.Parser, new string[2] { "TargetIndex", "TargetState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
