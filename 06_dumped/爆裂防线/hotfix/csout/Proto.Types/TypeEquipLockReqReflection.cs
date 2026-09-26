using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipLockReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipLockReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkVxdWlwTG9ja1JlcS5wcm90bxIFdHlwZXMaFWVudW0uRUJvb2xT" + "dGF0ZS5wcm90byJFCgxFcXVpcExvY2tSZXESDwoHRXF1aXBJZBgBIAEoCRIk" + "CgRMb2NrGAIgASgOMhYuRUJvb2xTdGF0ZS5FQm9vbFN0YXRlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipLockReq), (MessageParser)(object)EquipLockReq.Parser, new string[2] { "EquipId", "Lock" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
