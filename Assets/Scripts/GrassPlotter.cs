using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GrassPlotter
{
    [SerializeField] private GameObject grass;
    // Start is called before the first frame update
    [SerializeField] private float threshold = 0.7f;

    public void plot(Vector2 leftBottom, Vector2 rightTop, System.Random r, Transform transform)
    {
        int width = (int)Mathf.Ceil(rightTop.x - leftBottom.x);
        int height = (int)Mathf.Ceil(rightTop.y - leftBottom.y);
        for( int i =0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                float x = (i+getRandomFloat(r, 0, 0.3f)) + leftBottom.x;
                float y = (j + getRandomFloat(r, 0, 0.3f)) + leftBottom.y;
                float sample = Mathf.PerlinNoise(x, y);
                if ( sample <= threshold)
                {
                    continue;
                }
                growGrass(x, y, (sample) * 360, transform);
            }
        }
    }

    private void growGrass(float x, float z, float angle, Transform transform)
    {
        GameObject grass = GameObject.Instantiate(this.grass, transform, true);
        grass.transform.Rotate(new Vector3(0, 1, 0), angle);
        grass.transform.localPosition = new Vector3(0, 0, 0);
        grass.transform.localPosition += new Vector3(x, 0.28f, z);
    }

    public float getRandomFloat(System.Random r, float min, float max)
    {
        return (float)((max - min) * r.NextDouble() + min);
    }

}
