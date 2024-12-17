using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyLoader : Node
{
    private const string EnemyPath = "res://Resources/Enemies/";
    private readonly Dictionary<EnemyTypes, EnemyData> _cache = new();
    
    public EnemyData Load(EnemyTypes type)
    {
        if (_cache.TryGetValue(type, out var cachedEnemy))
        {
            return cachedEnemy;
        }
        
        var resourcePath = $"{EnemyPath}{type.ToString().ToLower()}.tres";
        var enemy = GD.Load<EnemyData>(resourcePath);

        if (enemy != null)
        {
            _cache[type] = enemy;
        }

        return enemy;
    }
}
