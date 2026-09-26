using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SnowVeilLandDetail : IMessage<SnowVeilLandDetail>, IMessage, IEquatable<SnowVeilLandDetail>, IDeepCloneable<SnowVeilLandDetail>, IBufferMessage
{
	private static readonly MessageParser<SnowVeilLandDetail> _parser = new MessageParser<SnowVeilLandDetail>((Func<SnowVeilLandDetail>)(() => new SnowVeilLandDetail()));

	private UnknownFieldSet _unknownFields;

	public const int IndexFieldNumber = 1;

	private int index_;

	public const int LandTypeFieldNumber = 2;

	private ESnowVeilLandType landType_;

	public const int LandStateFieldNumber = 3;

	private ECommonState landState_;

	public const int ScoreFieldNumber = 4;

	private int score_;

	public const int ItemFieldNumber = 5;

	private AwardDetail item_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SnowVeilLandDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSnowVeilLandDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Index
	{
		get
		{
			return index_;
		}
		set
		{
			index_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESnowVeilLandType LandType
	{
		get
		{
			return landType_;
		}
		set
		{
			landType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState LandState
	{
		get
		{
			return landState_;
		}
		set
		{
			landState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Score
	{
		get
		{
			return score_;
		}
		set
		{
			score_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AwardDetail Item
	{
		get
		{
			return item_;
		}
		set
		{
			item_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilLandDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilLandDetail(SnowVeilLandDetail other)
		: this()
	{
		index_ = other.index_;
		landType_ = other.landType_;
		landState_ = other.landState_;
		score_ = other.score_;
		item_ = ((other.item_ != null) ? other.item_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilLandDetail Clone()
	{
		return new SnowVeilLandDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SnowVeilLandDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SnowVeilLandDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Index != other.Index)
		{
			return false;
		}
		if (LandType != other.LandType)
		{
			return false;
		}
		if (LandState != other.LandState)
		{
			return false;
		}
		if (Score != other.Score)
		{
			return false;
		}
		if (!object.Equals(Item, other.Item))
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
		if (Index != 0)
		{
			num ^= Index.GetHashCode();
		}
		if (LandType != ESnowVeilLandType.EmptyValue)
		{
			num ^= LandType.GetHashCode();
		}
		if (LandState != ECommonState.No)
		{
			num ^= LandState.GetHashCode();
		}
		if (Score != 0)
		{
			num ^= Score.GetHashCode();
		}
		if (item_ != null)
		{
			num ^= Item.GetHashCode();
		}
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
		if (Index != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Index);
		}
		if (LandType != ESnowVeilLandType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)LandType);
		}
		if (LandState != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)LandState);
		}
		if (Score != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Score);
		}
		if (item_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Item);
		}
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
		if (Index != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Index);
		}
		if (LandType != ESnowVeilLandType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LandType);
		}
		if (LandState != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LandState);
		}
		if (Score != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Score);
		}
		if (item_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Item);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SnowVeilLandDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Index != 0)
		{
			Index = other.Index;
		}
		if (other.LandType != ESnowVeilLandType.EmptyValue)
		{
			LandType = other.LandType;
		}
		if (other.LandState != ECommonState.No)
		{
			LandState = other.LandState;
		}
		if (other.Score != 0)
		{
			Score = other.Score;
		}
		if (other.item_ != null)
		{
			if (item_ == null)
			{
				Item = new AwardDetail();
			}
			Item.MergeFrom(other.Item);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				Index = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				LandType = (ESnowVeilLandType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				LandState = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				Score = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				if (item_ == null)
				{
					Item = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Item);
				break;
			}
		}
	}
}
