using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TopologyPlotter
{
    private Object[] prefabs;
    private System.Random r;
    [SerializeField] private float threshold = 0.7f;

    public void Start()
    {
        prefabs = Resources.LoadAll<GameObject>("Topology");
    }

    public void plot(Vector2 leftBottom, Vector2 rightTop, System.Random r, Transform transform)
    {
        this.r = r;
        int width = (int)Mathf.Ceil(rightTop.x - leftBottom.x);
        int height = (int)Mathf.Ceil(rightTop.y - leftBottom.y);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = (i + getRandomFloat(r, 0, 0.3f)) + leftBottom.x;
                float y = (j + getRandomFloat(r, 0, 0.3f)) + leftBottom.y;
                float sample = Mathf.PerlinNoise(x, y);
                if (sample <= threshold)
                {
                    continue;
                }
                growTopology(x, y, (sample) * 360, transform);
            }
        }
    }

    private void growTopology(float x, float z, float angle, Transform transform)
    {
        GameObject prefab = (GameObject) (GetRandomItem<Object>(this.prefabs));
        GameObject obj = (GameObject.Instantiate(prefab, transform, true));
        obj.transform.Rotate(new Vector3(0, 1, 0), angle);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.transform.localPosition += new Vector3(x, 0f, z);
    }
    private T GetRandomItem<T>(T[] listToRandomize)
    {
        int randomNum = r.Next(0, listToRandomize.Length);
        return listToRandomize[randomNum];
    }
    public float getRandomFloat(System.Random r, float min, float max)
    {
        return (float)((max - min) * r.NextDouble() + min);
    }
}
