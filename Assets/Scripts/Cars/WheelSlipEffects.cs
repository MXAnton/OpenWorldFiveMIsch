using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct TextureSmoke
{
    public string typeOfMaterial;
    public Color smokeColor;
}

public class WheelSlipEffects : MonoBehaviour
{
    AudioSourceController audioController;
    ParticleSystem wheelSmokeParticleSystem;
    public WheelCollider wheelCol;
    CarController carController;

    public float wheelRadius;
    public float wheelSpeed;
    float sidewaysSlip;
    float forwardSlip;

    public GameObject skidTrailPrefab;

    WheelHit hit;

    [Header("Tire Smoke Vars")]
    public TerrainData mTerrainData;
    public int alphamapWidth;
    public int alphamapHeight;

    public float[,,] mSplatmapData;
    public int mNumTextures;

    public TextureSmoke[] textureSmokes;
    public Color normalTireSmokeColor;
    public Color wantedTireSmokeColor;
    public bool overrideTireSmokeColor;

    void Start()
    {
        GetTerrainProps();

        audioController = GetComponent<AudioSourceController>();
        wheelSmokeParticleSystem = GetComponent<ParticleSystem>();
        carController = GetComponentInParent<CarController>();
    }

    void Update()
    {
        if (wheelCol.GetGroundHit(out hit))
        {
            UpdateSlipValue();

            WheelSkidSound();
            WheelSmokes();
            WheelSkidTrail();
        }
    }

    void UpdateSlipValue()
    {
        float newSidewaysSlip = hit.sidewaysSlip;
        if (newSidewaysSlip < 0)
        {
            newSidewaysSlip = -newSidewaysSlip;
        }
        sidewaysSlip = newSidewaysSlip;

        float newForwardSlip = hit.forwardSlip;
        if (newForwardSlip < 0)
        {
            newForwardSlip = -newForwardSlip;
        }
        forwardSlip = newForwardSlip;
    }

    void WheelSkidSound()
    {
        float newVolume = 0;
        float newPitch = 0;

        if (sidewaysSlip > 0.3f)
        {
            newVolume += sidewaysSlip;
            newPitch += sidewaysSlip;
        }
        if (forwardSlip > 0.4f)
        {
            newVolume += forwardSlip;
            newPitch += forwardSlip;
        }

        audioController.SetVolume(newVolume / 2);
        audioController.SetPitch(newPitch / 2);
    }

    void WheelSkidTrail()
    {
        float slipRate = (sidewaysSlip + forwardSlip) / 2;

        if (slipRate > 0.2f && transform.childCount == 0)
        {
            Instantiate(skidTrailPrefab, new Vector3(transform.position.x, hit.point.y, transform.position.z), Quaternion.identity, transform);
        }
        else if (slipRate < 0.2f && transform.childCount > 0)
        {
            transform.GetChild(0).transform.parent = null;
        }
    }


    void WheelSmokes()
    {
        float slipRate = (sidewaysSlip + forwardSlip) / 2;
        wheelSmokeParticleSystem.emissionRate = slipRate * carController.wheelSmokeRate;

        float newStartAlpha = -0.2f;
        newStartAlpha += slipRate;
        if (newStartAlpha < 0.2f)
        {
            newStartAlpha = 0.2f;
        }


        Color color = wantedTireSmokeColor;

        if (!overrideTireSmokeColor)
        {
            color = GetColorFromGround();
        }



        wheelSmokeParticleSystem.startColor = new Color(color.r, color.g, color.b, newStartAlpha);
    }


    private void GetTerrainProps()
    {
        mTerrainData = Terrain.activeTerrain.terrainData;
        alphamapWidth = mTerrainData.alphamapWidth;
        alphamapHeight = mTerrainData.alphamapHeight;

        mSplatmapData = mTerrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
        mNumTextures = mSplatmapData.Length / (alphamapWidth * alphamapHeight);
    }

    private Vector3 ConvertToSplatMapCoordinate(Vector3 playerPos)
    {
        Vector3 vecRet = new Vector3();
        Terrain ter = Terrain.activeTerrain;
        Vector3 terPosition = ter.transform.position;
        vecRet.x = ((playerPos.x - terPosition.x) / ter.terrainData.size.x) * ter.terrainData.alphamapWidth;
        vecRet.z = ((playerPos.z - terPosition.z) / ter.terrainData.size.z) * ter.terrainData.alphamapHeight;
        return vecRet;
    }

    private int GetActiveTerrainTextureIdx(Vector3 pos)
    {
        Vector3 TerrainCord = ConvertToSplatMapCoordinate(pos);
        int ret = 0;
        float comp = 0f;
        for (int i = 0; i < mNumTextures; i++)
        {
            if (comp < mSplatmapData[(int)TerrainCord.z, (int)TerrainCord.x, i])
                ret = i;
        }
        return ret;
    }

    public int GetTerrainAtPosition(Vector3 pos)
    {
        int terrainIdx = GetActiveTerrainTextureIdx(pos);
        return terrainIdx;
    }


    Color GetTextureSmokeColor(string material)
    {
        foreach (TextureSmoke textureSmoke in textureSmokes)
        {
            if (textureSmoke.typeOfMaterial == material)
            {
                return textureSmoke.smokeColor;
            }
        }

        return normalTireSmokeColor;
    }

    Color GetColorFromGround()
    {
        Color newColor;

        RaycastHit raycastHit;
        Physics.Raycast(transform.position, -transform.up, out raycastHit);

        bool test = raycastHit.collider.TryGetComponent<MeshRenderer>(out MeshRenderer meshRendererHit);
        if (test)
        {
            Renderer renderer = raycastHit.collider.GetComponent<MeshRenderer>();
            if (renderer.material.mainTexture as Texture2D)
            {
                Texture2D texture2D = renderer.material.mainTexture as Texture2D;
                Vector2 pCoord = raycastHit.textureCoord;
                pCoord.x *= texture2D.width;
                pCoord.y *= texture2D.height;

                Vector2 tiling = renderer.material.mainTextureScale;
                newColor = texture2D.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x), Mathf.FloorToInt(pCoord.y * tiling.y));

                return newColor;
            }
        }
        else if (raycastHit.collider.GetComponent<TerrainCollider>())
        {
            int terrainTextureId = GetTerrainAtPosition(hit.point);

            string material = raycastHit.collider.GetComponent<TerrainTextureAlphamapController>().textureAlphamaps[terrainTextureId].typeOfMaterial;

            newColor = GetTextureSmokeColor(material);

            return newColor;
        }

        return normalTireSmokeColor;
    }
}
