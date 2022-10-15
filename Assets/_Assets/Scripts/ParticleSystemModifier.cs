using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemModifier : MonoBehaviour
{
    [SerializeField] private float minStartSpeed;
    [SerializeField] private float maxStartSpeed;

    [SerializeField] private float minRadius;
    [SerializeField] private float maxRadius;

    [SerializeField] private float minEmission;
    [SerializeField] private float maxEmission;

    private ParticleSystem pSystem;

    // Start is called before the first frame update
    void Start()
    {
        pSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    public void UpdateParticleSystem(float percent)
    {
        pSystem.startSpeed = Utils.RemapPercent(percent, minStartSpeed, maxStartSpeed);

        var shape = pSystem.shape;
        shape.radius = Utils.RemapPercent(percent, minRadius, maxRadius);

        pSystem.emissionRate = Utils.RemapPercent(percent, minEmission, maxEmission);
    }
}
