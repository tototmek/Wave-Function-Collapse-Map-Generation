using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    
    public WFCMap map;
    public int chunkSize;
    public Tilemap tilemap;
    private List<TileBase> tiles = new List<TileBase>();
    public Prototype[] prototypes;
    private HashSet<WFCPrototype> readyPrototypes = new HashSet<WFCPrototype>();
    private List<WFCPrototype> temp_prototypes = new List<WFCPrototype>();
    public Camera camera;
    public float scale = 3;


    private struct Tile
    {
        public Text text;
        public int x;
        public int y;

    }

    void Start()
    {
        // Parse prototypes
        int currentId = 0;
        foreach (Prototype data in prototypes)
        {
            // Create prototypes, set IDs
            WFCPrototype prototype = new WFCPrototype(4);
            data.id = currentId;
            prototype.Id = currentId++;
            data.Activate();
            prototype.weight = data.chance;
            temp_prototypes.Add(prototype);
            tiles.Add(data.tile);
            
        }
        foreach (Prototype data in prototypes)
        {
            // Adjacent to
            if (data.spawnOnlyAdjacentTo.Count > 0)
            {
                // Add correct prototypes
                foreach (Prototype allowedNeighbourData in data.spawnOnlyAdjacentTo)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (allowedNeighbourData.Active)
                        {
                            temp_prototypes[data.id].allowedNeighbours[i].Add(temp_prototypes[allowedNeighbourData.id]);
                            temp_prototypes[allowedNeighbourData.id].allowedNeighbours[i].Add(temp_prototypes[data.id]);
                        }
                    }
                }
            }
            // Left from
            if (data.spawnOnlyLeftFrom.Count > 0)
            {
                foreach (Prototype allowedNeighbourData in data.spawnOnlyLeftFrom)
                {
                    if (allowedNeighbourData.Active)
                    {
                        temp_prototypes[data.id].allowedNeighbours[1].Add(temp_prototypes[allowedNeighbourData.id]);
                        temp_prototypes[allowedNeighbourData.id].allowedNeighbours[0].Add(temp_prototypes[data.id]);
                    }
                }
            }
            // Right from
            if (data.spawnOnlyRightFrom.Count > 0)
            {
                foreach (Prototype allowedNeighbourData in data.spawnOnlyRightFrom)
                {
                    if (allowedNeighbourData.Active)
                    {
                        temp_prototypes[data.id].allowedNeighbours[0].Add(temp_prototypes[allowedNeighbourData.id]);
                        temp_prototypes[allowedNeighbourData.id].allowedNeighbours[1].Add(temp_prototypes[data.id]);
                    }
                }
            }
            // Above
            if (data.spawnOnlyAbove.Count > 0)
            {
                foreach (Prototype allowedNeighbourData in data.spawnOnlyAbove)
                {
                    if (allowedNeighbourData.Active)
                    {
                        temp_prototypes[data.id].allowedNeighbours[3].Add(temp_prototypes[allowedNeighbourData.id]);
                        temp_prototypes[allowedNeighbourData.id].allowedNeighbours[2].Add(temp_prototypes[data.id]);
                    }
                }
            }
            // Below
            if (data.spawnOnlyBelow.Count > 0)
            {
                foreach (Prototype allowedNeighbourData in data.spawnOnlyBelow)
                {
                    if (allowedNeighbourData.Active)
                    {
                        temp_prototypes[data.id].allowedNeighbours[2].Add(temp_prototypes[allowedNeighbourData.id]);
                        temp_prototypes[allowedNeighbourData.id].allowedNeighbours[3].Add(temp_prototypes[data.id]);
                    }
                }
            }
        }


        foreach (WFCPrototype prototype in temp_prototypes)
        {
            readyPrototypes.Add(prototype);
        }

        LoadNewMap();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            LoadNewMap();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            (int chunkX, int chunkY) = map.WorldToChunkCoords(
                    new Vector3(mousePos.x * scale, mousePos.y * scale, 0)
                );
            Debug.Log("Clicked at " + mousePos.x + ", " + mousePos.y);
            Debug.Log("Loading chunk " + chunkX + ", " + chunkY);
            LoadChunk(chunkX, chunkY);
        }
    }

    private void LoadChunk(int chunkX, int chunkY)
    {
        int[,] grid = map.GetChunk(chunkX, chunkY);
        for (int x = 0; x < grid.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < grid.GetLength(1) - 1; y++)
            {
                tilemap.SetTile(
                    new Vector3Int(
                        chunkX * (chunkSize) + x,
                        chunkY * (chunkSize) + y, 0),
                    tiles[grid[x, y]]);
            }
        }
    }

    private void LoadNewMap()
    {
        map = new WFCMap(chunkSize, temp_prototypes, new System.Random().Next(12124124));
        /*
        int[,] grid = map.GetMapAtPosition(Vector3.zero);
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Debug.Log(grid[x, y]);
                tilemap.SetTile(new Vector3Int(x, y, 0), tiles[grid[x, y]]);
            }
        }
        */
        LoadChunk(0, 0);
    }
}



