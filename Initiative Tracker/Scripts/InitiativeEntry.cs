using Godot;
using System;

public partial class InitiativeEntry : Control
{	
	// Buttons
	[Export] private Button _deleteButton;
	[Export] private Button _duplicateButton;

	// Grid Container
	public GridContainer ParentContainer { get; set; }

	public override void _Ready()
    {
		_deleteButton.Pressed += () => DeleteEntry();
		_duplicateButton.Pressed += () => DuplicateEntry();
    }

	public override void _Process(double delta)
    {
        
    }

	private void DeleteEntry() 
	{
		QueueFree();
	}

	private void DuplicateEntry() 
	{
		Node duplicate = (Control)Duplicate();
		ParentContainer.AddSibling(duplicate);
	}
}
