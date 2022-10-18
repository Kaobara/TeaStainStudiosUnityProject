using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
[System.Serializable]
public class neighbourMap
{
    // Start is called before the first frame update
    public IDictionary<socketType, IDictionary<int, socket>> socketNeighbours = new Dictionary<socketType, IDictionary<int, socket>>();

    public neighbourMap()
    {

    }
    public neighbourMap(neighbourMap obj)
    {
        this.socketNeighbours = new Dictionary<socketType, IDictionary<int, socket>>(obj.socketNeighbours);
    }

    public IDictionary<socketType, IDictionary<int, socket>> getMap()
    {
        return socketNeighbours;
    }

    public string saveToJson()
    {
        return JsonUtility.ToJson(this);

    }
    // saveFile and LoadFile codes were adapted from https://answers.unity.com/questions/1300019/how-do-you-save-write-and-load-from-a-file.html
    public void SaveFile()
    {
        string destination = "C:/Users/mukul/Desktop/Semester_2/Graphics_and_Interaction/project-2-tea-stain-studios/Assets/Prefabs/Map/neighbour.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);
        neighbourMap data = new neighbourMap(this);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
        /*file.Close();
        string data = socketNeighbours.saveToJson();
        
        Debug.Log(data);
        File.WriteAllText(destination, data);*/

    }

    public neighbourMap LoadFile()
    {
        string destination = "C:/Users/mukul/Desktop/Semester_2/Graphics_and_Interaction/project-2-tea-stain-studios/Assets/Prefabs/Map/neighbour.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            Debug.LogError("File not found");
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        neighbourMap data = (neighbourMap)bf.Deserialize(file);
        file.Close();
        return data;
    }
}

