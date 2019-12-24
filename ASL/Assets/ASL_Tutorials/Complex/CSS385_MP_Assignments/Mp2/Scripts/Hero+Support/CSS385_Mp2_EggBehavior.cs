using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    public class CSS385_Mp2_EggBehavior : MonoBehaviour
    {
        // All instance of CSS385_Mp2_EggBehavior share this one EggSystem
        private static CSS385_Mp2_EggSpawnSystem sEggSystem = null;
        public static void InitializeEggSystem(CSS385_Mp2_EggSpawnSystem e) { sEggSystem = e; }

        private const float kEggSpeed = 40f;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            transform.position += transform.up * (kEggSpeed * Time.smoothDeltaTime);

            // Figure out termination
            bool outside = CSS385_Mp2_GlobalBehavior.sTheGlobalBehavior.ObjectCollideWorldBound(GetComponent<Renderer>().bounds) == CSS385_Mp2_GlobalBehavior.WorldBoundStatus.Outside;
            if (outside)
            {
                DestroyThisEgg("Self");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Collision with hero (especially when first spawned) does not count
            if (collision.tag != "Player" && collision.name != "Egg(Clone)")
            {
                DestroyThisEgg(collision.gameObject.name);
            }
        }

        private void DestroyThisEgg(string name)
        {
            // Watch out!! a collision with overlap objects (e.g., two planes at the same location 
            // will result in two OnTriggerEntger2D() calls!!
            if (gameObject.activeSelf)
            {
                sEggSystem.DecEggCount();
                gameObject.SetActive(false);  // set inactive!
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Calling Egg Destroy on a destroyed egg: " + name);
            }
        }
    }
}