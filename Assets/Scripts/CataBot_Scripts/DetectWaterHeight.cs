using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DetectWaterHeight : MonoBehaviour
{
    public Vector3 waterPos;
    public WaterSurface water;
    WaterSearchParameters Search;
    WaterSearchResult SearchResult;

    void Update() {
        Search.startPosition = transform.position;
        if(water.FindWaterSurfaceHeight(Search, out SearchResult)) {
            waterPos = new Vector3(transform.position.x, SearchResult.height, transform.position.z);
        }
    }
}
