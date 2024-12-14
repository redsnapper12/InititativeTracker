using Godot;
using System;

public partial class EntryDataPacket : Resource
{

    public string Name { get; set; }
    public int AC { get; set; }
    public int HP { get; set; }

    public int DexterityModifier { get; set; }

    public GridContainer ParentContainer { get; set;}
}
