using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleConstraintBase
{
    public abstract void Apply(ResolvedMesh rsm, float mass);
}


public class DistanceConstraint : ParticleConstraintBase
{
    private MeshParticleEdge edge;
    private float restLength;
    private float compressionStiffness, stretchStiffness;

    public DistanceConstraint(MeshParticleEdge e, float compressionStiffness, float stretchStiffness)
    {
        edge = e;
        this.compressionStiffness = compressionStiffness;
        this.stretchStiffness = stretchStiffness;

        restLength = e.originalDistance;
    }

    public override void Apply(ResolvedMesh rsm, float mass)
    {
        //get positions
        Vector3 pi = edge.originatorParticle.projectedPos; //projectedPositions[edge.startIndex];
        Vector3 pj = edge.connectedParticle.projectedPos;

        //make edge vector
        Vector3 n = pi - pj;

        //get current length
        float d = n.magnitude;

        //normalize edge vector
        n.Normalize();

        float wi = mass;
        float wj = mass;
        float stiffness = d < restLength ? compressionStiffness : stretchStiffness;

        if(!edge.originatorParticle.isAnchored)
            edge.originatorParticle.projectedPos = pi - stiffness * wi
                                              / (wi + wj) * (d - restLength) * n;

        if (!edge.connectedParticle.isAnchored)
            edge.connectedParticle.projectedPos = pj + stiffness * wj
                                              / (wi + wj) * (d - restLength) * n;
    }
}
