using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonCityAttackReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonCityAttackReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlNlYXNvbkNpdHlBdHRhY2tSZXEucHJvdG8SBXR5cGVzIjUKE1Nl" + "YXNvbkNpdHlBdHRhY2tSZXESDgoGVGVhbUlkGAEgASgFEg4KBkNpdHlJZBgC" + "IAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonCityAttackReq), (MessageParser)(object)SeasonCityAttackReq.Parser, new string[2] { "TeamId", "CityId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
