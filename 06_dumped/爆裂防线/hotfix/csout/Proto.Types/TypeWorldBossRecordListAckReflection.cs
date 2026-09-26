using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossRecordListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossRecordListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLldvcmxkQm9zc1JlY29yZExpc3RBY2sucHJvdG8SBXR5cGVzGiR0" + "eXBlLldvcmxkQm9zc0h1cnRSZWNvcmREZXRhaWwucHJvdG8iTgoWV29ybGRC" + "b3NzUmVjb3JkTGlzdEFjaxI0CgpSZWNvcmRMaXN0GAEgAygLMiAudHlwZXMu" + "V29ybGRCb3NzSHVydFJlY29yZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldBossHurtRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossRecordListAck), (MessageParser)(object)WorldBossRecordListAck.Parser, new string[1] { "RecordList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
