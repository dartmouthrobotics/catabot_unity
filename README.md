# ASV Unity Simulator

![Statue of Liberty](docs/Statue%20of%20Liberty%20High%20Resolution.png)

# Setup Instructions

1. Download the project and extract if needed
2. Download the [ArcGIS Maps SDK for Unity package](https://developers.arcgis.com/unity/). We used version 1.3.0 of the package, but other versions may work as well.
3. Put the file `arcgis-maps-sdk-unity-1.3.0.tgz` in the base project folder (the same folder that contains the folders `Assets` and `Packages`)
4. Open the project in Unity. We used editor version `2022.3.12f1`, but later versions of Unity may work as well.

# Controls are:
* R and F - Forward and Backward left propeller
* U and J - Forward and Backward right propeller
* Hit R and U simultaneously to move forward
* Hit F and J simultaneously to move back
* Hit R and J together, or F and Y together, or any key by itself to rotate
**Do NOT hit WASD, Space, or Enter at any time**. These cause the ArcGIS world to move around. I need to find the relevant part of the code in the ArcGIS package and disable it.

# Limitations
* ArcGIS behaves differently depending on the resolution of the screen being rendered. A low resolution window produces low resolution terrain and buildings (e.g. a rough block where the Statue of Liberty should be or the depth of a river being too shallow). A high resolution window results in high resolution terrain and buildings (e.g. you can see the entire Statue of Liberty and rivers are the correct depth).
![Statue of Liberty Low Resolution](docs/Statue%20of%20Liberty%20Low%20Resolution.png) ![Statue of Liberty High Resolution](docs/Statue%20of%20Liberty%20High%20Resolution.png)
![Skyscrapers Low Resolution](docs/Skyscrapers%20Low%20Resolution.png) ![Skyscrapers High Resolution](docs/Skyscrapers%20High%20Resolution.png)
![River Level Low Resolution](docs/River%20water%20level%20Low%20Resolution.png) ![River Level High Resolution](docs/River%20water%20level%20High%20Resolution.png)
* At this time, the "ocean" will always be at a height of 0 meters. This can result in pockets of water in land if the land is below sea level.
![Water when land is below sea level](docs/Water%20where%20there%20should%20be%20none%20example.png)
* If the water that is supposed to be simulated is not at sea level (e.g. a lake in a mountain range like we have here in New Hampshire), there will be no water. This one can be fixed in the future by adding some way to manually set the water height.
* Alternatively, it's possible to [manually create a custom river/lake with unique heights and shapes within Unity](https://blog.unity.com/engine-platform/new-hdrp-water-system-in-2022-lts-and-2023-1) if the user wants to take the time to fully customize everything.
