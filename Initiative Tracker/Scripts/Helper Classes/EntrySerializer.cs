using System.Collections.Generic;
using Godot;

public partial class EntrySerializer : Control
{
    [Export] private ItemList _customList = null;
	[Export] private ItemList _builtInList = null;
	[Export] private string _builtInEntryDir;
    [Export] private string _rootDir;
    [Export] private InitiativeTracker _tracker;

    private List<string> _builtInPaths = new();
	private List<string> _customPaths = new();

    public override void _Ready()
    {
        _builtInList.ItemActivated += (idx) => OnBuiltInItemActivated((int)idx);
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
		using DirAccess dir = DirAccess.Open(_builtInEntryDir);
		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (fileName != "")
			{
				string path = _builtInEntryDir + fileName;
				_builtInPaths.Add(path);

				EntryData data = ResourceLoader.Load<EntryData>(path);
				_builtInList.AddItem(data.CharacterName);

				fileName = dir.GetNext();
			}

			foreach (string path in _builtInPaths)
			{
				GD.Print(path);
			}
		}
		else
		{
			GD.Print("An error occurred when trying to access the path.");
		}
	}

    private void OnBuiltInItemActivated(int index) 
	{
		LoadEntry(_builtInPaths[index]);
        Hide();
	}
}
