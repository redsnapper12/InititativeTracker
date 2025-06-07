using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

public partial class EntrySerializer : Control
{
	[ExportGroup("Control Nodes")]
	[Export] private InitiativeTracker _tracker;

	[ExportGroup("Load Menu")]
	[Export] private Control _loadMenu;
	[Export] private TabContainer _tabContainer;
	[Export] private CustomItemList _customEntriesItemList = null;
	[Export] private CustomItemList _customEncountersItemList = null;
	[Export] private CustomItemList _standardRulesMonstersItemList = null;
	[Export] private CustomItemList _standardEncountersItemList = null;
	[Export] private Button _closeMenuButton = null;
	[Export] private Button _loadSelectedButton = null;
	[Export] private Button _deleteSelectedButton = null;

	[ExportGroup("Encounter Save Dialog")]
	[Export] private Window _encounterSaveDialog;
	[Export] private LineEdit _encounterNameEdit;
	[Export] private Button _confirmEncounterSaveButton;
	[Export] private Button _cancelEncounterSaveButton;

	[ExportGroup("Entry Save Dialog")]
	[Export] private ConfirmationDialog _entrySaveDialog;

	[ExportGroup("Deletion Dialog")]
	[Export] private ConfirmationDialog _deletionDialog;

	[ExportGroup("Paths")]
	[Export] private string _customEntriesDirectory;
	[Export] private string _customEncountersDirectory;
	[Export] private string _standardRulesMonstersDirectory;
	[Export] private string _standardEncountersDirectory;
	
	private List<string> _customEntryPaths = [];
	private List<string> _customEncounterPaths = [];
	private List<string> _standardRulesMonstersPaths = [];
	private List<string> _standardEcounterPaths = [];
	private List<int> _deletionRestrictedTabs = [];
	List<int> _entryTabIndices = []; 


	public override void _Ready()
	{
		if(!DirAccess.DirExistsAbsolute(_customEntriesDirectory)) DirAccess.MakeDirRecursiveAbsolute(_customEntriesDirectory);
		if(!DirAccess.DirExistsAbsolute(_customEncountersDirectory)) DirAccess.MakeDirRecursiveAbsolute(_customEncountersDirectory);

		InitEvents();
		ConstructItemLists();

		// Restrict file deletion on the built-in tabs
		_deletionRestrictedTabs.Add(_standardRulesMonstersItemList.GetIndex());
		_deletionRestrictedTabs.Add(_standardEncountersItemList.GetIndex());

		// Ensure the delete button in the load menu is disabled if it starts on a restriced tab
		if(_deletionRestrictedTabs.Contains(_tabContainer.CurrentTab)) _deleteSelectedButton.Disabled = true;

		// Which tabs are for entries
		_entryTabIndices = [_standardRulesMonstersItemList.GetIndex(), _customEntriesItemList.GetIndex()];
	}

	public void PromptLoad() 
	{
		_loadMenu.Show();
	}

	public void PromptEncounterSave() 
	{
		_encounterSaveDialog.PopupCentered();
	}

	public async void PromptEntrySave(InitiativeEntry entry) 
	{
		_entrySaveDialog.PopupCentered();

		await ToSignal(_entrySaveDialog.GetOkButton(), "pressed");

		if(string.IsNullOrWhiteSpace(entry.CharacterName))
		{
			entry.CharacterName = "Unnamed Entry";
			SaveEntry(entry);
		}
		else
		{
			SaveEntry(entry);
		}

		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);

