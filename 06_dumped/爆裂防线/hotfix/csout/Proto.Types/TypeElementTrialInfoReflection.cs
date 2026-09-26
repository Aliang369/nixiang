using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeElementTrialInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkVsZW1lbnRUcmlhbEluZm8ucHJvdG8SBXR5cGVzGhZ0eXBlLlRy" + "aWFsRGV0YWlsLnByb3RvIokBChBFbGVtZW50VHJpYWxJbmZvEhMKC0NvbWJh" + "dENvdW50GAEgASgFEhEKCUJ1eUNvbWJhdBgCIAEoBRISCgpEYWlseUNvdW50" + "GAMgASgFEiUKCVRyaWFsTGlzdBgEIAMoCzISLnR5cGVzLlRyaWFsRGV0YWls" + "EhIKClRvdGFsTGV2ZWwYBSABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeTrialDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialInfo), (MessageParser)(object)ElementTrialInfo.Parser, new string[5] { "CombatCount", "BuyCombat", "DailyCount", "TrialList", "TotalLevel" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
