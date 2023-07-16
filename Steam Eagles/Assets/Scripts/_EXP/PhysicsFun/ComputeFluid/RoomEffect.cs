using System;
using System.Collections;
using System.Collections.Generic;
using _EXP.PhysicsFun.ComputeFluid.Computes;
using Buildings.Rooms;
using CoreLib.Extensions;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.VFX;


public class RoomEffect : MonoBehaviour
{
    [FoldoutGroup(VISUAL_EFFECT)]
    public VisualEffect effect;
    
    
    [FoldoutGroup(SIM_COMPUTE + VERSION_1)] public bool useBoundaryTexture = true;
    [FoldoutGroup(SIM_COMPUTE + VERSION_1)]public float updateRate = 0.5f;
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_1)]public float updateIoRate = 0.25f;
    
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_1)]public float sinkMultiplier = 1;
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_1)]public float sourceMultiplier = 1;
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_2)] public bool useCameraBoundary = false;
    [FoldoutGroup(VISUAL_EFFECT + "/Parameters"), ValidateInput(nameof(ValidateParamNameTex))] public string textureParameter = "Texture";
    [FoldoutGroup(VISUAL_EFFECT + "/Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string sizeParameter = "BoundsSize";
    [FoldoutGroup(VISUAL_EFFECT + "/Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string centerParameter = "BoundsCenter";
    [FoldoutGroup(VISUAL_EFFECT + "/Parameters"), ValidateInput(nameof(ValidateParamNameInt))] public string resolutionParameter = "Resolution";

    private RoomTextures _room;
    private RoomSimTextures _simTextures;
    private VisualEffect _visualEffect;
    
    private RoomSimTextures _roomSimTextures;
    private GasTexture _gasTexture;
    private CapturedRoomTexture _capturedRoomTexture;
    
  
    [FoldoutGroup(SIM_COMPUTE + VERSION_1)] public bool disableSim = true;
    [FoldoutGroup(SIM_COMPUTE + VERSION_1)] public  float laplacianCenter = -4.0f;
    [FoldoutGroup(SIM_COMPUTE + VERSION_1)] public  float laplacianNeighbor = 1.0f;
    [FoldoutGroup(SIM_COMPUTE + VERSION_1)] public float laplacianDiagnal = 0.5f;
    
    private ISimIOTextures _ioTexture;

    private IDisposable _currentComputeUpdateLoop;
    private int _textureParamId;
    private int _resolutionParamId;
    private int _useBoundaryParamId;
    private int _boundaryTexParamId;
    
    const string BOUNDARY_TEX_NAME = "BoundaryTexture";
    const string USE_BOUNDARY_TEX_NAME = "use boundary";
    
    public ISimIOTextures IOTexture => _ioTexture!=null ? _ioTexture : _ioTexture = GetComponentInParent<ISimIOTextures>();
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_2), PropertyOrder(-1)]
    public CapturedRoomTexture CapturedRoomTexture => _capturedRoomTexture ? _capturedRoomTexture : _capturedRoomTexture = GetComponentInParent<CapturedRoomTexture>();
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
    
    
    private void OnEnable()
    {
        if (_currentComputeUpdateLoop != null)
        {
            _currentComputeUpdateLoop.Dispose();
            _currentComputeUpdateLoop = null;
        }
        
        //effect setup
        GasTexture.ResetTexture();
        SimTextures.Init();
        
        var simRoutine = StartCoroutine(UpdateSim());
        var ioRoutine = StartCoroutine(UpdateIO());

        _currentComputeUpdateLoop = Disposable.Create(() =>
        {
            StopCoroutine(simRoutine);
            StopCoroutine(ioRoutine);
            GasTexture.ReleaseTextures();
        });
    }

    private void OnDisable()
    {
        if (_currentComputeUpdateLoop != null)
        {
            _currentComputeUpdateLoop.Dispose();
            _currentComputeUpdateLoop = null;
        }
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

    #region [Experimental]

    private const string VISUAL_EFFECT = "Visual Effect";
    private const string SIM_COMPUTE = "Sim Compute";
    private const string ENVIRONMENT_MAPPING = "Environment Mapping";
    private const string VERSION_1 = "/Version 1";
    private const string VERSION_2 = "/Version 2";
    
    [FoldoutGroup(SIM_COMPUTE)]
    [FoldoutGroup(SIM_COMPUTE + VERSION_1)]
    [Button,ButtonGroup(SIM_COMPUTE + VERSION_1 + "/Toolbar")]
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



    [FoldoutGroup(SIM_COMPUTE + VERSION_2)]
    public float testDeltaTime = 0.1f;
    [FoldoutGroup(ENVIRONMENT_MAPPING)]
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_1)]
    [Button,ButtonGroup(SIM_COMPUTE + VERSION_1 + "/Toolbar")]
    void TilemapUpdateCompute()
    {
        GasTexture.ResetTexture();
        SimTextures.Init();
    }
    
    [Button,ButtonGroup(SIM_COMPUTE + VERSION_1 + "/Toolbar")]
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
        var previousGasTexture = GasTexture.PressurePrevious;
        var velocityTexture = GasTexture.Velocity;
        var boundaryTexture = GetBoundaryTexture();
        SimCompute.DispatchDiffuse(
            gasTexture, previousGasTexture,
            boundaryTexture,velocityTexture,
            laplacianCenter, laplacianNeighbor, laplacianDiagnal
            );
        //swap the pressure textures so that the previous texture is now the current texture
        
    }

    private RenderTexture GetBoundaryTexture()
    {
        return (RenderTexture)SimTextures.textureSet.compositeBoundariesTexture.texture;
    }

    [Button,ButtonGroup(SIM_COMPUTE + VERSION_1 + "/Toolbar")]
    private void FullSimCompute()
    {
        RunIOCompute();
        RunCompute();
    }


    [ EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup(SIM_COMPUTE + VERSION_2 + "/Toolbar")]
    private void MergeIOTextures()
    {
        MergeIOTextures(this.SimTextures.SimInputTexture, SimTextures.SimOutputTexture);
    }

    [ EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup(SIM_COMPUTE + VERSION_2 + "/Toolbar")]
    private void TestUpdateVelocityFromHoles()
    {
        if(!CanCompute())return;
        var velTex = GasTexture.Velocity;
        CaptureWallsFromCamera();
        var holeTex = this.capturedTextures.WallTexture;
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


    [ EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup(SIM_COMPUTE + VERSION_2 + "/Toolbar")]
    private void TestUpdateVelocity() => UpdateVelocity(testDeltaTime);

    [ EnableIf(nameof(EnableButtons))]
    [Button, ButtonGroup(SIM_COMPUTE + VERSION_2 + "/Toolbar")]
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


    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_2)]
    public CapturedTextures capturedTextures
    {
        get
        {
            if (_capturedRoomTexture == null) _capturedRoomTexture = gameObject.AddComponent<CapturedRoomTexture>();
            return _capturedRoomTexture.CapturedTextures;
        }
    }
    
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_2)]
    [Button, ButtonGroup(ENVIRONMENT_MAPPING + VERSION_2 + "/Toolbar")]
    void CaptureWallsFromCamera()
    {
        int width = GasTexture.RenderTexture.width;
        int height = GasTexture.RenderTexture.height;
        CapturedRoomTexture.CaptureLayer(CaptureLayers.WALL, width, height);

    }

    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_2)]
    [Button, ButtonGroup(ENVIRONMENT_MAPPING + VERSION_2 + "/Toolbar")]
    void CaptureIO()
    {
        int width = GasTexture.RenderTexture.width;
        int height = GasTexture.RenderTexture.height;
        CapturedRoomTexture.CaptureLayer(CaptureLayers.INPUT, width, height);
    }
    
    [FoldoutGroup(ENVIRONMENT_MAPPING + VERSION_2)]
    [Button, ButtonGroup(ENVIRONMENT_MAPPING + VERSION_2 + "/Toolbar")]
    void CaptureBoundary()
    {
        int width = GasTexture.RenderTexture.width;
        int height = GasTexture.RenderTexture.height;
        CapturedRoomTexture.CaptureLayer(CaptureLayers.BOUNDARY, width, height);
    }

    #endregion
}