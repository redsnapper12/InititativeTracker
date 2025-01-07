using Godot;
using System;

public partial class InitiativeEntry : Control
{	
	[Export] private LineEdit _nameEdit;
	[Export] private SpinBox _initiativeSpinBox;
	[Export] private SpinBox _dexModifierSpinBox;
	[Export] private SpinBox _ACSpinBox;
	[Export] private SpinBox _HPSpinBox;
	[Export] private MenuButton _menuButton;
	private InitiativeTracker _tracker;
	private PopupMenu _popupMenu = new();
	private string _characterName = "";
	private int _initiative = 0;
	private int _dexModifier = 0;
	private int _AC = 0;
	private int _HP = 0;

	public InitiativeTracker Tracker { get => _tracker; set => _tracker = value; }

	public string CharacterName 
	{
		get => _characterName;
		set 
		{
			_nameEdit.Text = value;
			_characterName = value;
		}
	}

	public int Initiative { get => _initiative; set => _initiativeSpinBox.Value = value; }
	public int DexModifier { get => _dexModifier; set =>_dexModifierSpinBox.Value = value; }
	public int AC { get => _AC; set =>_ACSpinBox.Value = value;	}
	public int HP { get => _HP; set => _HPSpinBox.Value = value;}

	enum Actions 
	{
		Delete,
		Roll,
		Save,
		Load,
		Duplicate
	}

	public override void _Ready()
    {
		_popupMenu = _menuButton.GetPopup();
		ConstructOptionsPopup();

		InitEvents();
    }

	// Public Helpers
	public void RollInitiative() 
	{
		int roll = GD.RandRange(1, 20) + _dexModifier;
		
		_initiative = roll;
		_initiativeSpinBox.Value = roll;
	}


	// Private Helpers
	private void ConstructOptionsPopup() 
	{
		Actions[] actions = (Actions[])Enum.GetValues(typeof(Actions));

        for (int i = 0; i < actions.Length; i++)
		{
			_popupMenu.AddItem(actions[i].ToString(), i);
			
			if(actions[i] == Actions.Save || actions[i] == Actions.Load) _popupMenu.SetItemDisabled(i, true);
		}
	}

	private void HandlePopupMenuInput(int id)
	{
		switch (id)
		{
			case (int)Actions.Delete:
				DeleteEvent();
				break;
			case (int)Actions.Roll:
				RollInitiative();
				break;
			case (int)Actions.Save:
				SaveEvent();
				break;
			case (int)Actions.Load:
				LoadEvent();
				break;
			case (int)Actions.Duplicate:
				DuplicateEvent();
				break;
			default:
				GD.PrintErr("Popup menu recieved an invalid ID: " + id);
				break;
		};
	}

	private void InitEvents() 
	{
		// Line edit
		_nameEdit.TextChanged += (text) => _characterName = text;

		// Buttons
		_menuButton.Pressed += () => AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		_popupMenu.IdPressed += (id) => HandlePopupMenuInput((int)id);

		// Spin Boxes
		_initiativeSpinBox.ValueChanged += (value) => _initiative = (int)value;
		_HPSpinBox.ValueChanged += (value) => _HP = (int)value;
		_ACSpinBox.ValueChanged += (value) => _AC = (int)value;
		_dexModifierSpinBox.ValueChanged += (value) => 
		{
			_dexModifier = (int)value;
			_dexModifierSpinBox.Prefix =  value < 0 ? "" : "+";
		};
	}


	// Button events
	private void RollEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIRoll);
		RollInitiative();
	}

	private void DeleteEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIDelete);
		_tracker.RemoveEntryFromTracker(this);
	}

	private void DuplicateEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		
		InitiativeEntry duplicateEntry = _tracker.InititativeEntryScene.Instantiate<InitiativeEntry>();
		_tracker.AddEntryToTracker(duplicateEntry, GetIndex() + 1);

        duplicateEntry.CharacterName = _characterName;
    	duplicateEntry.Initiative = _initiative;
    	duplicateEntry.DexModifier = _dexModifier;
        duplicateEntry.AC = _AC;
        duplicateEntry.HP = _HP;
	}

	private void SaveEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		_tracker.EntrySerializer.SaveEntry(this);
	}

	private void LoadEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		_tracker.EntrySerializer.LoadEntry(this);
	}
}
