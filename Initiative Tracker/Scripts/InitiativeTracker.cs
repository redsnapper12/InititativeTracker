using Godot;
using Godot.Collections;


public partial class InitiativeTracker : Control
{
	// Adding Entries
	[Export] private LineEdit _nameEntry;
	[Export] private SpinBox _dexterityModifierEntry;
	[Export] private SpinBox _ACEntry;
	[Export] private SpinBox _HPEntry;

	[Export] private Button _addButton;

	[Export] private PackedScene _initiativeEntryScene;

	[Export] private GridContainer _gridContainer;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_addButton.Pressed += () => InstantiateInitiativeEntry();
	}

	private void InstantiateInitiativeEntry() 
	{
		// Instantiate scene
		InitiativeEntry newEntry = _initiativeEntryScene.Instantiate<InitiativeEntry>();

		// Pass data and add child to grid
		EntryDataPacket packet = ConstructPacket();
		newEntry.InitializeEntry(packet);
		_gridContainer.AddChild(newEntry);

		// Reset inputs to default values
		ClearInputs();
	}

	private EntryDataPacket ConstructPacket()
	{
        EntryDataPacket newPacket = new()
        {
            Name = _nameEntry.Text,
			AC = (int)_ACEntry.Value,
			HP = (int)_HPEntry.Value,
			DexterityModifier = (int)_dexterityModifierEntry.Value,
			ParentContainer = _gridContainer
		};

		return newPacket;
    }

	private void ClearInputs() 
	{
		_nameEntry.Clear();
		_dexterityModifierEntry.Value = _dexterityModifierEntry.MinValue;
		_ACEntry.Value = _ACEntry.MinValue;
		_HPEntry.Value = _HPEntry.MinValue;
	}
}
