using UnityEngine;

public class Explosion : MonoBehaviour {
    ParticleSystem ps;
	// Use this for initialization
	void Start () {
		ps = GetComponent<ParticleSystem>();
        GetComponent<AudioSource>().Play();
    }
	
	// Update is called once per frame
	void Update () {
        if (ps.isStopped)
        {
            Destroy(ps.gameObject);
        }
    }
}
