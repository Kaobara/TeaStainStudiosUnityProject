using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private const int maxSide = 4;
    private const int NROT = 4;
    public socketType type;
    public int rotId = 0;
    public bool isSymmetric;
    public float probability = 0.5f;
    public GameObject tile;
    public List<socketType> left = new List<socketType>();
    public List<socketType> right = new List<socketType>();
    public List<socketType> forward = new List<socketType>();
    public List<socketType> back = new List<socketType>();
    public List<List<socket>> skts = new List<List<socket>>(4);
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

        for (int i = 0; i < maxSide; i++)
        {
            this.skts.Add(new List<socket>());
        }
    }
    public socket(socket obj)
    {
        this.rotId = obj.rotId;
        this.type = obj.type;
        this.probability = obj.probability;
        this.isSymmetric = obj.isSymmetric;
        this.left = new List<socketType>(obj.left);
        this.right = new List<socketType>(obj.right);
        this.forward = new List<socketType>(obj.forward);
        this.back = new List<socketType>(obj.back);
        this.initial_sockets = obj.initial_sockets;
        this.initial_cordRot = obj.initial_cordRot;
        this.tile = obj.tile;
        for (int i = 0; i < maxSide; i++)
        {
            this.skts.Add(new List<socket>());
        }
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
        this.rotId = rotId;
        return (rot, cordrot);
    }

    public socket getRotatedSkt(int rotId)
    {
        socket skt = new socket(this);
        skt.applyRot(rotId);
        return skt;
    }
    public void applyRot(int rotId)
    {
        (var sockets, var socketcord) = this.rotate(rotId);
        this.right = sockets[(int)Directions.Right];
        this.forward = sockets[(int)Directions.Forward];
        this.left = sockets[(int)Directions.Left];
        this.back = sockets[(int)Directions.Back];
        this.rotId = rotId;
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