public class zbSubjectData
{
	public string iconPath;

	public string strTitle;

	public string strDesc;

	public float width;

	public float height;

	public float titleTop;

	public float descTop;

	public float marginBottom;

	public zbSubjectData(string _iconPath, string _strTitle, string _strDesc, float _width = 0f, float _height = 0f, float _titleTop = 0f, float _descTopc = 0f, float _marginBottom = 0f)
	{
		iconPath = _iconPath;
		strTitle = _strTitle;
		strDesc = _strDesc;
		width = _width;
		height = _height;
		titleTop = _titleTop;
		descTop = _descTopc;
		marginBottom = _marginBottom;
	}
}
