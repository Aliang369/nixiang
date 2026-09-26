using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFeatureWorldBossReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFeatureWorldBossReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkZlYXR1cmVXb3JsZEJvc3MucHJvdG8SBXR5cGVzGhh0eXBlLldv" + "cmxkQm9zc0luZm8ucHJvdG8aHHR5cGUuV29ybGRCb3NzU2VsZkluZm8ucHJv" + "dG8ihgEKEEZlYXR1cmVXb3JsZEJvc3MSMwoRV29ybGRCb3NzU2VsZkluZm8Y" + "ASABKAsyGC50eXBlcy5Xb3JsZEJvc3NTZWxmSW5mbxIrCg1Xb3JsZEJvc3NJ" + "bmZvGAIgASgLMhQudHlwZXMuV29ybGRCb3NzSW5mbxIQCghTZWxmUmFuaxgD" + "IAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeWorldBossInfoReflection.Descriptor,
			TypeWorldBossSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FeatureWorldBoss), (MessageParser)(object)FeatureWorldBoss.Parser, new string[3] { "WorldBossSelfInfo", "WorldBossInfo", "SelfRank" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
