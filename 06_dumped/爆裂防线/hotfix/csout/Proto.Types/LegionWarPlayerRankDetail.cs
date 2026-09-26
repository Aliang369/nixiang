using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarPlayerRankDetail : IMessage<LegionWarPlayerRankDetail>, IMessage, IEquatable<LegionWarPlayerRankDetail>, IDeepCloneable<LegionWarPlayerRankDetail>, IBufferMessage
{
	private static readonly MessageParser<LegionWarPlayerRankDetail> _parser = new MessageParser<LegionWarPlayerRankDetail>((Func<LegionWarPlayerRankDetail>)(() => new LegionWarPlayerRankDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RankDataFieldNumber = 1;

	private RankData rankData_;

	public const int RoleBaseInfoFieldNumber = 2;

	private RoleBaseInfo roleBaseInfo_;

	public const int LegionFlagInfoFieldNumber = 3;

	private LegionFlagInfo legionFlagInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarPlayerRankDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarPlayerRankDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankData RankData
	{
		get
		{
			return rankData_;
		}
		set
		{
			rankData_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionFlagInfo LegionFlagInfo
	{
		get
		{
			return legionFlagInfo_;
		}
		set
		{
			legionFlagInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPlayerRankDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPlayerRankDetail(LegionWarPlayerRankDetail other)
		: this()
	{
		rankData_ = ((other.rankData_ != null) ? other.rankData_.Clone() : null);
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		legionFlagInfo_ = ((other.legionFlagInfo_ != null) ? other.legionFlagInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPlayerRankDetail Clone()
	{
		return new LegionWarPlayerRankDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarPlayerRankDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarPlayerRankDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RankData, other.RankData))
		{
			return false;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
		{
			return false;
		}
		if (!object.Equals(LegionFlagInfo, other.LegionFlagInfo))
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
		if (rankData_ != null)
		{
			num ^= RankData.GetHashCode();
		}
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (legionFlagInfo_ != null)
		{
			num ^= LegionFlagInfo.GetHashCode();
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
		if (rankData_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RankData);
		}
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
		}
		if (legionFlagInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionFlagInfo);
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
		if (rankData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RankData);
		}
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (legionFlagInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionFlagInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarPlayerRankDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.rankData_ != null)
		{
			if (rankData_ == null)
			{
				RankData = new RankData();
			}
			RankData.MergeFrom(other.RankData);
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
		}
		if (other.legionFlagInfo_ != null)
		{
			if (legionFlagInfo_ == null)
			{
				LegionFlagInfo = new LegionFlagInfo();
			}
			LegionFlagInfo.MergeFrom(other.LegionFlagInfo);
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
			case 10u:
				if (rankData_ == null)
				{
					RankData = new RankData();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RankData);
				break;
			case 18u:
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			case 26u:
				if (legionFlagInfo_ == null)
				{
					LegionFlagInfo = new LegionFlagInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionFlagInfo);
				break;
			}
		}
	}
}
