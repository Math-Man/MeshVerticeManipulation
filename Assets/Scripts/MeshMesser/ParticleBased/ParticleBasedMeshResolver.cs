using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates the resolved mesh
/// </summary>
public class ParticleBasedMeshResolver : MonoBehaviour
{
    [SerializeField] private bool DrawGizmos = true;
    [SerializeField] private bool ShowIndeceIndicators = true;
    [SerializeField] private bool ShowOnlyUniqueEdges = true;
    [SerializeField] private bool ShowProjectedPositions = true;
    [SerializeField] private bool UseParallelProcessing = false;
    [Tooltip("Enabling This can be very Taxing")][SerializeField] private bool AllowDynamicColliders = false;
    public ResolvedMesh rsMesh;

    private MeshCollider meshCollider;
    public Vector3 gizmoOffset;

    private void Awake()
    {
        if (gizmoOffset == null)
            gizmoOffset = Vector3.zero;

        //Force use of mesh collider
        if (AllowDynamicColliders) 
        {
            var collider = GetComponent<Collider>();
            if (!(typeof(MeshCollider).IsInstanceOfType(collider)))
            {
                DestroyImmediate(collider);
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            else
            {
                meshCollider = (MeshCollider)collider;
            }
        }


        rsMesh = new ResolvedMesh(GetComponent<MeshFilter>().mesh);
        RecalculateParticles();

        rsMesh.mesh.MarkDynamic();
    }


    public void ResolveParticles()
    {
        Vector3[] vertices = (Vector3[])rsMesh.mesh.vertices.Clone();
        int[] triangles = (int[])rsMesh.mesh.triangles.Clone();

        for (int v = 0; v < triangles.Length; v++)
        {
            int verticeIndex = triangles[v];
            Vector3 verticePosition = transform.InverseTransformPoint(vertices[verticeIndex]);

            bool matchingParticleFound = false;
            if (UseParallelProcessing)
            {
                Parallel.ForEach(rsMesh.Particles, (mp, state) =>
                {
                    if ((verticePosition - mp.pos).sqrMagnitude < 0.001f)
                    {
                        mp.Indeces.Add(verticeIndex);
                        matchingParticleFound = true;
                        if (!rsMesh.ParticleIndeceDirectory.ContainsKey(verticeIndex))
                            rsMesh.ParticleIndeceDirectory.Add(verticeIndex, mp);
                        state.Break();
                    }
                });
            }
            else 
            {
                foreach (MeshParticle mp in rsMesh.Particles)
                {
                    if ((verticePosition - mp.pos).sqrMagnitude < 0.001f)
                    {
                        mp.Indeces.Add(verticeIndex);
                        matchingParticleFound = true;
                        if (!rsMesh.ParticleIndeceDirectory.ContainsKey(verticeIndex))
                            rsMesh.ParticleIndeceDirectory.Add(verticeIndex, mp);
                        break;
                    }
                }
            }

            //No matching
            if (!matchingParticleFound)
            {
                MeshParticle mp = new MeshParticle(verticePosition);
                mp.Indeces.Add(verticeIndex);
                rsMesh.Particles.Add(mp);
                if (!rsMesh.ParticleIndeceDirectory.ContainsKey(verticeIndex))
                    rsMesh.ParticleIndeceDirectory.Add(verticeIndex, mp);
            }
        }
    }


    public void ResolveParticleConnections() 
    {
        int[] triangles = (int[])rsMesh.mesh.triangles.Clone();
        for (int v = 0; v < triangles.Length; v+=3)
        {
            if (triangles.Length - v < 3)   
                break;

            if (v % 3 == 0) 
            {
                MeshParticle p1 = rsMesh.ParticleIndeceDirectory[triangles[v]];
                MeshParticle p2 = rsMesh.ParticleIndeceDirectory[triangles[v+1]];
                MeshParticle p3 = rsMesh.ParticleIndeceDirectory[triangles[v+2]];

                p1.connectedParticles.Add(p2);
                p1.connectedParticles.Add(p3);

                p2.connectedParticles.Add(p1);
                p2.connectedParticles.Add(p3);

                p3.connectedParticles.Add(p1);
                p3.connectedParticles.Add(p2);
            }
        }

        foreach (MeshParticle mp in rsMesh.Particles) 
        {
            mp.BuildEdgesAndUniqueEdges(rsMesh);
        }


    }

    private void OnDrawGizmos()
    {
        if (DrawGizmos && rsMesh != null && rsMesh.Particles != null && rsMesh.Particles.Count > 0) 
        {
            foreach (MeshParticle mp in rsMesh.Particles) 
            {

                Gizmos.color =  (new Color(0, 1, 0, 0.5f));
                Gizmos.DrawSphere(transform.TransformPoint(mp.pos + gizmoOffset), 0.06f);

                if (!ShowOnlyUniqueEdges)
                {
                    Gizmos.color = new Color(0, 0, 1, 0.2f);
                    foreach (MeshParticle mpConn in mp.connectedParticles)
                        Gizmos.DrawLine(transform.TransformPoint(mp.pos + gizmoOffset), transform.TransformPoint(mpConn.pos + gizmoOffset));
                }

                if (ShowIndeceIndicators) 
                {
                    Handles.Label(transform.TransformPoint(mp.pos + gizmoOffset), string.Join(",", mp.Indeces.Select(x => x.ToString()).ToArray()));
                }

                if (ShowProjectedPositions)
                {
                    Gizmos.color = (new Color(0, 1, 1, 0.5f));
                    Gizmos.DrawSphere(transform.TransformPoint(mp.pos + mp.body.Velocity + gizmoOffset), 0.06f);
                    Gizmos.DrawLine(transform.TransformPoint(mp.pos + gizmoOffset), transform.TransformPoint(mp.pos + mp.body.Velocity + gizmoOffset));
                }

            }

            if (ShowOnlyUniqueEdges)
            {
                Gizmos.color = new Color(1, 0, 1, 0.2f);
                foreach (MeshParticleEdge edge in rsMesh.UniqueEdges)
                    Gizmos.DrawLine(transform.TransformPoint(edge.originatorParticle.pos + gizmoOffset), 
                        transform.TransformPoint(edge.connectedParticle.pos + gizmoOffset));
            }


        }
    }

    /// <summary>
    /// Don't use this. Use the dictionary instead
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private MeshParticle getMeshParticleContainingIndex(int index) 
    {
        return rsMesh.Particles.Find(p => p.Indeces.Contains(index));
    }

    /// <summary>
    /// Updates the Particle velocity and position, does not update vertices
    /// Should be called on update if the particles are updated
    /// </summary>
    public void UpdateParticles() 
    {
        if(UseParallelProcessing)
        Parallel.ForEach(rsMesh.Particles, (mp, state) => 
        {
            mp.pos += mp.body.Velocity;
            mp.body.Velocity *= mp.body.VelocityDampening;
        }); 
        else
        foreach(MeshParticle mp in rsMesh.Particles)
        {
            mp.pos += mp.body.Velocity;
            mp.body.Velocity *= mp.body.VelocityDampening;
        }
    }

    /// <summary>
    /// Updates vertices depending on particle data
    /// Should be called with UpdateParticles()
    /// </summary>
    public void BashUpdateMeshVertices(Vector3 offset) 
    {
        Vector3[] vertices = (Vector3[])rsMesh.mesh.vertices.Clone();
        for (int i = 0; i < vertices.Length; i++) 
        {
            vertices[i] = (rsMesh.ParticleIndeceDirectory[i].pos + offset);

            if (rsMesh.ParticleIndeceDirectory[i].isAnchored)
                rsMesh.ParticleIndeceDirectory[i].body.Velocity = Vector3.zero;

        }
        rsMesh.mesh.SetVertices(vertices);
        rsMesh.mesh.RecalculateNormals();
        rsMesh.mesh.RecalculateBounds();

        if (AllowDynamicColliders)
            UpdateColliders();

    }



    public void UpdateColliders() 
    {
        if (meshCollider != null && AllowDynamicColliders) 
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = rsMesh.mesh;
        }
    }

