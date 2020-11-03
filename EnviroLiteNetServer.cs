using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnviroSamples;
using MultiplayerARPG;
using LiteNetLib;
using LiteNetLibManager;
namespace MultiplayerARPG
{
    public class EnviroLiteNetServer : LiteNetLibBehaviour
    {

        public float updateSmoothing = 15f;
        public int zoneID;
        public int weatherID = 0;
        [SerializeField]
        public LiteNetLibSyncField<float> networkHours = new LiteNetLibSyncField<float>();
        [SerializeField]
        public LiteNetLibSyncField<int> networkDays = new LiteNetLibSyncField<int>();
        [SerializeField]
        public LiteNetLibSyncField<int> networkYears = new LiteNetLibSyncField<int>();

        public bool isHeadless = true;

        public override void OnSetup()
        {
            base.OnSetup();
            if (!isHeadless)
            {
                EnviroSkyMgr.instance.StartAsServer();
            }

            EnviroSkyMgr.instance.Weather.updateWeather = true;

            RegisterNetFunction<EnviroSeasons.Seasons>(SendSeasonToClient);
            RegisterNetFunction<int, int>(SendWeatherToClient);

            EnviroSkyMgr.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) =>
            {
                CallNetFunction(SendSeasonToClient, FunctionReceivers.All, season);
            };
            EnviroSkyMgr.instance.OnZoneWeatherChanged += (EnviroWeatherPreset type, EnviroZone zone) =>
            {
                for (int i = 0; i < EnviroSkyMgr.instance.Weather.zones.Count; i++)
                {
                    for (int w = 0; w < EnviroSkyMgr.instance.Weather.WeatherPrefabs.Count; w++)
                    {
                        if (EnviroSkyMgr.instance.Weather.WeatherPrefabs[w] == EnviroSkyMgr.instance.Weather.zones[i].currentActiveZoneWeatherPrefab) {
                            CallNetFunction(SendWeatherToClient, FunctionReceivers.All, w, i);
                        }

                    }
                }
                
            };

            RegisterNetFunction<int>(RpcSeasonUpdate);
            RegisterNetFunction<int, int>(RpcWeatherUpdate);

        }

        // Use this for initialization
        public void Start()
        {

        }

        void SendWeatherToClient(int w, int z)
        {           
            RpcWeatherUpdate(w, z);

        }

        void SendSeasonToClient(EnviroSeasons.Seasons s)
        {
            RpcSeasonUpdate((int)s);
        }

        void RpcSeasonUpdate(int season)
        {
            EnviroSkyMgr.instance.ChangeSeason((EnviroSeasons.Seasons)season);
        }

        void RpcWeatherUpdate(int weather, int zone)
        {
            EnviroSkyMgr.instance.Weather.zones[zone].currentActiveZoneWeatherPrefab = EnviroSkyMgr.instance.Weather.WeatherPrefabs[weather];
            EnviroSkyMgr.instance.Weather.zones[zone].currentActiveZoneWeatherPreset = EnviroSkyMgr.instance.Weather.WeatherPrefabs[weather].weatherPreset;
        }

        // Update is called once per frame
        void Update()
        {
            if (EnviroSkyMgr.instance == null)
                return;

            if (!IsServer)
            {

                if (networkHours.Value < 1f && EnviroSkyMgr.instance.GetUniversalTimeOfDay() > 23f)
                    EnviroSkyMgr.instance.SetTimeOfDay(networkHours.Value);

                EnviroSkyMgr.instance.SetTimeOfDay(Mathf.Lerp(EnviroSkyMgr.instance.GetUniversalTimeOfDay(), networkHours.Value, Time.deltaTime * updateSmoothing));
                EnviroSkyMgr.instance.Time.ProgressTime = EnviroTime.TimeProgressMode.Simulated;

                

            }

            networkHours.Value = EnviroSkyMgr.instance.GetUniversalTimeOfDay();
            EnviroSkyMgr.instance.OnZoneWeatherChanged += (EnviroWeatherPreset type, EnviroZone zone) =>
            {
                for (int i = 0; i < EnviroSkyMgr.instance.Weather.zones.Count; i++)
                {
                    for (int w = 0; w < EnviroSkyMgr.instance.Weather.WeatherPrefabs.Count; w++)
                    {
                        if (EnviroSkyMgr.instance.Weather.WeatherPrefabs[w] == EnviroSkyMgr.instance.Weather.zones[i].currentActiveZoneWeatherPrefab)
                        {
                            CallNetFunction(SendWeatherToClient, FunctionReceivers.All, w, i);
                        }

                    }
                }
            };
        }
    }
}