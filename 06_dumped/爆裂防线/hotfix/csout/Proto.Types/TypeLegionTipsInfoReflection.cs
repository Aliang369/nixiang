using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionTipsInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionTipsInfoReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkxlZ2lvblRpcHNJbmZvLnByb3RvEgV0eXBlcxoZdHlwZS5MZWdp" + "b25CYXNlSW5mby5wcm90bxoYdHlwZS5MZWdpb25TZXRJbmZvLnByb3RvGhd0" + "eXBlLlJvbGVCYXNlSW5mby5wcm90byKZAQoOTGVnaW9uVGlwc0luZm8SLQoO" + "TGVnaW9uQmFzZUluZm8YASABKAsyFS50eXBlcy5MZWdpb25CYXNlSW5mbxIr" + "Cg1MZWdpb25TZXRJbmZvGAIgASgLMhQudHlwZXMuTGVnaW9uU2V0SW5mbxIr" + "Cg5MZWFkZXJCYXNlSW5mbxgDIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mb0Iu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionBaseInfoReflection.Descriptor,
			TypeLegionSetInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionTipsInfo), (MessageParser)(object)LegionTipsInfo.Parser, new string[3] { "LegionBaseInfo", "LegionSetInfo", "LeaderBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
