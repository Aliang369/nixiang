public interface ISelectablePanel
{
	void OnClicked(SelectableModuleBase module)
	{
	}

	void Select(SelectableModuleBase module);

	void Unselect(SelectableModuleBase module);

	bool IsSelected(SelectableModuleBase module);

	bool CanSelect(SelectableModuleBase module)
	{
		return true;
	}

	bool IsSelectDark(SelectableModuleBase module)
	{
		if (!CanSelect(module))
		{
			return !IsSelected(module);
		}
		return false;
	}
}
