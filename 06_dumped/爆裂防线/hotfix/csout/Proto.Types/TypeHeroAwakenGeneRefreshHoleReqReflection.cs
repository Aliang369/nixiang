using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenGeneRefreshHoleReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneRefreshHoleReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkhlcm9Bd2FrZW5HZW5lUmVmcmVzaEhvbGVSZXEucHJvdG8SBXR5" + "cGVzGhVlbnVtLkVTa2lsbFR5cGUucHJvdG8iSQocSGVyb0F3YWtlbkdlbmVS" + "ZWZyZXNoSG9sZVJlcRIpCglTa2lsbFR5cGUYASABKA4yFi5FU2tpbGxUeXBl" + "LkVTa2lsbFR5cGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneRefreshHoleReq), (MessageParser)(object)HeroAwakenGeneRefreshHoleReq.Parser, new string[1] { "SkillType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
