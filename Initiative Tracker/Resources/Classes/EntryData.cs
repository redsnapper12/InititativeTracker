using Godot;
using System;

[GlobalClass] public partial class EntryData : Resource
{
    [Export] public string CharacterName { get; set; } = "";
    [Export] public int Initiative { get; set; }
    [Export] public int DexModifier { get; set; }
    [Export] public int AC { get; set; }
    [Export] public int HP { get; set; }
}
