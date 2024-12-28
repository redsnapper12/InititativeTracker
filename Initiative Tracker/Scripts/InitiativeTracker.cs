using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

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
	public Queue<InitiativeEntry> CombatOrderQueue { get; set; }
	private InitiativeEntry _currentEntry;
	private int _round = 1;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_addEntryButton.Pressed += () => AddEntry();
		_rollAllButton.Pressed += () => RollEntries();
		_sortButton.Pressed += () => SortEntries();
		_clearButton.Pressed += () => ClearTracker();
		_nextButton.Pressed += () => AdvanceCombat();

		CombatOrderQueue = new();
	}

	private void AddEntry() 
	{
		InitiativeEntry newEntry = _initiativeEntryScene.Instantiate<InitiativeEntry>();
		newEntry.Tracker = this;

		_gridContainer.AddChild(newEntry);
		CombatOrderQueue.Enqueue(newEntry);
	}

	private void RollEntries()
	{

	}

	private void SortEntries() 
	{

	}

	private void ClearTracker() 
	{
		if(CombatOrderQueue.Count > 0)
		{
			foreach(InitiativeEntry entry in CombatOrderQueue) 
			{
				RemoveEntryFromQueue(entry);
				entry.QueueFree();
			}
		}
		else 
		{
			return;
		}
	}

	private void AdvanceCombat() 
	{

	}


	private void AddEntryToQueue() 
	{

	}

	private void UpdateQueue() 
	{
		// Convert queue to list, then sort, then reconstruct queue
		List<InitiativeEntry> order = CombatOrderQueue.OrderBy(e => e.Inititative).ToList();
		CombatOrderQueue = new Queue<InitiativeEntry>(order);

		GD.Print(CombatOrderQueue.ToString());
	}

	public void RemoveEntryFromQueue(InitiativeEntry entry)
	{
		List<InitiativeEntry> tempList = CombatOrderQueue.Where(e => e != entry).ToList();
		CombatOrderQueue = new Queue<InitiativeEntry>(tempList);
	}
}
