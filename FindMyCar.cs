using System.Collections.Generic;
using UnityEngine;

public class FindMyCar : IModApi
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
                bool found = false;
                Dictionary<string, int> messagesToSend = new Dictionary<string, int>();
                Dictionary<string, string> carInfo = new Dictionary<string, string>();
                (found, carInfo) = FindMyCarRegular(cInfo);
                if (found)
                {
                    if (messagesToSend.Count == 0)
                    {
                        string message = string.Format(Localization.Get("car_loc1", false));
                        messagesToSend.Add(message, senderId);
                    }
                    foreach (var car in carInfo)
                    {
                        string message = string.Format(Localization.Get("car_loc2", false), car.Key, car.Value);
                        messagesToSend.Add(message, senderId);
                    }
                }
                else
                {
                    string message = string.Format(Localization.Get("car_none", false));
                    sayToServer(message, senderId);
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

    public (bool, Dictionary<string, string>) FindMyCarRegular(ClientInfo cInfo)
    {
        Log.Out($"[FindMyCar] Client {cInfo.playerName} requested vehicle locations");
        List<EntityCreationData> ecd_vehicleList = VehicleManager.Instance.GetVehicleList();
        Dictionary<string, string> carInfo = new Dictionary<string, string>(); //car name, car location string
        Log.Out($"[FindMyCar] Scanning for {cInfo.playerName}'s vehicles");
        for (int i = 0; i < ecd_vehicleList.Count; i++)
        {
            EntityVehicle entityVehicle = EntityFactory.CreateEntity(ecd_vehicleList[i]) as EntityVehicle;
            PlatformUserIdentifierAbs ownerId = entityVehicle.vehicle.OwnerId;
            PlatformUserIdentifierAbs sendingPUIA = GameManager.Instance.getPersistentPlayerID(cInfo);
            if (ownerId.ToString() == sendingPUIA.ToString())
            {
                string carType = EntityClass.GetEntityClassName(ecd_vehicleList[i].entityClass);
                Log.Out($"[FindMyCar] Located {cInfo.playerName}'s {carType} at location: {ecd_vehicleList[i].pos.ToCultureInvariantString()}");
                string coords = MapCoords(ecd_vehicleList[i].pos);
                carInfo.Add(carType, coords);
            }
            EntityVehicle.Destroy(entityVehicle);
        }
        if (carInfo.Count > 0)
        {
            return (true, carInfo);
        }
        else
        {
            Log.Out($"[FindMyCar] Failed to Located {cInfo.playerName}'s car. Player does not have a car");
            return (false, carInfo);
        }
    }

    public string MapCoords(Vector3 location)
    {
        var x = (int)location.x;
        var z = (int)location.z;
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
        return $"{x} {xdir}, {z} {zdir}";
    }

    private void sayToServer(string msg, int playerId)
    {
        List<int> reciept = new List<int>();
        reciept.Add(playerId);
        GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, msg, reciept, EMessageSender.None);
    }
}