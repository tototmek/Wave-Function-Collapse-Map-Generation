using System;
using System.Threading;
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
    private List<WFCPrototype> temp_prototypes = new List<WFCPrototype>();
    public Camera camera;
    private float scale;
    private WorkerThread workerThread;
    private List<(int, int)> loadedChunks;
    private List<(int, int)> visibleChunks;
    [SerializeField] private Transform view;


    private struct Tile
    {
        public Text text;
        public int x;
        public int y;

    }

    void Start()
    {
        (temp_prototypes, tiles) = WFCUtils.GeneratePrototypes(prototypes);
        scale = 1 / transform.localScale.x;
        loadedChunks = new List<(int, int)>();
        LoadNewMap();
        workerThread = new WorkerThread(this);
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
            Debug.Log("At chunk " + chunkX + ", " + chunkY);
        }

        // Generate chunks on another thread
        if (workerThread.showRequests.Count > 0)
        {
            (int[,] grid, int x, int y) = workerThread.showRequests.Dequeue();
            ShowTiles(grid, x, y);
        }

        // Find all visible chunks
        visibleChunks = new List<(int, int)>();
        (int centerX, int centerY) = map.WorldToChunkCoords(
            new Vector3(view.position.x * scale, view.position.y * scale, 0));
        for (int x = centerX - 2; x <= centerX + 2; x++)
        {
            for (int y = centerY - 2; y <= centerY + 2; y++)
            {
                visibleChunks.Add((x, y));
            }
        }


        // Manage chunk loading and unloading
        foreach ((int x, int y) in visibleChunks)
        {
            if (!loadedChunks.Contains((x, y)))
            {
                loadedChunks.Add((x, y));
                RequestChunk(x, y);
            }
        }
        for  (int i = loadedChunks.Count -1; i >= 0; i--)
        {
            if (!visibleChunks.Contains(loadedChunks[i]))
            {
                (int x, int y) = loadedChunks[i];
                RemoveChunk(map.GetGridSize(), x, y);
                loadedChunks.RemoveAt(i);
            }
        }
    }

    public int[,] LoadChunk(int chunkX, int chunkY)
    {
        return map.GetChunk(chunkX, chunkY);
    }

    public void ShowTiles(int[,] grid, int chunkX, int chunkY)
    {
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

    public void RemoveChunk(int gridSize, int chunkX, int chunkY)
    {
        for (int x = 0; x < gridSize - 1; x++)
        {
            for (int y = 0; y < gridSize - 1; y++)
            {
                tilemap.SetTile(
                    new Vector3Int(
                        chunkX * (chunkSize) + x,
                        chunkY * (chunkSize) + y, 0),
                    null);
            }
        }
    }

    public void RequestChunk(int chunkX, int chunkY)
    {
        workerThread.PutRequest(chunkX, chunkY);
    }

    private void LoadNewMap()
    {
        map = new WFCMap(chunkSize, temp_prototypes, new System.Random().Next(12124124));
        LoadChunk(0, 0);
    }


}

public class WorkerThread
{
    private Thread thread;
    private Queue<(int, int)> requests;
    public Queue<(int[,], int, int)> showRequests;
    private MapController parent;
    public WorkerThread(MapController parent)
    {
        this.parent = parent;
        requests = new Queue<(int, int)>();
        showRequests = new Queue<(int[,], int, int)>();
        thread = new Thread(new ThreadStart(WaitAndGenerate));
        thread.Start();
    }

    private void WaitAndGenerate()
    {
        while(true)
        {
            if (requests.Count > 0)
            {
                (int x, int y) = requests.Dequeue();
                PutShowRequest(parent.LoadChunk(x, y), x, y);
            }
        }
    }

    public void PutRequest(int chunkX, int chunkY)
    {
        requests.Enqueue((chunkX, chunkY));
    }

    private void PutShowRequest(int[,] grid, int chunkX, int chunkY)
    {
        showRequests.Enqueue((grid, chunkX, chunkY));
    }
}



