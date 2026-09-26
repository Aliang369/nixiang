using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDailySignReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDailySignReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLkRhaWx5U2lnbi5wcm90bxIFdHlwZXMaF2VudW0uRUNvbW1vblN0" + "YXRlLnByb3RvGiV0eXBlLkRhaWx5U2lnblRhcmdldEF3YXJkRGV0YWlsLnBy" + "b3RvIpcBCglEYWlseVNpZ24SEQoJU2lnblRvdGFsGAEgASgFEg8KB1NpZ25E" + "YXkYAiABKAUSLgoKRGFpbHlTdGF0ZRgDIAEoDjIaLkVDb21tb25TdGF0ZS5F" + "Q29tbW9uU3RhdGUSNgoLVGFyZ2V0QXdhcmQYBCABKAsyIS50eXBlcy5EYWls" + "eVNpZ25UYXJnZXRBd2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeDailySignTargetAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DailySign), (MessageParser)(object)DailySign.Parser, new string[4] { "SignTotal", "SignDay", "DailyState", "TargetAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
