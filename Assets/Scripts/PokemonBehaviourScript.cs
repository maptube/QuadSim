using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonBehaviourScript : MonoBehaviour {

    public Transform explosionPrefab;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
            Destroy(collision.gameObject);
        }
    }

}
