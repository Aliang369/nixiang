using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFeatureListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFeatureListAckReflection()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkZlYXR1cmVMaXN0QWNrLnByb3RvEgV0eXBlcxoedHlwZS5GZWF0" + "dXJlRW5kbGVzc0hvcmRlLnByb3RvGhx0eXBlLkZlYXR1cmVMZWdpb25Cb3Nz" + "LnByb3RvGht0eXBlLkZlYXR1cmVXb3JsZEJvc3MucHJvdG8aG3R5cGUuRmVh" + "dHVyZVdvcmxkTGFpci5wcm90bxoYdHlwZS5IZXJvVHJpYWxJbmZvLnByb3Rv" + "IpQCCg5GZWF0dXJlTGlzdEFjaxIxChBGZWF0dXJlV29ybGRMYWlyGAEgASgL" + "MhcudHlwZXMuRmVhdHVyZVdvcmxkTGFpchIzChFGZWF0dXJlTGVnaW9uQm9z" + "cxgCIAEoCzIYLnR5cGVzLkZlYXR1cmVMZWdpb25Cb3NzEi4KEEZlYXR1cmVU" + "cmlhbEluZm8YAyABKAsyFC50eXBlcy5IZXJvVHJpYWxJbmZvEjEKEEZlYXR1" + "cmVXb3JsZEJvc3MYBCABKAsyFy50eXBlcy5GZWF0dXJlV29ybGRCb3NzEjcK" + "E0ZlYXR1cmVFbmRsZXNzSG9yZGUYBSABKAsyGi50eXBlcy5GZWF0dXJlRW5k" + "bGVzc0hvcmRlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeFeatureEndlessHordeReflection.Descriptor,
			TypeFeatureLegionBossReflection.Descriptor,
			TypeFeatureWorldBossReflection.Descriptor,
			TypeFeatureWorldLairReflection.Descriptor,
			TypeHeroTrialInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FeatureListAck), (MessageParser)(object)FeatureListAck.Parser, new string[5] { "FeatureWorldLair", "FeatureLegionBoss", "FeatureTrialInfo", "FeatureWorldBoss", "FeatureEndlessHorde" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
