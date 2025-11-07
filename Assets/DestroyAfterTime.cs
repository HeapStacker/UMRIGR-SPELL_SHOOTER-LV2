// ako želiš, kreiraj EarthImpactAutoDestroy.cs i dodaj:
using UnityEngine;
public class EarthImpactAutoDestroy : MonoBehaviour
{
    public float life = 1.0f;
    void Start() { Destroy(gameObject, life); }
}