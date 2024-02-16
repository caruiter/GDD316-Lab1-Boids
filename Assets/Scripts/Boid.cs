using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigid;
    private Neighborhood neighborhood;
    public int type;
    [SerializeField] GameObject propellor;
    public float height;

    // Use this for initialization
    void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();

        //Set a random initial position
        Vector3 position = Random.insideUnitSphere * Spawner.S.spawnRadius;

        //Set a random initial velocity
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;

        rigid.velocity = vel;

        LookAhead();

        //give boid a random type
        int randomType = Random.Range(0, 2);
        if (randomType == 1)
        {
            type = 2;
            propellor.SetActive(false);
            height = Random.Range(3,10);
            //pos += Vector3(0, 0, height);
            //pos = pos + Vector3(0, 0, height);
        }
        else
        {
            type = 1;
            propellor.SetActive(true);
            height = Random.Range(-10, -3);
        }

        position.y = height;
        pos = position;
        //pos.Set(position.x, height, position.z);
        transform.position = new Vector3 (position.x, height, position.z);

        //Give the Boid a random color, but make sure it's not too dark
        Color randColor = Color.black;
        while (randColor.r + randColor.g + randColor.b < 1.0f)
        {
            if(type == 1)
            {
                randColor = new Color(Random.value, Random.Range(0, .55f), 0) ;

            }
            else
            {
                randColor = new Color(0, Random.Range(0, .55f), Random.value);
            }
            

            //randColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = randColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }


    void LookAhead()
    {
        //Orients the Boid to look at the direction it's flying
        transform.LookAt(pos + rigid.velocity);
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = new Vector3(value.x, height, value.z); }
    }

    //FixedUpdate is called one per physics update (i.e. 50x/second)
    private void FixedUpdate()
    {
        Vector3 vel = rigid.velocity;
        Spawner spn = Spawner.S;

        //Collision Avoidance - avoid neigbors who are too close
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;
        // If the response is Vector3.zero, then no need to react
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        //Velocity matching - Try to match velocity with neigbors
        Vector3 velAlign = neighborhood.avgVel;
        // Only do more if the velAlign is not Vector3.zero
        if (velAlign != Vector3.zero)
        {
            // we're really interested in direction, so normalize the velocity
            velAlign.Normalize();
            // and then set it to the speeed we chose
            velAlign *= spn.velocity;
        }

        //Flock centering - move towards the center of local neighbors
        Vector3 velCenter = neighborhood.avgPos;

        //trying to seperate the types of boids
        if(type == 1)
        {
            //velCenter = (neighborhood.avgPos.x, neighborhood.avgPos.y, (neighborhood.avgPos.z + 2f));
            //velCenter += new Vector3(0, -25, 30);
            velCenter += new Vector3(neighborhood.avgPos.x, -25, (neighborhood.avgPos.z + 2f));
        }
        else
        {
            //velCenter -= new Vector3(0, 25, 30);
            velCenter += new Vector3(neighborhood.avgPos.x, 25, (neighborhood.avgPos.z + 2f));
        }

        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        //ATTRACTION - Move towards the Atttractor
        Vector3 delta = Attractor.POS - pos;
        //Check whether we're attracted or avoiding the Attractor
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 velAttract = delta.normalized * spn.velocity;

        //Apply all the velocities
        float fdt = Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.flockCentering * fdt);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
                }
            }
        }

        //check whether we're about to hit an obstacle - referenced Unity API
        RaycastHit isHit;
        if (Physics.Raycast(transform.position, transform.forward, out isHit, 5))
        {
            if (isHit.collider.gameObject.CompareTag("Obstacle"))
            {
                vel = Vector3.Lerp(vel, -velAttract, spn.attractPull * fdt * 2);
                Debug.Log("DODGE?");
            }
            //GameObject obj = Physics.Raycast(transform.position, transform.forward, out hit);
            //print("There is something in front of the object!");

        }



        //set vel to the velocity set on the spawner singleton
        vel = vel.normalized* spn.velocity;
        vel.y = 0;
        // Finally assign this to the Rigidbody
        rigid.velocity = vel;
        //apply restraint on height
        transform.position = pos;

        /**if(type == 1)
        {
            if (transform.position.y > height)
            {
                vel = new Vector3(vel.x, height, vel.y);
            }
        }**/


        //Lock in the direction of the new velocity
        LookAhead();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("CRASH");
    }
}