		_encounterSaveDialog.Hide();
	}

	private void PromptDeletion() 
	{
		int customEntriesTabIndex = _customEntriesItemList.GetIndex();

		if(_tabContainer.CurrentTab == customEntriesTabIndex) 
		{
			if (_customEntriesItemList.GetSelectedItems().Length <= 0) return;

			int index = _customEntriesItemList.GetSelectedItems()[0];
			string path = _customEntryPaths[index];
			DeleteCustomEntry(path);
		}
		else 
		{
			if (_customEncountersItemList.GetSelectedItems().Length <= 0) return;

			int index = _customEncountersItemList.GetSelectedItems()[0];
			string path = _customEncounterPaths[index];
			DeleteCustomEncounter(path);
		}

		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
	}

	// File Manipulation
	private List<string> GetXMLPathsFromDir(string path) 
	{
		List<string> tmp = [];

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
			GD.PrintErr("An error occurred when trying to access the custom path.");
			return null;
		}
	}

	string GetUniqueFilePath(string characterName, bool isEncounter)
	{
		string dir = isEncounter ? _customEncountersDirectory : _customEntriesDirectory;

		string basePath = dir + characterName;
		string originalPath = basePath + ".xml";
		
		// If the file doesn't exist yet, we can use the original name
		if (!Godot.FileAccess.FileExists(originalPath))
		{
			return originalPath;
		}
		
		// File exists, so we need to find a unique name
		int index = 1;
		string newPath;
		
		// Keep trying new filenames until we find one that doesn't exist
		do
		{
			newPath = $"{basePath}({index}).xml";
			index++;
		} while (Godot.FileAccess.FileExists(newPath));
		
		return newPath;
	}

	private void LoadEntry(string path) 
	{
		XmlParser parser = new();
		parser.Open(path);

		InitiativeEntry newEntry = _tracker.InititativeEntryScene.Instantiate<InitiativeEntry>();

		while (parser.Read() != Error.FileEof)
		{
			if (parser.GetNodeType() == XmlParser.NodeType.Element && parser.GetNodeName() != "monster")
			{
				var nodeName = parser.GetNodeName();

				parser.Read();

				if (parser.GetNodeType() == XmlParser.NodeType.Text)
                {
                    string nodeData = parser.GetNodeData();

					switch (nodeName)
					{
						case "name": 
							newEntry.CharacterName = nodeData;
							break;
						case "initiative":
							newEntry.Initiative = nodeData.ToInt();
							break;
						case "dexModifier":
							newEntry.DexModifier = nodeData.ToInt();
							break;
						case "armorClass":
							newEntry.AC = nodeData.ToInt();
							break;
						case "healthPoints":
							newEntry.HP = nodeData.ToInt();
							break;	
					}
				}	
			}
		}

		_tracker.AddEntryToTracker(newEntry);
	}

	private void LoadEncounter(string path) 
	{
		XmlParser parser = new();
		parser.Open(path);
		
		// Track if we're inside a monster element
		bool insideMonster = false;
		InitiativeEntry currentEntry = null;
		
		while (parser.Read() != Error.FileEof)
		{
			if (parser.GetNodeType() == XmlParser.NodeType.Element)
			{
				string elementName = parser.GetNodeName();
				
				if (elementName == "monster")
				{
					// Create a new entry when we find a monster tag
					insideMonster = true;
					currentEntry = _tracker.InititativeEntryScene.Instantiate<InitiativeEntry>();
					continue;
				}
				
				// Only process child elements if we're inside a monster
				if (insideMonster && elementName != "encounter")
				{
					string nodeName = elementName;
					parser.Read();
					
					if (parser.GetNodeType() == XmlParser.NodeType.Text)
					{
						string nodeData = parser.GetNodeData().Trim();
						
						switch (nodeName)
						{
							case "name":
								currentEntry.CharacterName = nodeData;
								break;
							case "initiative":
								currentEntry.Initiative = nodeData.ToInt();
								break;
							case "dexModifier":
								currentEntry.DexModifier = nodeData.ToInt();
								break;
							case "armorClass":
								currentEntry.AC = nodeData.ToInt();
								break;
							case "healthPoints":
								currentEntry.HP = nodeData.ToInt();
								break;
						}
					}
				}
			}
			else if (parser.GetNodeType() == XmlParser.NodeType.ElementEnd && parser.GetNodeName() == "monster")
			{
				// We've reached the end of a monster element, add it to the tracker
				if (currentEntry != null)
				{
					_tracker.AddEntryToTracker(currentEntry);
					currentEntry = null;
				}
				insideMonster = false;
			}
		}
		
		// In case we have an open entry at the end of the file
		if (insideMonster && currentEntry != null)
		{
			_tracker.AddEntryToTracker(currentEntry);
		}
	}

	private void SaveEntry(InitiativeEntry entry) 
	{
		string path = GetUniqueFilePath(entry.CharacterName, false);
		GD.Print(path);

		if(Godot.FileAccess.FileExists(path)) 
		{
			path.TrimSuffix(".xml");

			int index = 1;
			string suffix = ".xml";
			while (Godot.FileAccess.FileExists(path))
			{
				path.TrimSuffix(suffix);
				suffix = $"({index}).xml";
				path += suffix;
				index++;
			}
		}
		
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		
		if (file == null)
		{
			Error error = Godot.FileAccess.GetOpenError();
			GD.PrintErr($"Failed to open file for writing: {path}, Error: {error}");
			return;
		}
		
		// Write XML header
		file.StoreLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");		
		file.StoreLine("<monster>");
		file.StoreLine($"    <name>{entry.CharacterName}</name>");
		file.StoreLine($"    <initiative>{0}</initiative>");
		file.StoreLine($"    <dexModifier>{entry.DexModifier}</dexModifier>");
		file.StoreLine($"    <armorClass>{entry.AC}</armorClass>");
		file.StoreLine($"    <healthPoints>{entry.HP}</healthPoints>");
		file.StoreLine("</monster>");
		
		AddToItemList(_customEntriesItemList, path, false);
		GD.Print($"Entry successfully saved to {path}");
	}

	private void SaveEncounter(List<InitiativeEntry> encounter, string name) 
	{
		string path = GetUniqueFilePath(name, true);
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		
		if (file == null)
		{
			Error error = Godot.FileAccess.GetOpenError();
			GD.PrintErr($"Failed to open file for writing: {path}, Error: {error}");
			return;
		}
		
		// Write XML header
		file.StoreLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
		file.StoreLine("<encounter>");
		
		foreach (InitiativeEntry entry in encounter)
		{
			file.StoreLine("    <monster>");
			file.StoreLine($"        <name>{entry.CharacterName}</name>");
			file.StoreLine($"        <initiative>{0}</initiative>");
			file.StoreLine($"        <dexModifier>{entry.DexModifier}</dexModifier>");
			file.StoreLine($"        <armorClass>{entry.AC}</armorClass>");
			file.StoreLine($"        <healthPoints>{entry.HP}</healthPoints>");
			file.StoreLine("    </monster>");
		}
		
		// Close the encounter tag
		file.StoreLine("</encounter>");
		
		AddToItemList(_customEncountersItemList, path, true);
		GD.Print($"Encounter successfully saved to {path}");
	}

	private async void DeleteCustomEntry(string path) 
	{
		// Confirmation Popup
		_deletionDialog.PopupCentered();
		await ToSignal(_deletionDialog.GetOkButton(), "pressed");

		// Deletion
		DirAccess.RemoveAbsolute(path);
		_customEntryPaths.Remove(path);
		RemoveFromItemList(_customEntriesItemList, path, false);
		_customEntriesItemList.DeselectAll();

		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIDelete);

		GD.Print($"Entry successfully deleted at {path}");
	}

	
	private async void DeleteCustomEncounter(string path) 
	{
		// Confirmation Popup
		_deletionDialog.PopupCentered();
		await ToSignal(_deletionDialog.GetOkButton(), "pressed");

		// Deletion
		DirAccess.RemoveAbsolute(path);
		_customEncounterPaths.Remove(path);
		RemoveFromItemList(_customEncountersItemList, path, true);
		_customEncountersItemList.DeselectAll();

		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIDelete);

		GD.Print($"Encounter successfully deleted at {path}");
	}

	// Item List Manipulation
	private void ConstructItemLists() 
	{
		_standardRulesMonstersPaths = GetXMLPathsFromDir(_standardRulesMonstersDirectory);
		_standardEcounterPaths = GetXMLPathsFromDir(_standardEncountersDirectory);

		_customEntryPaths = GetXMLPathsFromDir(_customEntriesDirectory);
		_customEncounterPaths = GetXMLPathsFromDir(_customEncountersDirectory);

		// Fill out all item lists with the paths from the directories above
        FillItemList(_standardRulesMonstersItemList, _standardRulesMonstersPaths);
		FillItemList(_standardEncountersItemList, _standardEcounterPaths);

        FillItemList(_customEntriesItemList, _customEntryPaths);
		FillItemList(_customEncountersItemList, _customEncounterPaths);
	}

	private void FillItemList(CustomItemList list, List<string> paths) 
	{
		foreach (string path in paths)
		{
			string fileName = path.GetFile().TrimSuffix(".xml");

			if(fileName != null) 
			{
				list.AddItem(fileName);
			}
			else 
			{
				GD.PrintErr("Could not find file at: " + path);
			}
		}

		list.SortItemsByText();
		list.RefreshColors();
	}

	private void AddToItemList(CustomItemList list, string path, bool isEncounter) 
	{
		string fileName = path.GetFile().TrimSuffix(".xml");

		if(fileName != null) 
		{
			list.AddItem(fileName);
			list.SortItemsByText();
			list.RefreshColors();

			if(isEncounter) 
			{
				_customEncounterPaths.Add(path);
				_customEncounterPaths.Sort();
			} 
			else
			{
				_customEntryPaths.Add(path);
				_customEntryPaths.Sort();
			}
		}
		else 
		{
			GD.PrintErr("Could not find file at: " + path);
		}
	}

	private void RemoveFromItemList(CustomItemList list, string path, bool isEncounter) 
	{
		string fileName = path.GetFile().TrimSuffix(".xml");

		for (int i = 0; i < list.ItemCount; i++)
		{
			if(list.GetItemText(i) == fileName)
			{
				list.RemoveItem(i);
				list.SortItemsByText();
				list.RefreshColors();

				if(isEncounter)
				{
					_customEncounterPaths.Remove(path);
					_customEncounterPaths.Sort();
				} 
				else
				{
					_customEntryPaths.Remove(path);
					_customEntryPaths.Sort();
				}

				return;
			} 
		}

		GD.PrintErr("Not present in item list: " + path);
	}


	// Load Menu Events
	private void OnTabChanged(int tabIdx)
    {
    	_deleteSelectedButton.Disabled = _deletionRestrictedTabs.Contains(tabIdx);
    }

	private void OnLoadSelected() 
	{
		GD.Print("Called");
		if(_entryTabIndices.Contains(_tabContainer.CurrentTab)) 
		{
			if (_customEntriesItemList.GetSelectedItems().Length <= 0) return;

			int index = _customEntriesItemList.GetSelectedItems()[0];
			string path = _customEntryPaths[index];
			LoadEntry(path);
		}
		else 
		{
			if (_customEncountersItemList.GetSelectedItems().Length <= 0) return;

			int index = _customEncountersItemList.GetSelectedItems()[0];
			string path = _customEncounterPaths[index];
			LoadEncounter(path);
		}

		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		_loadMenu.Hide();
	}

	private void OnCustomEntryItemActivated(int index) 
	{
		LoadEntry(_customEntryPaths[index]);
		_loadMenu.Hide();
	}

	private void OnCustomEncounterItemActivated(int index) 
	{
		LoadEncounter(_customEncounterPaths[index]);
		_loadMenu.Hide();
	}

	private void OnStandardRulesMonsterItemActivated(int index) 
	{
		LoadEntry(_standardRulesMonstersPaths[index]);
		_loadMenu.Hide();
	}

	private void OnBuiltInEncounterItemActivated(int index) 
	{
		LoadEncounter(_standardEcounterPaths[index]);
		_loadMenu.Hide();
	}
	
	private void OnCloseLoadMenu()
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		_loadMenu.Hide();
	}


	// Encounter Save Events
	private void OnCloseEncounterSaveDialog() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		_encounterSaveDialog.Hide();
	}

	private void OnEncounterSaveDialogSubmitted() 
	{
		if(string.IsNullOrWhiteSpace(_encounterNameEdit.Text))
		{
			SaveEncounter(_tracker.Entries, "Unnamed Encounter");
		}
		else
		{
			SaveEncounter(_tracker.Entries, _encounterNameEdit.Text);
		}

		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);

		_encounterSaveDialog.Hide();
	}


	// Event Initialization
	private void InitEvents() 
	{
		_customEntriesItemList.ItemActivated += (index) => OnCustomEntryItemActivated((int)index);
		_customEncountersItemList.ItemActivated += (index) => OnCustomEncounterItemActivated((int)index);

		_standardRulesMonstersItemList.ItemActivated += (index) => OnStandardRulesMonsterItemActivated((int)index);
		_standardEncountersItemList.ItemActivated += (index) => OnBuiltInEncounterItemActivated((int)index);

		_tabContainer.TabChanged += (tabIdx) => OnTabChanged((int)tabIdx);
		_deleteSelectedButton.Pressed += () => PromptDeletion();
		_loadSelectedButton.Pressed += () => OnLoadSelected();
		_closeMenuButton.Pressed += () => OnCloseLoadMenu();

		_encounterSaveDialog.CloseRequested += () => OnCloseEncounterSaveDialog();
		_cancelEncounterSaveButton.Pressed += () => OnCloseEncounterSaveDialog();

		_confirmEncounterSaveButton.Pressed += () => OnEncounterSaveDialogSubmitted();
		_encounterNameEdit.TextSubmitted += (text) => OnEncounterSaveDialogSubmitted();
	}
}