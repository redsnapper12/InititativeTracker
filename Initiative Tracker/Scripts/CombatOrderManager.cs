using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CombatOrderManager
{
    private List<InitiativeEntry> _combatOrder;
    private InitiativeEntry _activeEntry;
    private int _activeEntryIndex = -1;

    public int EntryCount
    {
        get => _combatOrder.Count;
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

        bool removingActiveEntry = _activeEntry == entry;
        _combatOrder.Remove(entry);

        if (_combatOrder.Count == 0)
        {
            // Stop combat
            _activeEntryIndex = -1;
            _activeEntry = null;
        }
        else if (removingActiveEntry)
        {
            // If removing left us out of bounds, reset index to 0
            if (_activeEntryIndex >= _combatOrder.Count)
            {
                _activeEntryIndex = 0;
                _activeEntry = _combatOrder[_activeEntryIndex];
            }

            UpdateActiveEntryVisuals();
        }
        else if (_activeEntryIndex >= _combatOrder.Count)
        {   
            // if we removed an entry behind the active one move the index back by one to keep the state the same
            // e.g. Suppose we have an array of: [a, b, c, d, e] where c is our active entry and c's index is 2.
            // if we remove a, our array will update as such: [b, c, d, e] where c's new index is 1. To remediate this,
            // we decrement the active entry's index by one to save our state.
            _activeEntryIndex--;
            _activeEntry = _combatOrder[_activeEntryIndex];
        }
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
            _activeEntryIndex = (_activeEntryIndex + 1) % _combatOrder.Count;
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
