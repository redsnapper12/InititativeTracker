using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class InitiativeTracker : Control
{
	[Export] private EntrySerializer _entrySerializer;
	[Export] private PackedScene _initiativeEntryScene;
	[Export] private GridContainer _gridContainer;
	[Export] private Label _roundCounterLabel;

	[ExportCategory("Buttons")]
	[Export] private Button _addEntryButton;
	[Export] private Button _rollAllButton;
	[Export] private Button _saveButton;
	[Export] private Button _loadButton;
	[Export] private Button _sortButton;
	[Export] private Button _clearButton;
	[Export] private Button _nextButton;

	// Helper classes
	private readonly CombatOrderManager _combatOrderManager  = new();

	private List<InitiativeEntry> _entries = new();
	public List<InitiativeEntry> Entries { get => _entries; }

	public PackedScene InititativeEntryScene { get => _initiativeEntryScene; }
	public EntrySerializer EntrySerializer { get => _entrySerializer; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitButtonSignals();
	}

	// Public Helpers
	public void RemoveEntryFromTracker(InitiativeEntry entry) 
	{
		UpdateRoundCounter(_combatOrderManager.Round);

		_combatOrderManager.RemoveEntryFromCombat(entry);
		_entries.Remove(entry);
		entry.QueueFree();
	}

	public void AddEntryToTracker(InitiativeEntry entry) 
	{
		entry.Tracker = this;
		_gridContainer.AddChild(entry);
		_combatOrderManager.AddEntryToCombat(entry);
		_entries.Add(entry);
	}

	public void SortTracker() 
	{
		_entries = _entries.OrderByDescending(entry => entry.Initiative).ToList();
		for (int i = 0; i < _entries.Count; i++)
		{
			_gridContainer.MoveChild(_entries[i], i);
		}

		_combatOrderManager.SortCombatOrder();
	}

	// Button events
    private void AddEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);

		InitiativeEntry newEntry = _initiativeEntryScene.Instantiate<InitiativeEntry>();
		AddEntryToTracker(newEntry);
	}

	private void SaveEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		//_entrySerializer.SaveEntries(_entries);
		// TODO
	}

	private void LoadEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		_entrySerializer.PromptLoad();
	}

	private void ClearEvent() 
	{
		if(_entries.Count > 0)
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIDelete);

			foreach (InitiativeEntry entry in _entries)
			{
				entry.QueueFree();
			}

			_entries.Clear();
			_combatOrderManager.ClearCombatOrder();
			UpdateRoundCounter(_combatOrderManager.Round);
		}
		else
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		}
	}

	private void RollEvent()
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIRoll);

		foreach (InitiativeEntry entry in _entries)
		{
			entry.RollInitiative();
		}
	}

	private void SortEvent()
	{
		if(_entries.Count > 1)
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
			SortTracker();
		}
		else 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		}
	}

	private void AdvanceEvent() 
	{
		if(_entries.Count > 1)
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);

			_combatOrderManager.NextTurn();
			UpdateRoundCounter(_combatOrderManager.Round);
		}
		else 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		}
	}

	// Helpers
	private void UpdateRoundCounter(int round) 
	{
		_roundCounterLabel.Text = "Round " + round;
	}

	private void InitButtonSignals() 
	{
		_addEntryButton.Pressed += () => AddEvent();
		_rollAllButton.Pressed += () => RollEvent();
		_sortButton.Pressed += () => SortEvent();
		_clearButton.Pressed += () => ClearEvent();
		_nextButton.Pressed += () => AdvanceEvent();
		_saveButton.Pressed += () => SaveEvent();
		_loadButton.Pressed += () => LoadEvent();
	}
}
