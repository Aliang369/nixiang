using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHandbookModTargetAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHandbookModTargetAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkhhbmRib29rTW9kVGFyZ2V0QXdhcmRBY2sucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJv" + "dG8aF3R5cGUuSGFuZGJvb2tJbmZvLnByb3RvIpQBChlIYW5kYm9va01vZFRh" + "cmdldEF3YXJkQWNrEikKDEhhbmRib29rSW5mbxgBIAEoCzITLnR5cGVzLkhh" + "bmRib29rSW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERl" + "dGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeHandbookInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HandbookModTargetAwardAck), (MessageParser)(object)HandbookModTargetAwardAck.Parser, new string[3] { "HandbookInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
