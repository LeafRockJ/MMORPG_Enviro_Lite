using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using EnviroSamples;

public class EnviroLiteNetPlayer : LiteNetLibBehaviour
{

    public bool assignOnStart = true;
    public bool findSceneCamera = true;

    public GameObject Player;
    public Camera PlayerCamera;

    public override void OnSetup()
    {
        base.OnSetup();
        RegisterNetFunction(Cmd_RequestSeason);
        RegisterNetFunction(Cmd_RequestCurrentWeather);
        RegisterNetFunction<int, int>(RpcRequestCurrentWeather);
        RegisterNetFunction<int>(RpcRequestSeason);

        RegisterNetFunction<string>(Cmd_RequestWeatherChange);      // ADDED
    }

    // Use this for initialization
    void Start()
    {
        // Deactivate if it isn't ours!
        if (!IsOwnerClient && !IsServer)
        {
            this.enabled = false;
            return;
        }

        if (PlayerCamera == null && findSceneCamera)
        {
            StartCoroutine(FindIt());
        }

        if (IsOwnerClient)
        {
            if (assignOnStart && Player != null && PlayerCamera != null)
                EnviroSkyMgr.instance.AssignAndStart(Player, PlayerCamera);

            CallNetFunction(Cmd_RequestSeason, FunctionReceivers.Server);
            CallNetFunction(Cmd_RequestCurrentWeather, FunctionReceivers.Server);
        }
    }

    public IEnumerator FindIt()
    {
        yield return new WaitForSeconds(3);
        PlayerCamera = Camera.main;
    }

    private void Cmd_RequestSeason()
    {
        RpcRequestSeason((int)EnviroSkyMgr.instance.Seasons.currentSeasons);
    }

    private void RpcRequestSeason(int season)
    {
        EnviroSkyMgr.instance.ChangeSeason((EnviroSeasons.Seasons)season);
    }

    public void Cmd_RequestWeatherChange(string id)      // ADDED
    {
        EnviroSkyMgr.instance.ChangeWeather(id);
    }

    public void RequestWeatherChange(string weather)
    {
        CallNetFunction(Cmd_RequestWeatherChange, FunctionReceivers.Server, weather);
    }

    private void Cmd_RequestCurrentWeather()
    {
        for (int i = 0; i < EnviroSkyMgr.instance.Weather.zones.Count; i++)
        {
            for (int w = 0; w < EnviroSkyMgr.instance.Weather.WeatherPrefabs.Count; w++)
            {
                if (EnviroSkyMgr.instance.Weather.WeatherPrefabs[w] == EnviroSkyMgr.instance.Weather.zones[i].currentActiveZoneWeatherPrefab)
                    RpcRequestCurrentWeather(w, i);
            }
        }
    }

    private void RpcRequestCurrentWeather(int weather, int zone)
    {
        EnviroSkyMgr.instance.Weather.zones[zone].currentActiveZoneWeatherPrefab = EnviroSkyMgr.instance.Weather.WeatherPrefabs[weather];
        EnviroSkyMgr.instance.Weather.zones[zone].currentActiveZoneWeatherPreset = EnviroSkyMgr.instance.Weather.WeatherPrefabs[weather].weatherPreset;
    }
}
