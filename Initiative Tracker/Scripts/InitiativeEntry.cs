using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public partial class InitiativeEntry : Control
{	
	[Export] private MenuButton _menuButton;
	[Export] private SpinBox _initiativeSpinBox;
	private PopupMenu _popupMenu = new();

	private InitiativeTracker _tracker;
	private int _initiative;

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
			GD.Print(Initiative);
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
