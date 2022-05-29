using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCMap {
    [SerializeField] private int chunkSize = 16;
    private int actualGridSize;
    private Dictionary<(int, int), int[,]> map = new Dictionary<(int, int), int[,]>();
    private List<WFCPrototype> prototypes;
    private int seed;
    private System.Random random;

    public WFCMap(int chunkSize, List<WFCPrototype> prototypes, int seed)
    {
        this.prototypes = prototypes;
        this.chunkSize = chunkSize;
        this.seed = seed;
        random = new System.Random(seed);
        actualGridSize = chunkSize + 1;
    }

    public int[,] GetMapAtPosition(Vector3 position)
    {
        (int x, int y) = WorldToChunkCoords(position);
        return GetChunk(x, y);
    }

    public (int, int) WorldToChunkCoords(Vector3 position)
    {
        return (Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.y / chunkSize));
    }

    public int[,] GetChunk(int x, int y)
    {
        if (map.ContainsKey((x,y))) {
            return map[(x, y)];
        }
        else
        {
            return GenerateChunk(x, y);
        }
    }

    public bool HasChunk(int x, int y)
    {
        return map.ContainsKey((x, y));
    }

    public int GetGridSize()
    {
        return actualGridSize;
    }

    private int[,] GenerateChunk(int x, int y) 
    {
        int iterations = 0;
        WFCGrid grid = new WFCGrid(actualGridSize, prototypes, seed + x + 10000 * y);
        //Check if there's a chunk to the left
        bool hasLeft = (HasChunk(x - 1, y));
        int[,] leftChunk = new int[1,1];
        if (hasLeft)
        {
            leftChunk = GetChunk(x - 1, y);
        }
        //Check if there's a chunk to the right
        bool hasRight = (HasChunk(x + 1, y));
        int[,] rightChunk = new int[1, 1];
        if (hasRight)
        {
            rightChunk = GetChunk(x + 1, y);
        }
        //Check if there's a chunk to the top
        bool hasTop = (HasChunk(x, y + 1));
        int[,] topChunk = new int[1, 1];
        if (hasTop)
        {
            topChunk = GetChunk(x, y + 1);
        }
        //Check if there's a chunk to the bottom
        bool hasBottom = (HasChunk(x, y - 1));
        int[,] bottomChunk = new int[1, 1];
        if (hasBottom)
        {
            bottomChunk = GetChunk(x, y - 1);
        }
        //Close borders
        if (hasLeft || hasRight)
        {
            for (int i = 0; i < grid.Size; i++)
            {
                if (hasLeft) grid.CloseTile(0, i, leftChunk[grid.Size - 1, i]);
                if (hasRight) grid.CloseTile(grid.Size-1, i, rightChunk[0, i]);
            }
        }
        if (hasTop || hasBottom)
        {
            for (int i = 0; i < grid.Size; i++)
            {
                if (hasBottom) grid.CloseTile(i, 0, bottomChunk[i, grid.Size - 1]);
                if (hasTop) grid.CloseTile(i, grid.Size - 1, topChunk[i, 0]);
            }
        }
        
        //topleft
        if(HasChunk(x-1, y+1))
        {
            grid.CloseTile(0, grid.Size - 1, GetChunk(x - 1, y + 1)[grid.Size - 1, 0]);
        }
        //topright
        if (HasChunk(x + 1, y + 1))
        {
            grid.CloseTile(grid.Size - 1, grid.Size - 1, GetChunk(x + 1, y + 1)[0, 0]);
        }
        //bottomleft
        if (HasChunk(x - 1, y - 1))
        {
            grid.CloseTile(0, 0, GetChunk(x - 1, y - 1)[grid.Size - 1, grid.Size - 1]);
        }
        //bottomright
        if (HasChunk(x + 1, y - 1))
        {
            grid.CloseTile(grid.Size - 1, 0, GetChunk(x + 1, y - 1)[0, grid.Size - 1]);
        }
        

        //Fill with random biome
        int[] biomeIndexes = new int[] { 9, 16, 19, 29, 22, 24, 26, 25, 30 };
        int biomeIndex = random.Next(biomeIndexes.Length);
        grid.CloseTile(grid.Size / 2, grid.Size / 2, biomeIndexes[biomeIndex]);

        while (iterations < 12)
        {
            try
            {
                map[(x, y)] = grid.WaveFunctionCollapse();
                return map[(x, y)];
            }
            catch (Exception e)
            {
                iterations++;
                Debug.LogError(e.StackTrace);
                Debug.LogError(e.Message);
            }
        }
        throw new Exception("Could not generate chunk " + x + ", " + y);
    }
}
