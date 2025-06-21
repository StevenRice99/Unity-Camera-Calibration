using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEditor;
#elif UNITY_WEBGL
using System.IO.Compression;
using System.Runtime.InteropServices;
#endif
/// <summary>
/// Manage a camera to extract information.
/// </summary>
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
#if UNITY_EDITOR
[ExecuteInEditMode]
[AddComponentMenu("Camera Manager", 0)]
#endif
public class CameraManager : MonoBehaviour
{
#if !UNITY_EDITOR && UNITY_WEBGL
    /// <summary>
    /// For WebGL builds, this function is defined in "Assets/Plugins/WebGL/FileDownloader.jslib" which takes a byte array and prompts the user to download it as a file.
    /// </summary>
    [DllImport("__Internal")]
    private static extern void DownloadFile(string fileName, byte[] array, int size);
#endif
    /// <summary>
    /// The width of the camera in pixels.
    /// </summary>
#if UNITY_EDITOR
    [Tooltip("The width of the camera in pixels.")]
#endif
    [Min(1)]
    [SerializeField]
    private int width = 1920;
    
    /// <summary>
    /// The height of the camera in pixels.
    /// </summary>
#if UNITY_EDITOR
    [Tooltip("The height of the camera in pixels.")]
#endif
    [Min(1)]
    [SerializeField]
    private int height = 1080;
    
    /// <summary>
    /// The offset between left and right cameras in meters.
    /// </summary>
#if UNITY_EDITOR
    [Tooltip("The offset between left and right cameras in meters.")]
#endif
    [Min(float.Epsilon)]
    [SerializeField]
    private float offset = 0.25f;
    
    /// <summary>
    /// The camera this is controlling.
    /// </summary>
#if UNITY_EDITOR
    [Tooltip("The camera this is controlling.")]
    [HideInInspector]
#endif
    [SerializeField]
    private Camera cam;
    
    /// <summary>
    /// The width of the camera in pixels.
    /// </summary>
    public int Width
    {
        get => width;
        set
        {
            value = Mathf.Max(1, value);
            if (value == width)
            {
                return;
            }
            
            width = value;
            Scale();
        }
    }
    
    /// <summary>
    /// The height of the camera in pixels.
    /// </summary>
    public int Height
    {
        get => height;
        set
        {
            value = Mathf.Max(1, value);
            if (value == height)
            {
                return;
            }
            
            height = value;
            Scale();
        }
    }
    
    /// <summary>
    /// The width and height of the camera in pixels.
    /// </summary>
    public int2 Size
    {
        get => new(width, height);
        set
        {
            value = new(Mathf.Max(1, value.x), Mathf.Max(1, value.y));
            if (value.x == width && value.y == height)
            {
                return;
            }
            
            width = value.x;
            height = value.y;
            Scale();
        }
    }
    
    /// <summary>
    /// The offset between left and right cameras in meters.
    /// </summary>
    public float Offset
    {
        get => offset;
        set => offset = Mathf.Max(float.Epsilon, value);
    }
    
