using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class InitiativeTracker : Control
{
	[Export] private PackedScene _initiativeEntryScene;
	[Export] private GridContainer _gridContainer;

	[ExportCategory("Buttons")]
	[Export] private Button _addEntryButton;
	[Export] private Button _rollAllButton;
	[Export] private Button _sortButton;
	[Export] private Button _clearButton;
	[Export] private Button _nextButton;

	// Used for combat logic
	private CombatOrderManager _combatOrderManager;
	private List<InitiativeEntry> _entries;
	private int _round = 1;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_addEntryButton.Pressed += () => AddEntryToTracker();
		_rollAllButton.Pressed += () => RollEntries();
		_sortButton.Pressed += () => SortEntries();
		_clearButton.Pressed += () => ClearTracker();
		_nextButton.Pressed += () => AdvanceCombat();

		_combatOrderManager = new();
		_entries = new();
	}

	public void RemoveEntryFromTracker(InitiativeEntry entry) 
	{
		_combatOrderManager.RemoveEntryFromCombat(entry);
		_entries.Remove(entry);
		entry.QueueFree();
	}

    private void AddEntryToTracker() 
	{
		InitiativeEntry newEntry = _initiativeEntryScene.Instantiate<InitiativeEntry>();
		newEntry.Tracker = this;

		_gridContainer.AddChild(newEntry);
		_combatOrderManager.AddEntryToCombat(newEntry);
		_entries.Add(newEntry);
	}

	private void ClearTracker() 
	{
		foreach (InitiativeEntry entry in _entries)
		{
			entry.QueueFree();
		}

		_entries.Clear();
		_combatOrderManager.ClearCombatOrder();
	}

	private void RollEntries()
	{

	}

	private void SortEntries() 
	{
		_entries = _entries.OrderByDescending(entry => entry.Initiative).ToList();
		for (int i = 0; i < _entries.Count; i++)
		{
			_gridContainer.MoveChild(_entries[i], i);
		}
	
		_combatOrderManager.SortCombatOrder();
	}

	private void AdvanceCombat() 
	{
		if(_combatOrderManager.EntryCount > 0)
		{
			_combatOrderManager.NextTurn();
		}
	}
}
