using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ParticleBasedMeshResolver), typeof(Rigidbody))]
public class SpringWobble : MonoBehaviour
{

    [SerializeField] private float Dampening = 0.8f;
    [SerializeField] private float SpringForce = 1f;
    [SerializeField] private float VerticeForceMultiplier = 0.8f;
    [SerializeField] private float MaximumRigidBodyVelocityRatio = 0.5f;


    private Rigidbody rbody;
    private ParticleBasedMeshResolver resolver;
    private ResolvedMesh rsm;

    private Vector3[] projectedPositions;
    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        resolver = gameObject.GetComponent<ParticleBasedMeshResolver>();
        rsm = resolver.rsMesh;
        resolver.gizmoOffset = transform.position;
    }

    void Start()
    {
        projectedPositions = new Vector3[rsm.Particles.Count];
    }

    void Update()
    {

    }


    private void Resolve()
    {
        //Project the transform position depending on rigidbody velocity
        CollectProjections();

        //Rank vertices on distance to the projected position (difference between vertice position and current position. And do the same for projected position)
        //Ranking should be based on distance, descending
        Dictionary<int, float> ranking = CalculateVerticeRanks();


        //Add vertice velocity depending on the subValue of the vertice at index
        UpdateVerticeVelocities(ranking);



        //if projected position and the current position is same, reduce the speed very dramaticly 
        //added velocity should  be multiplier dependant on the rank




        //if two sequential projected positions are lowering, multiply with differnece between these
        //Same for increasing sequential positions

    }


    private void CollectProjections()
    {

        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            var p = rsm.Particles[i];
            projectedPositions[i] = p.pos + p.body.Velocity * i;
        }

    }

    /// <summary>
    /// There is some wacky math here, migth fix some stuff later
    /// </summary>
    /// <returns>index, avarage sub value</returns>
    private Dictionary<int, float> CalculateVerticeRanks()
    {
        float[] currentDistances = new float[rsm.Particles.Count];
        float[] projectedDistances = new float[rsm.Particles.Count];
        Dictionary<int, float> diff = new Dictionary<int, float>(); //index, diff in distance

        //Collect distances and diffs

        float positionalDifferenceAvarage = 0f;
        for (int i = 0; i < rsm.Particles.Count; i++)
        {
            var p = rsm.Particles[i];
            currentDistances[i] = (p.pos - transform.position).magnitude;
            projectedDistances[i] = ((p.pos + p.body.Velocity) - (transform.position + rbody.velocity)).magnitude;

            float posDiff = (projectedDistances[i] - currentDistances[i]);
            diff.Add(i, posDiff);
            positionalDifferenceAvarage += posDiff;
        }
        positionalDifferenceAvarage /= rsm.Particles.Count;

        //Convert diff to Subavarege distance
        diff.Values.ToList().ForEach(d => d = Mathf.Abs(positionalDifferenceAvarage - d) / 2);

        //Ranking
        diff.OrderByDescending(k => k.Value);

        return diff;
    }

    private void UpdateVerticeVelocities(Dictionary<int, float> ranking) 
    {
        Vector3 rbVelocity = rbody.velocity;
        for (int i = 0; i < ranking.Keys.Count; i++)
        {
            //Calculate velocity for vertice
            float multiplier = ranking[i] * MaximumRigidBodyVelocityRatio;
        }
    }

}
