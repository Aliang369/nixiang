using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldEventFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldEventFightStartAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLldvcmxkRXZlbnRGaWdodFN0YXJ0QWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5CYXR0bGVJbmZvLnByb3RvGhR0eXBlLldvcmxkSW5mby5wcm90byJ1" + "ChdXb3JsZEV2ZW50RmlnaHRTdGFydEFjaxIOCgZHcmlkSWQYASABKAUSJQoK" + "QmF0dGxlSW5mbxgCIAEoCzIRLnR5cGVzLkJhdHRsZUluZm8SIwoJV29ybGRJ" + "bmZvGAMgASgLMhAudHlwZXMuV29ybGRJbmZvQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleInfoReflection.Descriptor,
			TypeWorldInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldEventFightStartAck), (MessageParser)(object)WorldEventFightStartAck.Parser, new string[3] { "GridId", "BattleInfo", "WorldInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
