using UnityEngine;

public class RotatePivot180 : MonoBehaviour
{
	private Quaternion Rotation;

	private void Awake()
	{
		Rotation = Quaternion.AngleAxis(180f, Vector3.up);
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		int num = 0;
		while (num < vertices.Length)
		{
			ref Vector3 reference = ref vertices[num];
			reference = Rotation * vertices[num];
			num++;
			mesh.vertices = vertices;
			mesh.RecalculateNormals();
		}
	}
}
