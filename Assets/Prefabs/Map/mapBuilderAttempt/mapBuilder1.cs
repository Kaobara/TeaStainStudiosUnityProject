using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class mapBuilder1 : MonoBehaviour
{
    private const float TILE_SIZE = 8f;
    private const int DEFAULTROT = 90;
    private const int NROT = 4;
    private const int defaultHeight = 2;
    [SerializeField]private ComputeNeighbours computeNeighbours;
    [SerializeField] private int length;
    [SerializeField] private int width;
    [SerializeField] private int seed = 92879187;
    [SerializeField] private float Tile_offset = 0.5f;
    [SerializeField] [Range(min: defaultHeight, 10)] private int maxBuildingHeight;
    [SerializeField] private BuildingGenerator buildingGen;
    [SerializeField] private TopologyPlotter topologyPlotter;
    [SerializeField] private int chunkSize = 5;
    [SerializeField] private bool debugMode;
    [SerializeField] private int maxSearchDepth = 20;


    


    /* private class mapObject: socket
     {
         int rotId=0;

         public List<(int, int)> currentCoords=new List<(int, int)>();
         private void applyRot(int rotId)
         {
            (var sockets , var socketcord) = this.rotate(rotId);
             this.right = sockets[(int)Directions.Right];
             this.forward = sockets[(int)Directions.Forward];
             this.left = sockets[(int)Directions.Left];
             this.back = sockets[(int)Directions.Back];
             this.tile.transform.Rotate(new Vector3(0, 1, 0), (float)rotId * DEFAULTROT);
         }
     }

     private IDictionary<(int, int), mapObject> objectMap;*/

    // Start is called before the first frame update

    private List<GameObject> roadTiles = new List<GameObject>();
    private List<GameObject> grassTiles = new List<GameObject>();
    private List<GameObject> houseTiles = new List<GameObject>();
    private List<GameObject> riverTiles = new List<GameObject>();
    //private IDictionary<(int, int), List<socketType>> CSPMap = new Dictionary<(int, int), List<socketType>>();
    private IDictionary<(int, int), List<(socketType, int)>> CSPMap = new Dictionary<(int, int), List<(socketType, int)>>();
    private IDictionary<(int, int), bool> CSPMapSolved = new Dictionary<(int, int), bool>();
    private IDictionary<(int, int), socket> GridMap = new Dictionary<(int, int), socket>();
    List<(socketType, int)> allStates;
    System.Random r;
    void Start()
    {
        allStates = computeNeighbours.allStates;
        topologyPlotter.Start();
        initialiseMap();
        solveCSP();
        makeGrid();
    }

    private void makeGrid()
    {
        int k = 0;
       
        for(int i =0; i<this.length; i++)
        {
            for(int j=0; j < this.width; j++)
            {
                socket tileskt = new socket(getRandomSocket(CSPMap[(i, j)]));
                            
                tileskt.tile = Instantiate(tileskt.tile, transform, true);
                generateBuildings(tileskt);
                tileskt.tile.transform.Rotate(0, (float)(tileskt.rotId) * DEFAULTROT,0, Space.Self);
                tileskt.tile.transform.position = new Vector3(0, 0, 0);
                tileskt.tile.transform.localPosition = new Vector3(0, 0, 0);
                
                Vector2 centrePos = new Vector2(i * (TILE_SIZE - Tile_offset), j * (TILE_SIZE - Tile_offset));                
                tileskt.tile.transform.localPosition += new Vector3(centrePos.x, 0,centrePos.y );
                plotGrass(tileskt, centrePos,transform);
                nameObject(tileskt, k++, (i, j));
                
               // setGridskt(tileskt, (i,j));
                GridMap.Add((i, j), tileskt);
                
            }
        }
    }

    private int maxChunks(int gridSize, int chunkSize)
    {
        var q = gridSize / chunkSize;
        var r = gridSize % chunkSize;

        return (r == 0) ? q : q + 1;
    }

    // a chunk boundary is [min, max)
    private List<(int, int)> getChunkBounds(int xChunkNumber, int zChunkNumber)
    {
        List<(int, int)> ChunkBounds = new List<(int, int)>(2);
        
        (int, int) xBounds = correctBounds(getMinMaxBounds(xChunkNumber), this.length);
        (int, int) zBounds = correctBounds(getMinMaxBounds(zChunkNumber), this.width);
        ChunkBounds.Add(xBounds);
        ChunkBounds.Add(zBounds);
        return ChunkBounds;
    }
    private void plotGrass(socket skt, Vector2 centrePos, Transform t)
    {
        if (skt.type == socketType.grass)
        {
            Vector2 halfSize = new Vector2((TILE_SIZE) / 2, (TILE_SIZE) / 2);
            topologyPlotter.plot(centrePos - halfSize, centrePos + halfSize, r, t);
        }

    }
    private (int, int) getMinMaxBounds(int chunkNumber)
    {
        int minBound = getChunkMinBound(chunkNumber);
        int maxBound = getChunkMaxBound(chunkNumber);

        return (minBound, maxBound);
    }
    private int getChunkMaxBound(int chunkNumber)
    {
        return (chunkNumber + 1) * chunkSize;
    }
    private int getChunkMinBound(int chunkNumber)
    {
        return (chunkNumber * chunkSize) - 1;
    }
    private (int, int) correctBounds((int, int) n, int max)
    {
        return (correctToWorldBounds(n.Item1, max), correctToWorldBounds(n.Item2, max));
    }
    private int correctToWorldBounds(int n, int max)
    {
        return Mathf.Max(0, Mathf.Min(max, n));
    }
   
    private void generateBuildings(socket skt)
    {
        if(skt.type == socketType.house)
        {
            buildingGen.generate(r.Next(defaultHeight, maxBuildingHeight), skt.tile.transform, r);
        }
    }
    private void nameObject(socket tileskt, int k, (int, int) Loc)
    {
        int i = Loc.Item1;
        int j = Loc.Item2;
        switch (tileskt.type)
        {
            case socketType.road:
                tileskt.tile.gameObject.name = "RoadTile_" + tileskt.rotId.ToString() +"_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                roadTiles.Add(tileskt.tile);
                break;

            case socketType.grass:
                tileskt.tile.gameObject.name = "GrassTile_" + tileskt.rotId.ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                break;

            case socketType.house:
                tileskt.tile.gameObject.name = "HouseTile_" + tileskt.rotId.ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                houseTiles.Add(tileskt.tile);
                break;
            case socketType.river0:
                tileskt.tile.gameObject.name = "RiverTile0_" + tileskt.rotId.ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                riverTiles.Add(tileskt.tile);
                break;
            case socketType.river1:
                tileskt.tile.gameObject.name = "RiverTile1_" + tileskt.rotId.ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                riverTiles.Add(tileskt.tile);
                break;
            default:
                break;
        }

    }

    /*private void setGridskt(socket skt, (int, int) Loc)
    {
       
        socketType[] neighbourskts = new socketType[skt.initial_cordRot.Count];
        List<int> idxToAvoid = new List<int>();
        int k = -1;
       
        foreach ((int, int) l in skt.initial_cordRot)
        {
            int i = l.Item1;
            int j = l.Item2;
            
            k++;
            var x = i + Loc.Item1;
            var y = j + Loc.Item2;
            
            if (x >= length || y >= width || x < 0 || y < 0)
            {
                
                idxToAvoid.Add(k);
                neighbourskts[k] = socketType.grass;
                continue;
            }
            
            var types = CSPMap[(x, y)];
            
            neighbourskts[k]=types[0];
           
        }
        
        skt.setskts(neighbourskts.ToList(), idxToAvoid);
        
    }*/

    private void initialiseMap()
    {
        // intialise the radnom number generator
        r = new System.Random(seed);
        // initialise all points with a list of all possible states
        for (int i = 0; i < this.length; i++)
        {
            for (int j = 0; j < this.width; j++)
            {
                List<(socketType, int)> states = new(allStates);

                CSPMap.Add((i,j),states);
            }
        }

        // collapse a random point on the map 
        /*var x = r.Next(0, this.length);
        var y = r.Next(0, this.width);*/
        var x = 0;
        var y = 0;
        var skt = new socket(computeNeighbours._socketNeighbours[socketType.grass][0]);
        CSPMap[(x, y)].Clear();
        CSPMap[(x, y)].Add((skt.type, skt.rotId));
        List<(int, int)> bounds = getChunkBounds(x, y);
        (int, int) xBounds = bounds[0];
        (int, int) zBounds = bounds[1];
        propogateToNeighbour((x, y), CSPMap[(x, y)], xBounds, zBounds, 0);
    }

    private void solveCSP()
    {
        // Vector2Int nChunks = new Vector2Int(this.length / this.chunkSize, this.width / this.chunkSize);
        Vector2Int maxNChunks = new Vector2Int(maxChunks(this.length, this.chunkSize), maxChunks(this.width, this.chunkSize));
        for (int i = 0; i < maxNChunks.x; i++)
        {
            for (int j = 0; j < maxNChunks.y; j++)
            {
                List<(int, int)> bounds = getChunkBounds(i, j); 
                (int, int) xBounds = bounds[0];
                (int, int) zBounds = bounds[1];
                if (debugMode)
                {
                    Debug.Log("Chunk number" + i + "," + j + xBounds);
                }
                solveCSPChunk(xBounds, zBounds);
            }
        }

        var disjointSet = findNotInBoth(CSPMap.Keys.ToList(), CSPMapSolved.Keys.ToList());
        Debug.Log("Disjoint set");
        foreach(var item in disjointSet)
        {
            Debug.Log("Location: " + item);

            foreach (var i in CSPMap[item])
            {
                Debug.Log(i);
            }
            
        }
        Debug.Log("\n");
       // printMap();
    }
    private void solveCSPChunk((int, int) xBounds, (int, int) zBounds)
    {
        //int k = 0;
        for (int i = xBounds.Item1; i < xBounds.Item2; i++)
        {
            for (int j = zBounds.Item1; j < zBounds.Item2; j++)
            {
                if (debugMode)
                {
                    Debug.Log("Begin csp at Location: (" + i + ", " + j + "): ");
                }

                if (this.CSPMap[(i, j)].Count == 1)
                {

                    //already been solved
                   
                    Debug.Log("Solved " + "Location: (" + i + ", " + j + "): " + this.CSPMap[(i, j)][0].ToString());
                        //printMap();
                    
                    //check if this is a valid solution
                    if (preCheck((i,j), new(getPrefabSocket(this.CSPMap[(i,j)][0]))))
                    {
                        if (!CSPMapSolved.ContainsKey((i, j)))
                        {
                            CSPMapSolved.Add((i, j), true);
                        }
                           
                        continue;
                    }
                    
                }
                this.CSPMap[(i, j)] = findNewValidSet(i, j);

                Debug.Log("[new findnewvalidSet]Location: (" + i + ", " + j + "): ");
                foreach (var ass in this.CSPMap[(i, j)])
                {
                    Debug.Log("[new ]  " + ass);
                }
                propogateToNeighbour((i, j), new(this.CSPMap[(i, j)]), xBounds, zBounds,0);
            }

        }
    }

    private List<(socketType, int)> findNewValidSet(int i, int j)
    {
        List<List<(socketType, int)>> ps = new List<List<(socketType, int)>>();
        if (i - 1 < 0)
        {
            ps.Add(allStates);
        }
        else
        {
            ps.Add(new List<(socketType, int)>(getneighbourType(this.computeNeighbours._socketNeighbours[this.CSPMap[(i - 1, j)][0].Item1][this.CSPMap[(i - 1, j)][0].Item2], 0)));
        }

        if (j - 1 < 0)
        {
            ps.Add(allStates);
        }
        else
        {
            ps.Add(new List<(socketType, int)>(getneighbourType(this.computeNeighbours._socketNeighbours[this.CSPMap[(i, j - 1)][0].Item1][this.CSPMap[(i, j - 1)][0].Item2], 1)));

        }
       
        // List<(socketType, int)> up = null;
        // List<(socketType, int)> right = null;
        if (j + 1 >= width)
        {
            ps.Add(allStates);
        }
        else
        {
            if (this.CSPMap[(i, j + 1)].Count == 1)
            {
                ps.Add(new List<(socketType, int)>(getneighbourType(this.computeNeighbours._socketNeighbours[this.CSPMap[(i, j + 1)][0].Item1][this.CSPMap[(i, j + 1)][0].Item2], 3)));
            }
        }
        if (i + 1 >= length)
        {
            ps.Add(allStates);
        }
        else
        {
            if (this.CSPMap[(i + 1, j)].Count == 1)
            {
               ps.Add(new List<(socketType, int)>(getneighbourType(this.computeNeighbours._socketNeighbours[this.CSPMap[(i + 1, j)][0].Item1][this.CSPMap[(i + 1, j)][0].Item2], 2)));
            }
        }
        var intersect = findIntersect(ps[0], ps[1]);
        if(intersect.Count == 0)
        {
            ps.Clear();
            List<(socketType, int)> temp = new List<(socketType, int)>();
            temp.Add((socketType.road, 0));
            temp.Add((socketType.grass, 0));
            

            return temp;
        }
        List<(socketType, int)> tempI = new List<(socketType, int)>(intersect);
        for (int k=2; k<ps.Count; k++)
        {
            tempI= findIntersect(tempI, ps[k]);
        }
        if(tempI.Count == 0)
        {
            return intersect;
        }
        return tempI;
    }

    private void printMap()
    {
        for (int i = 0; i < this.length; i++)
        {
            for (int j = 0; j < this.width; j++)
            {
                
                printList((i, j));
            }
        }
    }

    private void printList((int, int) Loc)
    {
        Debug.Log("[propogate print List] Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + "rotID: "  + "nCSPMAP"+ CSPMap[Loc].Count.ToString());
        
        foreach (var k in CSPMap[Loc])
        {
            Debug.Log(k);
        }
        Debug.Log("\n");

    }
    private bool isInBounds((int, int) Loc, (int, int) xBounds, (int, int) zBounds)
    {
        if(Loc.Item1<xBounds.Item1 || Loc.Item1 > xBounds.Item2 || Loc.Item2<zBounds.Item1 || Loc.Item2>zBounds.Item2)
        {
            //out of x bounds or z Bounds, move on
            return false;
        }
        return true;
    }

    private bool propogateToNeighbour((int, int) Loc, List<(socketType, int)> types, (int, int) xBounds, (int, int) zBounds, int currentDepth)
    {
        // this tile is of type 'type' we need to check if making this change to current tile will cause issues
        //Debug.Log("inside propogate to neighbour");

        if (++currentDepth > maxSearchDepth)
        {
            return true;
        }
        /*if (CSPMapSolved.ContainsKey(Loc))
        {
            return true;
        }*/
        if (types.Count == 1 && (CSPMap[Loc][0].Item1 == types[0].Item1) && (CSPMap[Loc][0].Item2 == types[0].Item2))
        {
            return true;
        }
        
        socket skt = getRandomSocket(types);
        List<(socketType, int)> currentStates = new List<(socketType, int)>(CSPMap[Loc]);
        CSPMap[Loc].Clear();
        CSPMap[Loc].Add((skt.type, skt.rotId));

        bool check = propogate(Loc, skt, xBounds, zBounds, currentDepth);
        while (!check)
        {
            CSPMap[Loc].Clear();
            CSPMap[Loc].AddRange(currentStates);
            /* if (!skt.isSymmetric)
             {
                 var rotId = checkAllRot(skt, Loc, xBounds, zBounds, currentDepth);
                 if (rotId> -1)
                 {
                     rotIds[(Loc)] = rotId;
                     break;
                 }
             }*/

            types.Remove((skt.type, skt.rotId));
            if (types.Count == 0)
            {
                return false;
            }
            skt = getRandomSocket(types);
            CSPMap[Loc].Clear();
            CSPMap[Loc].Add((skt.type, skt.rotId));
            check = propogate(Loc, skt, xBounds, zBounds, currentDepth);
        }
       

        return true;
    }

    // return the rot id for which this worked
    /*private int checkAllRot(socket skt, (int, int) Loc, (int, int)xBounds, (int, int)zBounds, int currentDepth)
    {
        List<socketType> currentStates = new List<socketType>(CSPMap[Loc]);
        CSPMap[Loc].Clear();
        CSPMap[Loc].Add(skt.type);
        var nrot = (skt.type == socketType.river0) ? 2 : NROT;
        for (int i =1; i<NROT; i++)
        {
            skt.applyRot(i);
            rotIds[Loc] = i;
            if(propogate(Loc, skt, xBounds, zBounds, currentDepth))
            {
                skt.reset();
                return i;
            }
        }
        rotIds[Loc] = 0;    
        CSPMap[Loc].Clear();
        CSPMap[Loc].AddRange(currentStates);
        skt.reset();
        return -1;
    }*/
    
    // if this is a valid move and it doesnt fail any csp 
    private bool propogate((int, int) Loc, socket skt, (int, int) xBounds, (int, int) zBounds, int currentDepth)
    {
        /*if (CSPMapSolved.ContainsKey(Loc))
        {
            return true;
        }*/
        //Debug.Log("inside propogate");
        
        if (!preCheck(Loc, skt))
        {
            return false;
        }
        int k = 0;
        List<(int, int)> roadlocs = new List<(int, int)>();
        IDictionary<(int, int), List<(socketType, int)>> currentStates = new Dictionary<(int, int), List<(socketType, int)>>();
        IDictionary<(int, int), List<(socketType, int)>> newStates = new Dictionary<(int, int), List<(socketType, int)>>();
        
        // to ensure there is always two roads connected to this road
        int roadCount = 0;
        foreach((int, int) l in skt.initial_cordRot)
        {
            int i = l.Item1;
            int j = l.Item2;
            
            var x = i + Loc.Item1;
            var y = j + Loc.Item2;
            List<(socketType, int)> t = new List<(socketType, int)>();
            t.AddRange(getneighbourType(skt, k++));
            
            if (x >= length || y>=width || x<0 ||y<0)
            {
                roadCount++;
                // house is pointing out of the map
                if (t.Contains((socketType.road, 0)) && t.Count == 1)
                {
                    return false;
                }
                continue;
            }
           
            if (CSPMap[(x, y)].Count == 0)
            {
                return false;
            }
            if (t.Count == 0)
            {
                return false;
            }

            if (debugMode)
            {
                Debug.Log("[propogate] Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + CSPMap[Loc].Count.ToString() + CSPMap[Loc][0]);
                Debug.Log("Valid States " + "type: " + k.ToString() + " rotID " + CSPMap[Loc][0].Item2);
                Debug.Log("checkLocation: (" + x.ToString() + ", " + y.ToString() + "): ");
                Debug.Log("k: " + k.ToString());

                foreach (var d in t)
                {
                    Debug.Log(d);
                }
                Debug.Log("\n");
                printList((x, y));
            }
           
            List<(socketType,int)> validStates = new List<(socketType,int)>(findIntersect(t, CSPMap[(x, y)]));
            if (validStates == null) return false; 
           
            if (validStates.Count( ) == 0)
            {
                Debug.Log("false");
                return false;
            }

            if (validStates.Contains((socketType.road,0)))
            {
                roadlocs.Add((x, y));
                roadCount++;
            }
            if (debugMode)
            {
                foreach (var d in validStates)
                {
                    Debug.Log(((int)d.Item1).ToString());
                }
            }
            
            if (CSPMap[(x,y)].Count == validStates.Count)
            {
                //no change to this cell so continue;
                
                continue;
            }

             

            currentStates.Add((x,y),new List<(socketType,int)>(CSPMap[(x, y)]));
            newStates.Add((x, y), validStates);
            //Debug.Log("Before: ");
            //printList((x, y));
            CSPMap[(x,y)].Clear();
            //CSPMap[(x, y)].TrimExcess();
            CSPMap[(x, y)].AddRange(validStates);
            if (!isInBounds((x, y), xBounds, zBounds))
            {
                // in unexplored area
                continue;
            }
            if (!propogateToNeighbour((x, y), new(CSPMap[(x, y)]), xBounds, zBounds, currentDepth))
            {
                CSPMap[(x, y)].Clear();
                //CSPMap[(x, y)].TrimExcess();
                CSPMap[(x, y)] = new List<(socketType,int)>(currentStates[(x,y)]);
                return false;
            }

            //printList((x,y));
        }
        if (debugMode)
        {
            /* Debug.Log("propogated");
                     printMap();*/
            Debug.Log("road count: " + roadCount);
        }

        if (socketType.road == skt.type)
        {
            if (roadCount < 2)
            {
                return false;
            }
            else if (roadCount == 2)
            {
                List<(socketType, int)> validStates = new List<(socketType, int)>();
                validStates.Add((socketType.road, 0));
                foreach (var locs in roadlocs)
                {
                    CSPMap[locs].Clear();
                    CSPMap[locs].AddRange(validStates);
                }
            }
        }
        return true;
    }

    private List<(socketType, int)> getneighbourType(socket skt, int k)
    {
        //return possible neighbours on side k
        var neighbours = computeNeighbours._socketNeighbours[skt.type][skt.rotId].skts[k];
        List<(socketType, int)> lst = new List<(socketType, int)>();

        foreach(var n  in neighbours)
        {
            lst.Add((n.type, n.rotId));
        }

        return lst;
    }

    

    private bool preCheck((int,int) Loc, socket skt)
    {
       // same as propogate but first quickly checks if the 1 degree neighbour cells are ok before going into recursion
        int k = 0;
        List<(int, int)> roadlocs = new List<(int, int)>();
        // to ensure there is always two roads connected to this road
        int roadCount = 0;
        foreach ((int, int) l in skt.initial_cordRot)
        {
            int i = l.Item1;
            int j = l.Item2;

            var x = i + Loc.Item1;
            var y = j + Loc.Item2;
            List<(socketType, int)> t = new List<(socketType, int)>();
            t.AddRange(getneighbourType(skt, k));
            k++;
            if (x >= length || y >= width || x < 0 || y < 0)
            {
                roadCount++;
                // house is pointing out of the map
                if (t.Contains((socketType.road, 0)) && t.Count == 1)
                {
                    Debug.Log("false");
                    return false;
                }
                continue;
            }

            if (CSPMap[(x, y)].Count == 0)
            {
                return false;
            }
            if (t.Count == 0)
            {
                return false;
            }
            if (debugMode)
            {
                Debug.Log("[precheck] Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + CSPMap[Loc].Count.ToString() + CSPMap[Loc][0]);
                Debug.Log("Valid States " + "type: " + k.ToString());
                Debug.Log("checkLocation: (" + x.ToString() + ", " + y.ToString() + "): ");
                Debug.Log("k: " + k.ToString());

                foreach (var d in t)
                {
                    Debug.Log(d);
                }

                Debug.Log("end valid states \n");
                printList((x, y));
            }


            List<(socketType, int)> validStates = new List<(socketType, int)>(findIntersect(t, CSPMap[(x, y)]));
            if (validStates == null) return false;
            
            if (validStates.Count() == 0)
            {
                //Debug.Log("false");
                return false;
            }
            
            if (validStates.Contains((socketType.road,0)))
            {
                roadlocs.Add((x, y));
                roadCount++;
            }
        }

        if (socketType.road == skt.type)
        {
            if (roadCount < 1)
            {
                return false;
            }
        }

        if (debugMode)
        {
            Debug.Log("[precheck end] Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + CSPMap[Loc][0]);

        }

        return true;
    }

    private List<(socketType, int)> findIntersect(List<(socketType, int)> l1, List<(socketType, int)> l2)
    {
        List<(socketType, int)> intersect = new List<(socketType, int)>();
        foreach(var sktType in l1)
        {
            if (l2.Contains(sktType))
            {
                intersect.Add(sktType);
            }
        }
        return intersect;
    }
    private List<T> findNotInBoth<T>(List<T> l1, List<T> l2)
    {
        List<T> intersect = new List<T>();
        foreach (var sktType in l1)
        {
            if (!l2.Contains(sktType))
            {
                intersect.Add(sktType);
            }
        }
        return intersect;
    }

    private void CheckInDict<T, K>(IDictionary<T, K> dict, T key, K item)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key, item);
        }

    }

    private socket getRandomSocket(List<(socketType, int)> types)
    {

        if (types.Count == 1)
        {
            return new socket(getPrefabSocket(types[0]));
        }

        var uniqueSktTypes = new Dictionary<socketType, List<int>>();
        foreach(var type in types)
        {
            //check if this type already exists in our dictionary, if it doesnt add a list of ints(rotIds)
            CheckInDict(uniqueSktTypes, type.Item1, new List<int>());
            // now we can safely add our rotId
            uniqueSktTypes[type.Item1].Add(type.Item2);
        }

        float total = 0f;
        List<float> allProb = new List<float>();
        foreach (var key in uniqueSktTypes.Keys)
        {
            var p = getProb(key);
            allProb.Add(p);
            total += p;
        }
        float x = (float)r.NextDouble() *(total);
        total = 0f;
        int idx;
        int i = 0;
        foreach( var key in uniqueSktTypes.Keys)
        {
            total += allProb[i];
            if (x < total)
            {
                var rot = GetRandomItem(uniqueSktTypes[key]);
                return new socket(getPrefabSocket((key, rot)));
            }
            i++;
        }
        //uniqueSktTypes = null;
        idx =allProb.IndexOf(allProb.Max());
        idx = (idx < 0) ? 0 : (idx>=types.Count)?(types.Count-1): idx;
        return new socket(getPrefabSocket(types[idx]));
    }

    

    private float getProb(socketType type)
    {
        return computeNeighbours._socketNeighbours[type][0].probability;
    }

    private socket getPrefabSocket((socketType,int) type)
    {
        return computeNeighbours._socketNeighbours[type.Item1][type.Item2];
    }
    private T GetRandomItem<T>(List<T> listToRandomize)
    {
        int randomNum = r.Next(0, listToRandomize.Count);
        return listToRandomize[randomNum];
    }


}
    
