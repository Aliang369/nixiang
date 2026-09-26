using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldEventInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldEventInfoReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLldvcmxkRXZlbnRJbmZvLnByb3RvEgV0eXBlcxobdHlwZS5Xb3Js" + "ZEV2ZW50RGV0YWlsLnByb3RvIkwKDldvcmxkRXZlbnRJbmZvEioKCUV2ZW50" + "TGlzdBgBIAMoCzIXLnR5cGVzLldvcmxkRXZlbnREZXRhaWwSDgoGTmV4dFRt" + "GAIgASgDQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldEventDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldEventInfo), (MessageParser)(object)WorldEventInfo.Parser, new string[2] { "EventList", "NextTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
