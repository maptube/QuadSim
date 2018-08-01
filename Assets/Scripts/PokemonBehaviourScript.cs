using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonBehaviourScript : MonoBehaviour {

    public Transform explosionPrefab;

    public Vector3[] Positions;

    // Use this for initialization
    void Start () {

      Positions  = new Vector3[]
      {
        new Vector3(88.4f,92.1f,148.2f), //pikachu initial
        new Vector3(-159f,29.9f,87.4f), //charmander initial
        new Vector3(-67.5f,10.9f,-131.3f), //squirtle initial
        new Vector3(40.7f,21.5f,-79.5f),  //bulbasaur initial
        new Vector3(-97.4f,24.2f,-108f), //near squirtle
        new Vector3(32.74f,3.71f,-195.5f), //green square
        new Vector3(143.1f,31.0f,-97.7f), //extreme right white square in gulley
        new Vector3(163.1f,61.8f,-33.1f), //right top peak
        new Vector3(15.4f,61.8f,69f), //central peak at bend in road cutting
        new Vector3(-76.5f,35.5f,57.5f), //hidden in cutting half way up left ridge
        new Vector3(-5.92f,2.82f,-195.1f) //back at the start position
      };
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Returns true if two vectors are close to each other (Manhattan dist  less than 1)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private bool isClose(Vector3 a, Vector3 b)
    {
        return (Mathf.Abs(a.x-b.x)<1.0f)&&(Mathf.Abs(a.y-b.y)<1.0f)&&(Mathf.Abs(a.z-b.z)<1.0f);
    }

    /// <summary>
    /// Get a new position for a Pokemon based on finding one that's not already taken
    /// </summary>
    /// <param name="CurrentPos"></param>
    /// <returns></returns>
    private Vector3 GetNewPokemonPosition(Vector3 CurrentPos)
    {
        Vector3 PikachuPos = GameObject.Find("P2_Pikachu").transform.position;
        Vector3 CharmanderPos = GameObject.Find("BR_Charmander").transform.position;
        Vector3 BulbasaurPos = GameObject.Find("BR_Bulbasaur").transform.position;
        Vector3 SquirtlePos = GameObject.Find("BR_Squirtle").transform.position;

        int newi = Mathf.RoundToInt(Random.value * Positions.Length - 1)%Positions.Length;
        for (int i=0; i<Positions.Length; i++)
        {
            Vector3 newpos = Positions[newi];
            if (!isClose(newpos,PikachuPos)&&!isClose(newpos,CharmanderPos)&&!isClose(newpos,BulbasaurPos)&&!isClose(newpos,SquirtlePos))
            {
                return newpos;
            }
            newi=(newi+1)%Positions.Length;
        }
        return new Vector3(0, 0, 0);

    }

    void OnCollisionEnter(Collision collision)
    {
        string name = collision.gameObject.name;
        if ((name== "P2_Pikachu") ||(name == "BR_Squirtle")||(name== "BR_Bulbasaur") ||(name== "BR_Charmander"))
        {
            Debug.Log("COLLISION!!!! with "+name+"!!!!");
            //ContactPoint contact = collision.contacts[0];
            //Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            //Vector3 pos = contact.point;
            //Instantiate(explosionPrefab, pos, rot);
            //Destroy(collision.gameObject);
            collision.gameObject.transform.position = GetNewPokemonPosition(collision.gameObject.transform.position);
            Debug.Log("New pos=" + collision.gameObject.transform.position);
        }
    }

}
