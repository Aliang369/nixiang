using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldDungeonOpenReq : IMessage<WorldDungeonOpenReq>, IMessage, IEquatable<WorldDungeonOpenReq>, IDeepCloneable<WorldDungeonOpenReq>, IBufferMessage
{
	private static readonly MessageParser<WorldDungeonOpenReq> _parser = new MessageParser<WorldDungeonOpenReq>((Func<WorldDungeonOpenReq>)(() => new WorldDungeonOpenReq()));

	private UnknownFieldSet _unknownFields;

	public const int GridIdFieldNumber = 1;

	private int gridId_;

	public const int DungeonLevelFieldNumber = 2;

	private EDifficultyLevel dungeonLevel_;

	public const int SelectListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_selectList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> selectList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldDungeonOpenReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldDungeonOpenReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GridId
	{
		get
		{
			return gridId_;
		}
		set
		{
			gridId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EDifficultyLevel DungeonLevel
	{
		get
		{
			return dungeonLevel_;
		}
		set
		{
			dungeonLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> SelectList => selectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonOpenReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonOpenReq(WorldDungeonOpenReq other)
		: this()
	{
		gridId_ = other.gridId_;
		dungeonLevel_ = other.dungeonLevel_;
		selectList_ = other.selectList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonOpenReq Clone()
	{
		return new WorldDungeonOpenReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldDungeonOpenReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldDungeonOpenReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GridId != other.GridId)
		{
			return false;
		}
		if (DungeonLevel != other.DungeonLevel)
		{
			return false;
		}
		if (!selectList_.Equals(other.selectList_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (GridId != 0)
		{
			num ^= GridId.GetHashCode();
		}
		if (DungeonLevel != EDifficultyLevel.EmptyValue)
		{
			num ^= DungeonLevel.GetHashCode();
		}
		num ^= ((object)selectList_).GetHashCode();
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (GridId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GridId);
		}
		if (DungeonLevel != EDifficultyLevel.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)DungeonLevel);
		}
		selectList_.WriteTo(ref output, _repeated_selectList_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (GridId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GridId);
		}
		if (DungeonLevel != EDifficultyLevel.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DungeonLevel);
		}
		num += selectList_.CalculateSize(_repeated_selectList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldDungeonOpenReq other)
	{
		if (other != null)
		{
			if (other.GridId != 0)
			{
				GridId = other.GridId;
			}
			if (other.DungeonLevel != EDifficultyLevel.EmptyValue)
			{
				DungeonLevel = other.DungeonLevel;
			}
			selectList_.Add((IEnumerable<int>)other.selectList_);
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 8u:
				GridId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				DungeonLevel = (EDifficultyLevel)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
			case 26u:
				selectList_.AddEntriesFrom(ref input, _repeated_selectList_codec);
				break;
			}
		}
	}
}