    /// <summary>
    /// Recalculates particle set
    /// Can be taxing if called on update
    /// </summary>
    public void RecalculateParticles() 
    {
        rsMesh.ParticleIndeceDirectory = new Dictionary<int, MeshParticle>();
        rsMesh.Particles = new List<MeshParticle>();
        rsMesh.mesh = GetComponent<MeshFilter>().mesh;
        var time = Time.realtimeSinceStartup;
        ResolveParticles();
        ResolveParticleConnections();
        Debug.Log(Time.realtimeSinceStartup - time);
    }

}

public class ResolvedMesh
{
    public List<MeshParticle> Particles { get; set; }
    public Dictionary<int, MeshParticle> ParticleIndeceDirectory; //Multiple indeces may reference to the same particle for example in a cube there are 24 indeces directing total of 8 particeles
    public Mesh mesh;

    public List<MeshParticleEdge> UniqueEdges;

    public ResolvedMesh(Mesh mesh)
    {
        Particles = new List<MeshParticle>();
        ParticleIndeceDirectory = new Dictionary<int, MeshParticle>();
        UniqueEdges = new List<MeshParticleEdge>();
        this.mesh = mesh;
    }


}

public class MeshParticle
{
    public HashSet<int> Indeces { get; private set; }
    public Vector3 pos;
    public Vector3 projectedPos;
    public HashSet<MeshParticle> connectedParticles;
    public HashSet<MeshParticleEdge> connecteParticleEdges;
    public MeshParticleBody body;
    public bool isAnchored = false;

