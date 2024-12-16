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

	// Drag and Drop properties
    private bool _isDragging = false;
	private Vector2 _dragStart;
    private Control _dragGhost;
    private int _originalIndex;

	public override void _Ready()
    {
        GuiInput += OnGuiInput;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

	public override void _Process(double delta)
    {
        if (_isDragging && _dragGhost != null)
        {
            _dragGhost.GlobalPosition = GetGlobalMousePosition() - _dragStart;
        }
    }
	
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

	private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed && !_isDragging)
                {
                    StartDragging();
                }
                else if (_isDragging)
                {
                    StopDragging();
                }
            }
        }
    }

    private void StartDragging()
    {
        _isDragging = true;
        _dragStart = GetLocalMousePosition();
        _originalIndex = GetIndex();
        
        // Create a semi-transparent copy of this control as the drag ghost
        _dragGhost = (Control)Duplicate();
        _dragGhost.Modulate = new Color(1, 1, 1, 0.7f);
        _dragGhost.GlobalPosition = GlobalPosition;
        GetTree().Root.AddChild(_dragGhost);
        
        // Make this control semi-transparent while dragging
        Modulate = new Color(1, 1, 1, 0.3f);
    }

    private void StopDragging()
    {
        _isDragging = false;
        if (_dragGhost != null)
        {
            _dragGhost.QueueFree();
            _dragGhost = null;
        }
        Modulate = new Color(1, 1, 1, 1.0f);
        
        // Get the closest entry position and swap if necessary
        Vector2 dropPosition = GetGlobalMousePosition();
        InitiativeEntry targetEntry = FindClosestEntry(dropPosition);
        
        if (targetEntry != null && targetEntry != this)
        {
            int targetIndex = targetEntry.GetIndex();
            _parentContainer.MoveChild(this, targetIndex);
        }
    }

    private InitiativeEntry FindClosestEntry(Vector2 position)
    {
        InitiativeEntry closest = null;
        float closestDistance = float.MaxValue;

        foreach (Node child in _parentContainer.GetChildren())
        {
            if (child is InitiativeEntry entry && entry != this)
            {
                float distance = position.DistanceTo(entry.GlobalPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = entry;
                }
            }
        }

        return closest;
    }

    private void OnMouseEntered()
    {
        if (!_isDragging)
        {
            // Add hover effect
            Modulate = new Color(1, 1, 1, 0.8f);
        }
    }

    private void OnMouseExited()
    {
        if (!_isDragging)
        {
            // Remove hover effect
            Modulate = new Color(1, 1, 1, 1.0f);
        }
    }
}
