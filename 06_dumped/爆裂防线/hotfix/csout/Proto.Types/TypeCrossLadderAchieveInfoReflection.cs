using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderAchieveInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderAchieveInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkNyb3NzTGFkZGVyQWNoaWV2ZUluZm8ucHJvdG8SBXR5cGVzIooB" + "ChZDcm9zc0xhZGRlckFjaGlldmVJbmZvEhAKCExhZGRlcklkGAEgASgFEhQK" + "DEJlc3RMYWRkZXJJZBgCIAEoBRITCgtIaXN0b3J5QmVzdBgDIAEoBRINCgVT" + "Y29yZRgEIAEoBRISCgpGaWdodENvdW50GAUgASgFEhAKCFdpbkNvdW50GAYg" + "ASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderAchieveInfo), (MessageParser)(object)CrossLadderAchieveInfo.Parser, new string[6] { "LadderId", "BestLadderId", "HistoryBest", "Score", "FightCount", "WinCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
