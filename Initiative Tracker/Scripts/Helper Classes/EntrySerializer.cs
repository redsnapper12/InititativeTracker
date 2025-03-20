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
	[Export] private ItemList _customEntriesItemList = null;
	[Export] private ItemList _customEncountersItemList = null;
	[Export] private ItemList _standardRulesMonstersItemList = null;
	[Export] private ItemList _builtInEncountersItemList = null;
	[Export] private Button _closeMenuButton = null;

	[ExportGroup("Encounter Save Dialog")]
	[Export] private Window _encounterSaveDialog;
	[Export] private LineEdit _encounterNameEdit;
	[Export] private Button _confirmEncounterSaveButton;
	[Export] private Button _cancelEncounterSaveButton;

	[ExportGroup("Entry Save Dialog")]
	[Export] private ConfirmationDialog _entrySaveDialog;

	[ExportGroup("Paths")]
	[Export] private string _customEntriesDirectory;
	[Export] private string _customEncountersDirectory;
	[Export] private string _standardRulesMonstersDirectory;
	[Export] private string _builtInEncountersDirectory;
	
	private List<string> _customEntryPaths = new();
	private List<string> _customEncounterPaths = new();
	private List<string> _standardRulesMonstersPaths = new();
	private List<string> _builtInEcounterPaths = new();
	

	public override void _Ready()
	{
		if(!DirAccess.DirExistsAbsolute(_customEntriesDirectory)) DirAccess.MakeDirRecursiveAbsolute(_customEntriesDirectory);
		if(!DirAccess.DirExistsAbsolute(_customEncountersDirectory)) DirAccess.MakeDirRecursiveAbsolute(_customEncountersDirectory);

		InitEvents();
		ConstructItemLists();
	}

	public void PromptLoad() 
	{
		_loadMenu.Show();
	}

	public void PromptEncounterSave() 
	{
		_encounterSaveDialog.PopupCentered();
	}

	/// <summary>
	/// Displays a confirmation prompt for the user 
	/// </summary>
	/// <param name="entry"></param>
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

	/// <summary>
	/// Loads an XML file from a path, converts it to an entry and adds it to the tracker.
	/// </summary>
	/// <param name="path"></param>
	private void LoadEntry(string path) 
	{
		XmlParser parser = new();
		GD.Print(path);
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
		string path = _customEntriesDirectory + entry.CharacterName + ".xml";
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		
		if (file == null)
		{
			Error error = Godot.FileAccess.GetOpenError();
			GD.PrintErr($"Failed to open file for writing: {entry.CharacterName}, Error: {error}");
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
		GD.Print($"Entry saved to {path}");
	}

	private void SaveEncounter(List<InitiativeEntry> encounter, string name) 
	{
		string path = _customEncountersDirectory + name + ".xml";
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		
		if (file == null)
		{
			Error error = Godot.FileAccess.GetOpenError();
			GD.PrintErr($"Failed to open file for writing: {name}, Error: {error}");
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
		GD.Print($"Encounter saved to {path}");
	}


	private void ConstructItemLists() 
	{
		_standardRulesMonstersPaths = GetXMLPathsFromDir(_standardRulesMonstersDirectory);
		_builtInEcounterPaths = GetXMLPathsFromDir(_builtInEncountersDirectory);

		_customEntryPaths = GetXMLPathsFromDir(_customEntriesDirectory);
		_customEncounterPaths = GetXMLPathsFromDir(_customEncountersDirectory);

		// Fill out all item lists with the paths from the directories above
        FillItemList(_standardRulesMonstersItemList, _standardRulesMonstersPaths);
		FillItemList(_builtInEncountersItemList, _builtInEcounterPaths);

        FillItemList(_customEntriesItemList, _customEntryPaths);
		FillItemList(_customEncountersItemList, _customEncounterPaths);
	}

	private static List<string> GetXMLPathsFromDir(string path) 
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

	private static void FillItemList(ItemList list, List<string> paths) 
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
	}

	private void AddToItemList(ItemList list, string path, bool isEncounter) 
	{
		string fileName = path.GetFile().TrimSuffix(".xml");

		if(fileName != null) 
		{
			list.AddItem(fileName);
			list.SortItemsByText();

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

	private void RemoveFromItemList(ItemList list, string path, bool isEncounter) 
	{
		string fileName = path.GetFile().TrimSuffix(".xml");

		for (int i = 0; i < list.ItemCount; i++)
		{
			if(list.GetItemText(i) == fileName)
			{
				list.RemoveItem(i);
				list.SortItemsByText();

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

				return;
			} 
		}

		GD.PrintErr("Not present in item list: " + path);
	}


	// Loading
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
		LoadEncounter(_builtInEcounterPaths[index]);
		_loadMenu.Hide();
	}
	
	private void OnCloseLoadMenu()
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		_loadMenu.Hide();
	}


	// Saving
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

	private void InitEvents() 
	{
		_customEntriesItemList.ItemActivated += (idx) => OnCustomEntryItemActivated((int)idx);
		_customEncountersItemList.ItemActivated += (idx) => OnCustomEncounterItemActivated((int)idx);

		_standardRulesMonstersItemList.ItemActivated += (idx) => OnStandardRulesMonsterItemActivated((int)idx);
		_builtInEncountersItemList.ItemActivated += (idx) => OnBuiltInEncounterItemActivated((int)idx);

		_closeMenuButton.Pressed += () => OnCloseLoadMenu();

		_encounterSaveDialog.CloseRequested += () => OnCloseEncounterSaveDialog();
		_cancelEncounterSaveButton.Pressed += () => OnCloseEncounterSaveDialog();

		_confirmEncounterSaveButton.Pressed += () => OnEncounterSaveDialogSubmitted();
		_encounterNameEdit.TextSubmitted += (text) => OnEncounterSaveDialogSubmitted();
	}
}