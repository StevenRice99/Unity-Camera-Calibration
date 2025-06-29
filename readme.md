﻿# Camera Calibration Generator

Allows for configuring simulated physical cameras in [Unity](https://unity.com "Unity") and extracting screenshots along with calibrated data for external use in pixel matching.

- [Setup](#setup "Setup")
- [Usage](#usage "Usage")
- [Data](#data "Data")
- [Textures](#textures "Textures")

# Setup

First, install [Unity](https://unity.com "Unity"). Then, either clone this project and open it in [Unity](https://unity.com "Unity") and open the [Main](Assets/Main.unity "Main") scene to see an example, or follow the below steps to add this to a new [Unity](https://unity.com "Unity") project:

1. Install [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes "NaughtyAttributes") to your [Unity](https://unity.com "Unity") project.
2. Add the [CameraManager.cs](Assets/CameraManager.cs "CameraManager.cs") script from this repository to your [Unity](https://unity.com "Unity") project.
3. If you are planning to build for WebGL, in your project assets create the folder `Plugins/WebGL` and copy the [FileDownloader.jslib](Assets/Plugins/WebGL/FileDownloader.jslib "FileDownloader.jslib") file to it.

# Usage

1. Add a [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") component to a `GameObject`.
2. Configure the properties on the [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") as well as the `Camera` component itself within your scene.
    - If in the editor, modify the properties on the [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") and Camera component inspectors.
    - If in a build, use the GUI to modify properties.
    - The camera view you see is "between" where the left and right cameras will be, with the "Offset" field representing how far apart in meters the two screenshots will be.
        - For example, an offset of 0.25 will mean the left screenshot will shift 0.25 meters to the left to take it, and the right screenshot will shift 0.25 meters to the right to take it.
3. Click the `Generate Data` button.
    - If in the editor, this is on the [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") component's inspector.
    - If in a build, this is located at the bottom of the GUI.
    - **This will cause the application to hang for a few seconds, with the higher your resolution the longer the hang.**
4. After a few seconds for the previous step to finish, you can find the [data](#data "Data") that was generated.
    - In the editor or a desktop build, you can find the data in the root of your project under `Camera-Data/{scene}/{camera}` where `{scene}` is the name of the currently loaded scene and `{camera}` is the name of the [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") component's GameObject.
      - This will open automatically.
    - If in a web build, this will download all the generated [data](#data "Data") as a ZIP file.

# Data

The generated data produces the following files:

- `Left.png` - The screenshot from the left camera.
- `Right.png` - The screenshot from the right camera.
- `Calibration-2D.txt` - 2D pixel coordinates that correspond to matched world points from `Calibration-3D.txt`.
    - These are from the left camera.
- `Calibration-2D.txt` - 3D world coordinates that correspond to matched pixel points from `Calibration-2D.txt`.
    - These are from the left camera.
    - These are calculated with the left camera as the origin of the world.
- `Focal-Length-X.txt` - The X focal length in pixels for matrix calculations.
- `Focal-Length-Y.txt` - The Y focal length in pixels for matrix calculations.
- `Principal-Point-X.txt` - The X principal point in pixels for matrix calculations.
- `Principal-Point-Y.txt` - The Y principal point in pixels for matrix calculations.
- `Intrinsic-Matrix.txt` - The intrinsic matrix of the cameras.

# Textures

All sample textures are from [PolyHaven](https://polyhaven.com "PolyHaven") licenced under [CC0](https://polyhaven.com/license "CC0").