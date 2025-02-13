using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

public class DMV_Init : IModApi
{
    //private string serverChatName = "Server";
    //public static List<EntityCreationData> OnlinePlayersCarList = new List<EntityCreationData>();
    //public static List<EntityVehicle> OnlinePlayersCarList = new List<EntityVehicle>();

    public void InitMod(Mod _modInstance)
    {
        Log.Out(" Loading Patch: " + base.GetType().ToString());
        ModEvents.ChatMessage.RegisterHandler(new global::Func<ClientInfo, EChatType, int, string, string, List<int>, bool>(this.ChatMessage));
        //Harmony harmony = new Harmony(base.GetType().ToString());
        //harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    // check messages to see if someone requested drone location
    public bool ChatMessage(ClientInfo cInfo, EChatType type, int senderId, string msg, string mainName, List<int> recipientEntityIds)
    {

        //LO("Chat message detected");
        if (!string.IsNullOrEmpty(msg) && cInfo != null)
        {

            //PlatformUserIdentifierAbs sendingPUIA = GameManager.Instance.getPersistentPlayerID(cInfo);
            //Log.Out($"[-----this user send chat -------------] {sendingPUIA}");

            //this works,

            //LO("Chat message detected");
            if (msg == "/car")
            {
                //List<EntityCreationData> OnlinePlayersCarList = new List<EntityCreationData>();
                // Clear vehicle list
                //OnlinePlayersCarList.Clear();
                //update vechicle manager
                VehicleManager.Instance.Save();
                // get vehicles
                List<EntityCreationData> vehicles = VehicleManager.Instance.GetVehicles();
                // iterate vehicles and update 
                for (int i = 0; i < vehicles.Count; i++)
                {
                    //var a = vehicles[i] as Vehicle;
                    // Local chat message data
                    //OnlinePlayersCarList.Add(vehicles[i]);


                    PlatformUserIdentifierAbs sendingPUIA = GameManager.Instance.getPersistentPlayerID(cInfo);
                    Log.Out($"[-----this user send chat -------------] {sendingPUIA}");

                    // Server Console changes
                    EntityCreationData entityCreationData = vehicles[i];

                    // test
                    Entity entity = GameManager.Instance.World.GetEntity(entityCreationData.id);
                    EntityVehicle entityVehicle = entity as EntityVehicle;
                    PlatformUserIdentifierAbs ownerId = entityVehicle.vehicle.OwnerId;
                    Log.Out($"[------------------------------------] vehicle owner id: {ownerId}");

                    bool found = false;
                    //Log.Out($"compare: ownerId:{ownerId} sendingPUIA:{sendingPUIA} bool{ownerId == sendingPUIA} string bool:{ownerId.ToString() == sendingPUIA.ToString()}");
                    // found car
                    if (ownerId.ToString() == sendingPUIA.ToString())
                    {
                        //onlineDrone.pos.ToCultureInvariantString();
                        var x = (int)entityCreationData.pos.x;
                        var z = (int)entityCreationData.pos.z;
                        string xdir = "";
                        string zdir = "";
                        if (x < 0)
                        {
                            // west
                            xdir = "W";
                            x *= -1;
                        }
                        else
                        {
                            // east
                            xdir = "E";
                        }
                        if (z < 0)
                        {
                            // south
                            zdir = "S";
                            z *= -1;
                        }
                        else
                        {
                            // north
                            zdir = "N";
                        }
                        string carType = EntityClass.GetEntityClassName(entityCreationData.entityClass);
                        string location = $"{x} {xdir}, {z} {zdir}";
                        string message = string.Format(Localization.Get("car_loc", false), carType, location);
                        sayToServer(message, senderId);
                        found = true;
                    }
                    if (!found)
                    {
                        string message = string.Format(Localization.Get("car_none", false));
                        sayToServer(message, senderId);
                    }
                }
                return false;
            }
            return true;
        }
        return true;
    }
    private void sayToServer(string msg, int playerId)
    {
        List<int> reciept = new List<int>();
        reciept.Add(playerId);
        GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, msg, reciept, EMessageSender.None);
    }

}
