using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaCombatListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaCombatListAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkFyZW5hQ29tYmF0TGlzdEFjay5wcm90bxIFdHlwZXMaHHR5cGUu" + "QXJlbmFDb21iYXREZXRhaWwucHJvdG8aFHR5cGUuQXJlbmFJbmZvLnByb3Rv" + "ImcKEkFyZW5hQ29tYmF0TGlzdEFjaxIjCglBcmVuYUluZm8YASABKAsyEC50" + "eXBlcy5BcmVuYUluZm8SLAoKQ29tYmF0TGlzdBgCIAMoCzIYLnR5cGVzLkFy" + "ZW5hQ29tYmF0RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeArenaCombatDetailReflection.Descriptor,
			TypeArenaInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaCombatListAck), (MessageParser)(object)ArenaCombatListAck.Parser, new string[2] { "ArenaInfo", "CombatList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
