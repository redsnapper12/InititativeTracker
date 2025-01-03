using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CombatOrderManager
{
    private List<InitiativeEntry> _combatOrder;
    private InitiativeEntry _activeEntry;
    private int _round = 1;
    private int _activeEntryIndex = -1;

    public int EntryCount
    {
        get => _combatOrder.Count;
    }

    public int Round
    {
        get => _round;
    }

    public CombatOrderManager()
    {
        _combatOrder = new();
    }

    public void AddEntryToCombat(InitiativeEntry entry) 
	{	
		if(_combatOrder.Count == 0) 
        {
            _combatOrder.Add(entry);
            _activeEntryIndex = 0;
            _activeEntry = _combatOrder[_activeEntryIndex];
            UpdateActiveEntryVisuals();

            return;
        }

        _combatOrder.Add(entry);
	}

	public void RemoveEntryFromCombat(InitiativeEntry entry)
	{
		if(!_combatOrder.Contains(entry)) return;

        int removedIndex = _combatOrder.IndexOf(entry);
        bool removingActiveEntry = _activeEntry == entry;
        _combatOrder.Remove(entry);


        // If the combat order is now empty
        if (_combatOrder.Count == 0)
        {
            // Stop combat
            _activeEntryIndex = -1;
            _activeEntry = null;
            _round = 1;
            return;
        }

        // If we are removing an active entry
        if (removingActiveEntry)
        {
            // If the active entry we deleted is at the end
            if (_activeEntryIndex >= _combatOrder.Count)
            {
                _activeEntryIndex = 0;
                _round++;
            }

            _activeEntry = _combatOrder[_activeEntryIndex];
            UpdateActiveEntryVisuals();
            return;
        }

        // If we removed an entry before the active entry, adjust the index
        if (removedIndex < _activeEntryIndex)
        {
            _activeEntryIndex--;
        }

        // Any other case, out-of-bounds checking
        if (_activeEntryIndex >= _combatOrder.Count)
        {
            _activeEntryIndex = 0;
        }
        else if (_activeEntryIndex < 0)
        {
            _activeEntryIndex = _combatOrder.Count - 1;
        }

        _activeEntry = _combatOrder[_activeEntryIndex];
        UpdateActiveEntryVisuals();
	}

    public void SortCombatOrder()
    {
        if(_combatOrder.Count > 0) 
        {
            _combatOrder = _combatOrder.OrderBy(entry => entry.Initiative).ToList();
            _activeEntryIndex = 0;
        }
    }

    public void NextTurn() 
    {
        if (_combatOrder.Count == 0) return;
        IterateActiveEntry();
    }

    public void ClearCombatOrder() 
    {
        _combatOrder.Clear();
        _round = 1;
    }

    private void IterateActiveEntry() 
    {
        // Update visuals for old entry
        if (_activeEntryIndex >= 0 && _activeEntryIndex < _combatOrder.Count)
        {
            _activeEntry.Modulate = new(1.0f, 1.0f, 1.0f, 1.0f);
        }

        if (_combatOrder.Count > 0)
        {   
            // Use modulo to wrap
            int nextIdx = (_activeEntryIndex + 1) % _combatOrder.Count;

            // If we wrapped, increment round
            if(nextIdx == 0) _round++;

            _activeEntryIndex = nextIdx;
            _activeEntry = _combatOrder[_activeEntryIndex];
            UpdateActiveEntryVisuals();
        }
        else
        {
            _activeEntryIndex = -1;
            _activeEntry = null;
        }
    }

    private void UpdateActiveEntryVisuals()
    {
        if (_activeEntryIndex >= 0 && _activeEntryIndex < _combatOrder.Count)
        {
            _activeEntry.Modulate = new(0.9f, 0.9f, 1.0f, 0.75f);
        }
    }
}
