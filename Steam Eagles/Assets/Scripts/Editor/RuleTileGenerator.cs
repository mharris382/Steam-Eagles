//This tool is a part of the VinTools Unity Package: https://vinarkgames.itch.io/vintools

using System;
using System.Collections;
using System.Collections.Generic;
using Buildings.BuildingTilemaps;
using Spaces;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
public class RuleTileGenerator : EditorWindow
{
    Vector2 scrollpos;

    public string tileName = "NewRuleTile";

    //default neighbor positions, copied from the rule tile script so we don't need a reference to it
    public List<Vector3Int> NeighborPositions = new List<Vector3Int>()
    {
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(1, -1, 0),
    };



    [MenuItem("VinTools/Editor Windows/Rule Tile Generator")]
    public static void ShowWindow()
    {
        GetWindow<RuleTileGenerator>("Rule Tile Generator");
    }

    private void OnGUI()
    {
        scrollpos = GUILayout.BeginScrollView(scrollpos);

        //if no rule preset is set
        if (templ_neighbors.Count == 0)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Template setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prp = so.FindProperty("templateSprites");
            EditorGUILayout.PropertyField(prp, true); // True means show children
            so.ApplyModifiedProperties(); //apply modified properties

            GUILayout.Box("Shift select all of the sprites and drag them here. The texture needs to be read/write enabled in order to get colors from it.", EditorStyles.helpBox);

            EditorGUILayout.Space();

            if (GUILayout.Button("Load Template"))
            {
                LoadTemplate();
            }
        }

        //if there is a preset loaded 
        if (templ_neighbors.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            collumns = EditorGUILayout.IntField("Number of collumns", collumns);
            collumns = Mathf.Clamp(collumns, 1, int.MaxValue);
            previewBG = EditorGUILayout.ColorField("Preview BG color", previewBG);

            EditorGUILayout.Space();

            //show textures
            if (templateSprites.Length != tileSprites.Length) DisplayTilemapPreview(collumns, templateSprites);
            else DisplayTilemapPreview(collumns, tileSprites);

            EditorGUILayout.Space();

            GUILayout.Label("Tile Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prp = so.FindProperty("tileSprites");
            EditorGUILayout.PropertyField(prp, true); // True means show children
            so.ApplyModifiedProperties(); //apply modified properties

            EditorGUILayout.Space();

            if (tileSprites.Length == 0) GUILayout.Box("Set sprites to show other options", EditorStyles.helpBox);
            else if (tileSprites.Length != templateSprites.Length) GUILayout.Box("Amount of sprites needs to be the same as the template", EditorStyles.helpBox);
            else
            {
                if (setDefaultIndex)
                {
                    defaultSprite = tileSprites[defaultIndex];
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Default sprite");
                defaultSprite = (Sprite)EditorGUILayout.ObjectField(defaultSprite, typeof(Sprite), false);
                EditorGUILayout.EndHorizontal();

                colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Default collider", colliderType);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Default gameobject");
                defaultGameobject = (GameObject)EditorGUILayout.ObjectField(defaultGameobject, typeof(GameObject), false);
                EditorGUILayout.EndHorizontal();

                addGameobjectsToRules = EditorGUILayout.Toggle("Add gameobject to rules", addGameobjectsToRules);

                EditorGUILayout.Space();

                tileName = EditorGUILayout.TextField("Tile name", tileName);

                EditorGUILayout.Space();

                if (GUILayout.Button("Create tile!"))
                {
                    SaveTile(GenerateRuleTile(), tileName);
                }
            }

        }

        GUILayout.EndScrollView();
    }


    public Sprite[] templateSprites = new Sprite[0];

    public List<List<int>> templ_neighbors = new List<List<int>>();

    int defaultIndex = 0;
    bool setDefaultIndex = false;

    void LoadTemplate()
    {
        //reset lists
        templ_neighbors = new List<List<int>>();

        //loop through the template sprites
        int i = 0;
        foreach (var item in templateSprites)
        {
            //create a new list to store the rules in
            List<int> neighborRules = new List<int>();

            //get slice data
            Rect slice = item.rect;
            Color[] cols = item.texture.GetPixels((int)slice.x, (int)slice.y, (int)slice.width, (int)slice.height);

            //create texture
            Texture2D tex = new Texture2D((int)slice.width, (int)slice.height, TextureFormat.ARGB32, false);
            tex.SetPixels(0, 0, (int)slice.width, (int)slice.height, cols);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            //get the size of the texture
            Vector2Int size = new Vector2Int(tex.width, tex.height);

            bool def = true;

            //set rules based on the color of the pixels
            foreach (var neighbor in NeighborPositions)
            {
                int xPos = 0;
                int yPos = 0;

                //get x pixel coordinate
                switch (neighbor.x)
                {
                    case 0:
                        xPos = size.x / 2;
                        break;
                    case 1:
                        xPos = size.x - 1;
                        break;
                }

                //get y pixel coordinate
                switch (neighbor.y)
                {
                    case 0:
                        yPos = size.y / 2;
                        break;
                    case 1:
                        yPos = size.y - 1;
                        break;
                }

                //get the pixel color
                Color c = tex.GetPixel(xPos, yPos);

                //add the color to the array
                if (c == Color.white)
                {
                    neighborRules.Add(0);
                }
                else if (c == Color.green)
                {
                    neighborRules.Add(RuleTile.TilingRule.Neighbor.This);
                    def = false;
                }
                else if (c == Color.red)
                {
                    neighborRules.Add(RuleTile.TilingRule.Neighbor.NotThis);
                }
            }

            //set default index if available
            if (def)
            {
                defaultIndex = i;
                setDefaultIndex = true;
            }

            //add the list to the list of lists
            templ_neighbors.Add(neighborRules);

            i++;
        }
    }

