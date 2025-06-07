using Godot;
using System;

public partial class CustomItemList : ItemList
{
	[Export] Color color01 = Color.FromHtml("#252525");
	[Export] Color color02 = Color.FromHtml("#2d2d2d");

	public void RefreshColors()
	{
		for (int i = 0; i < ItemCount; i++)
		{
			Color color = i % 2 == 0 ? color01 : color02;

			SetItemCustomBgColor(i, color);
		}
	}
}
