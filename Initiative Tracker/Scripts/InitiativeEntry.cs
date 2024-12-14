using Godot;
using System;

public partial class InitiativeEntry : Control
{	
	// Labels
	[Export] private Label _nameLabel;
	private string _name;

	[Export] private Label _initiativeLabel;
	private int _initiative;

	[Export] private Label _dexterityModifierLabel;
	private int _dexterityModifier;

	[Export] private Label _ACLabel;
	private int _AC;

	[Export] private Label _HPLabel;
	private int _HP;

	// Buttons
	[Export] private Button _deleteButton;
	[Export] private Button _duplicateButton;

	// Grid Container
	GridContainer _parentContainer;


	public void InitializeEntry(EntryDataPacket entryDataPacket) 
	{
		_name = entryDataPacket.Name;
		_dexterityModifier = entryDataPacket.DexterityModifier;
		_AC = entryDataPacket.AC;	
		_HP = entryDataPacket.HP;
		_parentContainer = entryDataPacket.ParentContainer;

		_initiative = 0;

		InitializeLabels();
		InitializeButtons();
	}

	private void InitializeLabels()
	{
		_nameLabel.Text = _name;
		_initiativeLabel.Text = "Initiative: " + _initiative.ToString();
		_dexterityModifierLabel.Text = "Modifier: +" + _dexterityModifier.ToString();
		_ACLabel.Text = "AC: " + _AC.ToString();
		_HPLabel.Text = "HP: " + _HP.ToString();
	} 

	private void InitializeButtons() 
	{
		_deleteButton.Pressed += () => DeleteEntry();
		_duplicateButton.Pressed += () => DuplicateEntry();
	}

	private void DeleteEntry() 
	{
		QueueFree();
	}

	private void DuplicateEntry() 
	{
		Node duplicate = Duplicate();
		_parentContainer.AddSibling(duplicate);
	}
}
