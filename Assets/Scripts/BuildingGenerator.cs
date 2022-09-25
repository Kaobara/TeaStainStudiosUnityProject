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
        building.BuildingBase = GetRandomItem<GameObject>(this.buildingBases);
        var baseTransform = Instantiate(building.BuildingBase).transform;
        baseTransform.parent = transform;
        for (int i=0; i<Random.Range(0, this.maxBuildingHeight-defaultHeight);i++)
        {
            var floor = GetRandomItem<GameObject>(this.buildingMiddles);            
            var middleTransform = Instantiate(floor).transform;
            middleTransform.parent = transform;
            building.BuildingFloors.Add(floor);            
        }
        building.BuildingRoof = this.buildingRoof;
        var roofTransform = Instantiate(building.BuildingRoof).transform;
        roofTransform.parent = transform;
    }
    public T GetRandomItem<T>(List<T> listToRandomize)
    {
        int randomNum = Random.Range(0, listToRandomize.Count);        
        return listToRandomize[randomNum];
    }

}
