using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ImportScript : MonoBehaviour
{
    string path;
    public RawImage image;

    public void OpenImportModels()
    {
        path = EditorUtility.OpenFilePanel("Overwrite with stl", "", "stl");
    }
}
