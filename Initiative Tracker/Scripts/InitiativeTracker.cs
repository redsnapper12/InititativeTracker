using Godot;
using Godot.Collections;


public partial class InitiativeTracker : Control
{
	[Export] private Button _addButton;

	[Export] private PackedScene _initiativeEntryScene;

	[Export] private GridContainer _gridContainer;


	// Round Logic
	private InitiativeEntry _currentEntry;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_addButton.Pressed += () => InstantiateInitiativeEntry();
	}

	private void InstantiateInitiativeEntry() 
	{
		// Instantiate scene
		InitiativeEntry newEntry = _initiativeEntryScene.Instantiate<InitiativeEntry>();
		_gridContainer.AddChild(newEntry);
		newEntry.ParentContainer = _gridContainer;
	}
}
