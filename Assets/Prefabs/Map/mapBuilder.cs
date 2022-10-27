using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;

public class mapBuilder : MonoBehaviour
{
    private const float TILE_SIZE = 8f;
    private const int DEFAULTROT = 90;
    private const int NROT = 4;
    private const int defaultHeight = 2;
    [SerializeField] private socket roadTile;
    [SerializeField] private socket grassTile;
    [SerializeField] private socket houseTile;
    [SerializeField] private socket river0;
    [SerializeField] private socket river1;
    [SerializeField] private int length;
    [SerializeField] private int width;
    [SerializeField] private int seed = 92879187;
    [SerializeField] private float Tile_offset = 0.5f;
    [SerializeField] [Range(min: defaultHeight, 10)] private int maxBuildingHeight;
    [SerializeField] private BuildingGenerator buildingGen;
    [SerializeField] private TopologyPlotter topologyPlotter;
    [SerializeField] private int chunkSize = 5;
    [SerializeField] private bool debugMode;
    [SerializeField] private bool exploreMode;
    private int maxSearchDepth;

    [System.Serializable]
    public enum socketType
    {
        road,
        grass,
        house,
        river0,
        river1
    }

    [System.Serializable]
    public class socket
    {
        public socketType type;
        public bool isSymmetric;
        public float probability = 0.5f;
        public GameObject tile;
        public List<socketType> left = new List<socketType>();
        public List<socketType> right = new List<socketType>();
        public List<socketType> forward = new List<socketType>();
        public List<socketType> back = new List<socketType>();
        public List<List<socketType>> initial_sockets = new List<List<socketType>>();
        public List<(int, int)> initial_cordRot = new List<(int, int)>();

        public enum Directions
        {
            Right,
            Forward,
            Left,
            Back
        }

        public socket()
        {
            this.initial_sockets.Add(this.right);
            this.initial_sockets.Add(this.forward);
            this.initial_sockets.Add(this.left);
            this.initial_sockets.Add(this.back);
            this.initial_cordRot.Add((1, 0));
            this.initial_cordRot.Add((0, 1));
            this.initial_cordRot.Add((-1, 0));
            this.initial_cordRot.Add((0, -1));
        }
        public socket(socket obj)
        {
            this.type = obj.type;
            this.isSymmetric = obj.isSymmetric;
            this.probability = obj.probability;
            this.tile = obj.tile;
            this.left = new List<socketType>(obj.left);
            this.right = new List<socketType>(obj.right);
            this.forward = new List<socketType>(obj.forward);
            this.back = new List<socketType>(obj.back);
            this.initial_sockets = obj.initial_sockets;
            this.initial_cordRot = obj.initial_cordRot;
        }
        // rotates the modle stuff by 90deg and return them in a list, list is indexed based on Directions
        public (List<List<socketType>>, List<(int, int)>) rotate(int rotId)
        {
            List<List<socketType>> rot = new List<List<socketType>>();
            List<(int, int)> cordrot = new List<(int, int)>();
            for (int i = 0; i < this.initial_sockets.Count; i++)
            {
                rot.Add(this.initial_sockets[(i + rotId) % NROT]);
                cordrot.Add(this.initial_cordRot[(i + rotId) % NROT]);
            }

            return (rot, cordrot);
        }
        public void applyRot(int rotId)
        {
            (var sockets, var socketcord) = this.rotate(rotId);
            this.right = sockets[(int)Directions.Right];
            this.forward = sockets[(int)Directions.Forward];
            this.left = sockets[(int)Directions.Left];
            this.back = sockets[(int)Directions.Back];
        }

        public void reset()
        {
            this.right = this.initial_sockets[((int)Directions.Right)];
            this.forward = this.initial_sockets[(int)Directions.Forward];
            this.left = this.initial_sockets[(int)Directions.Left];
            this.back = this.initial_sockets[(int)Directions.Back];
        }

