using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAccountBindAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAccountBindAwardAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkFjY291bnRCaW5kQXdhcmRBY2sucHJvdG8SBXR5cGVzGhd0eXBl" + "LkF1eEF3YXJkSW5mby5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoV" + "dHlwZS5EYXRhQ2hhbmdlLnByb3RvIo4BChNBY2NvdW50QmluZEF3YXJkQWNr" + "EikKDEF1eEF3YXJkSW5mbxgBIAEoCzITLnR5cGVzLkF1eEF3YXJkSW5mbxIl" + "CglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRh" + "Q2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAuxAwardInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AccountBindAwardAck), (MessageParser)(object)AccountBindAwardAck.Parser, new string[3] { "AuxAwardInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
