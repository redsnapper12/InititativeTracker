using Godot;
using System;

public partial class InitiativeEntry : Control
{	
	[ExportGroup("Info")]
	[Export] private Label _initiativeLabel;
	[Export] private LineEdit _nameEdit;
	[Export] private SpinBox _dexModifierSpinBox;
	[Export] private SpinBox _ACSpinBox;
	[Export] private SpinBox _HPSpinBox;
	
	[ExportGroup("Option Buttons")]
	[Export] private TextureButton _rollInitiativeButton;
	[Export] private TextureButton _saveButton;
	[Export] private TextureButton _duplicateButton;
	[Export] private TextureButton _deleteButton;
	
	[ExportGroup("Visuals")]
	[Export] private PanelContainer _panelContainer;
	[Export] private StyleBoxFlat _activePanelStyleBox;

	private InitiativeTracker _tracker;
	private PopupMenu _popupMenu = new();
	private bool _isActive = false;
	private string _characterName = "";
	private int _initiative = 0;
	private int _dexModifier = 0;
	private int _AC = 0;
	private int _HP = 0;

	public InitiativeTracker Tracker { get => _tracker; set => _tracker = value; }

	public PanelContainer PanelContainer { get => _panelContainer; }

	public StyleBoxFlat ActivePanelStyleBox { get => _activePanelStyleBox; }

	public bool IsActive { get => _isActive; set => _isActive = value; }

	public string CharacterName 
	{
		get => _characterName;
		set 
		{
			_nameEdit.Text = value;
			_characterName = value;
		}
	}

	public int Initiative 
	{ 
		get => _initiative;
		set 
		{
			_initiativeLabel.Text = value.ToString();
			_initiative = value;
		}
	}

	public int DexModifier 
	{
		get => _dexModifier;
		set
		{
			_dexModifierSpinBox.Value = value;
			_dexModifierSpinBox.Prefix =  value < 0 ? "" : "+";
			_dexModifier = value;
		} 
	}

	public int AC
	{
		get => _AC;
		set 
		{
			_ACSpinBox.Value = value;
			_AC = value;	
		}
	}  
	
	public int HP 
	{
		get => _HP; 
		set
		{
			_HPSpinBox.Value = value;
			_HP = value;
		}
		
	}


	public override void _Ready()
    {
		_characterName = _nameEdit.Text;

		_ACSpinBox.GetLineEdit().ContextMenuEnabled = false;
		_HPSpinBox.GetLineEdit().ContextMenuEnabled = false;
		_dexModifierSpinBox.GetLineEdit().ContextMenuEnabled = false;

		InitEvents();
    }

	// Public Helpers
	public void RollInitiative() 
	{
		int roll = GD.RandRange(1, 20) + _dexModifier;
		
		_initiative = roll;
		_initiativeLabel.Text = roll.ToString();
	}

	// Button events
	private void RollEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIRoll);
		RollInitiative();
		if (_isActive) _tracker.UpdateDetailBlock(this);
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
		_tracker.AddEntryToTracker(duplicateEntry);

        duplicateEntry.CharacterName = _characterName;
    	duplicateEntry.Initiative = _initiative;
    	duplicateEntry.DexModifier = _dexModifier;
        duplicateEntry.AC = _AC;
        duplicateEntry.HP = _HP;
	}

	private void SaveEvent() 
	{
		AudioManager.Instance.PlaySound(AudioManager.Sounds.UIClick);
		_tracker.EntrySerializer.PromptEntrySave(this);
	}

	private void InitEvents() 
	{
		_rollInitiativeButton.Pressed += () => RollEvent();
		_saveButton.Pressed += () => SaveEvent();
		_duplicateButton.Pressed += () => DuplicateEvent();
		_deleteButton.Pressed += () => DeleteEvent();

		// Details
		_nameEdit.TextChanged += (text) => 
		{
			_characterName = text;
			if (_isActive) _tracker.UpdateDetailBlock(this);
		};

		_HPSpinBox.ValueChanged += (value) => 
		{
			_HP = (int)value;
			if (_isActive) _tracker.UpdateDetailBlock(this);
		};

		_ACSpinBox.ValueChanged += (value) => 
		{
			_AC = (int)value;
			if (_isActive) _tracker.UpdateDetailBlock(this);
		};

		_dexModifierSpinBox.ValueChanged += (value) => 
		{
			_dexModifier = (int)value;
			_dexModifierSpinBox.Prefix =  value < 0 ? "" : "+";
			if (_isActive) _tracker.UpdateDetailBlock(this);
		};
	}
}
