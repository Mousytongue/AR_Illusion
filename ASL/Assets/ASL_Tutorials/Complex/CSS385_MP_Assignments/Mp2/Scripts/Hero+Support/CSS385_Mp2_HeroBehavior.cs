using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;

namespace Mps
{
    public class CSS385_Mp2_HeroBehavior : MonoBehaviour
    {

        public CSS385_Mp2_EggSpawnSystem mEggSystem = null;
        private const float kHeroRotateSpeed = 90f / 2f; // 90-degrees in 2 seconds

        //  Hero state
        private int mHeroHit = 0;
        public void HeroHit() { mHeroHit++; }
        public string GetHeroState() { return "   Hero: Hit(" + mHeroHit + ") EggCount(" + EggsOnScreen() + ")"; }

        public bool m_IsMe;

        private void Awake()
        {
            // Actually since Hero spwans eggs, this can be done in the Start() function, but, 
            // just to show this can also be done here.
            Debug.Assert(mEggSystem != null);
            CSS385_Mp2_EggBehavior.InitializeEggSystem(mEggSystem);
        }

        public void Start()
        {
            //Find a match between the player + number (e.g. 1) and what peer id this player is. This will work for players 1-9 (as 10 would match with 1).
            //You can do other methods to match single player controlled objects to a single person (e.g., only player 1 can change player 1), but doing so
            //is up to you

            //Do to network latency, this start function will fire before this user has the correct name of this gameObject. Therefore,
            //This function was made public and the moment the name gets updated via the multi-purpose SendFloat4 function that is attached
            //to this player object upon its creation, calls this start function to ensure m_IsMe is assigned properly.
            if (Regex.IsMatch(gameObject.name, ASL.GameSparksManager.Instance().GetPlayerPeerId().ToString()))
            {
                m_IsMe = true;
            }
            else
            {
                m_IsMe = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_IsMe)
            {
                UpdateMotion();
                ProcessEggSpwan();
            }
        }

        private int EggsOnScreen() { return mEggSystem.GetEggCount(); }

        private void UpdateMotion()
        {
            //Allows mice not to over lap each other in single player testing
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            if (!screenRect.Contains(Input.mousePosition))
                return;

            GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
            //Handle rotation
            Quaternion rotate = Quaternion.AngleAxis(-1f * Input.GetAxis("Horizontal") * (kHeroRotateSpeed * Time.smoothDeltaTime), Vector3.forward);
                GetComponent<ASL.ASLObject>().SendAndIncrementLocalRotation(rotate);

            //Handle position
            Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                p.z = 0f;
                GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(p);
            });
        }

        private void ProcessEggSpwan()
        {
            if (mEggSystem.CanSpawn())
            {
                if (Input.GetKey("space"))
                {
                    mEggSystem.SpawnAnEgg(transform.position, transform.up);
                }
            }
        }
    }
}