using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ComputeNeighbours:MonoBehaviour
{
    private const int NROT = 4;
    private const int maxSide = 4;
    [SerializeField] private List<socket> skts;
    public List<(socketType, int)> allStates = new List<(socketType, int)>();


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
    public neighbourMap socketNeighbours;
    public IDictionary<socketType, IDictionary<int, socket>> _socketNeighbours;
    private IDictionary<((socketType, int, int), (socketType, int, int)), int> visited = new Dictionary<((socketType, int, int), (socketType, int, int)),int>();
    // Start is called before the first frame update
    void Start()
    {
        socketNeighbours = new neighbourMap();
        this._socketNeighbours = socketNeighbours.getMap();
        getAllCombinations();
        getAllStates();
       // printState();
        printMap();
       

    }
    private void printState()
    {
        foreach(var i in allStates)
        {
            Debug.Log(i.Item1.ToString() +" " +i.Item2.ToString());
        }
    }
    private void getAllStates()
    {
        foreach (var item in _socketNeighbours)
        {
            var sktType = item.Key;
            foreach (var item2 in item.Value)
            {
                
               // Debug.Log("rot: " + item2.Key);
                var rotId = item2.Key; 
                socket skt = item2.Value;
                allStates.Add((skt.type, rotId));
                if (skt.isSymmetric)
                {
                    break;
                }
            }
           // Debug.Log("\n");
        }
    }
        
    private void printMap()
    {
        foreach (var item in _socketNeighbours)
        {
            Debug.Log("socket type: " + item.Key + "\n");
            foreach (var item2 in item.Value)
            {
                Debug.Log("rot: " + item2.Key);
                socket skt = item2.Value;
                
                for(int side = 0; side<maxSide; side++)
                {
                    Debug.Log("side: " + ((socket.Directions)side).ToString());
                    foreach(var s in skt.skts[side])
                    {
                        Debug.Log(s.type.ToString() + " "+s.rotId);
                    }
                }
            }
            Debug.Log("\n");
        }
    }

    public void SaveFile()
    {
        string destination = "C:/Users/mukul/Desktop/Semester_2/Graphics_and_Interaction/project-2-tea-stain-studios/Assets/Prefabs/Map/neighbour.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);
        
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, this._socketNeighbours);
        file.Close();
        /*file.Close();
        string data = socketNeighbours.saveToJson();
        
        Debug.Log(data);
        File.WriteAllText(destination, data);*/

    }
    private void getAllCombinations()
    {
        foreach (socket skt in skts)
        {
            
            //Debug.Log(socketNeighbours.getMap().Count);
            CheckInDict(_socketNeighbours, skt.type, new Dictionary<int, socket>());
            CheckInDict(_socketNeighbours[skt.type], 0, new socket(skt));
            socket s = _socketNeighbours[skt.type][0];
           // Debug.Log("socket 1: "+ s.type);
            for (int side = 0; side < maxSide; side++)
            {
                foreach (socket compareSkt in skts)
                {
                  // Debug.Log("socket 2: " + compareSkt.type);
                   
                    CheckInDict(_socketNeighbours, compareSkt.type, new Dictionary<int, socket>());
                    
                    for (int rot = 0; rot < NROT; rot++)
                    {
                        CheckInDict(_socketNeighbours[compareSkt.type], rot, compareSkt.getRotatedSkt(rot));
                        var s2 = _socketNeighbours[compareSkt.type][rot];
                        int s2Side = (side + 2) % maxSide;
                        if (findIntersect(s, s2, side, s2Side))
                        {
                            //Debug.Log("s2: "+s2.type.ToString());
                            var s1VistedKey = (s.type, s.rotId, side);
                            var s2VisitedKey = (s2.type, s2.rotId, s2Side);
                            if (hasVisited(visited, (s1VistedKey, s2VisitedKey)) || hasVisited(visited, (s2VisitedKey, s1VistedKey)))
                            {
                                if (rot == 0 && s2.isSymmetric)
                                {
                                    break;
                                }
                                continue;
                            }

                            visited.Add((s1VistedKey, s2VisitedKey),0);
                            s.skts[side].Add(s2);
                            s2.skts[s2Side].Add(s);

                        }
                        if (rot == 0 && s2.isSymmetric)
                        {
                            break;
                        }
                    }

                }
            }
            
        }
    }

    private bool hasVisited<T, K>(IDictionary<T, K> dict, T key)
    {
        if (!dict.ContainsKey(key))
        {
           return false;

        }
        return true;
    }

    private void CheckInDict<T, K>(IDictionary<T, K> dict, T key, K item)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key,item);
        }
       
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

    private bool findIntersect(socket s0, socket s1, int side0, int side1)
    {
        if(getneighbourType(s0, side0).Contains(s1.type) && getneighbourType(s1, side1).Contains(s0.type))
        {
            return true;
        }
        return false;
    }

    
}
