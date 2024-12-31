using Godot;
using System;

public partial class InitiativeEntry : Control
{	
	[Export] private MenuButton _menuButton;
	[Export] private SpinBox _initiativeSpinBox;
	[Export] private SpinBox _dexModifierSpinBox;
	[Export] private SpinBox _ACSpinBox;
	[Export] private SpinBox _HPSpinBox;
	private PopupMenu _popupMenu = new();

	private InitiativeTracker _tracker;

	// Attributes
	private int _initiative;
	private int _dexModifier;
	private int _AC;
	private int _HP;

	public InitiativeTracker Tracker 
	{ 
		set => _tracker = value;

		get
		{	
			return _tracker;
		}
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
