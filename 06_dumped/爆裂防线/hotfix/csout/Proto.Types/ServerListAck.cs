using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ServerListAck : IMessage<ServerListAck>, IMessage, IEquatable<ServerListAck>, IDeepCloneable<ServerListAck>, IBufferMessage
{
	private static readonly MessageParser<ServerListAck> _parser = new MessageParser<ServerListAck>((Func<ServerListAck>)(() => new ServerListAck()));

	private UnknownFieldSet _unknownFields;

	public const int ServerIdFieldNumber = 1;

	private int serverId_;

	public const int ServerListFieldNumber = 2;

	private static readonly FieldCodec<ServerDetail> _repeated_serverList_codec = FieldCodec.ForMessage<ServerDetail>(18u, ServerDetail.Parser);

	private readonly RepeatedField<ServerDetail> serverList_ = new RepeatedField<ServerDetail>();

	public const int RoleListFieldNumber = 3;

	private static readonly FieldCodec<RoleBaseInfo> _repeated_roleList_codec = FieldCodec.ForMessage<RoleBaseInfo>(26u, RoleBaseInfo.Parser);

	private readonly RepeatedField<RoleBaseInfo> roleList_ = new RepeatedField<RoleBaseInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ServerListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeServerListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ServerId
	{
		get
		{
			return serverId_;
		}
		set
		{
			serverId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ServerDetail> ServerList => serverList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RoleBaseInfo> RoleList => roleList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServerListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServerListAck(ServerListAck other)
		: this()
	{
		serverId_ = other.serverId_;
		serverList_ = other.serverList_.Clone();
		roleList_ = other.roleList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServerListAck Clone()
	{
		return new ServerListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ServerListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ServerListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ServerId != other.ServerId)
		{
			return false;
		}
		if (!serverList_.Equals(other.serverList_))
		{
			return false;
		}
		if (!roleList_.Equals(other.roleList_))
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
		if (ServerId != 0)
		{
			num ^= ServerId.GetHashCode();
		}
		num ^= ((object)serverList_).GetHashCode();
		num ^= ((object)roleList_).GetHashCode();
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
		if (ServerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ServerId);
		}
		serverList_.WriteTo(ref output, _repeated_serverList_codec);
		roleList_.WriteTo(ref output, _repeated_roleList_codec);
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
		if (ServerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerId);
		}
		num += serverList_.CalculateSize(_repeated_serverList_codec);
		num += roleList_.CalculateSize(_repeated_roleList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ServerListAck other)
	{
		if (other != null)
		{
			if (other.ServerId != 0)
			{
				ServerId = other.ServerId;
			}
			serverList_.Add((IEnumerable<ServerDetail>)other.serverList_);
			roleList_.Add((IEnumerable<RoleBaseInfo>)other.roleList_);
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
				ServerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				serverList_.AddEntriesFrom(ref input, _repeated_serverList_codec);
				break;
			case 26u:
				roleList_.AddEntriesFrom(ref input, _repeated_roleList_codec);
				break;
			}
		}
	}
}
