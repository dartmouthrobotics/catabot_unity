using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFromElevationImage : MonoBehaviour {
    public string pathToImageFile = "C:\\Users\\dinit\\Downloads\\New York River Elevation.png";
    public float distanceBetweenPixels = 1f;
    public float minAltitude = -10f;
    public float maxAltitude = 20f;

    private Terrain[] terrains;

    private void Start() {
        CreateTerrainsFromImage(pathToImageFile);
    }

    private void CreateTerrainsFromImage(string imageFilePath) {
        byte[] imageBytes = System.IO.File.ReadAllBytes(pathToImageFile);
        Texture2D elevationTexture = new Texture2D(2, 2);
        elevationTexture.LoadImage(imageBytes);
        print("(" + elevationTexture.width + ", " + elevationTexture.height + ")");

        float[,] heights = new float[elevationTexture.height, elevationTexture.width];
        for(int i = 0; i < elevationTexture.width; i++) {
            for(int j = 0; j < elevationTexture.height; j++) {
                heights[j, i] = elevationTexture.GetPixel(i,j).r;
            }
        }

        print("Heights all set");

        TerrainData terrainData = new TerrainData();
        print("Terrain data made");
        terrainData.heightmapResolution = 4097;
        print("Set the resolution");
        terrainData.SetHeights(0,0,heights);
        print("Set the heights");
        terrainData.size = new Vector3(distanceBetweenPixels * 4096f, maxAltitude - minAltitude, distanceBetweenPixels * 4096f);
        print("Set the size");
        GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
        print("Created the terrain object");
        terrain.transform.position = new Vector3(0, minAltitude, 0);
    }
}
