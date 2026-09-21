public abstract class StoringModule
{
	public abstract object storeObject { get; }

	public abstract void OnSave();

	public abstract void LoadValueData();

	public abstract void LoadReferenceData();
}