    /// <summary>
    /// The vertical field of view of the Camera, in degrees.
    /// </summary>
    public float FieldOfView
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return float.Epsilon;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.fieldOfView;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.fieldOfView = Mathf.Clamp(value, float.Epsilon, 179);
        }
    }
    
    /// <summary>
    /// The size of the camera sensor, expressed in millimeters.
    /// </summary>
    public Vector2 SensorSize
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return new(0.1f, 0.1f);
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.sensorSize;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.sensorSize = new(Mathf.Max(value.x, 0.1f), Mathf.Max(value.y, 0.1f));
        }
    }
    
    /// <summary>
    /// The X size of the camera sensor, expressed in millimeters.
    /// </summary>
    public float SensorSizeX
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return 0.1f;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.sensorSize.x;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.sensorSize = new(Mathf.Max(value, 0.1f), cam.sensorSize.y);
        }
    }
    
    /// <summary>
    /// The Y size of the camera sensor, expressed in millimeters.
    /// </summary>
    public float SensorSizeY
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return 0.1f;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.sensorSize.y;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.sensorSize = new(cam.sensorSize.x, Mathf.Max(value, 0.1f));
        }
    }
    
    /// <summary>
    /// The camera focal length, expressed in millimeters.
    /// </summary>
    public float FocalLength
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return 0.1047227f;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.focalLength;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.focalLength = Mathf.Max(value, 0.1047227f);
        }
    }
    
    /// <summary>
    /// The lens offset of the camera. The lens shift is relative to the sensor size. For example, a lens shift of 0.5 offsets the sensor by half its horizontal size.
    /// </summary>
    public Vector2 LensShift
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return Vector2.zero;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.lensShift;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.lensShift = value;
        }
    }
    
    /// <summary>
    /// The X lens offset of the camera. The lens shift is relative to the sensor size. For example, a lens shift of 0.5 offsets the sensor by half its horizontal size.
    /// </summary>
    public float LensShiftX
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return 0;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.lensShift.x;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.lensShift = new(value, cam.lensShift.y);
        }
    }
    
    /// <summary>
    /// The Y lens offset of the camera. The lens shift is relative to the sensor size. For example, a lens shift of 0.5 offsets the sensor by half its horizontal size.
    /// </summary>
    public float LensShiftY
    {
        get
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return 0;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            return cam.lensShift.y;
        }
        set
        {
#if UNITY_EDITOR
            if (!GetCamera())
            {
                return;
            }
#else
            GetCamera();
#endif
            ConfigureCamera();
            cam.lensShift = new(cam.lensShift.x, value);
        }
    }
    
    /// <summary>
    /// The width of the last frame.
    /// </summary>
    private int _previousWidth;
    
    /// <summary>
    /// The height of the last frame.
    /// </summary>
    private int _previousHeight;
    
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        
#if UNITY_EDITOR
        if (!GetCamera())
        {
            return;
        }
#else
        GetCamera();
#endif
        ConfigureCamera();
        
        // Nothing to do if the screen size has not changed.
        if (Screen.width != _previousWidth || Screen.height != _previousHeight)
        {
            Scale();
        }
    }
    
    /// <summary>
    /// Scale the camera.
    /// </summary>
    private void Scale()
    {
        // Cache the previous values.
        _previousWidth = Screen.width;
        _previousHeight = Screen.height;
        
        // Get the relative scale to render at.
        float scaleHeight = (float)_previousWidth / _previousHeight / ((float) width / height);
        
        // Set the camera accordingly.
        Rect rect = cam.rect;
        if (scaleHeight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }
        
        cam.rect = rect;
    }
#if UNITY_EDITOR
    /// <summary>
    /// Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
    /// </summary>
    private void OnValidate()
    {
        width = Mathf.Max(width, 1);
        height = Mathf.Max(height, 1);
        if (GetCamera())
        {
            Scale();
        }
    }
#endif
#if UNITY_EDITOR
    /// <summary>
    /// Get the camera.
    /// </summary>
    /// <returns>True if we got the camera, false otherwise.</returns>
    private bool GetCamera()
#else
    /// <summary>
    /// Get the camera.
    /// </summary>
    private void GetCamera()
#endif
    {
        if (cam != null && cam.gameObject == gameObject)
        {
#if UNITY_EDITOR
            return true;
#else
            return;
#endif
        }
        
        cam = GetComponent<Camera>();
        if (cam != null)
        {
#if UNITY_EDITOR
            return true;
#else
            return;
#endif
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return false;
        }
#endif
        cam = gameObject.AddComponent<Camera>();
#if UNITY_EDITOR
        return true;
#endif
    }
    
    /// <summary>
    /// Validate the camera.
    /// </summary>
    private void ConfigureCamera()
    {
        // Set required values.
        cam.orthographic = false;
        cam.usePhysicalProperties = true;
    }
    
    /// <summary>
    /// Capture screenshots from all target camera positions with the properties of this camera. For WebGL builds, this will generate a single ZIP file and prompt for download. For other builds, it will save files to disk and attempt to open the folder.
    /// </summary>
#if UNITY_EDITOR
    [Button("Generate Data")]
#endif
    public void GenerateData()
    {
#if UNITY_EDITOR
        if (!GetCamera())
        {
            Debug.LogError($"No camera attached to \"{name}\" to generate data.", this);
            return;
        }
        
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError($"Not in a scene so cannot generate data from \"{name}\".", this);
            return;
        }
#else
        GetCamera();
        Scene scene = SceneManager.GetActiveScene();
