using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarFightResultReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxlZ2lvbldhckZpZ2h0UmVzdWx0UmVxLnByb3RvEgV0eXBlcxob" + "dHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvIloKF0xlZ2lvbldhckZpZ2h0" + "UmVzdWx0UmVxEhAKCFBsYXllcklkGAEgASgFEi0KDEJhdHRsZVJlc3VsdBgC" + "IAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm9CLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarFightResultReq), (MessageParser)(object)LegionWarFightResultReq.Parser, new string[2] { "PlayerId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
