using System;
using System.Collections;
using System.Collections.Generic;
using _EXP.PhysicsFun.ComputeFluid.Computes;
using Buildings.Rooms;
using CoreLib.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;


public class RoomEffect : MonoBehaviour
{
    public VisualEffect effect;
    public bool useBoundaryTexture = true;
    public float updateRate = 0.5f;
    public float updateIoRate = 0.25f;
    public float updateTilemapRate = 2;
    
    public float sinkMultiplier = 1;
    public float sourceMultiplier = 1;
    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameTex))] public string textureParameter = "Texture";
    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string sizeParameter = "BoundsSize";
    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string centerParameter = "BoundsCenter";

    [BoxGroup("Parameters"), ValidateInput(nameof(ValidateParamNameInt))] public string resolutionParameter = "Resolution";

    public bool disableSim = true;
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
    private int _useBoundaryParamId;
    private int _boundaryTexParamId;
    const string BOUNDARY_TEX_NAME = "BoundaryTexture";
    const string USE_BOUNDARY_TEX_NAME = "use boundary";
    public ISimIOTextures IOTexture => _ioTexture!=null ? _ioTexture : _ioTexture = GetComponentInParent<ISimIOTextures>();
    
    public RoomCamera RoomCamera => _roomCamera ? _roomCamera : _roomCamera = GetComponentInParent<RoomCamera>();
    public VisualEffect VisualEffect => _visualEffect ? _visualEffect : _visualEffect = GetComponentInChildren<VisualEffect>();
    public RoomTextures Room => _room ? _room : _room = GetComponentInParent<RoomTextures>();
    public GasTexture GasTexture => _gasTexture ? _gasTexture : _gasTexture = GetComponentInParent<GasTexture>();

    public RoomSimTextures SimTextures => _simTextures ? _simTextures : _simTextures = GetComponentInParent<RoomSimTextures>();
   
    private RenderTexture _mergedIoTexture;
    
    
    private void Awake()
    {
        _visualEffect = effect ? effect : GetComponentInChildren<VisualEffect>();
        _resolutionParamId = Shader.PropertyToID(resolutionParameter);
        _textureParamId = Shader.PropertyToID(textureParameter);
        _useBoundaryParamId = Shader.PropertyToID(USE_BOUNDARY_TEX_NAME);
        _boundaryTexParamId = Shader.PropertyToID(BOUNDARY_TEX_NAME);
    }



    #region [Validation]

    bool ValidateParamNameTex(string p)
    {
        if (_visualEffect == null) _visualEffect = this.effect;
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
            {
                VisualEffect.SetTexture(_textureParamId, tex);
                VisualEffect.SetTexture(_boundaryTexParamId, SimTextures.textureSet.compositeBoundariesTexture.texture);
                VisualEffect.SetBool(_useBoundaryParamId, useBoundaryTexture);
            }
            else
            {
                
                VisualEffect.SetTexture(textureParameter, tex);
                VisualEffect.SetTexture(BOUNDARY_TEX_NAME, SimTextures.textureSet.compositeBoundariesTexture.texture);
                VisualEffect.SetBool(USE_BOUNDARY_TEX_NAME, useBoundaryTexture);
            }
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
        SimCompute.AssignIO(GasTexture.Velocity, sinkTexture, sourceTexture, srcTextureSize, sinkTextureSize, sourceMultiplier, sinkMultiplier);
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
        if(!disableSim)
            RunComputeShader();
    }

    private void UpdateVisualEffectTexture()
    {
        _textureParamId = Shader.PropertyToID(textureParameter);
        VisualEffect.SetTexture(_textureParamId, GasTexture.RenderTexture);
    }

    private void RunComputeShader()
    {
        
        var gasTexture = GasTexture.RenderTexture;
        SimCompute.AssignDiffuse(gasTexture, (RenderTexture)SimTextures.textureSet.compositeBoundariesTexture.texture,
            laplacianCenter, laplacianNeighbor, laplacianDiagnal);
        SimCompute.DispatchDiffuse();
        return;
    }

    [Button(ButtonSizes.Large)]
    private void FullSimCompute()
    {
       RunIOCompute();
       RunCompute();
    }

    private void OnEnable()
    {
        GasTexture.ResetTexture();
        SimTextures.Init();
        StartCoroutine(UpdateSim());
        StartCoroutine(UpdateIO());
    }

    
    IEnumerator UpdateSim()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(updateRate);
            if (GasTexture.HasTexture == false)
            {
                Debug.LogError("No Gas Texture", this);
                continue;
            }
            RunCompute();
            UpdateVisualEffectTexture();
        }
    }
    IEnumerator UpdateIO()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(updateIoRate);
            RunIOCompute();
        }
    }


    [BoxGroup("Sim Version 2"), EnableIf(nameof(EnableButtons))]
    public float testDeltaTime = 0.1f;

    [BoxGroup("Sim Version 2"), EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup("Sim Version 2/Buttons")]
    private void MergeIOTextures()
    {
        MergeIOTextures(this.SimTextures.SimInputTexture, SimTextures.SimOutputTexture);
    }

    [BoxGroup("Sim Version 2"), EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup("Sim Version 2/Buttons")]
    private void TestUpdateVelocityFromHoles()
    {
        if(!CanCompute())return;
        var velTex = GasTexture.Velocity;
        CaptureWallsFromCamera();
        var holeTex = _wallCaptured;
        GasSimCompute.UpdateVelocityFromHoles(velTex, holeTex);
    }
    private void MergeIOTextures(RenderTexture source, RenderTexture sink)
    {
        
        if (_mergedIoTexture == null ||
            !_mergedIoTexture.SizeMatches(source))
        {
            if(_mergedIoTexture != null)_mergedIoTexture.Release();
            _mergedIoTexture = new RenderTexture(source.width, source.height, 0);
            _mergedIoTexture.enableRandomWrite = true;
            _mergedIoTexture.Create();
        }
        GasSimCompute.MergeSourceSinks(source, sink, _mergedIoTexture);
    }


    [BoxGroup("Sim Version 2"), EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup("Sim Version 2/Buttons")]
    private void TestUpdateVelocity() => UpdateVelocity(testDeltaTime);

    [BoxGroup("Sim Version 2"), EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup("Sim Version 2/Buttons")]
    private void TestUpdateGas() => UpdateGasState(testDeltaTime);
    
    
    
    private void UpdateVelocity(float dt)
    {
        if(!CanCompute())return;
        var gasTexture = GasTexture.RenderTexture;
        var velTexture = GasTexture.Velocity;
        var boundaryTexture = SimTextures.SolidTexture;
        GasSimCompute.UpdateVelocity(gasTexture, velTexture, boundaryTexture, _mergedIoTexture, dt);
    }


    bool CanCompute()
    {
        if (!GasTexture.HasTexture)
            return false;
        if(_mergedIoTexture == null)MergeIOTextures();
        return true;
    }
    
 
    private void UpdateGasState(float dt)
    {
        if(!CanCompute())return;
        var gasTexture = GasTexture.RenderTexture;
        var velTexture = GasTexture.Velocity;
        var boundaryTexture = SimTextures.SolidTexture;
        GasSimCompute.UpdateGasState(gasTexture, velTexture, boundaryTexture, _mergedIoTexture, dt);
    }


    bool EnableButtons()
    {
        return GasTexture.HasTexture && SimTextures.SolidTexture != null;
    }


    [ShowInInspector, PreviewField]
    private RenderTexture _wallCaptured;
    public LayerMask wallLayer;
    [Button]
    void CaptureWallsFromCamera()
    {
        if (_wallCaptured == null)
        {
            _wallCaptured = new RenderTexture(GasTexture.Velocity.width, GasTexture.Velocity.height, 1);
            _wallCaptured.enableRandomWrite = true;
            _wallCaptured.Create();
        }
        RoomCamera.CaptureRoom(_wallCaptured, wallLayer);
        GasSimCompute.InvertWalls(_wallCaptured);
    }
    // IEnumerator UpdateTilemap()
    // {
    //     while (enabled)
    //     {
    //         yield return new WaitForSeconds(updateTilemapRate);
    //         
    //     }
    // }
}