    int collumns = 6;

    public Color previewBG;

    void DisplayTilemapPreview(int collumns, Sprite[] tiles)
    {
        //set up values
        float sidePadding = position.width * .05f;
        float size = (position.width * .9f / (float)collumns) * .9f; 
        float fullSize = (position.width * .9f / (float)collumns);
        float space = (position.width * .9f / (float)collumns) * .05f;
        float yPos = GUILayoutUtility.GetLastRect().y + sidePadding + space;

        int rows = (tiles.Length / collumns) + (tiles.Length % collumns > 0 ? 1 : 0);

        //draw BG color
        Texture2D bg = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        bg.SetPixel(0, 0, previewBG);
        bg.filterMode = FilterMode.Point;
        bg.Apply();

        EditorGUI.DrawPreviewTexture(new Rect(0, yPos + 10 - sidePadding - space, position.width, rows * fullSize + 2 * sidePadding), bg);

        //draw grid
        for (int y = 0, i = 0; y < rows; y++)
        {
            for (int x = 0; x < collumns; x++)
            {
                if (i < tiles.Length)
                {
                    //get slice data
                    Rect slice = tiles[i].rect;
                    Color[] cols = tiles[i].texture.GetPixels((int)slice.x, (int)slice.y, (int)slice.width, (int)slice.height);

                    //create texture
                    Texture2D texture = new Texture2D((int)slice.width, (int)slice.height, TextureFormat.ARGB32, false);
                    texture.SetPixels(0, 0, (int)slice.width, (int)slice.height, cols);
                    texture.filterMode = FilterMode.Point;
                    texture.Apply();

                    //draw picture
                    EditorGUI.DrawPreviewTexture(new Rect(sidePadding + space + x * fullSize, yPos + 10 + y * fullSize, size, size), texture);

                    i++;
                }
            }

            EditorGUILayout.Space(fullSize);
        }

        EditorGUILayout.Space(2 * (sidePadding + space));
    }

    public Sprite[] tileSprites;

    public Sprite defaultSprite;
    public Tile.ColliderType colliderType = Tile.ColliderType.Sprite;
    public GameObject defaultGameobject;
    public bool addGameobjectsToRules;

    
    public RuleTile GenerateRuleTile()
    {
        RuleTile tile = ScriptableObject.CreateInstance<RuleTile>();

        //set default tile
        tile.m_DefaultSprite = defaultSprite;
        tile.m_DefaultColliderType = colliderType;
        tile.m_DefaultGameObject = defaultGameobject;

        //set tiling rules
        for (int i = 0; i < tileSprites.Length; i++)
        {
            RuleTile.TilingRule rule = new RuleTile.TilingRule();
            rule.m_Sprites[0] = tileSprites[i];
            rule.m_Neighbors = templ_neighbors[i];
            rule.m_ColliderType = colliderType;
            if (addGameobjectsToRules) rule.m_GameObject = defaultGameobject;

            tile.m_TilingRules.Add(rule);
        }

        return tile;
    }

    public static void SaveTile(RuleTile tile, string name)
    {
        AssetDatabase.CreateAsset(tile, $"Assets/{name}.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = tile;
    }
}


public class RuleTileCopier : EditorWindow
{
    
    [MenuItem("Tools/Rule Tile Copier")]
    public static void ShowWindow()
    {
        GetWindow<RuleTileCopier>("Rule Tile Generator");
    }

    public GameObject overrideDefaultGameobject;
    public RuleTile original;
    public Sprite fallbackSprite;
    public Sprite[] sprites = new Sprite[0];
    public TileType tileType = TileType.RULE_TILE;
    public string newTileName;

    private string GetNewTileName()
    {
        if (original == null) return "INVALID: Original Tile is null";
        return string.IsNullOrEmpty(newTileName) ? $"{original.name}" : newTileName;
    }

