using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;

public class VMV : IModApi
{
    public void InitMod(Mod _modInstance)
    {
        Log.Out(" Loading Patch: " + base.GetType().ToString());
        ModEvents.ChatMessage.RegisterHandler(new global::Func<ClientInfo, EChatType, int, string, string, List<int>, bool>(this.ChatMessage));
    }

    public bool ChatMessage(ClientInfo cInfo, EChatType type, int senderId, string msg, string mainName, List<int> recipientEntityIds)
    {
        
        if (!string.IsNullOrEmpty(msg) && cInfo != null)
        {
            if (msg == "/car" || msg == "/dudewheresmycar")
            {
                //update vechicle manager
                VehicleManager.Instance.Save();
                // get vehicles
                List<EntityCreationData> vehicles = VehicleManager.Instance.GetVehicles();
                Dictionary<string, int> messagesToSend = new Dictionary<string, int>();
                bool found = false;
                for (int i = 0; i < vehicles.Count; i++)
                {
                    PlatformUserIdentifierAbs sendingPUIA = GameManager.Instance.getPersistentPlayerID(cInfo);
                    EntityCreationData entityCreationData = vehicles[i];

                    // test
                    Entity entity = GameManager.Instance.World.GetEntity(entityCreationData.id);
                    EntityVehicle entityVehicle = entity as EntityVehicle;
                    PlatformUserIdentifierAbs ownerId = entityVehicle.vehicle.OwnerId;
                    //Log.Out($"[------------------------------------] vehicle owner id: {ownerId}");

                    //Log.Out($"compare: ownerId:{ownerId} sendingPUIA:{sendingPUIA} bool{ownerId == sendingPUIA} string bool:{ownerId.ToString() == sendingPUIA.ToString()}");
                    // found car
                    if (ownerId.ToString() == sendingPUIA.ToString())
                    {

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
                        if (messagesToSend.Count == 0)
                        {
                            string message = string.Format(Localization.Get("car_loc1", false));
                            messagesToSend.Add(message, senderId);
                            //sayToServer(message, senderId);
                        }

                        string message2 = string.Format(Localization.Get("car_loc2", false), carType, location);
                        messagesToSend.Add(message2, senderId);
                        //sayToServer(message2, senderId);
                        found = true;
                    }
                }
                if (!found)
                {
                    string message3 = string.Format(Localization.Get("car_none", false));
                    //sayToServer(message3, senderId);
                    messagesToSend.Add(message3, senderId);
                }
                if (messagesToSend.Count > 0)
                {
                    foreach (var kvp in messagesToSend)
                    {
                        sayToServer(kvp.Key, kvp.Value);
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
