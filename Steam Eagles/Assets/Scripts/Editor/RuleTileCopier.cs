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
using UnityEngine.Tilemaps;

public class RuleTileInspector : OdinEditorWindow
{
    public RuleTile tile;
    
    
    
    [MenuItem("Tools/Rule Tile RuleTileInspector")]
    public static void ShowWindow()
    {
        var window = GetWindow<RuleTileInspector>("Rule Tile Inspector");
        
    }

    protected override IEnumerable<object> GetTargets()
    {
        if (tile != null)
        {
            
        }
        return base.GetTargets();
    }
}
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

    protected override IEnumerable<object> GetTargets()
    {
        return base.GetTargets();
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
        Debug.Assert(originalSprites.Length == sprites.Length);
        for (int i = 0; i < sprites.Length; i++)
        {
            var newSprite = sprites[i];
            var originalSprite = originalSprites[i];
            var rule = ruleTile.m_TilingRules.First(t => t.m_Sprites[0] == originalSprite);
            var index = ruleTile.m_TilingRules.IndexOf(rule);
            var newRule = new RuleTile.TilingRule();
            newRule.m_Neighbors = rule.m_Neighbors.ToList();
            newRule.m_ColliderType = Tile.ColliderType.Sprite;
            newRule.m_Sprites = new[] { newSprite };
            ruleTiles[index] = newRule;
        }
        tile.m_TilingRules = ruleTiles.ToList();
        //for (int i = 0; i < ruleTile.m_TilingRules.Count; i++)
        //{
        //    var originalRule = ruleTile.m_TilingRules[i];
        //    var rule = new RuleTile.TilingRule();
//
        //    rule.m_Sprites[0] = sprites[i];
        //    rule.m_Neighbors = originalRule.m_Neighbors;
        //    rule.m_GameObject = overrideDefaultGameobject == null
        //        ? originalRule.m_GameObject
        //        : overrideDefaultGameobject;
        //    rule.m_ColliderType = originalRule.m_ColliderType;
        //    tile.m_TilingRules.Add(rule);
        //}
        return tile;
    }

    [ValidateInput(nameof(Validate))]
    public Sprite[] originalSprites;

    bool Validate(Sprite[] ogSprites, ref string errorMessage)
    {
        if (original == null)
        {
            errorMessage = "Assign Original rule first";
            return false;
        }
        if (ogSprites.Length != original.m_TilingRules.Count)
        {
            errorMessage = $"Expected number of sprites {original.m_TilingRules.Count} does not match number of sprites {ogSprites.Length}";
            return false;
        }
        foreach (var ogSprite in ogSprites)
        {
            var tile = original.m_TilingRules.FirstOrDefault(t => t.m_Sprites.Contains(ogSprite));
            if (tile == null)
            {
                errorMessage = $"No rule for sprite {ogSprite.name}";
                return false;
            }
        }

        return true;
    }
    
    public class RuleTileSetWrapper
    {
        private const int ROWS = 4;
        private readonly RuleTile _original;
        private readonly Sprite[] _replacements;


        public int[,] matrix;

        private static Color GREEN_COLOR = new Color(0.1f, 0.8f, 0.1f, 0.2f);
        private static Color RED_COLOR = new Color(0.8f, 0.1f, 0.1f, 0.2f);
        
        private  int DrawCell(Rect rect, int value)
        {
            var gridW = rect.width / 3f;
            var gridH = rect.height / 3f;
            var rule = _original.m_TilingRules[value];
            var sprite = _replacements[value];
            //var upBox = bounds.SplitGrid(bounds.width/3f, bounds.height/3f, )
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if(x == 1 && y == 1)
                        continue;
                    int index = x + y * 3;
                    var box = rect.SplitGrid(gridW, gridH, index);
                    
                } 
            }
            
            return value;
        }
        
        public bool IsValid => _replacements.Length == _original.m_TilingRules.Count;
        
        
        
        public RuleTileSetWrapper(RuleTile original, Sprite[] replacements)
        {
            _original = original;
            _replacements = replacements;
            int columns = Mathf.CeilToInt((float) _replacements.Length / ROWS);
        }
    }
}
#endif