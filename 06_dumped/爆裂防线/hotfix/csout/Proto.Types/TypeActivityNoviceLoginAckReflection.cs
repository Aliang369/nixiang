using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeActivityNoviceLoginAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeActivityNoviceLoginAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkFjdGl2aXR5Tm92aWNlTG9naW5BY2sucHJvdG8SBXR5cGVzGhd0" + "eXBlLkFjdGl2aXR5SW5mby5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90" + "bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvIpEBChZBY3Rpdml0eU5vdmljZUxv" + "Z2luQWNrEikKDEFjdGl2aXR5SW5mbxgBIAEoCzITLnR5cGVzLkFjdGl2aXR5" + "SW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIl" + "CgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeActivityInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ActivityNoviceLoginAck), (MessageParser)(object)ActivityNoviceLoginAck.Parser, new string[3] { "ActivityInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
