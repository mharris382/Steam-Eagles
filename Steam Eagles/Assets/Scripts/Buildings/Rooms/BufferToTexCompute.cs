using UnityEngine;

namespace Buildings.Rooms
{
    public class BufferToTexCompute
    {
        static ComputeShader _bufferToTexCompute;
        public static ComputeShader BufferToTexComputeShader => _bufferToTexCompute ? _bufferToTexCompute : _bufferToTexCompute = Resources.Load<ComputeShader>("BufferToTexture");
    }
}