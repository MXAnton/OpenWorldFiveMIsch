using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct TextureAlphamap
{
    public string typeOfMaterial;
}

public class TerrainTextureAlphamapController : MonoBehaviour
{
    public TextureAlphamap[] textureAlphamaps;
}