#endif
        // Ensure the camera is configured.
        ConfigureCamera();
        
        // Be safe and perform a scale.
        Scale();
        
        // Cache values for properties.
        double focalLength = cam.focalLength;
        Vector2 sensorSize = cam.sensorSize;
        Vector2 lensShift = cam.lensShift;
        
        // Store intrinsic values.
        double focalLengthX = focalLength * (width / sensorSize.x);
        double focalLengthY = focalLength * (height / sensorSize.y);
        double principalPointX = (0.5 + lensShift.x) * width;
        double principalPointY = (0.5 + lensShift.y) * height;
        
        // The root path for files inside the zip or on the filesystem.
        string dataRootPath = Path.Combine(scene.name, name);
        
        // This action will contain the core logic for generating file data.
        // It's defined here so it can be used for both WebGL (in-memory) and Standalone (disk) builds.
        void GenerateFileData(Action<string, byte[]> writeFileAction)
        {
            // Write camera intrinsic values.
            writeFileAction(Path.Combine(dataRootPath, "Focal-Length-X.txt"), Encoding.UTF8.GetBytes(focalLengthX.ToString(CultureInfo.InvariantCulture)));
            writeFileAction(Path.Combine(dataRootPath, "Focal-Length-Y.txt"), Encoding.UTF8.GetBytes(focalLengthY.ToString(CultureInfo.InvariantCulture)));
            writeFileAction(Path.Combine(dataRootPath, "Principal-Point-X.txt"), Encoding.UTF8.GetBytes(principalPointX.ToString(CultureInfo.InvariantCulture)));
            writeFileAction(Path.Combine(dataRootPath, "Principal-Point-Y.txt"), Encoding.UTF8.GetBytes(principalPointY.ToString(CultureInfo.InvariantCulture)));
            writeFileAction(Path.Combine(dataRootPath, "Intrinsic-Matrix.txt"), Encoding.UTF8.GetBytes($"{focalLengthX} {0} {principalPointX}\n{0} {focalLengthY} {principalPointY}\n0 0 1"));
            
            // Handle creating the offsets for the left and right cameras.
            float half = Mathf.Max(offset, float.Epsilon * 2) / 2;
            float[] offsets = { -half, half };
            
            // Cache the original position so we can restore it after
            Transform t = transform;
            Vector3 p = t.position;
            
            // Cache the raycast.
            RaycastHit[] hit = new RaycastHit[1];
            
            // Perform for the left and right camera, if we have both.
            for (int i = 0; i < 2; i++)
            {
                // Apply the offset.
                t.position = new(p.x + offsets[i], p.y, p.z);
                
                // Store the original camera rectangle and set it to full screen for the screenshot.
                Rect originalRect = cam.rect;
                cam.rect = new(0, 0, 1, 1);
                
                // Create a render texture to render the camera's view into.
                RenderTexture renderTexture = new(width, height, 24);
                cam.targetTexture = renderTexture;
                
                // Create a texture to hold the screenshot.
                Texture2D screenshot = new(width, height, TextureFormat.RGB24, false);
                
                // Render the camera's view.
                cam.Render();
                
                // If this is the left camera, get calibration data.
                bool left = i == 0;
                if (left)
                {
                    // Store all hit coordinates.
                    List<Coordinate> coordinates = new();
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Ray ray = cam.ScreenPointToRay(new(x, y, 0));
                            if (Physics.RaycastNonAlloc(ray, hit) > 0)
                            {
                                coordinates.Add(new(t.InverseTransformPoint(hit[0].point), new(x, y)));
                            }
                        }
                    }
                    
                    StringBuilder world = new();
                    StringBuilder pixels = new();
                    for (int j = 0; j < coordinates.Count; j++)
                    {
                        if (j > 0)
                        {
                            world.Append("\n");
                            pixels.Append("\n");
                        }
                        
                        world.Append(coordinates[j].WorldString());
                        pixels.Append(coordinates[j].PixelsString());
                    }
                    
                    // Save them.
                    writeFileAction(Path.Combine(dataRootPath, "Calibration-3D.txt"), Encoding.UTF8.GetBytes(world.ToString()));
                    writeFileAction(Path.Combine(dataRootPath, "Calibration-2D.txt"), Encoding.UTF8.GetBytes(pixels.ToString()));
                }
                
                RenderTexture.active = renderTexture;
                screenshot.ReadPixels(new(0, 0, width, height), 0, 0);
                screenshot.Apply();
                
                cam.targetTexture = null;
                cam.rect = originalRect;
                RenderTexture.active = null;

