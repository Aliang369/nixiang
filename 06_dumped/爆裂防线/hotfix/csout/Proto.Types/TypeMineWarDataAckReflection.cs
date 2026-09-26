using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeMineWarDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarDataAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLk1pbmVXYXJEYXRhQWNrLnByb3RvEgV0eXBlcxoVZW51bS5FRXJy" + "b3JDb2RlLnByb3RvGhp0eXBlLk1pbmVXYXJCYXNlSW5mby5wcm90bxoZdHlw" + "ZS5NaW5lV2FyTWFwSW5mby5wcm90byKbAQoOTWluZVdhckRhdGFBY2sSKQoJ" + "RXJyb3JDb2RlGAEgASgOMhYuRUVycm9yQ29kZS5FRXJyb3JDb2RlEi8KD01p" + "bmVXYXJCYXNlSW5mbxgCIAEoCzIWLnR5cGVzLk1pbmVXYXJCYXNlSW5mbxIt" + "Cg5NaW5lV2FyTWFwSW5mbxgDIAEoCzIVLnR5cGVzLk1pbmVXYXJNYXBJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEErrorCodeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarDataAck), (MessageParser)(object)MineWarDataAck.Parser, new string[3] { "ErrorCode", "MineWarBaseInfo", "MineWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
