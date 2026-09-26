using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBagSelectBoxOpenAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBagSelectBoxOpenAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkJhZ1NlbGVjdEJveE9wZW5BY2sucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8iYwoT" + "QmFnU2VsZWN0Qm94T3BlbkFjaxIlCglBd2FyZExpc3QYASADKAsyEi50eXBl" + "cy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAIgASgLMhEudHlwZXMuRGF0" + "YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BagSelectBoxOpenAck), (MessageParser)(object)BagSelectBoxOpenAck.Parser, new string[2] { "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
