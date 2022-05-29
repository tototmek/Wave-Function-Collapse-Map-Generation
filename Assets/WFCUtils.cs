using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class WFCUtils
{
    public static (List<WFCPrototype>, List<TileBase>) GeneratePrototypes(Prototype[] prototypes)
    {
        List<WFCPrototype> temp_prototypes = new List<WFCPrototype>();
        List<TileBase> tiles = new List<TileBase>();
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
        return (temp_prototypes, tiles);
    }
}
