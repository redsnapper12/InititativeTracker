using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class InitiativeTracker : Control
{
	[Export] private EntrySerializer _entrySerializer;
	[Export] private PackedScene _initiativeEntryScene;
	[Export] private VBoxContainer _vBoxContainer;
	[Export] private Label _roundCounterLabel;

	[ExportCategory("Detail Block")]
	[Export] private Label _activeName;
	[Export] private Label _activeHP;
	[Export] private Label _activeAC;
	[Export] private Label _activeInitiative;

	[ExportCategory("Tracker Action Buttons")]
	[Export] private TextureButton _addButton;
	[Export] private TextureButton _rollButton;
	[Export] private TextureButton _sortButton;
	[Export] private TextureButton _clearButton;
	[Export] private TextureButton _saveButton;
	[Export] private TextureButton _loadButton;
	[Export] private TextureButton _restartButton;
	[Export] private Button _nextButton;

	private List<InitiativeEntry> _entries = [];
	public List<InitiativeEntry> Entries { get => _entries; }

	// Helper classes
	private readonly CombatOrderManager _combatOrderManager  = new();
	public PackedScene InititativeEntryScene { get => _initiativeEntryScene; }
	public EntrySerializer EntrySerializer { get => _entrySerializer; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitEvents();
	}

	// Helpers
	public void RemoveEntryFromTracker(InitiativeEntry entry) 
	{
		UpdateRoundCounter(_combatOrderManager.Round);

		_combatOrderManager.RemoveEntryFromCombat(entry);
		UpdateDetailBlock(_combatOrderManager.ActiveEntry);
		_entries.Remove(entry);
		entry.QueueFree();
	}

	public void AddEntryToTracker(InitiativeEntry entry) 
	{
		entry.Tracker = this;
		_vBoxContainer.AddChild(entry);
		_combatOrderManager.AddEntryToCombat(entry);
		UpdateDetailBlock(_combatOrderManager.ActiveEntry);
		_entries.Add(entry);
	}

	public void SortTracker() 
	{
		_entries = _entries.OrderByDescending(entry => entry.Initiative).ToList();
		for (int i = 0; i < _entries.Count; i++)
		{
			_vBoxContainer.MoveChild(_entries[i], i);
		}

		_combatOrderManager.SortCombatOrder();
		UpdateDetailBlock(_combatOrderManager.ActiveEntry);
	}

	public void MoveEntryUp(InitiativeEntry entry) 
	{
		int idx = _entries.IndexOf(entry);
		
		if(idx == -1)
		{
			GD.PrintErr("Cannot move entry up because it does not exist in the tracker!");
			return;
		}

		if(idx == 0) 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		}
		else 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
			
			// Swap Elements
			(_entries[idx], _entries[idx - 1]) = (_entries[idx - 1], _entries[idx]);

			_combatOrderManager.SwapEntries(idx, idx - 1);
			_vBoxContainer.MoveChild(entry, idx - 1);
		}
	}

	public void MoveEntryDown(InitiativeEntry entry) 
	{
		int idx = _entries.IndexOf(entry);
		
		if(idx == -1)
		{
			GD.PrintErr("Cannot move entry down because it does not exist in the tracker!");
			return;
		}

		if(idx == _entries.Count) 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		}
		else 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
			
			// Swap Elements
			(_entries[idx], _entries[idx + 1]) = (_entries[idx + 1], _entries[idx]);

			_combatOrderManager.SwapEntries(idx, idx + 1);
			_vBoxContainer.MoveChild(entry, idx + 1);
		}
	}

	public void UpdateDetailBlock(InitiativeEntry activeEntry) 
	{
		if(activeEntry == null)
		{
			ResetDetailBlock();
			return;
		}

		_activeName.Text = activeEntry.CharacterName;
		_activeHP.Text = activeEntry.HP.ToString();
		_activeAC.Text = activeEntry.AC.ToString();
		_activeInitiative.Text = activeEntry.Initiative.ToString();
	}

	public void ResetDetailBlock() 
	{
		_activeName.Text = "No Combatant";
		_activeHP.Text = "N/A";
		_activeAC.Text = "N/A";
		_activeInitiative.Text = "N/A";
	}

	private void UpdateRoundCounter(int round) 
	{
		_roundCounterLabel.Text = "Round " + round;
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
		if(_entries.Count > 0)
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
			_entrySerializer.PromptEncounterSave();
		}
		else 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIDelete);
		}
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
			ResetDetailBlock();
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
			UpdateDetailBlock(_combatOrderManager.ActiveEntry);
			UpdateRoundCounter(_combatOrderManager.Round);
		}
		else 
		{
			AudioManager.Instance.PlaySound(AudioManager.Sounds.UIError);
		}
	}

	private void RestartEvent() 
	{
		_combatOrderManager.ResetRound();
		UpdateRoundCounter(_combatOrderManager.Round);
	}

	// Event Initialization
	private void InitEvents() 
	{
		_nextButton.Pressed += () => AdvanceEvent();
		_addButton.Pressed += () => AddEvent();
		_rollButton.Pressed += () => RollEvent();
		_sortButton.Pressed += () => SortEvent();
		_clearButton.Pressed += () => ClearEvent();
		_saveButton.Pressed += () => SaveEvent();
		_loadButton.Pressed += () => LoadEvent();
		_restartButton.Pressed += () => RestartEvent();
	}
}
