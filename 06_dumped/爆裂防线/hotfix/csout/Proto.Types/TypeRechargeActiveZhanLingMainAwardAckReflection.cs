using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeActiveZhanLingMainAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeActiveZhanLingMainAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci10eXBlLlJlY2hhcmdlQWN0aXZlWmhhbkxpbmdNYWluQXdhcmRBY2sucHJv" + "dG8SBXR5cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFD" + "aGFuZ2UucHJvdG8aHXR5cGUuUmVjaGFyZ2VBY3RpdmVJbmZvLnByb3RvIqkB" + "CiJSZWNoYXJnZUFjdGl2ZVpoYW5MaW5nTWFpbkF3YXJkQWNrEjUKElJlY2hh" + "cmdlQWN0aXZlSW5mbxgBIAEoCzIZLnR5cGVzLlJlY2hhcmdlQWN0aXZlSW5m" + "bxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpE" + "YXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeActiveZhanLingMainAwardAck), (MessageParser)(object)RechargeActiveZhanLingMainAwardAck.Parser, new string[3] { "RechargeActiveInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
