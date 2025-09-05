/*
Copyright (c) 2025 MultiSet AI. All rights reserved.
Licensed under the MultiSet License. You may not use this file except in compliance with the License. and you can’t re-distribute this file without a prior notice
For license details, visit www.multiset.ai.
Redistribution in source or binary forms must retain this notice.
*/

using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiSet
{
    public class MapMeshDownloader : MonoBehaviour
    {
        [Space(10)]
        [Tooltip("Drag and drop the MapSpace GameObject here.")]
        public GameObject m_mapSpace;

        private VpsMap m_vpsMap;
        private MapSet mapSet;
        private string mapOrMapsetCode;
        private string m_savePath;

        [HideInInspector]
        public bool isDownloading = false;
        int loadedMaps = 0;

        private bool itsMap = true;

        public void DownloadMesh()
        {
            if (Application.isPlaying)
            {
                return;
            }

            loadedMaps = 0;
            isDownloading = true;

            var multisetSdkManager = FindFirstObjectByType<MultisetSdkManager>();

            var singleFrameLocalizationManager = FindFirstObjectByType<SingleFrameLocalizationManager>();

            if (singleFrameLocalizationManager != null)
            {
                mapOrMapsetCode = singleFrameLocalizationManager.mapOrMapsetCode;
                itsMap = singleFrameLocalizationManager.localizationType == LocalizationType.Map;
            }

            if (string.IsNullOrWhiteSpace(mapOrMapsetCode))
            {
                isDownloading = false;
                Debug.LogError("Map or MapSet Code Missing in MapLocalizationManager!!");
                return;
            }

            var config = Resources.Load<MultiSetConfig>("MultiSetConfig");
            if (config != null)
            {
                multisetSdkManager.clientId = config.clientId;
                multisetSdkManager.clientSecret = config.clientSecret;

                if (!string.IsNullOrWhiteSpace(multisetSdkManager.clientId) && !string.IsNullOrWhiteSpace(multisetSdkManager.clientSecret))
                {
                    // Subscribe to the AuthCallBack event
                    EventManager<EventData>.StartListening("AuthCallBack", OnAuthCallBack);

                    multisetSdkManager.AuthenticateMultiSetSDK();
                }
                else
                {
                    isDownloading = false;
                    Debug.LogError("Please enter valid credentials in MultiSetConfig!");
                }
            }
            else
            {
                isDownloading = false;
                Debug.LogError("MultiSetConfig not found!");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from the AuthCallBack event
            EventManager<EventData>.StopListening("AuthCallBack", OnAuthCallBack);
        }

        private void OnAuthCallBack(EventData eventData)
        {
            if (eventData.AuthSuccess)
            {
                Debug.Log("Fetching Map data..");

                // Proceed with further actions after successful authentication
                if (itsMap)
                {
                    GetMapDetails(mapOrMapsetCode);
                }
                else
                {
                    GetMapSetDetails(mapOrMapsetCode);
                }
            }
            else
            {
                isDownloading = false;
                Debug.LogError("Authentication failed!");
            }

            // Unsubscribe from the AuthCallBack event
            EventManager<EventData>.StopListening("AuthCallBack", OnAuthCallBack);
        }

        #region MAP-DATA
        private void GetMapDetails(string mapIdOrCode)
        {
            MultiSetApiManager.GetMapDetails(mapIdOrCode, MapDetailsCallback);
        }

        private void MapDetailsCallback(bool success, string data, long statusCode)
        {
            if (string.IsNullOrEmpty(data))
            {
                isDownloading = false;
                Debug.LogError("Error : Map Details Callback: Empty or null data received!");
                return;
            }

            if (success)
            {
                m_vpsMap = JsonUtility.FromJson<VpsMap>(data);
                DownloadGlbFileEditor(m_vpsMap);
            }
            else
            {
                isDownloading = false;
                Debug.LogError("Get Map Details failed!" + data);
            }
        }

        public void DownloadGlbFileEditor(VpsMap vpsMap)
        {
            m_vpsMap = vpsMap;

            var directoryPath = Path.Combine(Application.dataPath, "MultiSet/MapData/" + mapOrMapsetCode);
            var finalFilePath = Path.Combine("Assets/MultiSet/MapData/" + mapOrMapsetCode, mapOrMapsetCode + ".glb");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            m_savePath = Path.Combine(directoryPath, mapOrMapsetCode + ".glb");

            if (File.Exists(m_savePath))
            {
                isDownloading = false;
                ImportAndAttachGLB(finalFilePath);
            }
            else
            {
                var meshLink = m_vpsMap.mapMesh.texturedMesh.meshLink;
                if (!string.IsNullOrWhiteSpace(meshLink))
                {
                    MultiSetApiManager.GetFileUrl(meshLink, FileUrlCallbackEditor);
                }
            }
        }

        private void FileUrlCallbackEditor(bool success, string data, long statusCode)
        {
            if (string.IsNullOrEmpty(data))
            {
                isDownloading = false;
                Debug.LogError("File URL Callback: Empty or null data received!");
                return;
            }

            if (success)
            {
                var meshUrl = JsonUtility.FromJson<FileData>(data);

                MultiSetHttpClient.DownloadFileAsync(meshUrl.url, (byte[] fileData) =>
                {
                    if (fileData != null)
                    {
                        try
                        {
                            File.WriteAllBytes(m_savePath, fileData);

                            // string mapId = Util.GetMapId(meshUrl.url);
                            var finalFilePath = Path.Combine("Assets/MultiSet/MapData/" + mapOrMapsetCode, mapOrMapsetCode + ".glb");

                            // Refresh the Asset Database to make Unity recognize the new file
#if UNITY_EDITOR
                            AssetDatabase.Refresh();
#endif

                            isDownloading = false;

                            if (File.Exists(m_savePath))
                            {
                                ImportAndAttachGLB(finalFilePath);
                            }
                            else
                            {
                                Debug.LogError("File not found at path: " + m_savePath);
                            }

                        }
                        catch (Exception e)
                        {
                            isDownloading = false;
                            Debug.LogError("Failed to save mesh file: " + e.Message);
                        }
                    }
                    else
                    {
                        isDownloading = false;
                        Debug.LogError("Failed to download mesh file.");
                    }
                });
            }
            else
            {
                isDownloading = false;
                var errorJSON = JsonUtility.FromJson<ErrorJSON>(data);
                Debug.LogError("Error : " + errorJSON.error);
            }

        }

        private void ImportAndAttachGLB(string finalFilePath = null)
        {
            var glbPath = finalFilePath;

            if (string.IsNullOrEmpty(glbPath))
            {
                Debug.LogError("GLB path cannot be empty!");
                return;
            }

            if (m_mapSpace == null)
            {
                Debug.LogError("MapSpace GameObject is not assigned!");
                return;
            }

#if UNITY_EDITOR
            var importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);

            if (importedObject == null)
            {
                Debug.LogError("Failed to load GLB file. Ensure the file exists at the specified path.");
                return;
            }

            // Check if a GameObject with the same name already exists in the hierarchy
            var existingObject = GameObject.Find(importedObject.name);
            if (existingObject != null)
            {
                Debug.LogWarning("Map Mesh with the name " + importedObject.name + " already exists in the hierarchy.");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(importedObject) as GameObject;
            if (instance != null)
            {
                instance.transform.SetParent(m_mapSpace.transform, false);

                // Add EditorOnly Tag to the instantiated GameObject
                instance.tag = "EditorOnly";

                //save the gameObject as prefab 
                var prefabPath = Path.Combine("Assets/MultiSet/MapData/", mapOrMapsetCode + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

            }
            else
            {
                Debug.LogError("Failed to instantiate the imported GLB object.");
            }

            //Show Default Unity Dialog
            EditorUtility.DisplayDialog("Map Mesh Ready", "Mesh File is loaded in the scene", "OK");

#endif
        }

        #endregion

        #region MAPSET-DATA

        private void GetMapSetDetails(string mapsetCode)
        {
            MultiSetApiManager.GetMapSetDetails(mapsetCode, MapSetDetailsCallback);
        }

        private void MapSetDetailsCallback(bool success, string data, long statusCode)
        {
            if (string.IsNullOrEmpty(data))
            {
                isDownloading = false;
                Debug.LogError("MapSet Details Callback: Empty or null data received.");
                return;
            }

            if (success)
            {
                var mapSetResult = JsonUtility.FromJson<MapSetResult>(data);

                if (mapSetResult != null && mapSetResult.mapSet.mapSetData != null)
                {
                    mapSet = mapSetResult.mapSet;

                    GetMapSetMesh(mapSet);
                }
            }
            else
            {
                isDownloading = false;
                var errorJSON = JsonUtility.FromJson<ErrorJSON>(data);
                Debug.LogError($" Load MapSet Info Failed: " + errorJSON.error + "  code: " + statusCode);
            }
        }

        public void GetMapSetMesh(MapSet mapSet)
        {
            this.mapSet = mapSet;

            var mapSetDataList = mapSet.mapSetData;

            foreach (var mapSetData in mapSetDataList)
            {
                var mapCode = mapSetData.map.mapCode;
                var directoryPath = Path.Combine(Application.dataPath, "MultiSet/MapData/" + mapSet.mapSetCode);
                var finalFilePath = Path.Combine("Assets/MultiSet/MapData/" + mapSet.mapSetCode, mapCode + ".glb");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (File.Exists(finalFilePath))
                {
                    isDownloading = false;
                    ImportAndAttachGLBMapset(finalFilePath, mapSetData.map._id);
                }
                else
                {
                    var meshLink = mapSetData.map.mapMesh.texturedMesh.meshLink;
                    MultiSetApiManager.GetFileUrl(meshLink, FileUrlCallbackMapset);
                }
            }
        }

        private void FileUrlCallbackMapset(bool success, string data, long statusCode)
        {
            if (string.IsNullOrEmpty(data))
            {
                isDownloading = false;
                Debug.LogError("File URL Callback: Empty or null data received!");
                return;
            }

            if (success)
            {
                var meshUrl = JsonUtility.FromJson<FileData>(data);

                MultiSetHttpClient.DownloadFileAsync(meshUrl.url, (byte[] fileData) =>
                {
                    if (fileData != null)
                    {
                        try
                        {
                            var mapId = Util.GetMapId(meshUrl.url);
                            var mapCode = GetMapCodeFromMapSetData(mapId);

                            var finalFilePath = Path.Combine("Assets/MultiSet/MapData/" + mapSet.mapSetCode, mapCode + ".glb");

                            var directoryPath = Path.Combine(Application.dataPath, "MultiSet/MapData/" + mapSet.mapSetCode);
                            m_savePath = Path.Combine(directoryPath, mapCode + ".glb");

                            File.WriteAllBytes(m_savePath, fileData);

#if UNITY_EDITOR
                            AssetDatabase.Refresh();
#endif

                            isDownloading = false;

                            if (File.Exists(finalFilePath))
                            {
                                ImportAndAttachGLBMapset(finalFilePath, mapId);
                            }
                            else
                            {
                                Debug.LogError("File not found at path: " + finalFilePath);
                            }

                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Failed to save mesh file: " + e.Message);
                        }
                    }
                    else
                    {
                        isDownloading = false;
                        Debug.LogError("Failed to download mesh file.");
                    }
                });
            }
            else
            {
                var errorJSON = JsonUtility.FromJson<ErrorJSON>(data);
                Debug.LogError("Error : " + JsonUtility.ToJson(errorJSON));
            }
        }

        private string GetMapCodeFromMapSetData(string mapId)
        {
            string mapCode = null;

            if (mapSet != null && mapSet.mapSetData != null)
            {
                foreach (var mapSetData in mapSet.mapSetData)
                {
                    if (mapSetData.map._id == mapId)
                    {
                        mapCode = mapSetData.map.mapCode;
                        break;
                    }
                }
            }

            return mapCode;
        }

        private void ImportAndAttachGLBMapset(string finalFilePath = null, string mapId = null)
        {
            var glbPath = finalFilePath;

            if (string.IsNullOrEmpty(glbPath))
            {
                Debug.LogError("GLB path cannot be empty!");
                return;
            }

            if (m_mapSpace == null)
            {
                Debug.LogError("Parent GameObject is not assigned!");
                return;
            }

#if UNITY_EDITOR

            var importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);

            if (importedObject == null)
            {
                Debug.LogError("Failed to load GLB file. Ensure the file exists at the specified path.");
                return;
            }

            // Check if a GameObject with the same name already exists in the scene
            var mapSetObject = GameObject.Find(mapSet.mapSetCode);
            if (mapSetObject == null)
            {
                mapSetObject = new GameObject(mapSet.mapSetCode);
                mapSetObject.transform.SetParent(m_mapSpace.transform, false);
                mapSetObject.tag = "EditorOnly";
            }

            // Check if a GameObject with the same name already exists in the scene
            var existingObject = GameObject.Find(importedObject.name);
            if (existingObject != null)
            {
                Debug.LogWarning("Map Mesh with the name " + importedObject.name + " already exists in the hierarchy.");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(importedObject) as GameObject;

            if (instance != null)
            {
                instance.transform.SetParent(mapSetObject.transform, false);

                Util.UpdateMeshPoseAndRotation(instance, mapSet, mapId);

                // Add EditorOnly Tag to the instantiated GameObject
                instance.tag = "EditorOnly";

                loadedMaps++;
            }
            else
            {
                Debug.LogError("Failed to instantiate the imported GLB object.");
            }

            if (loadedMaps == mapSet.mapSetData.Count)
            {
                //save the gameObject as prefab 
                var prefabPath = Path.Combine("Assets/MultiSet/MapData/", mapSet.mapSetCode + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(mapSetObject, prefabPath);

                var prefabInstance = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) as GameObject;
                prefabInstance.transform.SetParent(m_mapSpace.transform, false);
                DestroyImmediate(mapSetObject);

                //Show Default Unity Dialog
                EditorUtility.DisplayDialog("MapSet Mesh Ready", "MapSet Files are loaded in the scene", "OK");
            }
#endif
        }

        #endregion

    }
}