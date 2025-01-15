using System.Collections.Generic;
using System.IO;
using Godot;

public partial class EntrySerializer : Control
{
	[ExportGroup("Control Nodes")]
	[Export] private InitiativeTracker _tracker;
	[Export] private ItemList _customItemList = null;
	[Export] private ItemList _customEncountersItemList = null;
	[Export] private ItemList _standardRulesItemList = null;
	[Export] private ItemList _standardRulesEncountersItemList = null;
	[Export] private Button _closeMenuButton = null;

	[ExportGroup("Paths")]
	[Export] private string _customSavesDirectory;
	[Export] private string _customEncountersDirectory;
	[Export] private string _standardRulesMonstersDirectory;
	[Export] private string _standardRulesEncountersDirectory;
	

	private List<string> _customSavesPaths = new();
	private List<string> _customEncountersPaths = new();
	private List<string> _standardRulesMonstersPaths = new();
	private List<string> _standardRulesEncountersPaths = new();
	

	public override void _Ready()
	{
		if(!DirAccess.DirExistsAbsolute(_customSavesDirectory)) DirAccess.MakeDirAbsolute(_customSavesDirectory);
		if(!DirAccess.DirExistsAbsolute(_customEncountersDirectory)) DirAccess.MakeDirAbsolute(_customEncountersDirectory);

		_customItemList.ItemActivated += (idx) => OnCustomItemActivated((int)idx);
		_customEncountersItemList.ItemActivated += (idx) => OnCustomEncounterItemActivated((int)idx);

		_standardRulesItemList.ItemActivated += (idx) => OnStandardRulesItemActivated((int)idx);
		_standardRulesEncountersItemList.ItemActivated += (idx) => OnStandardRulesEncounterItemActivated((int)idx);
		
		_closeMenuButton.Pressed += () => OnCloseMenuButtonPressed();

		ConstructItemLists();
	}

	public void PromptLoad() 
	{
		Show();
	}

	public void PromptSave() 
	{

	}

	/// <summary>
	/// Loads an entry from a path
	/// </summary>
	/// <param name="path"></param>
	private void LoadEntry(string path) 
	{
		if(!ResourceLoader.Exists(path)) return;

		EntryData entryData = ResourceLoader.Load<EntryData>(path);

		if(entryData != null)
		{
			InitiativeEntry newEntry = _tracker.InititativeEntryScene.Instantiate<InitiativeEntry>();
			_tracker.AddEntryToTracker(newEntry);
			FillEntry(newEntry, entryData);
		}
	}

	private void LoadEncounter(string path) 
	{
		if(!ResourceLoader.Exists(path)) return;

		EncounterData encounterData = ResourceLoader.Load<EncounterData>(path);

		if(encounterData != null)
		{
			foreach (EntryData entryData in encounterData.Encounter)
			{
				InitiativeEntry newEntry = _tracker.InititativeEntryScene.Instantiate<InitiativeEntry>();
				_tracker.AddEntryToTracker(newEntry);
				FillEntry(newEntry, entryData);
			}
		}
	}

	private void SaveEntry(InitiativeEntry entry) 
	{
		
	}

	private void SaveEntries(List<InitiativeEntry> entry) 
	{

	}


	private void ConstructItemLists() 
	{
		_customSavesPaths = GetEntryPathsFromDir(_customSavesDirectory);
		_customEncountersPaths = GetEntryPathsFromDir(_customEncountersDirectory);
		_standardRulesMonstersPaths = GetEntryPathsFromDir(_standardRulesMonstersDirectory);
		_standardRulesEncountersPaths = GetEntryPathsFromDir(_standardRulesEncountersDirectory);
		
		FillEntryItemList(_customItemList, _customSavesPaths);
		FillEncounterItemList(_customEncountersItemList, _customEncountersPaths);
		FillEntryItemList(_standardRulesItemList, _standardRulesMonstersPaths);
		FillEncounterItemList(_standardRulesEncountersItemList, _standardRulesEncountersPaths);
	}

	private void FillEntry(InitiativeEntry entry, EntryData data) 
	{
		entry.CharacterName = data.CharacterName;
		entry.Initiative = data.Initiative;
		entry.DexModifier = data.DexModifier;
		entry.AC = data.AC;
		entry.HP = data.HP;
	}

	private List<string> GetEntryPathsFromDir(string path) 
	{
		List<string> tmp = new();

		using DirAccess dir = DirAccess.Open(path);
		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (fileName != "")
			{
				string entry_path = path + fileName;
				tmp.Add(entry_path);
				fileName = dir.GetNext();
			}

			return tmp;
		}
		else
		{
			GD.Print("An error occurred when trying to access the custom path.");
			return null;
		}
	}

	private void FillEntryItemList(ItemList list, List<string> entry_paths) 
	{
		foreach (string path in entry_paths)
		{
			EntryData data = ResourceLoader.Load<EntryData>(path);

			if(data != null) 
			{
				list.AddItem(data.CharacterName);
			}
			else 
			{
				GD.PrintErr("Could not find entry at: " + path);
			}
		}
	}

	private void FillEncounterItemList(ItemList list, List<string> entry_paths) 
	{
		foreach (string path in entry_paths)
		{
			EncounterData data = ResourceLoader.Load<EncounterData>(path);

			if(data != null) 
			{
				list.AddItem(data.EncounterName);
			}
			else 
			{
				GD.PrintErr("Could not find entry at: " + path);
			}
		}
	}

	private void OnCustomItemActivated(int index) 
	{
		LoadEntry(_customSavesPaths[index]);
		Hide();
	}

	private void OnCustomEncounterItemActivated(int index) 
	{
		LoadEncounter(_customEncountersPaths[index]);
		Hide();
	}

	private void OnStandardRulesItemActivated(int index) 
	{
		LoadEntry(_standardRulesMonstersPaths[index]);
		Hide();
	}

	private void OnStandardRulesEncounterItemActivated(int index) 
	{
		LoadEncounter(_standardRulesEncountersPaths[index]);
		Hide();
	}

	private void OnCloseMenuButtonPressed() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		Hide();
	}
}
