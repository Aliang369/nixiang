using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeActiveZhanLingElementAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeActiveZhanLingElementAwardAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CjB0eXBlLlJlY2hhcmdlQWN0aXZlWmhhbkxpbmdFbGVtZW50QXdhcmRBY2su" + "cHJvdG8SBXR5cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRh" + "dGFDaGFuZ2UucHJvdG8aHXR5cGUuUmVjaGFyZ2VBY3RpdmVJbmZvLnByb3Rv" + "IqwBCiVSZWNoYXJnZUFjdGl2ZVpoYW5MaW5nRWxlbWVudEF3YXJkQWNrEjUK" + "ElJlY2hhcmdlQWN0aXZlSW5mbxgBIAEoCzIZLnR5cGVzLlJlY2hhcmdlQWN0" + "aXZlSW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFp" + "bBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeActiveZhanLingElementAwardAck), (MessageParser)(object)RechargeActiveZhanLingElementAwardAck.Parser, new string[3] { "RechargeActiveInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
