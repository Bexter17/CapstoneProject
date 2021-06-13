using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attachPlatform : MonoBehaviour
{
	public GameObject Player;

	

	private void OnTriggerStay(Collider other)
	{
		/*
		if (other.gameObject == Player)
		{
			Player.transform.parent = other.gameObject.transform;
		}

        else
        {
			Player.transform.parent = null;
		}
		*/
	}


}
