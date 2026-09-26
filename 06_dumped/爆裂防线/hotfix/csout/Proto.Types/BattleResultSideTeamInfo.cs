using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleResultSideTeamInfo : IMessage<BattleResultSideTeamInfo>, IMessage, IEquatable<BattleResultSideTeamInfo>, IDeepCloneable<BattleResultSideTeamInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleResultSideTeamInfo> _parser = new MessageParser<BattleResultSideTeamInfo>((Func<BattleResultSideTeamInfo>)(() => new BattleResultSideTeamInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SideInfoFieldNumber = 1;

	private BattleResultSideInfo sideInfo_;

	public const int MasterTeamInfoFieldNumber = 2;

	private BattleResultTeamInfo masterTeamInfo_;

	public const int HelperTeamInfoFieldNumber = 3;

	private BattleResultTeamInfo helperTeamInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleResultSideTeamInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleResultSideTeamInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideInfo SideInfo
	{
		get
		{
			return sideInfo_;
		}
		set
		{
			sideInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultTeamInfo MasterTeamInfo
	{
		get
		{
			return masterTeamInfo_;
		}
		set
		{
			masterTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultTeamInfo HelperTeamInfo
	{
		get
		{
			return helperTeamInfo_;
		}
		set
		{
			helperTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideTeamInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideTeamInfo(BattleResultSideTeamInfo other)
		: this()
	{
		sideInfo_ = ((other.sideInfo_ != null) ? other.sideInfo_.Clone() : null);
		masterTeamInfo_ = ((other.masterTeamInfo_ != null) ? other.masterTeamInfo_.Clone() : null);
		helperTeamInfo_ = ((other.helperTeamInfo_ != null) ? other.helperTeamInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideTeamInfo Clone()
	{
		return new BattleResultSideTeamInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleResultSideTeamInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleResultSideTeamInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(SideInfo, other.SideInfo))
		{
			return false;
		}
		if (!object.Equals(MasterTeamInfo, other.MasterTeamInfo))
		{
			return false;
		}
		if (!object.Equals(HelperTeamInfo, other.HelperTeamInfo))
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
		if (sideInfo_ != null)
		{
			num ^= SideInfo.GetHashCode();
		}
		if (masterTeamInfo_ != null)
		{
			num ^= MasterTeamInfo.GetHashCode();
		}
		if (helperTeamInfo_ != null)
		{
			num ^= HelperTeamInfo.GetHashCode();
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
		if (sideInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SideInfo);
		}
		if (masterTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MasterTeamInfo);
		}
		if (helperTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HelperTeamInfo);
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
		if (sideInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SideInfo);
		}
		if (masterTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MasterTeamInfo);
		}
		if (helperTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HelperTeamInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleResultSideTeamInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.sideInfo_ != null)
		{
			if (sideInfo_ == null)
			{
				SideInfo = new BattleResultSideInfo();
			}
			SideInfo.MergeFrom(other.SideInfo);
		}
		if (other.masterTeamInfo_ != null)
		{
			if (masterTeamInfo_ == null)
			{
				MasterTeamInfo = new BattleResultTeamInfo();
			}
			MasterTeamInfo.MergeFrom(other.MasterTeamInfo);
		}
		if (other.helperTeamInfo_ != null)
		{
			if (helperTeamInfo_ == null)
			{
				HelperTeamInfo = new BattleResultTeamInfo();
			}
			HelperTeamInfo.MergeFrom(other.HelperTeamInfo);
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
				if (sideInfo_ == null)
				{
					SideInfo = new BattleResultSideInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SideInfo);
				break;
			case 18u:
				if (masterTeamInfo_ == null)
				{
					MasterTeamInfo = new BattleResultTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MasterTeamInfo);
				break;
			case 26u:
				if (helperTeamInfo_ == null)
				{
					HelperTeamInfo = new BattleResultTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HelperTeamInfo);
				break;
			}
		}
	}
}
