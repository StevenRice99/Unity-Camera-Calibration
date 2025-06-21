# Camera Calibration Generator

Allows for configuring simulated physical cameras in [Unity](https://unity.com "Unity") and extracting screenshots along with calibrated data for external use in pixel matching.

- [Setup](#setup "Setup")
- [Usage](#usage "Usage")
- [Data](#data "Data")
- [Textures](#textures "Textures")

# Setup

First, install [Unity](https://unity.com "Unity"). Then, either clone this project and open it in [Unity](https://unity.com "Unity") and open the [Main](Assets/Main.unity "Main") scene to see an example, or follow the below steps to add this to a new [Unity](https://unity.com "Unity") project:

1. Install [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes "NaughtyAttributes") to your [Unity](https://unity.com "Unity") project.
2. Add the [CameraManager.cs](Assets/CameraManager.cs "CameraManager.cs") script from this repository to your [Unity](https://unity.com "Unity") project.

# Usage

1. Add a [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") component to a `GameObject`.
2. Configure the properties on the [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") as well as the `Camera` component itself within your scene.
    - When editing, the camera is "between" where the left and right cameras will be, with the "Offset" field representing how far apart in meters the two screenshots will be.
    - For example, an offset of 0.25 will mean the left screenshot will shift 0.25 meters to the left to take it, and the right screenshot will shift 0.25 meters to the right to take it.
3. Click the `Generate Data` button on the [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") component.
4. After a few seconds, in the root of your project, you can find the data in `Camera-Data/{scene}/{camera}` where `{scene}` is the name of the currently loaded scene and `{camera}` is the name of the [Camera Manager](Assets/CameraManager.cs "CameraManager.cs") component's GameObject. This will open automatically.

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