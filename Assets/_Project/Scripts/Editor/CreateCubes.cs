using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Ashes.Editor
{
    public class CreateCubes : EditorWindow
    {
        [MenuItem("From The Ashes/Board Tool")]
        private static void ShowWindow() => GetWindow<CreateCubes>("Board Tool");

        private List<CubeValues> _cubeValuesList = new List<CubeValues>();
        private Transform _selectedPosition = null;
        private bool _isGameObjSeleted = false;
        private bool _toolUpdateDraw = false;
        private int _boardEditIndex = 0;
        private string[] _labels = null;

        [Serializable]
        public struct CubeValues
        {
            [HideInInspector]
            public Vector2 Position;
            public Color Color;
            [Range(0, 1)]
            public float Scale;
            [Range(-0.5f, 0.5f)]
            public float OutlineSize;

            public Vector3 Offset;
        }

        public UnityEngine.Object CubePrefab;

        [Range (1, 10)]
        public int Lines = 1;
        [Range (1, 10)]
        public int Columns = 1;
        [Range (0, 1)]
        public float Scale = 1f;
        [Range(-0.5f, 0.5f)]
        public float OutlineSize = 0.3f;
        public Color OutlineColor = Color.white;

        public bool LockBoard = false;

        [SerializeField]
        public CubeValues SingleCubeEdit = new CubeValues();

        SerializedObject so;
        SerializedProperty propPrefab;
        SerializedProperty propLines;
        SerializedProperty propColumns;
        SerializedProperty propScale;
        SerializedProperty propOutline;
        SerializedProperty propColor;
        SerializedProperty propLock;
        SerializedProperty propCube;
        

        private void OnEnable()
        {
            so = new SerializedObject(this);
            propPrefab = so.FindProperty("CubePrefab");
            propLines = so.FindProperty("Lines");
            propColumns = so.FindProperty("Columns");
            propScale = so.FindProperty("Scale");
            propOutline = so.FindProperty("OutlineSize");
            propColor = so.FindProperty("OutlineColor");
            
            propLock = so.FindProperty("LockBoard");
            propCube = so.FindProperty("SingleCubeEdit");

            UpdateCubePositions();

            Selection.selectionChanged += UpdateSelected;
            SceneView.duringSceneGui += DuringSceneGui;
        }
        
        private void OnDisable()
        {
            Selection.selectionChanged -= UpdateSelected;
            SceneView.duringSceneGui -= DuringSceneGui;
        }

        private void UpdateSelected()
        {
            if (LockBoard) return;

            if (Selection.gameObjects.Length == 0)
            {
                _isGameObjSeleted = false;
                _selectedPosition = null;
            }
            else
            {
                _isGameObjSeleted = true;
                _selectedPosition = Selection.activeGameObject.transform;
            }
        }

        private void DuringSceneGui(SceneView scene)
        {
            _cubeValuesList.ForEach(DrawScaledWireCube);
        }

        private void UpdateCubePositions()
        {
            _cubeValuesList = CreateCubeValues();
        }

        private List<CubeValues> CreateCubeValues()
        {
            List<CubeValues> valuesList = new List<CubeValues>();
            for (int j = 0; j < Lines; j++)
            {
                for (int i = 0; i < Columns; i++)
                {
                    float anchor = -(((float)Columns) - 1) / 2;
                    float offset = Mathf.Round((j + 1) / 2);
                    
                    CubeValues cube = new CubeValues()
                    {
                        Position = new Vector2(anchor + i - offset, -j),
                        Color = OutlineColor,
                        Scale = Scale,
                        OutlineSize = OutlineSize       
                    };

                    valuesList.Add(cube);
                }
            }
            return valuesList;
        }

        private void DrawScaledWireCube(CubeValues values)
        {
            Vector3 drawpos = GetWorldRelativePosition(values.Position) + values.Offset;
            float outlinedSize = values.Scale * (1 + values.OutlineSize);
            float min = Mathf.Min(outlinedSize, values.Scale);
            float max = Mathf.Max(outlinedSize, values.Scale);

            Handles.color = values.Color;
            Handles.DrawWireCube(drawpos, Vector3.one * max);
            Handles.color = Color.white;
            Handles.DrawWireCube(drawpos, Vector3.one * min);
        }

        private void OnGUI()
        {
            so.Update();
            EditorGUILayout.PropertyField(propPrefab, new GUIContent("Cube Prefab:"));
            so.ApplyModifiedProperties();
            
            GUILayout.Space(10);

            _toolUpdateDraw |= DisplayBoardEditor();

            if (LockBoard)
            {
                GUILayout.Space(10);
                _toolUpdateDraw |= DisplayIndividualEditor();
            }
            
            GUILayout.Space(10);

            if (GUILayout.Button("Make the Cubes!"))
            {
                _cubeValuesList.ForEach((value) =>
                {
                    GameObject go = PrefabUtility.InstantiatePrefab(CubePrefab, _selectedPosition) as GameObject;
                    go.transform.position = GetWorldRelativePosition(value.Position) + value.Offset;

                    OutlineCube oc = go.GetComponent<OutlineCube>();
                    oc.Initialize(value.Color, value.Scale, value.OutlineSize);
                });
                
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            }

            if (_toolUpdateDraw)
            {
                SceneView.RepaintAll();
                _toolUpdateDraw = false;
            }
        }

        private bool DisplayBoardEditor()
        {
            bool modified = false;
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Edit Board Values:");
                using (new EditorGUI.DisabledScope(LockBoard))
                {
                    if (DisplayBoardValues())
                    {
                        UpdateCubePositions();
                        modified = true;
                    }
                }

                if (LockBoardToogle())
                {
                    if (_isGameObjSeleted)
                    {
                        ApplyTransformOffset();
                    }

                    UpdateSingleEditLabels();
                    modified = true;
                }
            }

            return modified;
        }

        private bool DisplayBoardValues()
        {
            so.Update();
            EditorGUILayout.PropertyField(propLines);
            EditorGUILayout.PropertyField(propColumns);
            EditorGUILayout.PropertyField(propScale);
            EditorGUILayout.PropertyField(propOutline);
            EditorGUILayout.PropertyField(propColor);

            return (so.ApplyModifiedProperties());
        }

        private bool LockBoardToogle()
        {
            so.Update();
            EditorGUILayout.PropertyField(propLock, new GUIContent("Lock Board"));
            return (so.ApplyModifiedProperties());
        }

        private void ApplyTransformOffset()
        {
            for (int i = 0; i < _cubeValuesList.Count; i++)
            {
                CubeValues values = _cubeValuesList[i];
                values.Offset = LockBoard ? _selectedPosition.position : Vector3.zero;
                _cubeValuesList[i] = values;
            }
        }

        private void UpdateSingleEditLabels()
        {
            UpdateDisplayedCubeValues();

            _labels = _cubeValuesList.ConvertAll(cube =>
                $"{cube.Position.x}, {cube.Position.y}"
            ).ToArray();
        }

        private void UpdateDisplayedCubeValues()
        {
            SingleCubeEdit = _cubeValuesList[_boardEditIndex];
        }

        private bool DisplayIndividualEditor()
        {
            bool modified = false;
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Edit Individual Values:");
                if (CubeSelectionGrid())
                {
                    UpdateDisplayedCubeValues();
                }

                modified = EditIndividualCubeValues();
            }

            return modified;
        }

        private bool CubeSelectionGrid()
        {
            int newIndex = GUILayout.SelectionGrid(_boardEditIndex, _labels, Columns);

            return IfBlock(_boardEditIndex != newIndex, () =>
            {
                _boardEditIndex = newIndex;
            });
        }

        private bool EditIndividualCubeValues()
        {
            so.Update();
            EditorGUILayout.PropertyField(propCube, new GUIContent("Cube Values:"));

            return IfBlock(so.ApplyModifiedProperties(), () => 
            {
                _cubeValuesList[_boardEditIndex] = SingleCubeEdit;
            });
        }

        private Vector3 GetWorldRelativePosition(Vector2 boardPos)
        {
            return GetWorldRelativePosition(BoardToWorld(boardPos));
        }

        private Vector3 GetWorldRelativePosition(Vector3 boardPos)
        {
            Vector3 result = boardPos;

            if (_isGameObjSeleted && !LockBoard)
            {
                result += _selectedPosition.position;
            }

            return result;
        }

        private Vector3 BoardToWorld(Vector2 v)
        {
            return new Vector3(v.x, v.y, v.y - v.x);
        }

        private bool IfBlock(bool cond, Action action)
        {
            if (cond)
            {
                action();
            }
            return cond;
        }
    }
}
