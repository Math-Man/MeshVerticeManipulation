using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//Very simple cloth physics based on particlebasedmeshresolver.cs
//Uses a projected position system instead of relying solely on velocity
[RequireComponent(typeof(ParticleBasedMeshResolver))]
public class ClothApplicator : MonoBehaviour
{
    private ParticleBasedMeshResolver resolver;
    private ResolvedMesh rsm;
    [SerializeField] private List<int> anchoredVertices;

    public float timestep = 0.02f;
    public int iterationNum = 5;
    public float vertexMass = 2;
    public float stetchyness = 0.8f;
    public float pulliness = 0.8f;
    public Vector3 gravity;

    List<ParticleConstraintBase> constraints;
    private float nextFrameTime = 0f;

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
        transform.position = Vector3.zero;
        constraints = new List<ParticleConstraintBase>();
    }

    private void Start()
    {
        resolver = gameObject.GetComponent<ParticleBasedMeshResolver>();
        rsm = resolver.rsMesh;
        resolver.gizmoOffset = startPos;

        UpdateAnchoredParticles();
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            rsm.Particles[i].pos = transform.TransformPoint(rsm.Particles[i].pos);
        }

        AddDistanceConstraints();
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            rsm.Particles[i].pos = transform.InverseTransformPoint(rsm.Particles[i].pos);
        }
    }

    private void Update()
    {
        // calculate the timestep 
        nextFrameTime += Time.deltaTime;
        int iter = 0;
        while (nextFrameTime > 0)
        {
            if (nextFrameTime < timestep)
            {
                break;
            }

            float dt = Mathf.Min(nextFrameTime, timestep);
            nextFrameTime -= dt;
            iter++;

            ApplyExternalForce(gravity, dt);
            ApplyExplicitEuler(dt);

            for (int j = 0; j < iterationNum; j++)
            {
                constraints = constraints.OrderBy(a => Guid.NewGuid()).ToList();
                for (int i = 0; i < constraints.Count; i++)
                {
                    constraints[i].Apply(rsm, 1.0f / vertexMass);
                }
            }
            UpdateVertices(dt);
            ApplyFriction();
        }
        resolver.BashUpdateMeshVertices(startPos);
    }

    public void UpdateAnchoredParticles()
    {
        foreach (int verticeIndex in anchoredVertices)
        {
            rsm.ParticleIndeceDirectory[verticeIndex].isAnchored = true;
        }
    }

    private void AddDistanceConstraints()
    {
        foreach (MeshParticleEdge e in rsm.UniqueEdges)
        {
            constraints.Add(new DistanceConstraint(e, stetchyness, pulliness));
        }
    }

    private void ApplyExternalForce(Vector3 gravity, float dt)
    {
        var invMass = 1.0f / vertexMass;
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            rsm.Particles[i].body.Velocity +=  (gravity) * invMass * dt;
        }
    }

    private void UpdateVertices(float dt)
    {
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            if (!rsm.Particles[i].isAnchored)
                rsm.Particles[i].body.Velocity = (rsm.Particles[i].projectedPos - rsm.Particles[i].pos) / dt;
            if (!rsm.Particles[i].isAnchored)
                rsm.Particles[i].pos = rsm.Particles[i].projectedPos;
        }
    }

    private void ApplyExplicitEuler(float dt)
    {
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            if(!rsm.Particles[i].isAnchored)
                rsm.Particles[i].projectedPos = rsm.Particles[i].pos + rsm.Particles[i].body.Velocity * dt;
        }
    }

    private void ApplyFriction()
    {
        // do simple air resistance by reducing speed of every vertex
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            rsm.Particles[i].body.Velocity *= 0.998f;
        }

    }

}
