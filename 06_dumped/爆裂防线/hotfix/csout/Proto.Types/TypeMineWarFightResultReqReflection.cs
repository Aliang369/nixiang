using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk1pbmVXYXJGaWdodFJlc3VsdFJlcS5wcm90bxIFdHlwZXMaG3R5" + "cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90byJpChVNaW5lV2FyRmlnaHRSZXN1" + "bHRSZXESDwoHUG9pbnRJZBgBIAEoBRIQCghQbGF5ZXJJZBgCIAEoBRItCgxC" + "YXR0bGVSZXN1bHQYAyABKAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZvQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarFightResultReq), (MessageParser)(object)MineWarFightResultReq.Parser, new string[3] { "PointId", "PlayerId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
