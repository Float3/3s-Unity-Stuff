#if UDON
using UdonSharp;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace _3.StealLightMap
{
	[ExecuteInEditMode]
	public class StealLightmap : UdonSharpBehaviour
	{
		public MeshRenderer lightmappedObject;
		private MeshRenderer _currentRenderer;

		private void Start()
		{
			_currentRenderer = gameObject.GetComponent<MeshRenderer>();
			RendererInfoTransfer();
		}

		private void OnEnable()
		{
			Start();
		}

		#if UNITY_EDITOR
			private void OnBecameVisible()
			{
				RendererInfoTransfer();
			}
		#endif

		private void RendererInfoTransfer()
		{
			if (lightmappedObject == null || _currentRenderer == null)
				return;

			_currentRenderer.lightmapIndex = lightmappedObject.lightmapIndex;
			_currentRenderer.lightmapScaleOffset = lightmappedObject.lightmapScaleOffset;
			_currentRenderer.realtimeLightmapIndex = lightmappedObject.realtimeLightmapIndex;
			_currentRenderer.realtimeLightmapScaleOffset = lightmappedObject.realtimeLightmapScaleOffset;
			_currentRenderer.lightProbeUsage = lightmappedObject.lightProbeUsage;
		}
	}
}
#else
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace _3.StealLightMap
{
    [ExecuteInEditMode]
    public class StealLightmap : MonoBehaviour
    {
        public MeshRenderer lightmappedObject;

        private MeshRenderer _currentRenderer;

        private void Awake()
        {
            _currentRenderer = gameObject.GetComponent<MeshRenderer>();
            RendererInfoTransfer();
        }

        private void OnEnable()
        {
            Awake();
        }

#if UNITY_EDITOR
        private void OnBecameVisible()
        {
            RendererInfoTransfer();
        }
#endif

        private void RendererInfoTransfer()
        {
            if (lightmappedObject == null || _currentRenderer == null)
                return;

            _currentRenderer.lightmapIndex = lightmappedObject.lightmapIndex;
            _currentRenderer.lightmapScaleOffset = lightmappedObject.lightmapScaleOffset;
            _currentRenderer.realtimeLightmapIndex = lightmappedObject.realtimeLightmapIndex;
            _currentRenderer.realtimeLightmapScaleOffset = lightmappedObject.realtimeLightmapScaleOffset;
            _currentRenderer.lightProbeUsage = lightmappedObject.lightProbeUsage;
        }
    }
}
#endif