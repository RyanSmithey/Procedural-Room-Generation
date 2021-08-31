using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public GameObject FloorHolder;
    public GameObject CeilingHolder;
    public bool Generate = false;
    private Mesh mesh;
    private Mesh CeilingMesh;


    void OnDrawGizmos()
    {
        if (Generate)
        {
            Generate = false;

            GenFloors();
            GenCeilings();
        }
    }

    private void GenFloors()
    {
        mesh = new Mesh();

        List<Vector3> Verticies = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Color> Colors = new List<Color>();
        List<Vector2> UVS = new List<Vector2>();
        List<Vector3> Normals = new List<Vector3>();

        Color color;

        int Index = 0;
        foreach (AbstractGen.Room Iroom in AbstractGen.AllRooms)
        {
            Verticies.Add(new Vector3(Iroom.Location.x, 0, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, 0, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x, 0, Iroom.Location.y + Iroom.Size.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, 0, Iroom.Location.y + Iroom.Size.y));

            UVS.Add(new Vector2(0, 0));
            UVS.Add(new Vector2(Iroom.Size.x, 0));
            UVS.Add(new Vector2(0, Iroom.Size.y));
            UVS.Add(new Vector2(Iroom.Size.x, Iroom.Size.y));

            Normals.Add(Vector3.up);
            Normals.Add(Vector3.up);
            Normals.Add(Vector3.up);
            Normals.Add(Vector3.up);

            Triangles.Add(Index + 2);
            Triangles.Add(Index + 1);
            Triangles.Add(Index);

            Triangles.Add(Index + 1);
            Triangles.Add(Index + 2);
            Triangles.Add(Index + 3);

            color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
            for (int i = 0; i < 4; i++)
            {
                Colors.Add(color);
            }
            Index += 4;
        }


        mesh.vertices = Verticies.ToArray();
        mesh.triangles = Triangles.ToArray();
        //mesh.normals = Normals.ToArray();
        mesh.colors = Colors.ToArray();
        mesh.uv = UVS.ToArray();
        mesh.RecalculateNormals();

        FloorHolder.GetComponent<MeshFilter>().mesh = mesh;
    }

    private void GenCeilings()
    {
        CeilingMesh = new Mesh();

        List<Vector3> Verticies = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Color> Colors = new List<Color>();
        List<Vector2> UVS = new List<Vector2>();
        List<Vector3> Normals = new List<Vector3>();

        Color color;

        int Index = 0;
        foreach (AbstractGen.Room Iroom in AbstractGen.AllRooms)
        {
            Verticies.Add(new Vector3(Iroom.Location.x               , GeometryGeneration.RoomHeight, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, GeometryGeneration.RoomHeight, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x               , GeometryGeneration.RoomHeight, Iroom.Location.y + Iroom.Size.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, GeometryGeneration.RoomHeight, Iroom.Location.y + Iroom.Size.y));

            UVS.Add(new Vector2(0, 0));
            UVS.Add(new Vector2(Iroom.Size.x, 0));
            UVS.Add(new Vector2(0, Iroom.Size.y));
            UVS.Add(new Vector2(Iroom.Size.x, Iroom.Size.y));

            Normals.Add(-Vector3.up);
            Normals.Add(-Vector3.up);
            Normals.Add(-Vector3.up);
            Normals.Add(-Vector3.up);

            Triangles.Add(Index);
            Triangles.Add(Index + 1);
            Triangles.Add(Index + 2);

            Triangles.Add(Index + 3);
            Triangles.Add(Index + 2);
            Triangles.Add(Index + 1);

            color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
            for (int i = 0; i < 4; i++)
            {
                Colors.Add(color);
            }
            Index += 4;
        }


        CeilingMesh.vertices = Verticies.ToArray();
        CeilingMesh.triangles = Triangles.ToArray();
        //mesh.normals = Normals.ToArray();
        CeilingMesh.colors = Colors.ToArray();
        CeilingMesh.uv = UVS.ToArray();
        CeilingMesh.RecalculateNormals();

        CeilingHolder.GetComponent<MeshFilter>().mesh = CeilingMesh;
    }
}
