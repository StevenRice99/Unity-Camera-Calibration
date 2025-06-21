using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Mathematics;
#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
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
    /// The offset between left and right cameras.
    /// </summary>
#if UNITY_EDITOR
    [Tooltip("The offset between left and right cameras.")]
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
        set => width = Mathf.Max(1, value);
    }
    
    /// <summary>
    /// The height of the camera in pixels.
    /// </summary>
    public int Height
    {
        get => height;
        set => height = Mathf.Max(1, value);
    }
    
    /// <summary>
    /// The offset between left and right cameras.
    /// </summary>
    public float Offset
    {
        get => offset;
        set => offset = Mathf.Max(float.Epsilon, value);
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
        if (cam == null || cam.gameObject != gameObject)
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return;
                }
#endif
                cam = gameObject.AddComponent<Camera>();
            }
        }
#if UNITY_EDITOR
        else if (EditorUtility.IsDirty(cam))
        {
            ConfigureCamera();
        }
#endif
        // Nothing to do if the screen size has changed.
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
        float scaleHeight = (float)_previousWidth / _previousHeight / ((float) Mathf.Max(width, 1) / Mathf.Max(height, 1));
        
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
        if (cam == null || cam.gameObject != gameObject)
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return;
                }
#endif
                cam = gameObject.AddComponent<Camera>();
            }
        }
        
        ConfigureCamera();
        Scale();
    }
#endif
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
    /// Capture screenshots from all target camera positions with the properties of this camera.
    /// </summary>
#if UNITY_EDITOR
    [Button("Generate Data")]
#endif
    public void GenerateData()
    {
        // Ensure the camera is configured.
        ConfigureCamera();
        
        // Be safe and perform a scale.
        Scale();
        
        // Cache values for properties.
        double focalLength = cam.focalLength;
        Vector2 sensorSize = cam.sensorSize;
        Vector2 lensShift = cam.lensShift;
        
        // Store intrinsic values.
        double focalLengthX = focalLength * ((double) width / sensorSize.x);
        double focalLengthY = focalLength * ((double) height / sensorSize.y);
        double principalPointX = (0.5 + lensShift.x) * width;
        double principalPointY = (0.5 + lensShift.y) * height;
        
        // Get the folder to save to.
        string root = Application.dataPath;
#if UNITY_EDITOR
        root = root.Replace("/Assets", string.Empty);
#endif
        root = Path.Combine(root, "Camera-Data");
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }
        
        root = Path.Combine(root, name);
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }
        
        // Write camera intrinsic values.
        File.WriteAllText(Path.Combine(root, "Focal-Length-X.txt"), focalLengthX.ToString(CultureInfo.InvariantCulture));
        File.WriteAllText(Path.Combine(root, "Focal-Length-Y.txt"), focalLengthY.ToString(CultureInfo.InvariantCulture));
        File.WriteAllText(Path.Combine(root, "Principal-Point-X.txt"), principalPointX.ToString(CultureInfo.InvariantCulture));
        File.WriteAllText(Path.Combine(root, "Principal-Point-Y.txt"), principalPointY.ToString(CultureInfo.InvariantCulture));
        File.WriteAllText(Path.Combine(root, "Intrinsic-Matrix.txt"), $"{focalLengthX} {0} {principalPointX}\n{0} {focalLengthY} {principalPointY}\n0 0 1");
        
        // Handle if the offset is as small as possible.
        float[] offsets;
        if (offset <= float.Epsilon)
        {
            offsets = new[] { -float.Epsilon, float.Epsilon };
        }
        else
        {
            float half = offset / 2;
            offsets = new[] { -half, half };
        }
        
        // Cache the original position so we can restore it after
        Transform t = transform;
        Vector3 p = t.position;
        
        // Cache the raycast.
        RaycastHit[] hit = new RaycastHit[1];
        
        // Perform for the left and right camera, if we have both.
        for (int i = 0; i < offsets.Length; i++)
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
                        // Create a ray from the camera going through the current pixel.
                        Ray ray = cam.ScreenPointToRay(new(x, y, 0));
                        
                        // Perform the raycast.
                        if (Physics.RaycastNonAlloc(ray, hit) > 0)
                        {
                            // If the ray hits a collider, create a new coordinate pair and add it to our list.
                            coordinates.Add(new(t.InverseTransformPoint(hit[0].point), new(x, y)));
                        }
                    }
                }
                
                // Get the formatted strings.
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
                File.WriteAllText(Path.Combine(root, "Calibration-3D.txt"), world.ToString());
                File.WriteAllText(Path.Combine(root, "Calibration-2D.txt"), pixels.ToString());
            }
            
            // Set the active render texture and read the pixels.
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new(0, 0, width, height), 0, 0);
            screenshot.Apply();
            
            // Clean up by resetting the camera's target texture and restoring the original rectangle.
            cam.targetTexture = null;
            
            // Restore the original camera rectangle.
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
            // Encode the texture to PNG format.
            byte[] bytes = screenshot.EncodeToPNG();
            
            // Clean up the screenshot texture.
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
            // Save the image.
            string side = left ? "Left" : "Right";
            File.WriteAllBytes(Path.Combine(root, $"{side}.png"), bytes);
        }
        
        // Restore the original position.
        t.position = p;
    }
    
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