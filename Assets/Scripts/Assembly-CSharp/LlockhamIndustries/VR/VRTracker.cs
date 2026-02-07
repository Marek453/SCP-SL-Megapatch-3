using UnityEngine;
using UnityEngine.XR;

namespace LlockhamIndustries.VR
{
	[RequireComponent(typeof(Rigidbody))]
	public class VRTracker : MonoBehaviour
	{
		public enum VRTarget
		{
			LeftHand = 0,
			RightHand = 1,
			Head = 2
		}

		public VRPlayspace playspace;

		public VRTarget target;

		public Vector3 offset;

		private Rigidbody rb;

		private XRNode node;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
			rb.isKinematic = true;
			switch (target)
			{
			case VRTarget.Head:
				node = XRNode.Head;
				break;
			case VRTarget.LeftHand:
				node = XRNode.LeftHand;
				break;
			case VRTarget.RightHand:
				node = XRNode.RightHand;
				break;
			}
		}

		private void FixedUpdate()
		{
			Quaternion quaternion = base.transform.parent.rotation * InputTracking.GetLocalRotation(node);
			if (target == VRTarget.Head)
			{
				Vector3 eulerAngles = quaternion.eulerAngles;
				eulerAngles.x = 0f;
				eulerAngles.z = 0f;
				quaternion = Quaternion.Euler(eulerAngles);
			}
			Vector3 vector = base.transform.parent.position + InputTracking.GetLocalPosition(node) + quaternion * offset;
			if (playspace != null)
			{
				vector = playspace.ClampNode(vector);
			}
			rb.MoveRotation(quaternion);
			rb.MovePosition(vector);
		}
	}
}