    private Type GetNewTileType()
    {
        if (original == null) return null;
        switch (tileType)
        {
            case TileType.PIPE:
                return typeof(PipeTile);
            case TileType.SOLID:
                return typeof(SolidTile);
            case TileType.WIRE:
                return typeof(WireTile);
            case TileType.ORIGINAL:
                return original.GetType();
            case TileType.LADDER:
                return typeof(LadderTile);
            case TileType.RULE_TILE:
                return typeof(RuleTile);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void OnGUI()
    {
        if (original == null)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Select Original Rule Tile to Copy", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prp = so.FindProperty("original");
            EditorGUILayout.PropertyField(prp, true);
            so.ApplyModifiedProperties();
        }
        else
        {
            int cnt = original.m_TilingRules.Count;
            GUILayout.Label($"Required Number of Sprite: {cnt}", EditorStyles.boldLabel);
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prp = so.FindProperty("sprites");
            SerializedProperty tileNameProp = so.FindProperty("newTileName");
            SerializedProperty tileTypeProp = so.FindProperty("tileType");
            
            EditorGUILayout.PropertyField(prp, true);
            EditorGUILayout.PropertyField(tileNameProp, true);
            EditorGUILayout.PropertyField(tileTypeProp, true);
            so.ApplyModifiedProperties();
            DoOverrideGOField(so);
            if (original.m_TilingRules.Count == sprites.Length)
            {
                if (GUILayout.Button($"Copy Rule Tile as {tileType} Tile"))
                {
                    var newTileName = GetNewTileName();
                    SaveTile(GetRuleTileCopy(original),newTileName);
                }
            }
        }
    }

   public  enum TileType
    {
        PIPE,
        SOLID,
        PUZZLE,    
        ORIGINAL,
        WIRE,
        LADDER,
        RULE_TILE
    }
    public static void SaveTile(RuleTile tile, string name)
    {
        AssetDatabase.CreateAsset(tile, $"Assets/Tiles/{name}.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = tile;
    }
    private void DoOverrideGOField(SerializedObject so)
    {
        SerializedProperty prp = so.FindProperty("overrideDefaultGameobject");
        EditorGUILayout.PropertyField(prp,true);
        so.ApplyModifiedProperties();
    }

    private RuleTile GetRuleTileCopy(RuleTile ruleTile)
    {
        var tile = CopyRuleTile(ruleTile, GetNewTileType());
        tile.name = GetNewTileName();
        return tile;
    }

    private PipeTile CopyRuleTile(RuleTile ruleTile)
    {
        var tile = ScriptableObject.CreateInstance<PipeTile>();
        tile.m_DefaultSprite = ruleTile.m_DefaultSprite;
        tile.m_DefaultColliderType = ruleTile.m_DefaultColliderType;
        tile.m_DefaultGameObject = overrideDefaultGameobject == null ? ruleTile.m_DefaultGameObject : overrideDefaultGameobject;
        RuleTile.TilingRule[] ruleTiles = new RuleTile.TilingRule[ruleTile.m_TilingRules.Count];
        for (int i = 0; i < ruleTile.m_TilingRules.Count; i++)
        {
            var originalRule = ruleTile.m_TilingRules[i];
            var rule = new RuleTile.TilingRule();

            rule.m_Sprites[0] = sprites[i];
            rule.m_Neighbors = originalRule.m_Neighbors;
            rule.m_GameObject = overrideDefaultGameobject == null
                ? originalRule.m_GameObject
                : overrideDefaultGameobject;
            rule.m_ColliderType = originalRule.m_ColliderType;
            tile.m_TilingRules.Add(rule);
        }

        return tile;
    }
    private RuleTile CopyRuleTile(RuleTile ruleTile, Type ruleTileType)
    {
        var tile = ScriptableObject.CreateInstance(ruleTileType) as RuleTile;
        tile.m_DefaultSprite = sprites[0];
        tile.m_DefaultColliderType = ruleTile.m_DefaultColliderType;
        tile.m_DefaultGameObject = overrideDefaultGameobject == null ? ruleTile.m_DefaultGameObject : overrideDefaultGameobject;
        RuleTile.TilingRule[] ruleTiles = new RuleTile.TilingRule[ruleTile.m_TilingRules.Count];
        for (int i = 0; i < ruleTile.m_TilingRules.Count; i++)
        {
            var originalRule = ruleTile.m_TilingRules[i];
            var rule = new RuleTile.TilingRule();

            rule.m_Sprites[0] = sprites[i];
            rule.m_Neighbors = originalRule.m_Neighbors;
            rule.m_GameObject = overrideDefaultGameobject == null
                ? originalRule.m_GameObject
                : overrideDefaultGameobject;
            rule.m_ColliderType = originalRule.m_ColliderType;
            tile.m_TilingRules.Add(rule);
        }

        return tile;
    }
}
#endif