#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

public class RuleTileCopier : OdinEditorWindow
{
    
    [MenuItem("Tools/Rule Tile Copier")]
    public static void ShowWindow()
    {
        GetWindow<RuleTileCopier>("Rule Tile Generator");
    }
    
        
    [PropertyOrder(-2)]
    [EnableIf(nameof(CanCreateTile))]
    [Button(ButtonSizes.Gigantic)]
    public void CreateAndSaveTile()
    {
        SaveTile(GetRuleTileCopy(original), newTileName);
    }
    
    
    [PropertyOrder(-1000)]
    [OnValueChanged(nameof(updateSprites))]
    [Required]
    public RuleTile original;
    
    
        
    [PropertyOrder(-90)]
    [ValueDropdown(nameof(GetTypeOptions))]
    public string selectedType = "";


    [PropertyOrder(-80)]
    public string newTileName;
    
    
    
    [BoxGroup("Sprites")] [ShowIf(nameof(HasOriginal))]
    public Sprite defaultSprite;
    
    [PropertyOrder(1000)] [FoldoutGroup("Sprites/Extras")] [ShowIf(nameof(HasOriginal))]
    public GameObject overrideDefaultGameobject;
    

    
    [FoldoutGroup("Sprites/Extras"),PropertyOrder(-7)] [ShowIf(nameof(HasOriginal))]
    public Sprite fallbackSprite;
    
    [InfoBox("@spritesInfo")] [PropertyOrder(-8)] [BoxGroup("Sprites")] [ShowIf(nameof(HasOriginal))]
    public Sprite[] sprites = new Sprite[0];

    public string spritesInfo
    {
        get
        {
            if (!HasOriginal()) return "must assign original tile";
            var cnt = original.m_TilingRules.Count;
            return $"Expecting {cnt} sprites in this tileset";
        }
    }
    
    [FoldoutGroup("Sprites/Extras",false)] [ToggleGroup("Sprites/Extras/" + nameof(useMultiSprites))]
    public bool useMultiSprites;
    
    
    [ToggleGroup("Sprites/Extras/" + nameof(useMultiSprites))] [ShowIf(nameof(useMultiSprites))]
    public List<Sprite[]> multiSprites = new List<Sprite[]>();

    
    bool HasOriginal() => original != null;
    
    void updateSprites()
    {
        if (original == null)
            return;

        if (useMultiSprites)
        {
            multiSprites = new List<Sprite[]>(original.m_TilingRules.Count);
            foreach (var rule in original.m_TilingRules)    
            {
                multiSprites.Add(new Sprite[rule.m_Sprites.Length]);
            }
        }
    }

    private bool hasOriginal => original != null;

    [PropertyOrder(1001)] [EnableIf(nameof(hasOriginal))] [ButtonGroup("Copy"), Button]
    void CopySprites()
    {
        if (original == null)
        {
            return;
        }

        defaultSprite = original.m_DefaultSprite;
        sprites = new Sprite[original.m_TilingRules.Count];
        for (int i = 0; i < original.m_TilingRules.Count; i++)
        {
            sprites[i] = original.m_TilingRules[i].m_Sprites[0];
        }
    }
    
    [PropertyOrder(1000)] [EnableIf(nameof(hasOriginal))] [ButtonGroup("Copy"), Button]
    void CopyMultiSprites()
    {
        if (original == null)
        {
            return;
        }
        multiSprites = new List<Sprite[]>(original.m_TilingRules.Count);
        foreach (var rule in original.m_TilingRules)    
        {
            multiSprites.Add(new Sprite[rule.m_Sprites.Length]);
            for (int i = 0; i < rule.m_Sprites.Length; i++)
            {
                multiSprites[^1][i] = rule.m_Sprites[i];
            }
        }
    }


    public bool CanCopy() => HasOriginal() && !string.IsNullOrEmpty(selectedType) && !string.IsNullOrEmpty(newTileName);

    [Obsolete]
    [HideInInspector]
    public TileType tileType = TileType.RULE_TILE;
    

    [HideInInspector]
    public Type customTileType;

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

    [Button]
    public void CopyOriginalSprites()
    {
        if (original == null)
        {
            return;
        }

        sprites = new Sprite[original.m_TilingRules.Count];

        for (int i = 0; i < sprites.Length; i++)
        {
            var rule = original.m_TilingRules[i];
            sprites[i] = rule.m_Sprites[0];
        }
    }

    ValueDropdownList<string> GetTypeOptions()
    {
        var vdl = new ValueDropdownList<string>();
        vdl.Add("");
        foreach (var tileType in GetAllTileTypes())
        {
            vdl.Add(tileType.Name, tileType.AssemblyQualifiedName);
        }
        return vdl;
    }
    private IEnumerable<Type> GetAllTileTypes()
    {
        return typeof(PuzzleTile).Assembly.DefinedTypes
            .Where(t => t.IsSubclassOf(typeof(PuzzleTile)))
            .Where(t => !t.IsAbstract);
    }

    protected override void OnGUI()
    {
            base.OnGUI();
            EditorGUILayout.Space();
            GUILayout.Label("Select Original Rule Tile to Copy", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prp = so.FindProperty("original");
            EditorGUILayout.PropertyField(prp, true);
           
            if (original == null)
            {
                so.ApplyModifiedProperties();
                return;
            }
            int cnt = original.m_TilingRules.Count;
            GUILayout.Label($"Required Number of Sprite: {cnt}", EditorStyles.boldLabel);
             target = this;
             so = new SerializedObject(target);
             prp = so.FindProperty("sprites");
             SerializedProperty tileTypeProp = so.FindProperty("selectedType");
            SerializedProperty tileNameProp = so.FindProperty("newTileName");
            EditorGUILayout.PropertyField(tileNameProp, true);
            EditorGUILayout.PropertyField(tileTypeProp, true);
            if (string.IsNullOrEmpty(tileNameProp.stringValue))
            {
                so.ApplyModifiedProperties();
                return;
            }
            if (string.IsNullOrEmpty(tileNameProp.stringValue) )
            {
                so.ApplyModifiedProperties();
                return;
            }

            if (Type.GetType(tileTypeProp.stringValue) == null)
            {
                so.ApplyModifiedProperties();
                return;
            }
            
            

            EditorGUILayout.PropertyField(prp, true);
           
            so.ApplyModifiedProperties();
            DoOverrideGOField(so);
            if (original.m_TilingRules.Count == sprites.Length)
            {
                if (GUILayout.Button($"Copy Rule Tile as {tileType} Tile"))
                {
                    var newTileName = GetNewTileName();
                    SaveTile(GetRuleTileCopy(original), newTileName);
                }
            }
        
    }

    bool CanCreateTile()
    {
        if (!CanCopy()) 
            return false;
        
        var type = Type.GetType(selectedType);
        var tileName = newTileName;
        return type != null && type.InheritsFrom(typeof(PuzzleTile));
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
        var tile = CopyRuleTile(ruleTile, Type.GetType(selectedType));
        tile.name = GetNewTileName();
        tile.m_DefaultSprite = this.defaultSprite;
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