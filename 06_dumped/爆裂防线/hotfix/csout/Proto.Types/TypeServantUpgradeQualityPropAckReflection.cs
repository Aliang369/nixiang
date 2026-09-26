using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantUpgradeQualityPropAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantUpgradeQualityPropAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLlNlcnZhbnRVcGdyYWRlUXVhbGl0eVByb3BBY2sucHJvdG8SBXR5" + "cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2Uu" + "cHJvdG8aFnR5cGUuU2VydmFudEluZm8ucHJvdG8ilQEKHFNlcnZhbnRVcGdy" + "YWRlUXVhbGl0eVByb3BBY2sSJwoLU2VydmFudEluZm8YASABKAsyEi50eXBl" + "cy5TZXJ2YW50SW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2Fy" + "ZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5n" + "ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeServantInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantUpgradeQualityPropAck), (MessageParser)(object)ServantUpgradeQualityPropAck.Parser, new string[3] { "ServantInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
