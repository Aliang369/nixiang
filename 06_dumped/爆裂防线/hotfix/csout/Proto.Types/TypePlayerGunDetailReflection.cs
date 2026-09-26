using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePlayerGunDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePlayerGunDetailReflection()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlBsYXllckd1bkRldGFpbC5wcm90bxIFdHlwZXMiUwoPUGxheWVy" + "R3VuRGV0YWlsEg0KBUd1bklkGAEgASgFEgwKBFN0YXIYAiABKAUSEQoJRGFt" + "YWdlUGN0GAMgASgCEhAKCEJ1bGxldElkGAQgASgFQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PlayerGunDetail), (MessageParser)(object)PlayerGunDetail.Parser, new string[4] { "GunId", "Star", "DamagePct", "BulletId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
