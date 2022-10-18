using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBoundCreator
{
    private float tileSize;
    private int length;
    private int width;
    private float tile_offset = 0.5f;
    private float offset = 0.8f;
    private int height = 40;

    // Start is called before the first frame update
   public WorldBoundCreator(float tileSize, int length, int width, float tile_offset)
    {
        this.tileSize = tileSize;
        this.length = length;
        this.width = width;
        this.tile_offset = tile_offset;
    }
    public void create()
    {
        float _length = length *(tileSize- tile_offset);
        float _width = width * (tileSize - tile_offset);
        GameObject cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        /*if you need add position, scale and color to the cube*/
        cubeObject.transform.localPosition = new Vector3(-tileSize / 2 - offset, 0, (_width) / 2 - 2*tileSize/3);
        cubeObject.transform.localScale = new Vector3(2, height, _width + tileSize/2 + offset);
        cubeObject.GetComponent<MeshRenderer>().enabled = false;

        GameObject cubeObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        /*if you need add position, scale and color to the cube*/
        cubeObject2.transform.localPosition = new Vector3(_length / 2 - 2*tileSize / 3, 0, - tileSize / 2 - offset);
        cubeObject2.transform.localScale = new Vector3(_length+tileSize/2 +offset, height, 2);
        cubeObject2.GetComponent<MeshRenderer>().enabled = false;

        GameObject cubeObject3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        /*if you need add position, scale and color to the cube*/
        cubeObject3.transform.localPosition = new Vector3((_length) / 2 - 2 * tileSize / 3, 0, _width - tileSize/2 + offset);
        cubeObject3.transform.localScale = new Vector3(_length+tileSize/2 +offset, height, 2);
        cubeObject3.GetComponent<MeshRenderer>().enabled = false;


        GameObject cubeObject4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        /*if you need add position, scale and color to the cube*/
        cubeObject4.transform.localPosition = new Vector3((_length) - tileSize/2 + offset, 0, (_width) / 2 - 2 * tileSize / 3);
        cubeObject4.transform.localScale = new Vector3(2, height, _width + tileSize/2 + offset);
        cubeObject4.GetComponent<MeshRenderer>().enabled = false;


    }



    
    
}
