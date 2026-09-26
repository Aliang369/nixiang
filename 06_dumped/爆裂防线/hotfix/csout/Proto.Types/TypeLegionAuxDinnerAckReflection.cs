using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionAuxDinnerAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionAuxDinnerAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbkF1eERpbm5lckFjay5wcm90bxIFdHlwZXMaFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoZdHlw" + "ZS5MZWdpb25TZWxmSW5mby5wcm90byKRAQoSTGVnaW9uQXV4RGlubmVyQWNr" + "Ei0KDkxlZ2lvblNlbGZJbmZvGAEgASgLMhUudHlwZXMuTGVnaW9uU2VsZklu" + "Zm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoK" + "RGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionAuxDinnerAck), (MessageParser)(object)LegionAuxDinnerAck.Parser, new string[3] { "LegionSelfInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
