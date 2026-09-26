using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSuitRemoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSuitRemoveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlN1aXRSZW1vdmVBY2sucHJvdG8SBXR5cGVzGhV0eXBlLkRhdGFD" + "aGFuZ2UucHJvdG8aGXR5cGUuUGxheWVyU3VpdEluZm8ucHJvdG8iXwoNU3Vp" + "dFJlbW92ZUFjaxInCghTdWl0SW5mbxgBIAEoCzIVLnR5cGVzLlBsYXllclN1" + "aXRJbmZvEiUKCkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdl" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerSuitInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SuitRemoveAck), (MessageParser)(object)SuitRemoveAck.Parser, new string[2] { "SuitInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