#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Destroy(renderTexture);
                }
                else
                {
                    DestroyImmediate(renderTexture);
                }
#else
                Destroy(renderTexture);
#endif
                byte[] bytes = screenshot.EncodeToPNG();
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Destroy(screenshot);
                }
                else
                {
                    DestroyImmediate(screenshot);
                }
#else
                Destroy(screenshot);
#endif
                string side = left ? "Left" : "Right";
                writeFileAction(Path.Combine(dataRootPath, $"{side}.png"), bytes);
            }
            
            // Restore the original position.
            t.position = p;
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        // Create a zip file in memory and trigger a download.
        using (MemoryStream memoryStream = new())
        {
            // Use a ZipArchive to write files into the memory stream.
            ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true);
            // The writeFileAction for WebGL creates entries in the zip archive.
            void AddFileToArchive(string filePath, byte[] fileData)
            {
                // Ensure forward slashes for zip entry paths.
                ZipArchiveEntry entry = archive.CreateEntry(filePath.Replace('\\', '/'));
                using Stream entryStream = entry.Open();
                entryStream.Write(fileData, 0, fileData.Length);
            }
            
            GenerateFileData(AddFileToArchive);
            
            // Manually dispose this in the outer scope to ensure there are no issues.
            archive.Dispose();
            
            // Download the generated zip file.
            DownloadFile($"{scene.name}-{name}.zip", memoryStream.ToArray(), (int)memoryStream.Length);
        }
#else
        // Save files directly to the disk.
        string root = Application.dataPath;
#if UNITY_EDITOR
        root = root.Replace("/Assets", string.Empty);
#endif
        root = Path.Combine(root, "Camera-Data");
        
        // The writeFileAction for Standalone/Editor saves files to disk.
        void WriteBytesToFile([NotNull] string filePath, [NotNull] byte[] fileData)
        {
            string finalPath = Path.Combine(root, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath) ?? string.Empty);
            File.WriteAllBytes(finalPath, fileData);
        }
        
        GenerateFileData(WriteBytesToFile);
        string finalRoot = Path.Combine(root, dataRootPath).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
#if UNITY_EDITOR
        Debug.Log($"Data generated to \"{finalRoot}\".", this);
        EditorUtility.ClearDirty(gameObject);
#endif
        // Open the directory in the file explorer.
        switch (Application.platform)
        {
#if UNITY_EDITOR
            case RuntimePlatform.WindowsEditor:
#endif
            case RuntimePlatform.WindowsPlayer:
                Process.Start(finalRoot);
                return;
#if UNITY_EDITOR
            case RuntimePlatform.OSXEditor:
#endif
            case RuntimePlatform.OSXPlayer:
                Process.Start("open", finalRoot);
                return;
#if UNITY_EDITOR
            case RuntimePlatform.LinuxEditor:
#endif
            case RuntimePlatform.LinuxPlayer:
                Process.Start("xdg-open", finalRoot);
                return;
        }
#endif
    }
