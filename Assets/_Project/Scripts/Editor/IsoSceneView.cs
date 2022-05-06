using UnityEngine;
using UnityEditor;

namespace Ashes.Editor
{
    public class IsoSceneView : MonoBehaviour
    {
        [MenuItem("Navigation/Hex")]
        static void HexView()
        {
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(35f, 45f, 0f);
            SceneView.lastActiveSceneView.pivot = new Vector3(0f, 0f, 0f);
            SceneView.lastActiveSceneView.size = 5f; // unit
            SceneView.lastActiveSceneView.orthographic = true; // or false
        }

        [MenuItem("Navigation/Top Square")]
        static void TopSquareView()
        {
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(90f, 45f, 0f);
            SceneView.lastActiveSceneView.pivot = new Vector3(0f, 0f, 0f);
            SceneView.lastActiveSceneView.size = 5f; // unit
            SceneView.lastActiveSceneView.orthographic = true; // or false
        }

        [MenuItem("Navigation/Left Square")]
        static void LeftSquareView()
        {
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0f, 90f, 0f);
            SceneView.lastActiveSceneView.pivot = new Vector3(0f, 0f, 0f);
            SceneView.lastActiveSceneView.size = 5f; // unit
            SceneView.lastActiveSceneView.orthographic = true; // or false
        }

        [MenuItem("Navigation/Right Square")]
        static void RightSquareView()
        {
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0f, 0, 0f);
            SceneView.lastActiveSceneView.pivot = new Vector3(0f, 0f, 0f);
            SceneView.lastActiveSceneView.size = 5f; // unit
            SceneView.lastActiveSceneView.orthographic = true; // or false
        }
    }
}