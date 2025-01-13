using System;
using System.Collections.Generic;
using Godot;

public partial class AudioManager : AudioStreamPlayer
{	
	[Export] private AudioStreamWav[] sounds;
	private static AudioManager _instance;
  	public static AudioManager Instance => _instance;

	public enum Sounds 
	{
		UIClick,
		UIDelete,
		UIRoll,
		UIError
	}

	public override void _EnterTree()
	{
		if(_instance != null)
		{
			QueueFree();
		}
		_instance = this;
  	}

	public void PlaySound(Sounds sound) 
	{
		Stream = sounds[(int)sound];
		Play();
	}
}
