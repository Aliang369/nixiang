using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHandbookInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHandbookInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkhhbmRib29rSW5mby5wcm90bxIFdHlwZXMaGnR5cGUuTW9kSGFu" + "ZGJvb2tJbmZvLnByb3RvGiB0eXBlLk1vbnN0ZXJIYW5kYm9va0RldGFpbC5w" + "cm90byJyCgxIYW5kYm9va0luZm8SMQoLTW9uc3Rlckxpc3QYASADKAsyHC50" + "eXBlcy5Nb25zdGVySGFuZGJvb2tEZXRhaWwSLwoPTW9kSGFuZGJvb2tJbmZv" + "GAIgASgLMhYudHlwZXMuTW9kSGFuZGJvb2tJbmZvQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeModHandbookInfoReflection.Descriptor,
			TypeMonsterHandbookDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HandbookInfo), (MessageParser)(object)HandbookInfo.Parser, new string[2] { "MonsterList", "ModHandbookInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
