using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BuildingGenerator
{
    // Start is called before the first frame update
    // External parameters/variables
    private const int defaultHeight = 2;
    
    [SerializeField] private List<GameObject> buildingBases;
    [SerializeField] private List<GameObject> buildingMiddles;
    [SerializeField] private GameObject buildingRoof;    
    [SerializeField] private GameObject buildingLongChimney;
    [SerializeField] private GameObject buildingGround;
    private Vector3 buildingOffset = new Vector3(-0.1f, 3.139151f, 0.1f);
    private Vector3 middleFloorOffset = new Vector3(-0.03259f, -0.01084f, 0.003151f);
    private Vector3 roofOffset = new Vector3(-0.0576f, -1.02915f, -0.06615f);
    private System.Random r;
    
    private struct Building{
        public GameObject BuildingGround;
        public GameObject BuildingBase;
        public List<GameObject> BuildingFloors;
        public GameObject BuildingRoof;    
        
    }
    private Building building;
    

    public void generate(int BuildingHeight, Transform transform, System.Random rand)
    {
        this.r = rand;
        building.BuildingGround = GameObject.Instantiate(this.buildingGround, transform, true);
        building.BuildingGround.transform.localPosition = new Vector3(0, 0, 0);
        building.BuildingGround.transform.localPosition += new Vector3(0, -0.15f, 0);
        building.BuildingBase = GameObject.Instantiate(GetRandomItem<GameObject>(this.buildingBases), transform, true);
        float heightOffset = 1.5f;
        
        Vector3 buildingShift = new Vector3(0.8f,0f, 0.5f);
        building.BuildingBase.transform.localPosition = new Vector3(0, 0, 0);
        building.BuildingBase.transform.localPosition += buildingShift + new Vector3(0,heightOffset,0);        
        building.BuildingFloors = new List<GameObject>();
        for (int i = 0; i < BuildingHeight - defaultHeight; i++)
        {
            var floor = GameObject.Instantiate(GetRandomItem<GameObject>(this.buildingMiddles), transform, true);
            heightOffset +=  middleFloorOffset.y+ buildingOffset.y;
            floor.transform.localPosition = new Vector3(0, 0, 0);
            floor.transform.localPosition += new Vector3(middleFloorOffset.x, heightOffset, middleFloorOffset.z) + buildingShift;           
            building.BuildingFloors.Add(floor);
        }
        building.BuildingRoof = GameObject.Instantiate(this.buildingRoof, transform, true);
        building.BuildingRoof.transform.localPosition = new Vector3(0, 0, 0);
        building.BuildingRoof.transform.localPosition += new Vector3(0f, heightOffset, 0) + buildingOffset + roofOffset + buildingShift;        
    }
    private T GetRandomItem<T>(List<T> listToRandomize)
    {
        int randomNum = r.Next(0, listToRandomize.Count);        
        return listToRandomize[randomNum];
    }

   

}
