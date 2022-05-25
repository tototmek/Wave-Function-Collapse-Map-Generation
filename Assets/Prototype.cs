using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class Prototype : ScriptableObject
{
    public TileBase tile;
    public float chance = 1;
    public List<Prototype> spawnOnlyAdjacentTo;
    public List<Prototype> spawnOnlyLeftFrom;
    public List<Prototype> spawnOnlyRightFrom;
    public List<Prototype> spawnOnlyAbove;
    public List<Prototype> spawnOnlyBelow;
    [NonSerialized] public int id;
    public bool Active { get; private set; }

    public void Activate()
    {
        Active = true;
    }
}