    public MeshParticle(Vector3 vertexPosition)
    {
        pos = vertexPosition;
        projectedPos = pos;

        Indeces = new HashSet<int>();
        connectedParticles = new HashSet<MeshParticle>();
        connecteParticleEdges = new HashSet<MeshParticleEdge>();
        body = new MeshParticleBody(this);
    }

    public void BuildEdgesAndUniqueEdges(ResolvedMesh rsMesh)
    {
        foreach (MeshParticle mp in connectedParticles) 
        {
            var edge = new MeshParticleEdge(this, mp);

            if(edge != null)
                connecteParticleEdges.Add(edge);

            //Look through edges that are just build, if an edge with flipped connections are going to be made instead its going to be ignored
            //Build unique edges list
            bool existsUniqueEdge = rsMesh.UniqueEdges.Exists(ed =>
                ( (ed.connectedParticle != null && ed.connectedParticle.Equals(mp)) &&
                (ed.originatorParticle != null && ed.originatorParticle.Equals(this))) ||
                ((ed.connectedParticle != null && ed.originatorParticle.Equals(mp)))
                && (ed.originatorParticle != null && ed.connectedParticle.Equals(this))
            );

            if (!existsUniqueEdge && edge != null)
                rsMesh.UniqueEdges.Add(edge);

        }
    }

}

public class MeshParticleEdge
{
    public MeshParticle originatorParticle;
    public MeshParticle connectedParticle;
    public float originalDistance;

    public MeshParticleEdge(MeshParticle org, MeshParticle conn) 
    {
        originatorParticle = org;
        connectedParticle = conn;
        originalDistance = (org.pos - conn.pos).magnitude;
    }
}

public class MeshParticleBody
{
    public Vector3 Velocity;
    public Vector3 SpringAcceleration;
    public float Weight;
    public MeshParticle particle;
    public float VelocityDampening;

    public MeshParticleBody(MeshParticle particle) 
    {
        Velocity = Vector3.zero;
        Weight = 1f;
        this.particle = particle;
        VelocityDampening = 0.0f;
    }

    public void ApplyForce(Vector3 force)
    {
        if(!particle.isAnchored)
            Velocity += force;
    }

    /// <summary>
    /// Applies mesh particle force towrds another meshParticle
    /// </summary>
    /// <param name="mp"></param>
    public void ApplyForceTowardsParticle(MeshParticle mp, float force) 
    {
        ApplyForce((mp.pos - this.particle.pos).normalized * force);
    }

    public void ApplyForceAwayFromParticle(MeshParticle mp, float force)
    {
        ApplyForce(-(mp.pos - this.particle.pos).normalized * force);
    }

}