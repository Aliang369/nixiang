using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDefenseDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDefenseDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkRlZmVuc2VEZXRhaWwucHJvdG8SBXR5cGVzGhdlbnVtLkVEZWZl" + "bnNlVHlwZS5wcm90bxoidHlwZS5CYXR0bGVEZWZlbnNlQXR0ckRldGFpbC5w" + "cm90byJyCg1EZWZlbnNlRGV0YWlsEi8KC0RlZmVuc2VUeXBlGAEgASgOMhou" + "RURlZmVuc2VUeXBlLkVEZWZlbnNlVHlwZRIwCghBdHRyTGlzdBgCIAMoCzIe" + "LnR5cGVzLkJhdHRsZURlZmVuc2VBdHRyRGV0YWlsQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEDefenseTypeReflection.Descriptor,
			TypeBattleDefenseAttrDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DefenseDetail), (MessageParser)(object)DefenseDetail.Parser, new string[2] { "DefenseType", "AttrList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
