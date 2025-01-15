
using Godot;
using Godot.Collections;

[GlobalClass] public partial class EncounterData : Resource
{
    [Export] public string EncounterName { get; set; } = "";
    [Export] public Array<EntryData> Encounter { get; set; } = new();
}