        public void setskts(List<socketType> neighbourskts, List<int> idxToAvoid)
        {
            for (int i = 0; i < 4; i++)
            {
                switch ((Directions)i)
                {
                    case Directions.Right:
                        this.right.Clear();
                        if (idxToAvoid.Contains(i))
                        {
                            break;
                        }
                        this.right.Add(neighbourskts[(int)Directions.Right]);
                        break;
                    case Directions.Forward:
                        this.forward.Clear();
                        if (idxToAvoid.Contains(i))
                        {
                            break;
                        }
                        this.forward.Add(neighbourskts[(int)Directions.Forward]);
                        break;
                    case Directions.Back:
                        this.back.Clear();
                        if (idxToAvoid.Contains(i))
                        {
                            break;
                        }
                        this.back.Add(neighbourskts[(int)Directions.Back]);
                        break;
                    case Directions.Left:
                        this.left.Clear();
                        if (idxToAvoid.Contains(i))
                        {
                            break;
                        }
                        this.left.Add(neighbourskts[(int)Directions.Left]);
                        break;
                }
            }

        }

    }

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
    private WorldBoundCreator worldBoundCreator;
    private List<GameObject> roadTiles = new List<GameObject>();
    private List<GameObject> grassTiles = new List<GameObject>();
    private List<GameObject> houseTiles = new List<GameObject>();
    private List<GameObject> riverTiles = new List<GameObject>();
    private IDictionary<(int, int), List<socketType>> CSPMap = new Dictionary<(int, int), List<socketType>>();
    private IDictionary<(int, int), socket> GridMap = new Dictionary<(int, int), socket>();
    private IDictionary<(int, int), int> rotIds = new Dictionary<(int, int), int>();
    List<socketType> allStates = Enum.GetValues(typeof(socketType)).Cast<socketType>().ToList();
    System.Random r;
    void Start()
    {
                
        seed = exploreMode? PlayerPrefs.GetInt("Exploration Seed"):seed;
        r = new System.Random(seed);
        if (exploreMode)
        {
            this.length = r.Next(5, 15);
            this.width = r.Next(5, 15);
        }
        worldBoundCreator = new WorldBoundCreator(TILE_SIZE, this.length, this.width, this.Tile_offset);
        worldBoundCreator.create();
        maxSearchDepth = 2 * this.chunkSize;
        topologyPlotter.Start();
        initialiseMap();
        solveCSP();
        makeGrid();
    }

    private void makeGrid()
    {
        int k = 0;

        for (int i = 0; i < this.length; i++)
        {
            for (int j = 0; j < this.width; j++)
            {

                socket tileskt = new socket(getRandomSocket(CSPMap[(i, j)]));
                socket prefapskt = getPrefabSocket(tileskt.type);
                tileskt.tile = Instantiate(prefapskt.tile, transform, true);
                generateBuildings(tileskt);
                tileskt.tile.transform.Rotate(0, (float)(rotIds[(i, j)]) * DEFAULTROT, 0, Space.Self);
                tileskt.tile.transform.position = new Vector3(0, 0, 0);
                tileskt.tile.transform.localPosition = new Vector3(0, 0, 0);

                Vector2 centrePos = new Vector2(i * (TILE_SIZE - Tile_offset), j * (TILE_SIZE - Tile_offset));
                tileskt.tile.transform.localPosition += new Vector3(centrePos.x, 0, centrePos.y);
                plotGrass(tileskt, centrePos, transform);
                nameObject(tileskt, k++, (i, j));

                setGridskt(tileskt, (i, j));
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
        if (skt.type == socketType.house)
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
                tileskt.tile.gameObject.name = "RoadTile_" + rotIds[(i, j)].ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                roadTiles.Add(tileskt.tile);
                break;

            case socketType.grass:
                tileskt.tile.gameObject.name = "GrassTile_" + rotIds[(i, j)].ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                break;

            case socketType.house:
                tileskt.tile.gameObject.name = "HouseTile_" + rotIds[(i, j)].ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                houseTiles.Add(tileskt.tile);
                break;
            case socketType.river0:
                tileskt.tile.gameObject.name = "RiverTile0_" + rotIds[(i, j)].ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                riverTiles.Add(tileskt.tile);
                break;
            case socketType.river1:
                tileskt.tile.gameObject.name = "RiverTile1_" + rotIds[(i, j)].ToString() + "_(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                riverTiles.Add(tileskt.tile);
                break;
            default:
                break;
        }

    }

    private void setGridskt(socket skt, (int, int) Loc)
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

            neighbourskts[k] = types[0];

        }

