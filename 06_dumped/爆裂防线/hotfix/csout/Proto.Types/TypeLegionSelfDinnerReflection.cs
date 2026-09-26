using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionSelfDinnerReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionSelfDinnerReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkxlZ2lvblNlbGZEaW5uZXIucHJvdG8SBXR5cGVzGhdlbnVtLkVB" + "Y3RpdmVTdGF0ZS5wcm90byJsChBMZWdpb25TZWxmRGlubmVyEg0KBUluZGV4" + "GAEgASgFEg8KB1N0YXJ0VG0YAiABKAMSDQoFRW5kVG0YAyABKAMSKQoFU3Rh" + "dGUYBCABKA4yGi5FQWN0aXZlU3RhdGUuRUFjdGl2ZVN0YXRlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEActiveStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionSelfDinner), (MessageParser)(object)LegionSelfDinner.Parser, new string[4] { "Index", "StartTm", "EndTm", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
