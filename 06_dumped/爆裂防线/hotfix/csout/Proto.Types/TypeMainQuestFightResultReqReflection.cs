using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMainQuestFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMainQuestFightResultReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLk1haW5RdWVzdEZpZ2h0UmVzdWx0UmVxLnByb3RvEgV0eXBlcxob" + "dHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvIlkKF01haW5RdWVzdEZpZ2h0" + "UmVzdWx0UmVxEg8KB0xldmVsSWQYASABKAUSLQoMQmF0dGxlUmVzdWx0GAIg" + "ASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mb0IuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MainQuestFightResultReq), (MessageParser)(object)MainQuestFightResultReq.Parser, new string[2] { "LevelId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
