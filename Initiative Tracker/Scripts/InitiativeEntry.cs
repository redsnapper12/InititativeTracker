using Godot;
using System;

public partial class InitiativeEntry : Control
{	
	[Export] public LineEdit _nameEdit;
	[Export] public SpinBox _initiativeSpinBox;
	[Export] public SpinBox _dexModifierSpinBox;
	[Export] public SpinBox _ACSpinBox;
	[Export] public SpinBox _HPSpinBox;
	[Export] public MenuButton _menuButton;
	private PopupMenu _popupMenu = new();

	private InitiativeTracker _tracker;

	// Attributes
	private string _characterName = "";
	private int _initiative = 0;
	private int _dexModifier = 0;
	private int _AC = 0;
	private int _HP = 0;

	public InitiativeTracker Tracker 
	{ 
		set => _tracker = value;

		get
		{	
			return _tracker;
		}
	}

	public string CharacterName 
	{
		set
		{
			_characterName = value;
			_nameEdit.Text = value;
		}

		get => _characterName;
	}
	
	public int Initiative 
	{
		set
		{
			_initiative = value;
			_initiativeSpinBox.Value = value;
		}

		get => _initiative;
	}
	
	public int DexModifier
	{
		set
		{
			_dexModifier = value;
			_dexModifierSpinBox.Value = value;
		}

		get => _dexModifier;
	}

	public int AC
	{
		set
		{
			_AC = value;
			_ACSpinBox.Value = value;
		}

		get => _AC;
	}

	public int HP
	{
		set
		{
			_HP = value;
			_HPSpinBox.Value = value;
		}

		get => _HP;
	}

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

		_popupMenu.IdPressed += (id) => HandlePopupMenuInput((int)id);

		_initiativeSpinBox.ValueChanged += (value) => 
		{ 
			_initiative = (int)value; 
		};

		_dexModifierSpinBox.ValueChanged += (value) => 
		{ 
			_dexModifier = (int)value; 
			if(value < 0) 
			{
				_dexModifierSpinBox.Prefix = "";
			}
			else 
			{
				_dexModifierSpinBox.Prefix = "+";
			}
		};
    }

	public void RollInitiative() 
	{
		int roll = GD.RandRange(1, 20);
		_initiativeSpinBox.Value = roll + _dexModifier;
	}

	private void DeleteEntry() 
	{
		_tracker.RemoveEntryFromTracker(this);
	}

	private void DuplicateEntry() 
	{
		
	}

	private void SaveEntry() 
	{

	}

	private void LoadEntry() 
	{

	}

	private void ConstructOptionsPopup() 
	{
		Actions[] actions = (Actions[])Enum.GetValues(typeof(Actions));

        for (int i = 0; i < actions.Length; i++)
		{
			_popupMenu.AddItem(actions[i].ToString(), i);
		}
	}

	private void HandlePopupMenuInput(int id) 
	{
		switch (id)
			{
				case (int)Actions.Delete:
					DeleteEntry();
					break;
				case (int)Actions.Roll:
					RollInitiative();
					break;
				case (int)Actions.Save:
					SaveEntry();
					break;
				case (int)Actions.Load:
					LoadEntry();
					break;
				case (int)Actions.Duplicate:
					DuplicateEntry();
					break;
				default:
					GD.PrintErr("Popup menu recieved an invalid ID: " + id);
					break;
			};
	}
}
