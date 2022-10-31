using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rt : MonoBehaviour
{
    public RenderTexture rt1;
    // private bool first = true;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
       
        SaveTexture();
       
    }
    
    // Use this for initialization
    public void SaveTexture()
    {
        byte[] bytes = toTexture2D(rt1).EncodeToPNG();
        System.IO.File.WriteAllBytes("C:/Users/mukul/Desktop/Semester_2/Graphics_and_Interaction/SavedScreen.png", bytes);
    }
    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        Destroy(tex);//prevents memory leak
        return tex;
    }

}
