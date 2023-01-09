using UnityEngine;
using Unity.Netcode.Components;

namespace Unity.Netcode.Components
{
    [DisallowMultipleComponent]
    public class NetworkTransformClient : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}

