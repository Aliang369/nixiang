using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroAwakenGeneRemoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneRemoveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkhlcm9Bd2FrZW5HZW5lUmVtb3ZlQWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5EYXRhQ2hhbmdlLnByb3RvGhl0eXBlLkhlcm9Bd2FrZW5JbmZvLnBy" + "b3RvIm8KF0hlcm9Bd2FrZW5HZW5lUmVtb3ZlQWNrEi0KDkhlcm9Bd2FrZW5J" + "bmZvGAEgASgLMhUudHlwZXMuSGVyb0F3YWtlbkluZm8SJQoKRGF0YUNoYW5n" + "ZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHeroAwakenInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneRemoveAck), (MessageParser)(object)HeroAwakenGeneRemoveAck.Parser, new string[2] { "HeroAwakenInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
