using System.Collections.Generic;
using System.Linq;

public partial class CombatOrderManager
{
    private List<InitiativeEntry> _combatOrder;
    private InitiativeEntry _activeEntry;
    private int _round = 1;
    private int _activeEntryIndex = -1;

    public int Round { get => _round; }
    public InitiativeEntry ActiveEntry
    { 
        get => _activeEntry; 
        set 
        {   
            // Remove styling and set previous active entry as inactive
            if(_activeEntry != null)
            {
                _activeEntry.IsActive = false;
                if(_activeEntry.PanelContainer.HasThemeStyleboxOverride("panel")) _activeEntry.PanelContainer.RemoveThemeStyleboxOverride("panel");
            } 

            // Set new active entry
            _activeEntry = value;

            // Add style and set new entry as active entry
            if(_activeEntry != null)
            {  
                _activeEntry.IsActive = true;
                if(!_activeEntry.PanelContainer.HasThemeStyleboxOverride("panel")) _activeEntry.PanelContainer.AddThemeStyleboxOverride("panel", _activeEntry.ActivePanelStyleBox);
            } 
        }
    }

    public CombatOrderManager()
    {
        _combatOrder = [];
    }

    public void AddEntryToCombat(InitiativeEntry entry, int index = -1) 
	{	
		if(_combatOrder.Count == 0) 
        {
            _combatOrder.Add(entry);
            _activeEntryIndex = 0;
            ActiveEntry = _combatOrder[_activeEntryIndex];

            return;
        }

        if(index == -1)
        {
            _combatOrder.Add(entry);
        }
        else
        {
            _combatOrder.Insert(index, entry);
        }    
	}

	public void RemoveEntryFromCombat(InitiativeEntry entry)
	{
		if(!_combatOrder.Contains(entry)) return;

        int removedIndex = _combatOrder.IndexOf(entry);
        bool removingActiveEntry = ActiveEntry == entry;
        
        _combatOrder.Remove(entry);


        // If the combat order is now empty
        if (_combatOrder.Count == 0)
        {
            // Stop combat
            _activeEntryIndex = -1;
            ActiveEntry = null;
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

            ActiveEntry = _combatOrder[_activeEntryIndex];
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

        ActiveEntry = _combatOrder[_activeEntryIndex];
	}

    public void SortCombatOrder()
    {
        if(_combatOrder.Count > 0) 
        {
            _combatOrder = _combatOrder.OrderByDescending(entry => entry.Initiative).ToList();
            _activeEntryIndex = 0;
            ActiveEntry = _combatOrder[_activeEntryIndex];
        }
    }

    public void SwapEntries(int i, int j) 
    {
        (_combatOrder[j], _combatOrder[i]) = (_combatOrder[i], _combatOrder[j]);

        if(i == _activeEntryIndex)
        {
            _activeEntryIndex = j;
            ActiveEntry = _combatOrder[_activeEntryIndex];
        }
        else if(j == _activeEntryIndex)
        {
            _activeEntryIndex = i;
            ActiveEntry = _combatOrder[_activeEntryIndex];
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
        ActiveEntry = null;
        _round = 1;
    }

    public void ResetRound() 
    {
        _round = 1;

        if(_combatOrder.Count > 0)
        {
            _activeEntryIndex = 0;
            ActiveEntry = _combatOrder[_activeEntryIndex];
        } 
    }

    private void IterateActiveEntry() 
    {
        if (_combatOrder.Count > 0)
        {   
            // Use modulo to wrap
            int nextIdx = (_activeEntryIndex + 1) % _combatOrder.Count;

            // If we wrapped, increment round
            if(nextIdx == 0) _round++;

            _activeEntryIndex = nextIdx;
            ActiveEntry = _combatOrder[_activeEntryIndex];
        }
        else
        {
            _activeEntryIndex = -1;
            ActiveEntry = null;
        }
    }
}
