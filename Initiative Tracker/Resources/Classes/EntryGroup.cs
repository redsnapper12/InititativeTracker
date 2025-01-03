using Godot;
using Godot.Collections;


public partial class EntryGroup : Resource
{
    [Export] public Array<EntryData> Entries { get; set; } = new Godot.Collections.Array<EntryData> {};
}
