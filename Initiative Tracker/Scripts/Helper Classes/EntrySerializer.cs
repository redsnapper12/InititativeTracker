using System.Collections.Generic;
using Godot;

public partial class EntrySerializer : Node
{
    [Export] private FileDialog _saveFileDialog;
    [Export] private FileDialog _loadFileDialog;
    [Export] private string _singleEntryDir;
    [Export] private string _groupEntryDir;
    [Export] private string _builtInEntryDir;
    [Export] private string _rootDir;

    private bool _isSavingSingularEntry = false;
    private bool _isLoadingSingularEntry = false;
    private bool _isLoadingBuiltInEntry = true;


    private InitiativeEntry _entryToSave;
    private List<InitiativeEntry> _entriesToSave = new();
    private InitiativeEntry _entryToReplace;

    private InitiativeTracker _tracker;
    public InitiativeTracker Tracker { set => _tracker = value; }

    public override void _Ready()
    {
        DirAccess.MakeDirRecursiveAbsolute(_rootDir);
        DirAccess.MakeDirRecursiveAbsolute(_singleEntryDir);
        DirAccess.MakeDirRecursiveAbsolute(_groupEntryDir);

        _saveFileDialog.FileSelected += (path) => OnSaveFileSelected(path);
        _loadFileDialog.FileSelected += (path) => OnLoadFileSelected(path);
    }

    public void SaveEntry(InitiativeEntry entryToSave)
    {   
        _loadFileDialog.Access = FileDialog.AccessEnum.Userdata;
        _saveFileDialog.RootSubfolder = _singleEntryDir;

        _entryToSave = entryToSave;
        _saveFileDialog.Popup(); 
    }

    public void SaveEntries(List<InitiativeEntry> entriesToSave)
    {   
        _loadFileDialog.Access = FileDialog.AccessEnum.Userdata;
        _saveFileDialog.RootSubfolder = _groupEntryDir;

        _entriesToSave = entriesToSave;
        _isSavingSingularEntry = false;

        _saveFileDialog.Popup(); 
    }

    public void LoadEntry(InitiativeEntry entryToReplace)
    {
        _loadFileDialog.Access = FileDialog.AccessEnum.Userdata;
        _loadFileDialog.RootSubfolder = _singleEntryDir;

        _entryToReplace = entryToReplace;
        _isLoadingSingularEntry = true;

        _loadFileDialog.Popup();
    }

    public void LoadBuiltInEntry(InitiativeEntry entryToReplace = null)
    {
        _loadFileDialog.Access = FileDialog.AccessEnum.Resources;
        _loadFileDialog.RootSubfolder = _builtInEntryDir;
        _isLoadingSingularEntry = true;

        if(entryToReplace != null) 
        {
            _entryToReplace = entryToReplace;
        }      

        _loadFileDialog.Popup();
    }

    public void LoadEntries()
    {
        _loadFileDialog.Access = FileDialog.AccessEnum.Userdata;
        _loadFileDialog.RootSubfolder = _groupEntryDir;

        _isLoadingSingularEntry = false;
        _loadFileDialog.Popup();
    }

    private void OnSaveFileSelected(string path) 
    {
        EntryGroup entryGroup = new();
        if(!_isSavingSingularEntry)
        {
            foreach (InitiativeEntry entry in _entriesToSave)
            {
                EntryData entryData = new()
                {
                    CharacterName = entry.CharacterName,
                    Initiative = entry.Initiative,
                    DexModifier = entry.DexModifier,
                    AC = entry.AC,
                    HP = entry.HP
                };
                entryGroup.Entries.Add(entryData);
            }
        }
        else
        {
            EntryData entryData = new()
            {
                CharacterName = _entryToSave.CharacterName,
                Initiative = _entryToSave.Initiative,
                DexModifier = _entryToSave.DexModifier,
                AC = _entryToSave.AC,
                HP = _entryToSave.HP
            };

            entryGroup.Entries.Add(entryData);
        }

        if(!path.EndsWith(".tres")) path += ".tres";

        Error error = ResourceSaver.Save(entryGroup, path);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to save entry group to: {path}");
        }
    }

    private void OnLoadFileSelected(string path) 
    {
        if(!ResourceLoader.Exists(path)) return;

        if(!_isLoadingSingularEntry)
        {
            EntryGroup entryGroup = ResourceLoader.Load<EntryGroup>(path);
            if(entryGroup == null) return;

            foreach (EntryData data in entryGroup.Entries)
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
        else 
        {
            EntryData entryData = ResourceLoader.Load<EntryData>(path);
            if(entryData == null) return;

            if(_isLoadingBuiltInEntry) 
            {
                InitiativeEntry newEntry = _tracker.InititativeEntryScene.Instantiate<InitiativeEntry>();
                _tracker.AddEntryToTracker(newEntry);

                newEntry.CharacterName = entryData.CharacterName;
                newEntry.Initiative = entryData.Initiative;
                newEntry.DexModifier = entryData.DexModifier;
                newEntry.AC = entryData.AC;
                newEntry.HP = entryData.HP;
            }
            else 
            {
                _entryToReplace.CharacterName = entryData.CharacterName;
                _entryToReplace.Initiative = entryData.Initiative;
                _entryToReplace.DexModifier = entryData.DexModifier;
                _entryToReplace.AC = entryData.AC;
                _entryToReplace.HP = entryData.HP; 

                _entryToReplace = null;
            }
        }
    }

    private void OnTrackerReady(InitiativeTracker tracker)
    {
        _tracker = tracker;
    }
}
