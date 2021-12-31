using System.Collections.Generic;
using UnityEngine;

namespace JPBotelho
{
	[ExecuteInEditMode]
	public class SplineTester : MonoBehaviour
	{
		public CatmullRom spline;

		public Transform[] controlPoints;

		[Range(2, 25)]
		public int resolution;
		public bool closedLoop;

		[Range(0, 20)]
		public float normalExtrusion;

		[Range(0, 20)]
		public float tangentExtrusion;

		public bool drawNormal, drawTangent;

		void Start()
		{
			if(spline == null)
			{
				spline = new CatmullRom(controlPoints, resolution, closedLoop);
			}
		}

		void Update()
		{
			if(spline != null)
			{
				spline.Update(controlPoints);
				spline.Update(resolution, closedLoop);
				spline.DrawSpline(Color.white);

				if(drawNormal)
					spline.DrawNormals(normalExtrusion, Color.red);

				if(drawTangent)
					spline.DrawTangents(tangentExtrusion, Color.cyan);
			}
			else
			{
				spline = new CatmullRom(controlPoints, resolution, closedLoop);
			}
		}
	}
}