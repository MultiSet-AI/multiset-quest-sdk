/*
Copyright (c) 2025 MultiSet AI. All rights reserved.
Licensed under the MultiSet License. You may not use this file except in compliance with the License. and you can’t re-distribute this file without a prior notice
For license details, visit www.multiset.ai.
Redistribution in source or binary forms must retain this notice.
*/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace MultiSet
{
    /// Unity-specific implementation of frame capture that implements IFrameCaptureProvider
    /// IMPORTANT: IFrameCaptureProvider must come from the DLL, not from a local file
    public class FrameCaptureManager : MonoBehaviour, IFrameCaptureProvider
    {
        [Header("Camera References")]
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;
        [SerializeField] private RawImage m_webCamImage; // Optional: display camera feed

        // Events from interface
        public event Action<CapturedFrameData> OnFrameCaptured;
        public event Action<string> OnCaptureError;

        private void Awake()
        {
            m_webCamTextureManager = FindFirstObjectByType<WebCamTextureManager>();

            if (m_webCamTextureManager == null)
            {
                Debug.LogError("WebCamTextureManager is not found!");
            }
        }

        /// Captures current webcam frame with associated camera pose and intrinsics
        public CapturedFrameData CaptureFrame()
        {
            if (m_webCamTextureManager == null)
            {
                OnCaptureError?.Invoke("WebCamTextureManager is not found in scene!");
                return null;
            }

            var webCamTexture = m_webCamTextureManager.WebCamTexture;
            if (webCamTexture == null || !webCamTexture.isPlaying)
            {
                OnCaptureError?.Invoke("WebCamTexture is not available or not playing!");
                return null;
            }

            try
            {
                // --- STEP 1: Capture Image Data ---
                var texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
                texture.SetPixels(webCamTexture.GetPixels());
                texture.Apply();

                // --- STEP 2: Get Associated Camera Pose ---
                var cameraEye = m_webCamTextureManager.Eye;
                var cameraPose = PassthroughCameraUtils.GetCameraPoseInWorld(cameraEye);

                // --- STEP 3: Get Camera Intrinsics ---
                var cameraDetails = PassthroughCameraUtils.GetCameraIntrinsics(cameraEye);
                var intrinsics = new CameraIntrinsics
                {
                    fx = cameraDetails.FocalLength.x,
                    fy = cameraDetails.FocalLength.y,
                    px = cameraDetails.PrincipalPoint.x,
                    py = cameraDetails.PrincipalPoint.y,
                    width = cameraDetails.Resolution.x,
                    height = cameraDetails.Resolution.y
                };

                // --- STEP 4: Encode Image ---
                var imageBytes = texture.EncodeToJPG(90);
                // Optional: Display captured image in UI
                if (m_webCamImage != null)
                {
                    m_webCamImage.texture = texture;
                }

                // Create captured data object using the interface type
                var capturedData = new CapturedFrameData
                {
                    ImageBytes = imageBytes,
                    CameraPosition = cameraPose.position,
                    CameraRotation = cameraPose.rotation,
                    CameraIntrinsics = intrinsics,
                    TextureReference = texture // Store as object
                };

                // Trigger event
                OnFrameCaptured?.Invoke(capturedData);

                return capturedData;
            }
            catch (Exception e)
            {
                OnCaptureError?.Invoke($"Failed to capture image: {e.Message}");
                return null;
            }
        }

        /// Check if webcam is ready for capture
        public bool IsReadyToCapture()
        {
            return m_webCamTextureManager != null &&
                   m_webCamTextureManager.WebCamTexture != null &&
                   m_webCamTextureManager.WebCamTexture.isPlaying;
        }

        /// Cleanup texture if needed
        public void CleanupTexture(CapturedFrameData data)
        {
            if (data?.TextureReference is Texture2D texture)
            {
                Destroy(texture);
            }
        }
    }
}