using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleBasedMeshResolver))]
public class MakeCircle : MonoBehaviour
{
    private ParticleBasedMeshResolver resolver;
    private ResolvedMesh rsm;

    private float[] ogPosZ;
    int frameIndex = 0;

    void Start()
    {
        resolver = gameObject.GetComponent<ParticleBasedMeshResolver>();
        rsm = resolver.rsMesh;
        resolver.gizmoOffset = transform.position;

        ogPosZ = new float[rsm.Particles.Count];

        //Spherize
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            rsm.Particles[i].pos = (transform.TransformPoint(rsm.Particles[i].pos).normalized * 5f) - resolver.gizmoOffset;
            ogPosZ[i] = rsm.Particles[i].pos.z;
        }
        resolver.BashUpdateMeshVertices(resolver.gizmoOffset);
    }


}

