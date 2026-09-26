using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePlayerGunInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePlayerGunInfoReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlBsYXllckd1bkluZm8ucHJvdG8SBXR5cGVzGhp0eXBlLlBsYXll" + "ckd1bkRldGFpbC5wcm90byJQCg1QbGF5ZXJHdW5JbmZvEicKB0d1bkxpc3QY" + "ASADKAsyFi50eXBlcy5QbGF5ZXJHdW5EZXRhaWwSFgoOVG90YWxEYW1hZ2VQ" + "Y3QYAiABKAJCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypePlayerGunDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PlayerGunInfo), (MessageParser)(object)PlayerGunInfo.Parser, new string[2] { "GunList", "TotalDamagePct" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
