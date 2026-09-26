using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCrossMineServerDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMineServerDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNyb3NzTWluZVNlcnZlckRldGFpbC5wcm90bxIFdHlwZXMaHmVu" + "dW0uRU1pbmVXYXJTZXJ2ZXJTdGF0ZS5wcm90byKiAQoVQ3Jvc3NNaW5lU2Vy" + "dmVyRGV0YWlsEhAKCFNlcnZlcklkGAEgASgFEhIKClNlcnZlck5hbWUYAiAB" + "KAkSNwoFU3RhdGUYAyABKA4yKC5FTWluZVdhclNlcnZlclN0YXRlLkVNaW5l" + "V2FyU2VydmVyU3RhdGUSFQoNQ29zdENvbWJhdE51bRgEIAEoBRITCgtDcm9z" + "c09wZW5UbRgFIAEoA0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEMineWarServerStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMineServerDetail), (MessageParser)(object)CrossMineServerDetail.Parser, new string[5] { "ServerId", "ServerName", "State", "CostCombatNum", "CrossOpenTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
