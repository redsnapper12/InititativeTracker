using Godot;
using System;
using System.Collections.Generic;

public partial class EntrySerializer
{
    private readonly string _savePath = "saves";
    private FileDialog _saveFileDialog;
    private FileDialog _loadFileDialog;
    private InitiativeTracker _tracker;

    public EntrySerializer(InitiativeTracker tracker, FileDialog saveFileDialog, FileDialog loadFileDialog)
    {
        _tracker = tracker;
        _saveFileDialog = saveFileDialog;
        _loadFileDialog = loadFileDialog;
        
        DirAccess.MakeDirRecursiveAbsolute(_savePath);

        _saveFileDialog.FileSelected += (path) => OnSaveFileSelected(path);
        _loadFileDialog.FileSelected += (path) => OnLoadFileSelected(path);

        _saveFileDialog.Filters = new string[] { "*.tres ; Resource Files" };
        _loadFileDialog.Filters = new string[] { "*.tres ; Resource Files" };
    }

    public void SaveEntry()
    {   
        _saveFileDialog.Popup(); 
    }

    public void LoadEntry()
    {
        _loadFileDialog.Popup();
    }

    private void OnSaveFileSelected(string path) 
    {
        EntryGroup entryGroup = new();

        foreach (InitiativeEntry entry in _tracker.Entries)
        {
            EntryData entryData = new()
            {
                CharacterName = entry._nameEdit.Text,
                Initiative = (int)entry._initiativeSpinBox.Value,
                DexModifier = (int)entry._dexModifierSpinBox.Value,
                AC = (int)entry._ACSpinBox.Value,
                HP = (int)entry._HPSpinBox.Value
            };

            entryGroup.Entries.Add(entryData);
        }

        Error error = ResourceSaver.Save(entryGroup, path);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to save tracker state to: {path}");
        }
    }

    private void OnLoadFileSelected(string path) 
    {
        if(ResourceLoader.Exists(path))
        {
            EntryGroup entryGroup = ResourceLoader.Load<EntryGroup>(path);
            
            if(entryGroup != null) 
            {
                foreach (EntryData data in entryGroup.Entries)
                {
                    InitiativeEntry newEntry = _tracker._initiativeEntryScene.Instantiate<InitiativeEntry>();

                    newEntry.CharacterName = data.CharacterName;
                    newEntry.Initiative = data.Initiative;
                    newEntry.DexModifier = data.DexModifier;
                    newEntry.AC = data.AC;
                    newEntry.HP = data.HP;

                    _tracker.AddEntryToTracker(newEntry);
                }
            }
        }
    }
}
