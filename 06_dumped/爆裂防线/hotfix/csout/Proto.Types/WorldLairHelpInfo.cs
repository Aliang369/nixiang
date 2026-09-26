using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldLairHelpInfo : IMessage<WorldLairHelpInfo>, IMessage, IEquatable<WorldLairHelpInfo>, IDeepCloneable<WorldLairHelpInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldLairHelpInfo> _parser = new MessageParser<WorldLairHelpInfo>((Func<WorldLairHelpInfo>)(() => new WorldLairHelpInfo()));

	private UnknownFieldSet _unknownFields;

	public const int HelpRoleInfoFieldNumber = 1;

	private RoleBaseInfo helpRoleInfo_;

	public const int HelpLairInfoFieldNumber = 2;

	private WorldLairDetail helpLairInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldLairHelpInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldLairHelpInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo HelpRoleInfo
	{
		get
		{
			return helpRoleInfo_;
		}
		set
		{
			helpRoleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairDetail HelpLairInfo
	{
		get
		{
			return helpLairInfo_;
		}
		set
		{
			helpLairInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairHelpInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairHelpInfo(WorldLairHelpInfo other)
		: this()
	{
		helpRoleInfo_ = ((other.helpRoleInfo_ != null) ? other.helpRoleInfo_.Clone() : null);
		helpLairInfo_ = ((other.helpLairInfo_ != null) ? other.helpLairInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairHelpInfo Clone()
	{
		return new WorldLairHelpInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldLairHelpInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldLairHelpInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(HelpRoleInfo, other.HelpRoleInfo))
		{
			return false;
		}
		if (!object.Equals(HelpLairInfo, other.HelpLairInfo))
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
		if (helpRoleInfo_ != null)
		{
			num ^= HelpRoleInfo.GetHashCode();
		}
		if (helpLairInfo_ != null)
		{
			num ^= HelpLairInfo.GetHashCode();
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
		if (helpRoleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HelpRoleInfo);
		}
		if (helpLairInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HelpLairInfo);
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
		if (helpRoleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HelpRoleInfo);
		}
		if (helpLairInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HelpLairInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldLairHelpInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.helpRoleInfo_ != null)
		{
			if (helpRoleInfo_ == null)
			{
				HelpRoleInfo = new RoleBaseInfo();
			}
			HelpRoleInfo.MergeFrom(other.HelpRoleInfo);
		}
		if (other.helpLairInfo_ != null)
		{
			if (helpLairInfo_ == null)
			{
				HelpLairInfo = new WorldLairDetail();
			}
			HelpLairInfo.MergeFrom(other.HelpLairInfo);
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
				if (helpRoleInfo_ == null)
				{
					HelpRoleInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HelpRoleInfo);
				break;
			case 18u:
				if (helpLairInfo_ == null)
				{
					HelpLairInfo = new WorldLairDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HelpLairInfo);
				break;
			}
		}
	}
}
