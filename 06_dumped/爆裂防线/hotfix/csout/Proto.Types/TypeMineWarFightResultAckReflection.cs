using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarFightResultAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk1pbmVXYXJGaWdodFJlc3VsdEFjay5wcm90bxIFdHlwZXMaFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8aG3R5cGUuQmF0dGxlUmVzdWx0SW5mby5w" + "cm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhp0eXBlLk1pbmVXYXJCYXNl" + "SW5mby5wcm90bxoZdHlwZS5NaW5lV2FyTWFwSW5mby5wcm90byL0AQoVTWlu" + "ZVdhckZpZ2h0UmVzdWx0QWNrEi8KD01pbmVXYXJCYXNlSW5mbxgBIAEoCzIW" + "LnR5cGVzLk1pbmVXYXJCYXNlSW5mbxItCg5NaW5lV2FyTWFwSW5mbxgCIAEo" + "CzIVLnR5cGVzLk1pbmVXYXJNYXBJbmZvEi0KDEJhdHRsZVNldHRsZRgDIAEo" + "CzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm8SJQoJQXdhcmRMaXN0GAQgAygL" + "MhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgFIAEoCzIRLnR5" + "cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarFightResultAck), (MessageParser)(object)MineWarFightResultAck.Parser, new string[5] { "MineWarBaseInfo", "MineWarMapInfo", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
