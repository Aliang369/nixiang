using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBossSubFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubFightStartAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVCb3NzU3ViRmlnaHRTdGFydEFjay5wcm90bxIFdHlw" + "ZXMaFXR5cGUuQmF0dGxlSW5mby5wcm90byJ4ChtPcGVyYXRlQm9zc1N1YkZp" + "Z2h0U3RhcnRBY2sSDAoEVVVJRBgBIAEoCRIRCglCb3NzTGV2ZWwYAiABKAUS" + "JQoKQmF0dGxlSW5mbxgDIAEoCzIRLnR5cGVzLkJhdHRsZUluZm8SEQoJRGFt" + "YWdlQWRkGAQgASgCQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubFightStartAck), (MessageParser)(object)OperateBossSubFightStartAck.Parser, new string[4] { "UUID", "BossLevel", "BattleInfo", "DamageAdd" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
