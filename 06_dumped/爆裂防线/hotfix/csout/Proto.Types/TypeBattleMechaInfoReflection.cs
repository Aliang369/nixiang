using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleMechaInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleMechaInfoReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkJhdHRsZU1lY2hhSW5mby5wcm90bxIFdHlwZXMaIHR5cGUuQmF0" + "dGxlTWVjaGFBdHRyRGV0YWlsLnByb3RvGiF0eXBlLkJhdHRsZU1lY2hhU2tp" + "bGxEZXRhaWwucHJvdG8isQEKD0JhdHRsZU1lY2hhSW5mbxIPCgdNZWNoYUlk" + "GAEgASgFEg0KBUxldmVsGAIgASgFEgwKBFN0YXIYAyABKAUSMAoJU2tpbGxM" + "aXN0GAQgAygLMh0udHlwZXMuQmF0dGxlTWVjaGFTa2lsbERldGFpbBIuCghB" + "dHRyTGlzdBgFIAMoCzIcLnR5cGVzLkJhdHRsZU1lY2hhQXR0ckRldGFpbBIO" + "CgZFbmVyZ3kYBiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleMechaAttrDetailReflection.Descriptor,
			TypeBattleMechaSkillDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleMechaInfo), (MessageParser)(object)BattleMechaInfo.Parser, new string[6] { "MechaId", "Level", "Star", "SkillList", "AttrList", "Energy" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
