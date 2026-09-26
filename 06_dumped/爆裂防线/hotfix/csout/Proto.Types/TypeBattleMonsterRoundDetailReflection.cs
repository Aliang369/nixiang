using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleMonsterRoundDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleMonsterRoundDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkJhdHRsZU1vbnN0ZXJSb3VuZERldGFpbC5wcm90bxIFdHlwZXMa" + "InR5cGUuQmF0dGxlQm9ybk1vbnN0ZXJEZXRhaWwucHJvdG8ikgEKGEJhdHRs" + "ZU1vbnN0ZXJSb3VuZERldGFpbBINCgVSb3VuZBgBIAEoBRIRCglCb3JuU3Rh" + "cnQYAiABKAUSDwoHQm9ybkVuZBgDIAEoBRIRCglCb3JuQ291bnQYBCABKAUS" + "MAoIQm9ybkxpc3QYBSADKAsyHi50eXBlcy5CYXR0bGVCb3JuTW9uc3RlckRl" + "dGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleBornMonsterDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleMonsterRoundDetail), (MessageParser)(object)BattleMonsterRoundDetail.Parser, new string[5] { "Round", "BornStart", "BornEnd", "BornCount", "BornList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
