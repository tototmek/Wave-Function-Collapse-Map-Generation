using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WFCGrid
{
    // The wave superoposition: [x, y, prototype index], 1 if prototype allowed, otherwise 0;
    private bool[][][] wave;
    // Prototype allowed neighbours: [prototype index, direction index, allowed neighbour id]
    private bool[,,] constraints;
    // Field saying whether tile is closed
    private bool[,] closed;
    private int[,] map;
    private int closedCount = 0;
    private System.Random random;
    private (int, int)[] directions = new (int, int)[4] { (-1, 0), (1, 0), (0, 1), (0, -1) };
    private int[] correspondingDirection = new int[4] { 1, 0, 3, 2 };
    private float[] prototypeWeights;

    public int Size { get; private set; }
    public int PrototypesCount { get; private set; }

    public WFCGrid(int mapSize, List<WFCPrototype> prototypes, int seed)
    {
        InitializeEmptyMap(mapSize, prototypes, seed);
    }

    public void InitializeEmptyMap(int mapSize, List<WFCPrototype> prototypes, int seed)
    {
        if (mapSize < 1) throw new InvalidOperationException("Map Size must be larger than 0!");
        random = new System.Random(seed);
        Size = mapSize;
        PrototypesCount = prototypes.Count;
        wave = new bool[Size][][];
        closed = new bool[Size, Size];
        closedCount = 0;
        map = new int[Size, Size];
        for (int x = 0; x < Size; x++)
        {
            wave[x] = new bool[Size][];
            for (int y = 0; y < Size; y++)
            {
                wave[x][y] = new bool[PrototypesCount];
                for (int k = 0; k < PrototypesCount; k++)
                {
                    wave[x][y][k] = true;
                }
                closed[x, y] = false;
                map[x, y] = 0;
            }
        }
        constraints = new bool[PrototypesCount, 4, PrototypesCount];
        prototypeWeights = new float[PrototypesCount];
        for (int k = 0; k < PrototypesCount; k++)
        {
            for (int j = 0; j < PrototypesCount; j++)
            {
                for (int d = 0; d < 4; d++)
                {
                    constraints[k, d, j] = prototypes[k].allowedNeighbours[d].Contains(prototypes[j]);
                }
            }
            prototypeWeights[k] = prototypes[k].weight;
        }
    }

    public int[,] WaveFunctionCollapse()
    {
        while(!AllTilesClosed())
        {
            (int x, int y) = GetLowestEntropyTile();
            CloseTile(x, y);
            PropagateConstraints(x, y);
        }
        return map;
    }

    private void CloseTile(int x, int y)
    {
        if (closed[x, y]) return;
        closed[x, y] = true;
        closedCount++;
        int chosen = RandomIndexFromBoolArray(wave[x][y], prototypeWeights);
        map[x, y] = chosen;
        for (int k = 0; k < PrototypesCount; k++)
        {
            wave[x][y][k] = false;
        }
        wave[x][y][chosen] = true;
        PropagateConstraints(x, y);
    }

    public void CloseTile(int x, int y, int prototypeIndex)
    {
        if (closed[x, y]) return;
        closed[x, y] = true;
        closedCount++;
        int chosen = prototypeIndex;
        map[x, y] = chosen;
        for (int k = 0; k < PrototypesCount; k++)
        {
            wave[x][y][k] = false;
        }
        wave[x][y][chosen] = true;
        PropagateConstraints(x, y);
    }

    public int RandomIndexFromBoolArray(bool[] array, float[] weights)
    {
        float totalRoute = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            if (array[i]) totalRoute += weights[i];
        }
        float route = 0;
        float randomNumber = 0.001f * random.Next((int)(totalRoute * 1000));
        for (int i = 0; i < weights.Length; i++)
        {
            if (array[i])
            {
                route += weights[i];
                if (route >= randomNumber)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private bool AllTilesClosed()
    {
        return closedCount >= Size * Size;
    }

    private (int,int) GetLowestEntropyTile()
    {
        int minEntropy = int.MaxValue;
        int entropy;
        int resultX = -1;
        int resultY = -1;
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                if (!closed[x, y])
                {
                    entropy = GetEntropy(x, y);
                    if (entropy < minEntropy)
                    {
                        minEntropy = entropy;
                        resultX = x;
                        resultY = y;
                    }
                }
            }
        }
        return (resultX, resultY);
    }

    public int GetEntropy(int x, int y)
    {
        int entropy = 0;
        for (int k=0; k<PrototypesCount; k++)
        {
            if (wave[x][y][k]) entropy++;
        }
        return entropy;
    }

    private void PropagateConstraints(int x, int y)
    {
        int nX;
        int nY;
        bool passed1;
        bool passed2;

        for (int i = 0; i < 4; i++)
        {
            nX = x + directions[i].Item1;
            nY = y + directions[i].Item2;

            if (nX >= 0 && nX < Size && nY >= 0 && nY < Size)
            {
                for (int nK = 0; nK < PrototypesCount; nK++)
                {
                    passed1 = false;
                    passed2 = false;
                    // Remove all neighbour's prototypes which are not among my
                    // allowed prototypes to the direction
                    for (int k = 0; k < PrototypesCount; k++)
                    {
                        if (wave[x][y][k] && constraints[k,i,nK])
                        {
                            passed1 = true;
                            break;
                        }
                    }
                    // Remove all neighbour's prototypes which don't allow
                    // any of my prototypes to the opposite direction
                    for (int k = 0; k < PrototypesCount; k++)
                    {
                        if (wave[nX][nY][nK] && constraints[nK, correspondingDirection[i], k])
                        {
                            passed2 = true;
                            break;
                        }
                    }
                    if (wave[nX][nY][nK] != passed1 && passed2)
                    {
                        wave[nX][nY][nK] = passed1 && passed2;
                        PropagateConstraints(nX, nY);
                    }
                }
            }
        }
    }

    public void Reset(List<WFCPrototype> prototypes, int seed)
    {
        InitializeEmptyMap(Size, prototypes, seed);
    }

    public void CloseBorder(int prototype)
    {
        for (int x=0; x< Size; x++)
        {
            CloseTile(x, 0, prototype);
            CloseTile(x, Size-1, prototype);
        }
        for (int y = 0; y < Size; y++)
        {
            CloseTile(0, y, prototype);
            CloseTile(Size - 1, y, prototype);
        }
    }

    public int GetIdAt(int x, int y)
    {
        return map[x, y];
    }
}

public class WFCPrototype
{
    public int Id { get; set; }
    public float weight = 1;
    public HashSet<WFCPrototype>[] allowedNeighbours;

    public WFCPrototype(int nSides)
    {
        // Set up constraint lists
        allowedNeighbours = new HashSet<WFCPrototype>[nSides];
        for (int i = 0; i < nSides; i++)
        {
            allowedNeighbours[i] = new HashSet<WFCPrototype>();
        }
    }
}