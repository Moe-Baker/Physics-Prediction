using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Default
{
	public class ProjectileShooter : MonoBehaviour
	{
		[SerializeField]
		GameObject prefab = default;

		[SerializeField]
		ForceData force = new ForceData(Vector3.forward * 50 + Vector3.up * 3, ForceMode.VelocityChange);
		[Serializable]
		public struct ForceData
        {
			[SerializeField]
            Vector3 vector;
            public Vector3 Vector => vector;

			[SerializeField]
            ForceMode mode;
            public ForceMode Mode => mode;

            public ForceData(Vector2 vector, ForceMode mode)
            {
				this.vector = vector;
				this.mode = mode;
            }
        }

		[SerializeField]
		PredectionProperty prediction = default;
		[Serializable]
		public class PredectionProperty
        {
			[SerializeField]
			int iterations = 40;
			public int Iterations => iterations;

			[SerializeField]
            LineRenderer line = default;
            public LineRenderer Line => line;
        }

		public const KeyCode Key = KeyCode.Mouse0;

		Transform InstanceContainer;

        void Start()
        {
			InstanceContainer = new GameObject("Projectiles Container").transform;
		}

        void Update()
        {
			LookAtMouse();

			Shoot();
		}

        void LookAtMouse()
        {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out var hit))
            {
				var direction = hit.point - transform.position;

				var target = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, target, 240f * Time.deltaTime);
			}
        }

		void Shoot()
		{
			if (Input.GetKeyUp(Key))
			{
				prediction.Line.positionCount = 0;

				var instance = Instantiate(prefab).GetComponent<Rigidbody>();
				instance.transform.SetParent(InstanceContainer);
				Shoot(instance);

				TrajectoryPredictionDrawer.Hide();
			}
		}

		void Shoot(GameObject gameObject)
        {
			var rigidbody = gameObject.GetComponent<Rigidbody>();

			Shoot(rigidbody);
        }
		void Shoot(Rigidbody rigidbody)
		{
			var relativeForce = transform.TransformVector(force.Vector);

			rigidbody.AddForce(relativeForce, force.Mode);

			rigidbody.transform.position = transform.position;
			rigidbody.transform.rotation = transform.rotation;
		}

		void FixedUpdate()
        {
			Predict();
		}

		void Predict()
        {
			if (Input.GetKey(Key))
			{
				PredictionSystem.Start(prediction.Iterations);

				var points = PredictionSystem.RecordPrefab(prefab, Shoot);

				PredictionSystem.Simulate();

				prediction.Line.positionCount = points.Length;
				prediction.Line.SetPositions(points);
			}
		}
	}
}