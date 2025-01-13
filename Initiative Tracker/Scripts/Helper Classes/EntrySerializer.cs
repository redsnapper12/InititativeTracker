using System.Collections.Generic;
using Godot;

public partial class EntrySerializer : Control
{
	[Export] private ItemList _customList = null;
	[Export] private ItemList _standardRulesList = null;
	[Export] private Button _closeMenuButton = null;
	[Export] private string standardRulesEntriesDir;
	[Export] private string _customSaveDir;
	[Export] private InitiativeTracker _tracker;

	private List<string> _standardRulesPaths = new();
	private List<string> _customPaths = new();

	public override void _Ready()
	{
		if(!DirAccess.DirExistsAbsolute(_customSaveDir)) 
		{
			DirAccess.MakeDirAbsolute(_customSaveDir);
		}

		_standardRulesList.ItemActivated += (idx) => OnstandardRulesItemActivated((int)idx);
		_customList.ItemActivated += (idx) => OnCustomItemActivated((int)idx);
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

		EntryData data = ResourceLoader.Load<EntryData>(path);

		if(data != null)
		{
			InitiativeEntry newEntry = _tracker.InititativeEntryScene.Instantiate<InitiativeEntry>();
			_tracker.AddEntryToTracker(newEntry);
			newEntry.CharacterName = data.CharacterName;
			newEntry.Initiative = data.Initiative;
			newEntry.DexModifier = data.DexModifier;
			newEntry.AC = data.AC;
			newEntry.HP = data.HP;
		}
	}

	/// <summary>
	/// Saves an entry to the user saves folder
	/// </summary>
	/// <param name="entry"></param>
	private void SaveEntry(InitiativeEntry entry) 
	{
		
	}

	private void SaveEntries(List<InitiativeEntry> entry) 
	{

	}


	private void ConstructItemLists() 
	{
		_customPaths = GetEntryPathsFromDir(_customSaveDir);
		_standardRulesPaths = GetEntryPathsFromDir(standardRulesEntriesDir);
		
		FillItemList(_customList, _customPaths);
		FillItemList(_standardRulesList, _standardRulesPaths);
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

	private void FillItemList(ItemList list, List<string> entry_paths) 
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

	private void OnstandardRulesItemActivated(int index) 
	{
		LoadEntry(_standardRulesPaths[index]);
		Hide();
	}

	private void OnCustomItemActivated(int index) 
	{
		LoadEntry(_customPaths[index]);
		Hide();
	}

	private void OnCloseMenuButtonPressed() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		Hide();
	}
}
