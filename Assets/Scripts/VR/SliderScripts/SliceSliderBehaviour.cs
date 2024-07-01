using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace UnityVolumeRendering
{
    public class SliceSliderBehaviour : NetworkBehaviour
    {
        public void MoveSliceY(float percent)
        {
            if (percent <= 0 || percent >= 100)
            {
                return;
            }

            MoveSliceYRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void MoveSliceYRpc(float percent)
        {
            SlicingPlane slicingPlane = FindObjectsOfType<SlicingPlane>()[0];
            Debug.Log(slicingPlane);
            float slicePosition = percent / 100.0f - 0.5f;
            slicingPlane.gameObject.transform.localPosition = new Vector3(slicingPlane.gameObject.transform.localPosition.x, slicePosition, slicingPlane.gameObject.transform.localPosition.z);
        }

        public void MoveSliceX(float percent)
        {
            if (percent <= 0 || percent >= 100)
            {
                return;
            }

            MoveSliceXRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void MoveSliceXRpc(float percent)
        {
            SlicingPlane slicingPlane = FindObjectsOfType<SlicingPlane>()[0];
            Debug.Log(slicingPlane);
            float slicePosition = percent / 100.0f - 0.5f;
            slicingPlane.gameObject.transform.localPosition = new Vector3(slicePosition, slicingPlane.gameObject.transform.localPosition.y, slicingPlane.gameObject.transform.localPosition.z);
        }

        public void MoveSliceZ(float percent)
        {
            if (percent <= 0 || percent >= 100)
            {
                return;
            }

            MoveSliceZRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void MoveSliceZRpc(float percent)
        {
            SlicingPlane slicingPlane = FindObjectsOfType<SlicingPlane>()[0];
            Debug.Log(slicingPlane);
            float slicePosition = percent / 100.0f - 0.5f;
            slicingPlane.gameObject.transform.localPosition = new Vector3(slicingPlane.gameObject.transform.localPosition.x, slicingPlane.gameObject.transform.localPosition.y, slicePosition);
        }

        public void RotateSliceX(float percent)
        {
            if (percent <= 0 || percent >= 100)
            {
                return;
            }

            RotateSliceXRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void RotateSliceXRpc(float percent)
        {
            SlicingPlane slicingPlane = FindObjectsOfType<SlicingPlane>()[0];
            Debug.Log(percent);
            float sliceXRotation = percent * 1.8f - 90f;
            slicingPlane.gameObject.transform.localEulerAngles = new Vector3(sliceXRotation, 0f, 0f);
        }

        public void RotateSliceY(float percent)
        {
            if (percent <= 0 || percent >= 100)
            {
                return;
            }

            RotateSliceYRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void RotateSliceYRpc(float percent)
        {
            SlicingPlane slicingPlane = FindObjectsOfType<SlicingPlane>()[0];
            Debug.Log(percent);
            float sliceYRotation = percent * 1.8f - 90f;
            slicingPlane.gameObject.transform.localEulerAngles = new Vector3(0f, sliceYRotation, 0f);
        }

        public void RotateSliceZ(float percent)
        {
            if (percent <= 0 || percent >= 100)
            {
                return;
            }

            RotateSliceZRpc(percent);
        }

        [Rpc(SendTo.Everyone)]
        private void RotateSliceZRpc(float percent)
        {
            SlicingPlane slicingPlane = FindObjectsOfType<SlicingPlane>()[0];
            Debug.Log(percent);
            float sliceZRotation = percent * 1.8f - 90f;
            slicingPlane.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, sliceZRotation);
        }
    }
}
