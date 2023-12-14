# ASV Unity Simulator

![Statue of Liberty](docs/Statue%20of%20Liberty%20High%20Resolution.png)

# Basic Setup Instructions

1. Download the project and extract if needed
2. Open the project in Unity. We used editor version `2022.3.12f1`, but later versions of Unity may work as well.
3. Current demo scenes are located at `Assets/Scenes/3DAttributes.unity` and `Assets/Scenes/TerrainTest.unity`

# ROS Setup Instructions

1. On a Linux operating system (e.g. Ubuntu) that already has ROS installed, you will also need to install the ros_tcp_endpoint. While we've already set up the Unity half of the connection in the project, [this tutorial covers the entire setup process](https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/setup.md). Complete the ROS Environemnt section, and then make sure that your local project matches the Unity Setup section. There is an additional demo project section that you do not need to complete.
2. There are three scripts that have extra ROS functionality available. They are disabled by default in the scene TerrainTest and can be activated as follows:
    - TF Broadcaster - RosBroadcastAllFrames - Check the Broadcast ROS Messages box to start sending the position of the robot boat and any added traffic boats.
    - catabot - Radar - Check the Broadcast ROS Messages box to start sending the lidar distances as a laserscan message on the scan topic
    - catabot - RosMove - Check the Connect On Awake button to be able to control the robot from ROS with twist messages on the cmd_vel topic

# Controls are:
* R and F - Forward and Backward left propeller
* U and J - Forward and Backward right propeller
* Hit R and U simultaneously to move forward
* Hit F and J simultaneously to move back
* Hit R and J together, or F and Y together, or any key by itself to rotate
* Hit spacebar to save a screenshot (.png format) in the project directory

# Limitations
* Terrain and buildings will only be as high resolution as what has been imported into the project. As you can see in the following pictures, even the same location can look completely different depending on the data you use to create your terrain models.
![Statue of Liberty Low Resolution](docs/Statue%20of%20Liberty%20Low%20Resolution.png) ![Statue of Liberty High Resolution](docs/Statue%20of%20Liberty%20High%20Resolution.png)
![Skyscrapers Low Resolution](docs/Skyscrapers%20Low%20Resolution.png) ![Skyscrapers High Resolution](docs/Skyscrapers%20High%20Resolution.png)
![River Level Low Resolution](docs/River%20water%20level%20Low%20Resolution.png) ![River Level High Resolution](docs/River%20water%20level%20High%20Resolution.png)
* At this time, the "ocean" will always be at a height of 0 meters. This can result in pockets of water in land if the land is below sea level.
![Water when land is below sea level](docs/Water%20where%20there%20should%20be%20none%20example.png)
* If the water that is supposed to be simulated is not at sea level (e.g. a lake in a mountain range like we have here in New Hampshire), there will be no water. This will be fixed in the future by adding some way to manually set the water height.
* Alternatively, it's possible to [manually create a custom river/lake with unique heights and shapes within Unity](https://blog.unity.com/engine-platform/new-hdrp-water-system-in-2022-lts-and-2023-1) if the user wants to take the time to fully customize everything.
