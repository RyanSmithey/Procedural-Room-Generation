using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravSimMaster : MonoBehaviour
{
    public int NumSets = 5;
    private BSPGenerator BSPGen;

    public ComputeShader GravSim;
    public ComputeShader AddShader;
    public ComputeShader SetValues;
    public ComputeShader DisplayRC;
    public ComputeShader FinalizeTex;

    public bool Sim = true;

    private ComputeBuffer RoomPieces;
    private ComputeBuffer RoomCenters;

    private RenderTexture _target;
    public RenderTexture Final;

    private RoomCenter[] ValueData;


    int threadGroupsX;
    int threadGroupsY;

    struct RoomPiece
    {
        public int IsActive;
        public float Drag;
        public float Mass;
        public Vector2 Velocity;
        public Vector2 Position;
        public Vector3 Color;
    }
    struct RoomCenter
    {
        public Vector2 Velocity;
        public Vector2 Position;
        public Vector3 Mass;
    }

    void Start()
    {
        BSPGen = gameObject.GetComponent<BSPGenerator>();
        
        threadGroupsX = Mathf.CeilToInt(256 / 8.0f);
        threadGroupsY = Mathf.CeilToInt(256 / 8.0f);

        InitRenderTexture();

        SetComputeBuffers();

        threadGroupsX = Mathf.CeilToInt(256 / 8.0f);
        threadGroupsY = Mathf.CeilToInt(256 / 8.0f);

        SetValues.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void Update()
    {
        if (Sim)
        {
            //FinalizeTex.Dispatch(0, threadGroupsX, threadGroupsY, 1);
            GravSim.SetFloat("DeltaTime", Time.deltaTime * 0.1f);
            DisplayRC.SetFloat("DeltaTime", Time.deltaTime * 0.1f);

            GravSim.Dispatch(0, NumSets * 32, 1, 1);

            DisplayRC.Dispatch(0, Mathf.CeilToInt(BSPGen.TempRooms.Count / 32.0f), 1, 1);
            AddShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            Graphics.Blit(_target, Final);
        }
        else
        {
            GetData();
        }
    }

    private void SetComputeBuffers()
    {
        RoomPiece  RP;
        RoomCenter RC = new RoomCenter();

        List<RoomCenter> RCS = new List<RoomCenter>();

        for (int i = 0; i < BSPGen.TempRooms.Count; i++)
        {
            RC.Mass = Vector3.zero;
            RC.Position = BSPGen.TempRooms[i].Location;
            RC.Velocity = Vector2.zero;
            RCS.Add(RC);
        }

        List<RoomPiece> RPS = new List<RoomPiece>();
        for (int i = 0; i < 32 * NumSets; i++)
        {
            RP = new RoomPiece();

            RP.Drag = -0.05f;
            RP.Velocity = Random.insideUnitCircle * 1.0f;
            RP.Position = new Vector2(Random.Range(-BSPGen.FloorSize.x / 2.0f, BSPGen.FloorSize.x / 2.0f), Random.Range(-BSPGen.FloorSize.y / 2.0f, BSPGen.FloorSize.y / 2.0f));

            RP.Color = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            RP.Mass = 10.0f * BSPGen.TempRooms.Count / (32.0f * NumSets);
            RP.IsActive = 1;

            RPS.Add(RP);
        }
        
        // Assign to compute buffer
        RoomPieces = new ComputeBuffer(RPS.Count, 40);
        RoomCenters = new ComputeBuffer(RCS.Count, 28);

        RoomPieces.SetData(RPS);
        RoomCenters.SetData(RCS);


        GravSim.SetTexture(0, "Result", _target);
        GravSim.SetBuffer(0, "RoomPieces", RoomPieces);
        GravSim.SetBuffer(0, "RoomCenters", RoomCenters);
        GravSim.SetFloat("MinMass", 0.0f);

        DisplayRC.SetBuffer(0, "RoomCenters", RoomCenters);
        FinalizeTex.SetBuffer(0, "RoomCenters", RoomCenters);

        AddShader.SetTexture(0, "Result", _target);
        DisplayRC.SetTexture(0, "Result", _target);
        SetValues.SetTexture(0, "Result", _target);
        FinalizeTex.SetTexture(0, "Result", _target);

        DisplayRC.SetFloat  ("Xsize", BSPGen.FloorSize.x);
        DisplayRC.SetFloat  ("Ysize", BSPGen.FloorSize.y);
        GravSim.SetFloat  ("Xsize", BSPGen.FloorSize.x);
        GravSim.SetFloat  ("Ysize", BSPGen.FloorSize.y);
        FinalizeTex.SetFloat("Xsize", BSPGen.FloorSize.x);
        FinalizeTex.SetFloat("Ysize", BSPGen.FloorSize.y);
    }

    private void InitRenderTexture()
    {
        //Release render texture if we already have one
        if (_target != null) { _target.Release(); }

        // Get a render target for Ray Tracing
        _target = new RenderTexture(256, 256, 0,
            RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        _target.enableRandomWrite = true;
        _target.Create();
    }
    
    private void GetData()
    {
        ValueData = new RoomCenter[BSPGen.TempRooms.Count];

        RoomCenters.GetData(ValueData);
        BSPGen.Colors = new List<Vector3>();

        foreach (RoomCenter RC in ValueData)
        {
            BSPGen.Colors.Add(RC.Mass);
        }
        BSPGen.ReDoRooms = true;
    }
}