#if !UNITY_EDITOR
    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// </summary>
    private void OnGUI()
    {
        // Width.
        GUI.Label(new(10, 10, 100, 20), "Width");
        string temp = GUI.TextField(new(100, 10, 100, 20), Width.ToString());
        if (int.TryParse(temp, out int i))
        {
            Width = i;
        }
        
        // Height.
        GUI.Label(new(10, 30, 100, 20), "Height");
        temp = GUI.TextField(new(100, 30, 100, 20), Height.ToString());
        if (int.TryParse(temp, out i))
        {
            Height = i;
        }
        
        // Offset.
        GUI.Label(new(10, 50, 100, 20), "Offset");
        temp = GUI.TextField(new(100, 50, 100, 20), Offset.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out float f))
        {
            Offset = f;
        }
        
        // Field of View.
        GUI.Label(new(10, 70, 100, 20), "Field of View");
        temp = GUI.TextField(new(100, 70, 100, 20), FieldOfView.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            FieldOfView = f;
        }
        
        // Sensor Size X.
        GUI.Label(new(10, 90, 100, 20), "Sensor Size X");
        temp = GUI.TextField(new(100, 90, 100, 20), SensorSizeX.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            SensorSizeX = f;
        }
        
        // Sensor Size Y.
        GUI.Label(new(10, 110, 100, 20), "Sensor Size Y");
        temp = GUI.TextField(new(100, 110, 100, 20), SensorSizeY.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            SensorSizeY = f;
        }
        
        // Focal Length.
        GUI.Label(new(10, 130, 100, 20), "Focal Length");
        temp = GUI.TextField(new(100, 130, 100, 20), FocalLength.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            FocalLength = f;
        }
        
        // Lens Shift X.
        GUI.Label(new(10, 150, 100, 20), "Lens Shift X");
        temp = GUI.TextField(new(100, 150, 100, 20), LensShiftX.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            LensShiftX = f;
        }
        
        // Lens Shift Y.
        GUI.Label(new(10, 170, 100, 20), "Lens Shift Y");
        temp = GUI.TextField(new(100, 170, 100, 20), LensShiftY.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            LensShiftY = f;
        }
        
        Transform t = transform;
        Vector3 p = t.position;
        
        // Position X.
        GUI.Label(new(10, 190, 100, 20), "Position X");
        temp = GUI.TextField(new(100, 190, 100, 20), p.x.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            p = new(f, p.y, p.z);
        }
        
        // Position Y.
        GUI.Label(new(10, 210, 100, 20), "Position Y");
        temp = GUI.TextField(new(100, 210, 100, 20), p.y.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            p = new(p.x, f, p.z);
        }
        
        // Position Z.
        GUI.Label(new(10, 230, 100, 20), "Position Z");
        temp = GUI.TextField(new(100, 230, 100, 20), p.z.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            p = new(p.x, p.y, f);
        }

        t.position = p;
        Vector3 r = transform.eulerAngles;
        
        // Rotation X.
        GUI.Label(new(10, 250, 100, 20), "Rotation X");
        temp = GUI.TextField(new(100, 250, 100, 20), r.x.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            r = new(f, r.y, r.z);
        }
        
        // Rotation Y.
        GUI.Label(new(10, 270, 100, 20), "Rotation Y");
        temp = GUI.TextField(new(100, 270, 100, 20), r.y.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            r = new(r.x, f, r.z);
        }
        
        // Rotation Z.
        GUI.Label(new(10, 290, 100, 20), "Rotation Z");
        temp = GUI.TextField(new(100, 290, 100, 20), r.y.ToString(CultureInfo.InvariantCulture));
        if (float.TryParse(temp, out f))
        {
            r = new(r.x, r.y, f);
        }
        
        t.eulerAngles = r;
        
        // Generate Data.
        if (GUI.Button(new(10, 315, 190, 20), "Generate Data"))
        {
            GenerateData();
        }
    }
#endif
    /// <summary>
    /// Format this as a string.
    /// </summary>
    /// <returns>The formatted string.</returns>
    public override string ToString()
    {
        return $"Camera \"{name}\": {width} x {height}";
    }
    
    /// <summary>
    /// Store matching coordinates from world space and the corresponding camera pixels.
    /// </summary>
    private readonly struct Coordinate
    {
        /// <summary>
        /// The world coordinates.
        /// </summary>
        private readonly Vector3 _world;
        
        /// <summary>
        /// The pixel coordinates.
        /// </summary>
        private readonly int2 _pixels;
        
        /// <summary>
        /// Configure this coordinate.
        /// </summary>
        /// <param name="world">The world coordinates.</param>
        /// <param name="pixels">The pixel coordinates.</param>
        public Coordinate(Vector3 world, int2 pixels)
        {
            _world = world;
            _pixels = pixels;
        }
        
        /// <summary>
        /// Get the world string.
        /// </summary>
        /// <returns>The pixels string.</returns>
        public string WorldString()
        {
            return $"{_world.x} {_world.y} {_world.z}";
        }
        
        /// <summary>
        /// Get the pixels string.
        /// </summary>
        /// <returns>The pixels string.</returns>
        public string PixelsString()
        {
            return $"{_pixels.x} {_pixels.y}";
        }
        
        /// <summary>
        /// Format this as a string.
        /// </summary>
        /// <returns>The formatted string.</returns>
        public override string ToString()
        {
            return $"World = {WorldString()} | Pixel = {PixelsString()}";
        }
    }
}