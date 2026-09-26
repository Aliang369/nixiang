using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRankFirstReachAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankFirstReachAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlJhbmtGaXJzdFJlYWNoQWNrLnByb3RvEgV0eXBlcxoUZW51bS5F" + "UmFua1R5cGUucHJvdG8aH3R5cGUuUmFua0ZpcnN0UmVhY2hEZXRhaWwucHJv" + "dG8iawoRUmFua0ZpcnN0UmVhY2hBY2sSJgoIUmFua1R5cGUYASABKA4yFC5F" + "UmFua1R5cGUuRVJhbmtUeXBlEi4KCVJlYWNoTGlzdBgCIAMoCzIbLnR5cGVz" + "LlJhbmtGaXJzdFJlYWNoRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumERankTypeReflection.Descriptor,
			TypeRankFirstReachDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankFirstReachAck), (MessageParser)(object)RankFirstReachAck.Parser, new string[2] { "RankType", "ReachList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
