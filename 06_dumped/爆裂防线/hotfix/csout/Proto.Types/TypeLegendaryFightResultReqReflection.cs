using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegendaryFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegendaryFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxlZ2VuZGFyeUZpZ2h0UmVzdWx0UmVxLnByb3RvEgV0eXBlcxob" + "dHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvImsKF0xlZ2VuZGFyeUZpZ2h0" + "UmVzdWx0UmVxEg8KB0xldmVsSWQYASABKAUSEAoIUGxheWVySWQYAiABKAMS" + "LQoMQmF0dGxlUmVzdWx0GAMgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegendaryFightResultReq), (MessageParser)(object)LegendaryFightResultReq.Parser, new string[3] { "LevelId", "PlayerId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
