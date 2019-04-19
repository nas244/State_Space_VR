using System;
using System.Diagnostics;
using TriLib;
using TriLib.Samples;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(AssetDownloader))]
[RequireComponent(typeof(FileOpenDialog))]
public class ImportScript : MonoBehaviour
{
    public static ImportScript Instance { get; private set; }

    public bool Async;

    public void ImportButton(bool button)
    {
        if (button)
        {
            LoadImportModelsButtonClick();
        }
    }

    protected void Awake()
    {
        Instance = this;
    }

    private GameObject _rootGameObject;

    private void AssetLoader_OnMetadataProcessed(AssimpMetadataType metadataType, uint metadataIndex, string metadataKey, object metadataValue)
    {
        Debug.Log("Found metadata of type [" + metadataType + "] at index [" + metadataIndex + "] and key [" + metadataKey + "] with value [" + metadataValue + "]");
    }

    private void PreLoadSetup()
    {
        if (_rootGameObject != null)
        {
            Destroy(_rootGameObject);
            _rootGameObject = null;
        }
    }


    private AssetLoaderOptions GetAssetLoaderOptions()
    {
        var assetLoaderOptions = AssetLoaderOptions.CreateInstance();
        assetLoaderOptions.DontLoadCameras = false;
        assetLoaderOptions.DontLoadLights = false;
        assetLoaderOptions.AddAssetUnloader = true;
        return assetLoaderOptions;
    }

    private void LoadImportModelsButtonClick()
    {
        var fileOpenDialog = FileOpenDialog.Instance;
        fileOpenDialog.Title = "Please select a File";
        fileOpenDialog.Filter = AssetLoaderBase.GetSupportedFileExtensions() + "*.zip;";
#if UNITY_EDITOR && UNITY_WINRT && (NET_4_6 || NETFX_CORE || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
        fileOpenDialog.ShowFileOpenDialog(delegate (byte[] fileBytes, string filename)
        {
            LoadInternal(filename, fileBytes);
        }
#else
        fileOpenDialog.ShowFileOpenDialog(delegate (string filename)
        {
            LoadInternal(filename);
        });
#endif


    }

    private void LoadInternal(string filename, byte[] fileBytes = null)
    {
        PreLoadSetup();
        var assetLoaderOptions = GetAssetLoaderOptions();
        if (!Async)
        {
            using (var import = new AssetLoader())
            {
                import.OnMetadataProcessed += AssetLoader_OnMetadataProcessed;
                try
                {
#if !UNITY_EDITOR && UNITY_WINRT && (NET_4_6 || NETFX_CORE || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                    var extension = FileUtils.GetFileExtension(filename);
                    _rootGameObject = assetLoader.LoadFromMemoryWithTextures(fileBytes, extension, assetLoaderOptions, _rootGameObject);
#else
                    if (fileBytes != null && fileBytes.Length > 0)
                    {
                        var extension = FileUtils.GetFileExtension(filename);
                        _rootGameObject = import.LoadFromMemoryWithTextures(fileBytes, extension, assetLoaderOptions, _rootGameObject);
                    }
                    else if (!string.IsNullOrEmpty(filename))
                    {
                        _rootGameObject = import.LoadFromFileWithTextures(filename, assetLoaderOptions);
                    }
                    else
                    {
                        throw new System.Exception("File not selected");
                    }
#endif
                }
                catch(Exception exception)
                {
                    ErrorDialog.Instance.ShowDialog(exception.ToString());
                }
            }
        }
        else
        {
            using (var import = new AssetLoaderAsync())
            {
                import.OnMetadataProcessed += AssetLoader_OnMetadataProcessed;
                try
                {
                    if(fileBytes != null && fileBytes.Length > 0)
                    {
                        var extension = FileUtils.GetFileExtension(filename);
                        import.LoadFromMemoryWithTextures(fileBytes, extension, assetLoaderOptions, null, delegate (GameObject loadedGameObject)
                        {
                        });
                    }
                    else if (!string.IsNullOrEmpty(filename))
                    {
                        import.LoadFromFileWithTextures(filename, assetLoaderOptions, null, delegate (GameObject loadedGameObject)
                        {
                        });
                    }
                    else
                    {
                        throw new Exception("File not selected");
                    }
                }
                catch (Exception exception)
                {
                    ErrorDialog.Instance.ShowDialog(exception.ToString());
                }
            }
        }
    }
}
