using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkZhY2lsaXR5RmlnaHRSZXN1bHRSZXEucHJvdG8SBXR5cGVzGht0" + "eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8iawoWRmFjaWxpdHlGaWdodFJl" + "c3VsdFJlcRISCgpGYWNpbGl0eUlkGAEgASgFEg4KBlJvb21JZBgCIAEoBRIt" + "CgxCYXR0bGVSZXN1bHQYAyABKAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityFightResultReq), (MessageParser)(object)FacilityFightResultReq.Parser, new string[3] { "FacilityId", "RoomId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
