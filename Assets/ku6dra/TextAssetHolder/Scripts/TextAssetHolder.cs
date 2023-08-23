using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TextAssetHolder : UdonSharpBehaviour
{
    public TextAsset[] Data;
}
