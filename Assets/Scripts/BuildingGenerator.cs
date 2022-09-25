using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    // External parameters/variables
    private const int defaultHeight = 2;
    [SerializeField] private List<GameObject> buildingBases;
    [SerializeField] private List<GameObject> buildingMiddles;
    [SerializeField] private GameObject buildingRoof;
    [SerializeField] private GameObject grass;
    [SerializeField] private GameObject buildingLongChimney;
    [SerializeField] private GameObject basePlane;    
    [SerializeField] [Range(min: defaultHeight,10)] private int maxBuildingHeight;
    [SerializeField] private Vector3 buildingOffset = new Vector3(-0.1f, 3.139151f, 0.1f);
    [SerializeField] private Vector3 middleFloorOffset = new Vector3(-0.03259f, -0.01084f, 0.003151f);
    [SerializeField] private Vector3 roofOffset = new Vector3(-0.0576f, -1.02915f, -0.06615f);
    private struct Building{
        public GameObject BuildingBase;
        public List<GameObject> BuildingFloors;
        public GameObject BuildingRoof;        
        
    }
    private Building building;
    
    void Start()
    {
        generate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void generate()
    {
        building.BuildingBase = Instantiate(GetRandomItem<GameObject>(this.buildingBases), transform, true);

        float heightOffset=0f;
        building.BuildingFloors = new List<GameObject>();
        for (int i = 0; i < this.maxBuildingHeight - defaultHeight; i++)
        {
            var floor = Instantiate(GetRandomItem<GameObject>(this.buildingMiddles), transform, true);
            heightOffset +=  middleFloorOffset.y+ buildingOffset.y;
            floor.transform.localPosition += new Vector3(middleFloorOffset.x, heightOffset, middleFloorOffset.z);           
            building.BuildingFloors.Add(floor);
        }
        building.BuildingRoof = Instantiate(this.buildingRoof, transform, true);
        building.BuildingRoof.transform.localPosition += new Vector3(0f, heightOffset, 0f) + buildingOffset + roofOffset;
    }
    public T GetRandomItem<T>(List<T> listToRandomize)
    {
        int randomNum = Random.Range(0, listToRandomize.Count);        
        return listToRandomize[randomNum];
    }

}
