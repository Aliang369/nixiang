using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHandbookMonsterUpgradeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHandbookMonsterUpgradeAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkhhbmRib29rTW9uc3RlclVwZ3JhZGVBY2sucHJvdG8SBXR5cGVz" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aF3R5cGUuSGFuZGJvb2tJbmZvLnBy" + "b3RvIm0KGUhhbmRib29rTW9uc3RlclVwZ3JhZGVBY2sSKQoMSGFuZGJvb2tJ" + "bmZvGAEgASgLMhMudHlwZXMuSGFuZGJvb2tJbmZvEiUKCkRhdGFDaGFuZ2UY" + "AiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHandbookInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HandbookMonsterUpgradeAck), (MessageParser)(object)HandbookMonsterUpgradeAck.Parser, new string[2] { "HandbookInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
