using System.Collections.Generic;

public class TestMenuConfigData
{
	public bool startUnlockAll;

	public List<ItemDefinition> initDeck;

	public PlayerCharacterDef selectedCharacter;

	public DifficultyLevel selectedDifficulty;

	public ContractDef selectedContract;

	public FixedContractRouteDef fixedContractRoute;

	public RoomRef selectedRoom;

	public TestMenuSelectEncounterOption selectEncounterOption;

	public int initEnemyLevel = 1;

	public ImportedTestSetup initImportedTestSetup;
}
