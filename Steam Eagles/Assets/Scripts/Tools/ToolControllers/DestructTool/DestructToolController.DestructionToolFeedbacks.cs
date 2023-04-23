using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Tools.DestructTool
{
    public partial class DestructToolController
    {
        [System.Serializable]
        public class DestructionToolFeedbacks
        {
            public void Init()
            {
                
            }

            public abstract class DestructionToolFeedbackBase : IDisposable
            {
                private BoolReactiveProperty _active = new BoolReactiveProperty();
                private IDisposable _disposable;

                public virtual void Initialize()
                {
                    _disposable = _active.Subscribe(OnActiveChanged);
                }
                
                public void UpdateTool(bool active, int hits)
                {
                    _active.Value = active;
                    UpdateToolFeedback(active, hits);
                }

                protected abstract void OnActiveChanged(bool active);
                protected abstract void UpdateToolFeedback(bool active, int hits);
                public void Dispose()
                {
                    _disposable.Dispose();
                    _active.Dispose();
                }
            }
            [System.Serializable]
            public class DestructionToolFeedbackAudio
            {
                [Required]public AudioSource toolActiveLoop;
                [Required]public AudioSource toolHittingLoop;
                public bool overlayActiveSounds;


                public void UpdateToolSounds(bool active, int hits)
                {
                    if (!active)
                    {
                        toolActiveLoop.Stop();
                        toolHittingLoop.Stop();
                    }
                    else
                    {
                        if (hits <= 0)
                        {
                            toolActiveLoop.Play();
                            toolHittingLoop.Stop();
                        }
                        else
                        {
                            toolHittingLoop.Play();
                            if (overlayActiveSounds)
                                toolActiveLoop.Play();
                            else
                                toolActiveLoop.Stop();
                        }
                    }
                }
            }

            [System.Serializable]
            public class DestructionToolFeedbackVFX
            {
                [Required]public ParticleSystem toolActiveLoop;
                [Required]public ParticleSystem toolHittingLoop;
                public bool overlayActiveSounds;


                public void UpdateToolSounds(bool active, int hits)
                {
                    if (!active)
                    {
                        toolActiveLoop.Stop();
                        toolHittingLoop.Stop();
                    }
                    else
                    {
                        if (hits <= 0)
                        {
                            toolActiveLoop.Play();
                            toolHittingLoop.Stop();
                        }
                        else
                        {
                            toolHittingLoop.Play();
                            if (overlayActiveSounds)
                                toolActiveLoop.Play();
                            else
                                toolActiveLoop.Stop();
                        }
                    }
                }
            }
        }
    }
}