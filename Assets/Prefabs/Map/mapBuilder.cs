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
    [SerializeField] private socket roadTile;
    [SerializeField] private socket grassTile;
    [SerializeField] private socket houseTile;
    [SerializeField] private int length;
    [SerializeField] private int width;
    [SerializeField] private int seed = 92879187;
    [SerializeField] private float Tile_offset = 0.5f;
   
    [System.Serializable]
    public enum socketType
    {
        road, 
        grass, 
        house
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
                rot.Add( this.initial_sockets[(i + rotId) % NROT]);
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

        public void setskts(List<socketType> neighbourskts,   List<int> idxToAvoid)
        {
            for(int i =0; i< 4; i++)
            {                
                switch((Directions)i){
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

    private List<GameObject> roadTiles = new List<GameObject>();
    private List<GameObject> grassTiles = new List<GameObject>();
    private List<GameObject> houseTiles = new List<GameObject>();

    private IDictionary<(int, int), List<socketType>> CSPMap = new Dictionary<(int, int), List<socketType>>();
    private IDictionary<(int, int), socket> GridMap = new Dictionary<(int, int), socket>();
    private IDictionary<(int, int), int> rotIds = new Dictionary<(int, int), int>();
    List<socketType> allStates = Enum.GetValues(typeof(socketType)).Cast<socketType>().ToList();
    System.Random r;
    void Start()
    {       
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
                socket prefapskt = getPrefabSocket(tileskt.type);
                tileskt.tile = Instantiate(prefapskt.tile, transform, true);
                tileskt.tile.transform.Rotate(new Vector3(0, 1, 0), (float)(rotIds[(i, j)]) * DEFAULTROT);
                tileskt.tile.transform.position = new Vector3(0, 0, 0);
                tileskt.tile.transform.localPosition = new Vector3(0, 0, 0);
                tileskt.tile.transform.localPosition += new Vector3(i * (TILE_SIZE-Tile_offset), 0, j * (TILE_SIZE-Tile_offset));
               
                nameObject(tileskt, k++, (i, j));
                
                setGridskt(tileskt, (i,j));
                GridMap.Add((i, j), tileskt);
                
            }
        }
    }
    private void nameObject(socket tileskt, int k, (int, int) Loc)
    {
        int i = Loc.Item1;
        int j = Loc.Item2;
        switch (tileskt.type)
        {
            case socketType.road:
                tileskt.tile.gameObject.name = "RoadTile_" + "(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                roadTiles.Add(tileskt.tile);
                break;

            case socketType.grass:
                tileskt.tile.gameObject.name = "GrassTile_" + "(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                grassTiles.Add(tileskt.tile);
                break;

            case socketType.house:
                tileskt.tile.gameObject.name = "HouseTile_" + "(" + i.ToString() + "," + j.ToString() + ")_" + (k).ToString();
                houseTiles.Add(tileskt.tile);
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
            
            neighbourskts[k]=types[0];
           
        }
        
        skt.setskts(neighbourskts.ToList(), idxToAvoid);
        
    }

    private void initialiseMap()
    {
        // intialise the radnom number generator
        r = new System.Random(seed);
        // initialise all points with a list of all possible states
        for (int i = 0; i < this.length; i++)
        {
            for (int j = 0; j < this.width; j++)
            {
                List<socketType> states = new(allStates);

                CSPMap.Add((i,j),states);
                rotIds.Add((i, j), 0);
                
            }
        }
        
        // collapse a random point on the map 
        var x = r.Next(0, this.length);
        var y = r.Next(0, this.width);
        CSPMap[(x, y)].Clear();
        CSPMap[(x, y)].Add(socketType.grass);
        propogateToNeighbour((x, y), CSPMap[(x, y)]);
    }

    private void solveCSP()
    {
        for (int i = 0; i < this.length; i++)
        {
            for (int j = 0; j < this.width; j++)
            {
               if(this.CSPMap[(i,j)].Count == 1)
                {
                    //already been solved
                    //Debug.Log("Solved");
                    //printMap();
                    continue;
                }
                propogateToNeighbour((i, j), this.CSPMap[(i, j)]);
            }
        }
        printMap();
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
        Debug.Log("Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + "rotID: " + rotIds[Loc] + "nCSPMAP"+ CSPMap[Loc].Count.ToString());
        
        foreach (var k in CSPMap[Loc])
        {
            Debug.Log(k);
        }
        Debug.Log("\n");

    }

    private bool propogateToNeighbour((int, int) Loc, List<socketType> types)
    {
        // this tile is of type 'type' we need to check if making this change to current tile will cause issues
        //Debug.Log("inside propogate to neighbour");
        socket skt = getRandomSocket(types);
        List<socketType> currentStates = new List<socketType>(CSPMap[Loc]);
        CSPMap[Loc].Clear();
        CSPMap[Loc].Add(skt.type);

        bool check = propogate(Loc, skt);
        while (!check)
        {
            CSPMap[Loc].Clear();
            CSPMap[Loc].AddRange(currentStates);
            if (!skt.isSymmetric)
            {
                var rotId = checkAllRot(skt, Loc);
                if (rotId> -1)
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
            check = propogate(Loc, skt);
        }
       

        return true;
    }

    // return the rot id for which this worked
    private int checkAllRot(socket skt, (int, int) Loc)
    {
        List<socketType> currentStates = new List<socketType>(CSPMap[Loc]);
        CSPMap[Loc].Clear();
        CSPMap[Loc].Add(skt.type);

        for (int i =1; i<4; i++)
        {
            skt.applyRot(i);
            if(propogate(Loc, skt))
            {
                skt.reset();
                return i;
            }
        }
        CSPMap[Loc].Clear();
        CSPMap[Loc].AddRange(currentStates);
        skt.reset();
        return -1;
    }
    
    // if this is a valid move and it doesnt fail any csp 
    private bool propogate((int, int) Loc, socket skt)
    {
        //Debug.Log("inside propogate");
        
        int k = 0;
        List<(int, int)> roadlocs = new List<(int, int)>();
        // to ensure there is always two roads connected to this road
        int roadCount = 0;
        foreach((int, int) l in skt.initial_cordRot)
        {
            int i = l.Item1;
            int j = l.Item2;
            
            var x = i + Loc.Item1;
            var y = j + Loc.Item2;
            List<socketType> t = new List<socketType>();
            t.AddRange(getneighbourType(skt, k++));
            
            if (x >= length || y>=width || x<0 ||y<0)
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
            
                     
            Debug.Log("Location: (" + Loc.Item1.ToString() + ", " + Loc.Item2.ToString() + "): " + CSPMap[Loc].Count.ToString());
            Debug.Log("Valid States "+ "type: "+ k.ToString() + " rotID " + rotIds[Loc]);
            Debug.Log("checkLocation: (" + x.ToString() + ", " + y.ToString() + "): " );
            Debug.Log("k: " + k.ToString());
            if (t.Count == 0)
            {
                return false;
            }
            foreach(var d in t)
            {
                Debug.Log(((int)d).ToString());
            }
            printList((x, y));

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
            foreach (var d in validStates)
            {
                Debug.Log(((int)d).ToString());
            }
            if (CSPMap[(x,y)].Count == validStates.Count)
            {
                //no change to this cell so continue;
                continue;
            }

            List<socketType> currentStates = new List<socketType>(CSPMap[(x, y)]);
            
            //Debug.Log("Before: ");
            //printList((x, y));
            CSPMap[(x,y)].Clear();
            CSPMap[(x, y)].TrimExcess();
            CSPMap[(x, y)].AddRange(validStates);
            if(!propogateToNeighbour((x, y), CSPMap[(x, y)]))
            {
                CSPMap[(x, y)].Clear();
                CSPMap[(x, y)].TrimExcess();
                CSPMap[(x, y)] = new List<socketType>(currentStates);
                return false;
            }

            //printList((x,y));
        }
        /* Debug.Log("propogated");
         printMap();*/
        Debug.Log("road count: "+ roadCount);
        if( socketType.road == skt.type)
        {
            if (roadCount < 2)
            {
                return false;
            }else if(roadCount == 2)
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

    

    private bool roadCheck()
    {

        return false;
    }

    private List<socketType> findIntersect(List<socketType> l1, List<socketType> l2)
    {
        List<socketType> intersect = new List<socketType>();
        foreach(socketType sktType in l1)
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
        
        if(types.Count == 1)
        {
            return getPrefabSocket(types[0]);
        }
        
        float total = 0f;
        List<float> allProb = new List<float>();
        for (int i = 0; i < types.Count; i++)
        {
            var temp_skt = getPrefabSocket(types[i]);
            allProb.Add(temp_skt.probability);
            total += temp_skt.probability;
        }
        float x = (float)r.NextDouble() *(total);
        total = 0f;
        int idx;
        for(int i =0; i<types.Count; i++)
        {
            total += allProb[i];
            if (x < total)
            {
                return getPrefabSocket(types[i]);
            }
        }

        idx =allProb.IndexOf(allProb.Max());
        idx = (idx < 0) ? 0 : (idx>=types.Count)?(types.Count-1): idx;
        return getPrefabSocket(types[idx]);
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
                
            default:
                return this.grassTile;
        }
            
    }


}
    
