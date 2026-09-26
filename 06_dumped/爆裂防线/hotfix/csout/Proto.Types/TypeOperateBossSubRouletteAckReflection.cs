using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBossSubRouletteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubRouletteAckReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVCb3NzU3ViUm91bGV0dGVBY2sucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhx0eXBlLkJvc3NTdWJqZWN0RGV0" + "YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8irQEKGU9wZXJhdGVC" + "b3NzU3ViUm91bGV0dGVBY2sSMQoPQm9zc1N1YmplY3RMaXN0GAEgAygLMhgu" + "dHlwZXMuQm9zc1N1YmplY3REZXRhaWwSJQoJQXdhcmRMaXN0GAIgAygLMhIu" + "dHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVz" + "LkRhdGFDaGFuZ2USDwoHSGl0TGlzdBgEIAMoBUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBossSubjectDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubRouletteAck), (MessageParser)(object)OperateBossSubRouletteAck.Parser, new string[4] { "BossSubjectList", "AwardList", "DataChange", "HitList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
