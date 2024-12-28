using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class InitiativeEntry : Control
{	
	[Export] private MenuButton _menuButton;
	private PopupMenu _popupMenu = new();

	public InitiativeTracker Tracker { get; set; }

	public int Inititative { get; }

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
    }

	private void DeleteEntry() 
	{
		Tracker.RemoveEntryFromQueue(this);
		QueueFree();
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

	private void ConstructOptionsPopup() 
	{
		Actions[] actions = (Actions[])Enum.GetValues(typeof(Actions));

        for (int i = 0; i < actions.Length; i++)
		{
			_popupMenu.AddItem(actions[i].ToString(), i);
		}
	}
}