        skt.setskts(neighbourskts.ToList(), idxToAvoid);

    }

    private void initialiseMap()
    {
        // intialise the radnom number generator
        
        // initialise all points with a list of all possible states
        for (int i = 0; i < this.length; i++)
        {
            for (int j = 0; j < this.width; j++)
            {
                List<socketType> states = new(allStates);

                CSPMap.Add((i, j), states);
                rotIds.Add((i, j), 0);

            }
        }

        // collapse a random point on the map 
        /*var x = r.Next(0, this.length);
        var y = r.Next(0, this.width);*/
        var x = 0;
        var y = 0;
        var skt = exploreMode?getPrefabSocket(socketType.grass):getRandomSocket(allStates);
        CSPMap[(x, y)].Clear();
        
        CSPMap[(x, y)].Add(skt.type);
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

        // printMap();
    }
    private void solveCSPChunk((int, int) xBounds, (int, int) zBounds)
    {
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
                    if (debugMode)
                    {
                        Debug.Log("Solved " + "Location: (" + i + ", " + j + "): " + this.CSPMap[(i, j)][0].ToString() + "rotID: " + rotIds[(i, j)]);
                        //printMap();
                    }

                    continue;
                }
                propogateToNeighbour((i, j), new(this.CSPMap[(i, j)]), xBounds, zBounds, 0);
            }
        }
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
        Debug.Log("[propogate print List] Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + "rotID: " + rotIds[Loc] + "nCSPMAP" + CSPMap[Loc].Count.ToString());

        foreach (var k in CSPMap[Loc])
        {
            Debug.Log(k);
        }
        Debug.Log("\n");

    }
    private bool isInBounds((int, int) Loc, (int, int) xBounds, (int, int) zBounds)
    {
        if (Loc.Item1 < xBounds.Item1 || Loc.Item1 >= xBounds.Item2 || Loc.Item2 < zBounds.Item1 || Loc.Item2 >= zBounds.Item2)
        {
            //out of x bounds or z Bounds, move on
            return false;
        }
        return true;
    }

    private bool propogateToNeighbour((int, int) Loc, List<socketType> types, (int, int) xBounds, (int, int) zBounds, int currentDepth)
    {
        // this tile is of type 'type' we need to check if making this change to current tile will cause issues
        //Debug.Log("inside propogate to neighbour");

        ++currentDepth;
        socket skt = getRandomSocket(types);
        List<socketType> currentStates = new List<socketType>(CSPMap[Loc]);
        
        CSPMap[Loc].Clear();
        CSPMap[Loc].Add(skt.type);

        bool check = propogate(Loc, skt, xBounds, zBounds, currentDepth);
        while (!check)
        {
            CSPMap[Loc].Clear();
            CSPMap[Loc].AddRange(currentStates);
            if (!skt.isSymmetric)
            {
                var rotId = checkAllRot(skt, Loc, xBounds, zBounds, currentDepth);
                if (rotId > -1)
                {
                    rotIds[(Loc)] = rotId;
                    break;
                }
            }

            types.Remove(skt.type);

            if (types.Count == 0)
            {
                return false;
            }
            skt = getRandomSocket(types);
            CSPMap[Loc].Clear();
            CSPMap[Loc].Add(skt.type);
            check = propogate(Loc, skt, xBounds, zBounds, currentDepth);
        }


        return true;
    }

    // return the rot id for which this worked
    private int checkAllRot(socket skt, (int, int) Loc, (int, int) xBounds, (int, int) zBounds, int currentDepth)
    {
        List<socketType> currentStates = new List<socketType>(CSPMap[Loc]);
        CSPMap[Loc].Clear();
        CSPMap[Loc].Add(skt.type);
        var nrot = (skt.type == socketType.river0) ? 2 : NROT;
        for (int i = 1; i < NROT; i++)
        {
            skt.applyRot(i);
            rotIds[Loc] = i;
            if (propogate(Loc, skt, xBounds, zBounds, currentDepth))
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
    }

    // if this is a valid move and it doesnt fail any csp 
    private bool propogate((int, int) Loc, socket skt, (int, int) xBounds, (int, int) zBounds, int currentDepth)
    {
        //Debug.Log("inside propogate");
        bool preCheckOk = preCheck(Loc, skt);
        if (!preCheckOk)
        {
            return false;
        }
        if(currentDepth > maxSearchDepth)
        {
            return preCheckOk;
        }
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
            List<socketType> t = new List<socketType>();
            t.AddRange(getneighbourType(skt, k++));

            if (x >= length || y >= width || x < 0 || y < 0)
            {
                roadCount++;
                // house is pointing out of the map
                if (t.Contains(socketType.road) && t.Count == 1)
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
                Debug.Log("Valid States " + "type: " + k.ToString() + " rotID " + rotIds[Loc]);
                Debug.Log("checkLocation: (" + x.ToString() + ", " + y.ToString() + "): ");
                Debug.Log("k: " + k.ToString());

                foreach (var d in t)
                {
                    Debug.Log(((int)d).ToString());
                }
                Debug.Log("\n");
                printList((x, y));
            }

            List<socketType> validStates = new List<socketType>(findIntersect(t, CSPMap[(x, y)]));
            if (validStates == null) return false;

            if (validStates.Count() == 0)
            {
               // Debug.Log("false");
                return false;
            }

            if (validStates.Contains(socketType.road))
            {
                roadlocs.Add((x, y));
                roadCount++;
            }
            if (debugMode)
            {
                foreach (var d in validStates)
                {
                    Debug.Log(((int)d).ToString());
                }
            }

            if (CSPMap[(x, y)].Count == validStates.Count)
            {
                //no change to this cell so continue;

                continue;
            }



            List<socketType> currentStates = new List<socketType>(CSPMap[(x, y)]);

            //Debug.Log("Before: ");
            //printList((x, y));
            CSPMap[(x, y)].Clear();
           
            CSPMap[(x, y)].AddRange(validStates);
            if (!isInBounds((x, y), xBounds, zBounds))
            {
                // in unexplored area
                continue;
            }
            if (!propogateToNeighbour((x, y), new(CSPMap[(x, y)]), xBounds, zBounds, currentDepth))
            {
                CSPMap[(x, y)].Clear();
               
                CSPMap[(x, y)] = new List<socketType>(currentStates);
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
                List<socketType> validStates = new List<socketType>();
                validStates.Add(socketType.road);
                foreach (var locs in roadlocs)
                {
                    CSPMap[locs].Clear();
                    CSPMap[locs].AddRange(validStates);
                }
            }
        }
        return true;
    }

    private List<socketType> getneighbourType(socket skt, int k)
    {
        switch ((socket.Directions)k)
        {
            case socket.Directions.Right:
                return skt.right;
            case socket.Directions.Forward:
                return skt.forward;
            case socket.Directions.Left:
                return skt.left;
            case socket.Directions.Back:
                return skt.back;
            default:
                return null;
        }
    }



    private bool preCheck((int, int) Loc, socket skt)
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
            List<socketType> t = new List<socketType>();
            t.AddRange(getneighbourType(skt, k++));

            if (x >= length || y >= width || x < 0 || y < 0)
            {
                // house is pointing out of the map
                if (t.Contains(socketType.road) && t.Count == 1)
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
                Debug.Log("[precheck] Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + CSPMap[Loc].Count.ToString() + CSPMap[Loc][0]);
                Debug.Log("Valid States " + "type: " + k.ToString() + " rotID " + rotIds[Loc]);
                Debug.Log("checkLocation: (" + x.ToString() + ", " + y.ToString() + "): ");
                Debug.Log("k: " + k.ToString());

                foreach (var d in t)
                {
                    Debug.Log(((int)d).ToString());
                }
                Debug.Log("\n");
                printList((x, y));
            }


            List<socketType> validStates = new List<socketType>(findIntersect(t, CSPMap[(x, y)]));
            if (validStates == null) return false;

            if (validStates.Count() == 0)
            {
                //Debug.Log("false");
                return false;
            }

            if (validStates.Contains(socketType.road))
            {
                roadlocs.Add((x, y));
                roadCount++;
            }

            if (CSPMap[(x, y)].Count == validStates.Count)
            {
                //no change to this cell so continue;

                continue;
            }

        }

        if (socketType.road == skt.type)
        {
            if (roadCount < 2)
            {
                return false;
            }
        }

        if (debugMode)
        {
            Debug.Log("[precheck] Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + CSPMap[Loc][0] + " rotID " + rotIds[Loc]);

        }

        return true;
    }

    private List<socketType> findIntersect(List<socketType> l1, List<socketType> l2)
    {
        List<socketType> intersect = new List<socketType>();
        foreach (socketType sktType in l1)
        {
            if (l2.Contains(sktType))
            {
                intersect.Add(sktType);
            }
        }
        return intersect;
    }

    private socket getRandomSocket(List<socketType> types)
    {

        if (types.Count == 1)
        {
            return new socket(getPrefabSocket(types[0]));
        }

        float total = 0f;
        List<float> allProb = new List<float>();
        for (int i = 0; i < types.Count; i++)
        {
            var temp_skt = getPrefabSocket(types[i]);
            allProb.Add(temp_skt.probability);
            total += temp_skt.probability;
        }
        float x = (float)r.NextDouble() * (total);
        total = 0f;
        int idx;
        for (int i = 0; i < types.Count; i++)
        {
            total += allProb[i];
            if (x < total)
            {
                return new socket(getPrefabSocket(types[i]));
            }
        }

        idx = allProb.IndexOf(allProb.Max());
        idx = (idx < 0) ? 0 : (idx >= types.Count) ? (types.Count - 1) : idx;
        return new socket(getPrefabSocket(types[idx]));
    }




    private socket getPrefabSocket(socketType type)
    {
        switch (type)
        {
            case socketType.road:
                return this.roadTile;

            case socketType.grass:
                return this.grassTile;

            case socketType.house:
                return this.houseTile;

            case socketType.river0:
                return this.river0;
            case socketType.river1:
                return this.river1;
            default:
                return this.grassTile;
        }

    }


}

