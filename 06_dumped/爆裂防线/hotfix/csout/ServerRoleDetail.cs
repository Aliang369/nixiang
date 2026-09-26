using Proto.Types;

public class ServerRoleDetail
{
	public ServerDetail m_serverDetail;

	public RoleBaseInfo m_roleInfo;

	public ServerRoleDetail(ServerDetail serverDetail, RoleBaseInfo roleInfo)
	{
		m_serverDetail = serverDetail;
		m_roleInfo = roleInfo;
	}
}
