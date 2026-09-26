using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroAwakenAdvanceAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenAdvanceAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkhlcm9Bd2FrZW5BZHZhbmNlQWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5EYXRhQ2hhbmdlLnByb3RvGhl0eXBlLkhlcm9Bd2FrZW5JbmZvLnByb3Rv" + "ImwKFEhlcm9Bd2FrZW5BZHZhbmNlQWNrEi0KDkhlcm9Bd2FrZW5JbmZvGAEg" + "ASgLMhUudHlwZXMuSGVyb0F3YWtlbkluZm8SJQoKRGF0YUNoYW5nZRgCIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHeroAwakenInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenAdvanceAck), (MessageParser)(object)HeroAwakenAdvanceAck.Parser, new string[2] { "HeroAwakenInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
