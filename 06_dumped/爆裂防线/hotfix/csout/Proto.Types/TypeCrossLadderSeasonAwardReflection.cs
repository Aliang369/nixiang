using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCrossLadderSeasonAwardReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderSeasonAwardReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkNyb3NzTGFkZGVyU2Vhc29uQXdhcmQucHJvdG8SBXR5cGVzGhVl" + "bnVtLkVCb29sU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwucHJvdG8i" + "eAoWQ3Jvc3NMYWRkZXJTZWFzb25Bd2FyZBIQCghMYWRkZXJJZBgBIAEoBRIl" + "CglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgVTdGF0" + "ZRgDIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0ZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderSeasonAward), (MessageParser)(object)CrossLadderSeasonAward.Parser, new string[3] { "LadderId", "AwardList", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
