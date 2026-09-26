using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossPeakFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossPeakFightStartAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkNyb3NzUGVha0ZpZ2h0U3RhcnRBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkJhdHRsZUluZm8ucHJvdG8iXwoWQ3Jvc3NQZWFrRmlnaHRTdGFydEFj" + "axIQCghQbGF5ZXJJZBgBIAEoBRIMCgRSYW5rGAIgASgFEiUKCkJhdHRsZUlu" + "Zm8YAyABKAsyES50eXBlcy5CYXR0bGVJbmZvQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossPeakFightStartAck), (MessageParser)(object)CrossPeakFightStartAck.Parser, new string[3] { "PlayerId", "Rank", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
