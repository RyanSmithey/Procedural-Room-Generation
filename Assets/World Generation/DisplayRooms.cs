using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DisplayRooms : MonoBehaviour
{
    [SerializeField]
    private BSPGenerator BSP;

    public bool Generate = false;
    public bool Reset = false;

    public int MaxRooms = 50;
    public int PossibleRooms = 100;
    public float[] RoomDimensions = new float[] { 1f, 2f, 1f, 2f };//MinW, MaxW, MinL, MaxL
    public float[] HouseFloorSize = new float[] { 20f, 20f }; //MaxX, MaxY
    public float DoorWidth = 0.6f;


    private List<int[]> RoomConnections;

    private int StartRoom;
    void OnDrawGizmos()
    {
        //Generate Initial Structure
        if (Generate)
        {
            Generate = false;

            //Set Generaation Values
            AbstractGen.PossibleRooms = PossibleRooms;
            AbstractGen.MaxRooms = 100000;
            AbstractGen.RoomDimensions = RoomDimensions;
            AbstractGen.DoorWidth = DoorWidth;

            AbstractGen.ShrinkRooms();
            AbstractGen.FindRoomConnections();
            AbstractGen.AddConnectionInformationToRooms();
            StartRoom = Mathf.RoundToInt(Random.Range(-0.49f, AbstractGen.AllRooms.Count + 0.49f));
            AbstractGen.CullUnconnectedRooms(StartRoom);

            AbstractGen.Sort(0, ref StartRoom);
            AbstractGen.FindRoomConnections();
            AbstractGen.AddConnectionInformationToRooms();

            AbstractGen.FindDoors();
            AbstractGen.AddPathways();
        }

        if (Reset)
        {
            Reset = false;
            AbstractGen.RoomConnections = null;
            AbstractGen.AllRooms = null;
        }

        //Draw Rooms
        DrawRooms();

        //Draw Doors
        if (AbstractGen.RoomConnections != null)
        {
            DrawConnections();

            foreach (AbstractGen.Room IndividualRoom in AbstractGen.AllRooms)
            {
                if (IndividualRoom.DebugPoints != null)
                {
                    foreach (Vector2 DebugPoint in IndividualRoom.DebugPoints)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(new Vector3(DebugPoint.x, DebugPoint.y, 1.0f), Vector3.one / 2);
                    }
                }
                
                foreach (Vector2[] Door in IndividualRoom.Doors)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(new Vector3(Door[0].x, Door[0].y, 0.5f), new Vector3(Door[1].x, Door[1].y, 0.5f));
                }
                
                if (IndividualRoom.Pathways != null)
                {
                    foreach (AbstractGen.Box B in IndividualRoom.Pathways)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(new Vector3(B.Location.x, 0.5f, B.Location.y), new Vector3(B.Size.x, 0.5f, B.Size.y));
                    }
                }
            }
        }

    }

    void DrawRooms()
    {
        if (AbstractGen.AllRooms == null)
        {
            return;
        }

        foreach (AbstractGen.Room IndividualRoom in AbstractGen.AllRooms)
        {
            Vector3 Center = new Vector3(IndividualRoom.Location.x + 0.5f * IndividualRoom.Size.x, 0.0f, IndividualRoom.Location.y + 0.5f * IndividualRoom.Size.y);
            Vector3 Size = new Vector3(IndividualRoom.Size.x, 0.0f, IndividualRoom.Size.y);

            if (IndividualRoom.Index == StartRoom)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(Center, Size);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(Center, Size);
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Center, Size);
        }
    }

    void DrawConnections()
    {
        Gizmos.color = Color.cyan;
        foreach (AbstractGen.Room IRoom in AbstractGen.AllRooms)
        {
            Vector2 C1 = IRoom.Location + IRoom.Size / 2.0f;

            foreach (int x in IRoom.Connections)
            {
                Vector2 C2 = AbstractGen.AllRooms[x].Location + AbstractGen.AllRooms[x].Size / 2.0f;

                Gizmos.DrawLine(new Vector3(C1.x, 0.1f, C1.y), new Vector3(C2.x, 0.1f, C2.y));
            }
        }
    }
}
