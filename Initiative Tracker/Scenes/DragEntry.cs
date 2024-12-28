using Godot;
using System;


// TODO: UPDATE DRAG CLASS (Some of this is written by claude and does not fit the style of the project)
public partial class DragEntry : TextureRect
{
	[Export] InitiativeEntry _initiativeEntry;

	// Drag and Drop properties
    private bool _isDragging = false;
	private Vector2 _dragStart;
    private Control _dragGhost;
    private int _originalIndex;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Dragging
        GuiInput += OnGuiInput;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_isDragging && _dragGhost != null)
        {	
			// Set ghost position to be centered on the mouse
            _dragGhost.GlobalPosition = GetGlobalMousePosition() - Size / 2;
        }
	}

	private InitiativeEntry FindClosestEntry(Vector2 position)
    {
        InitiativeEntry closest = null;
        float closestDistance = float.MaxValue;

        foreach (Node child in _initiativeEntry.Tracker.GetChildren())
        {
            if (child is InitiativeEntry entry && entry != _initiativeEntry)
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
        _dragGhost = (Control)_initiativeEntry.Duplicate();
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
        
        if (targetEntry != null && targetEntry != _initiativeEntry)
        {
            int targetIndex = targetEntry.GetIndex();
            _initiativeEntry.Tracker.MoveChild(_initiativeEntry, targetIndex);
        }
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
