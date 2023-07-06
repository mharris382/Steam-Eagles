using System;
using Buildings.Rooms;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class RoomEffect : MonoBehaviour
{
    public bool useLocalSpace = true;
    
    public float sinkMultiplier = 1;
    public float sourceMultiplier = 1;
    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameTex))] public string textureParameter = "Texture";
    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string sizeParameter = "BoundsSize";
    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string centerParameter = "BoundsCenter";

    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameInt))] public string resolutionParameter = "Resolution";
    
    
    private RoomTextures _room;
    private RoomSimTextures _simTextures;
    private VisualEffect _visualEffect;
    private RoomCamera _roomCamera;
    private RoomSimTextures _roomSimTextures;
    private GasTexture _gasTexture;
  public  float laplacianCenter = -4.0f;
  public  float laplacianNeighbor = 1.0f;
  public float laplacianDiagnal = 0.5f;
    private ISimIOTextures _ioTexture;
    private int _textureParamId;
    private int _resolutionParamId;
    public ISimIOTextures IOTexture => _ioTexture!=null ? _ioTexture : _ioTexture = GetComponentInParent<ISimIOTextures>();
    
    public VisualEffect VisualEffect => _visualEffect ? _visualEffect : _visualEffect = GetComponent<VisualEffect>();
    public RoomTextures Room => _room ? _room : _room = GetComponentInParent<RoomTextures>();
    public GasTexture GasTexture => _gasTexture ? _gasTexture : _gasTexture = GetComponentInParent<GasTexture>();

    public RoomSimTextures SimTextures => _simTextures ? _simTextures : _simTextures = GetComponentInParent<RoomSimTextures>();
    private void Awake()
    {
        _resolutionParamId = Shader.PropertyToID(resolutionParameter);
        _textureParamId = Shader.PropertyToID(textureParameter);
    }



    #region [Validation]

    bool ValidateParamNameTex(string p)
    {
        if (string.IsNullOrEmpty(p))
        {
            return false;
        }

        try
        {
            var v = VisualEffect.GetTexture(p);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    bool ValidateParamNameV3(string p)
    {
        if (string.IsNullOrEmpty(p))
        {
            return false;
        }

        try
        {
            var v = VisualEffect.GetVector3(p);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    bool ValidateParamNameInt(string p)
    {
        if (string.IsNullOrEmpty(p))
        {
            return false;
        }

        try
        {
            var v = VisualEffect.GetInt(p);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    #endregion

    #region [Initialization]

    [Button, EnableIf("@GasTexture.HasTexture")]
    void InitAll()
    {
        InitializeEffectResolution();
        InitializeEffectTexture();        
    }
    
    void InitializeEffectResolution()
    {
        var res = GasTexture.Resolution;
        if (Application.isPlaying)
        {
            VisualEffect.SetInt(_resolutionParamId, res);
        }
        else
        {
            VisualEffect.SetInt(resolutionParameter, res);
        }
    }
    void InitializeEffectTexture()
    {
        if (GasTexture.TryGetTexture(out var tex))
        {
            if (Application.isPlaying)
                VisualEffect.SetTexture(_textureParamId, tex);
            else
                VisualEffect.SetTexture(textureParameter, tex);
        }
    }

    #endregion

    [Button,ButtonGroup()]
    void RunIOCompute()
    {
        if (GasTexture.HasTexture == false)
        {
            Debug.LogError("No Gas Texture", this);
            return;
        }
        var sinkTexture = IOTexture.SimOutputTexture;
        var sourceTexture = IOTexture.SimInputTexture;
        var gasTexture = GasTexture.RenderTexture;
        int sinkTextureSize =  gasTexture.width / sinkTexture.width;
        int srcTextureSize = gasTexture.width / sourceTexture.width; 
        Debug.Assert(sinkTextureSize == gasTexture.height / sinkTexture.height, $"Expected {sinkTextureSize} but got {gasTexture.height/sinkTexture.height}",this);
        Debug.Assert(srcTextureSize == gasTexture.height / sourceTexture.height, $"Expected {srcTextureSize} but got {gasTexture.height/sourceTexture.height}",this);
        SimCompute.AssignIO(gasTexture, sinkTexture, sourceTexture, srcTextureSize, sinkTextureSize, sourceMultiplier, sinkMultiplier);
        SimCompute.DispatchIO(gasTexture);
    }

    [Button,ButtonGroup()]
    void TilemapUpdateCompute()
    {
        GasTexture.ResetTexture();
        SimTextures.Init();
    }
    [Button,ButtonGroup()]
    void RunCompute()
    {
        if (GasTexture.HasTexture == false)
        {
            Debug.LogError("No Gas Texture", this);
            return;
        }
        var gasTexture = GasTexture.RenderTexture;
        SimCompute.AssignDiffuse(gasTexture,(RenderTexture)SimTextures.textureSet.compositeBoundariesTexture.texture, laplacianCenter,laplacianNeighbor, laplacianDiagnal);
        SimCompute.DispatchDiffuse();
        _textureParamId = Shader.PropertyToID(textureParameter);
        VisualEffect.SetTexture(_textureParamId, gasTexture);
    }
    [Button(ButtonSizes.Large)]
    private void FullSimCompute()
    {
       RunIOCompute();
       RunCompute();
    }


    private void Update()
    {
        FullSimCompute();
    }
}