using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace Mps
{
    public class CSS385_Mp2_GlobalBehavior : MonoBehaviour
    {
        public static CSS385_Mp2_GlobalBehavior sTheGlobalBehavior = null;

        public Text mGameStateEcho = null;  // Defined in UnityEngine.UI
        public static CSS385_Mp2_HeroBehavior mHero = null;
        public CSS385_Mp2_EnemySpawnSystem mEnemySystem = null;
        public CSS385_Mp2_WayPointSystem mWayPoints = null;
        public GameObject mPlayer;

        #region World Bound support
        private Bounds mWorldBound;  // this is the world bound
        private Vector2 mWorldMin;  // Better support 2D interactions
        private Vector2 mWorldMax;
        private Vector2 mWorldCenter;
        private Camera mMainCamera;
        #endregion

        private void Awake()
        {
            // This must occur before EnemySystem's Start();
            Debug.Assert(mEnemySystem != null);
            Debug.Assert(mWayPoints != null);

            #region world bound support
            mMainCamera = Camera.main; // This is the default main camera
            mWorldBound = new Bounds(Vector3.zero, Vector3.one);
            UpdateWorldWindowBound();
            #endregion

            // initialize the Enemy spawn region: before enemy system's Start()!!
            mEnemySystem.SetSpawnRegion(mWorldMin, mWorldMax);

            // Make sure all enemy sees the same EnemySystem and CSS385_Mp2_WayPointSystem
            CSS385_Mp2_EnemyBehavior.InitializeEnemySystem(mEnemySystem, mWayPoints);

            CSS385_Mp2_GlobalBehavior.sTheGlobalBehavior = this;  // Singleton pattern
        }

        // Use this for initialization
        void Start()
        {
            //If we are the "Host" - ASL is a P2P model, so there is technically no host, but this is a way you can assign a player to be a "host" if necessary
            //In this example, the "Host" is in charge of creating everyone's player
            if (ASL.GameSparksManager.Instance().AmLowestPeer())
            {
                //For each player in the game, create a player object
                for (int i = 0; i < ASL.GameSparksManager.Instance().GetSessionInfo().GetPlayerList().Count; i++)
                {
                    //Since player start position does not matter in this application, we can use it to help determine which player we just spawned
                    //Spawning players is a lot easier if you have them pre-created in the scene before start and then assign them via name
                    //Instead of creating them like we do in this example, though this example does allow in theory, unlimited players
                    //In practice - it only allows for up to 9 since our regex only checks 0-9
                    ASL.ASLHelper.InstanitateASLObject("Hero", new Vector3(i + 1, 0, 0), Quaternion.identity, "", "", LocallyModifyPlayer, null, ModifyPlayer);
                }
            }
        }

        public static void LocallyModifyPlayer(GameObject _gameobject) //Used to call the SendFloat4 function for all users
        {
            float[] emptyFloatArray = { 0, 0, 0, 0 };
            _gameobject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                _gameobject.GetComponent<ASL.ASLObject>().SendFloat4(emptyFloatArray);
            });

        }

        public static void ModifyPlayer(string _id, float[] _floatCodes)
        {
            //No need to worry about floatCodes as this is the only thing I want the player to do. (using SendFloat4 function as a trigger instead of sending floats)
            //If I wanted to communicate other things, then I could set up a switch statement for floatCodes.
            if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject player))
            {
                player.gameObject.name = player.gameObject.name + player.gameObject.transform.position.x;
                //Does come with prefab so we need to find it in the scene
                player.gameObject.GetComponent<CSS385_Mp2_EggSpawnSystem>().mCoolDownTimeBar = GameObject.Find("EggCoolDownBar").GetComponent<RectTransform>();

                //If player name matches with our peer id, then set this manager to follow that player
                if (Regex.IsMatch(player.gameObject.name, ASL.GameSparksManager.Instance().GetPlayerPeerId().ToString()))
                {
                    player.gameObject.GetComponent<CSS385_Mp2_HeroBehavior>().Start(); //Now that the player is properly assigned - call the start function on this object
                    player.gameObject.GetComponent<CSS385_Mp2_EggSpawnSystem>().Start(); //Now that the player is properly assigned - call the start function on this object
                    mHero = player.gameObject.GetComponent<CSS385_Mp2_HeroBehavior>();
                }
            }
        }

        void Update()
        {
            EchoGameState(); // always do this
        }

        #region Game Window World size bound support
        public enum WorldBoundStatus
        {
            CollideTop,
            CollideLeft,
            CollideRight,
            CollideBottom,
            Outside,
            Inside
        };

        /// <summary>
        /// This function must be called anytime the MainCamera is moved, or changed in size
        /// </summary>
        public void UpdateWorldWindowBound()
        {
            // get the main 
            if (null != mMainCamera)
            {
                float maxY = mMainCamera.orthographicSize;
                float maxX = mMainCamera.orthographicSize * mMainCamera.aspect;
                float sizeX = 2 * maxX;
                float sizeY = 2 * maxY;
                float sizeZ = Mathf.Abs(mMainCamera.farClipPlane - mMainCamera.nearClipPlane);

                // Make sure z-component is always zero
                Vector3 c = mMainCamera.transform.position;
                c.z = 0.0f;
                mWorldBound.center = c;
                mWorldBound.size = new Vector3(sizeX, sizeY, sizeZ);

                mWorldCenter = new Vector2(c.x, c.y);
                mWorldMin = new Vector2(mWorldBound.min.x, mWorldBound.min.y);
                mWorldMax = new Vector2(mWorldBound.max.x, mWorldBound.max.y);
            }
        }

        public Vector2 WorldCenter { get { return mWorldCenter; } }
        public Vector2 WorldMin { get { return mWorldMin; } }
        public Vector2 WorldMax { get { return mWorldMax; } }

        public WorldBoundStatus ObjectCollideWorldBound(Bounds objBound)
        {
            WorldBoundStatus status = WorldBoundStatus.Inside;

            if (mWorldBound.Intersects(objBound))
            {
                if (objBound.max.x > mWorldBound.max.x)
                    status = WorldBoundStatus.CollideRight;
                else if (objBound.min.x < mWorldBound.min.x)
                    status = WorldBoundStatus.CollideLeft;
                else if (objBound.max.y > mWorldBound.max.y)
                    status = WorldBoundStatus.CollideTop;
                else if (objBound.min.y < mWorldBound.min.y)
                    status = WorldBoundStatus.CollideBottom;
                else if ((objBound.min.z < mWorldBound.min.z) || (objBound.max.z > mWorldBound.max.z))
                    status = WorldBoundStatus.Outside;
            }
            else
                status = WorldBoundStatus.Outside;

            return status;
        }

        public WorldBoundStatus ObjectClampToWorldBound(Transform t)
        {
            WorldBoundStatus status = WorldBoundStatus.Inside;
            Vector3 p = t.position;

            if (p.x > WorldMax.x)
            {
                status = WorldBoundStatus.CollideRight;
                p.x = WorldMax.x;
            }
            else if (t.position.x < WorldMin.x)
            {
                status = WorldBoundStatus.CollideLeft;
                p.x = WorldMin.x;
            }

            if (p.y > WorldMax.y)
            {
                status = WorldBoundStatus.CollideTop;
                p.y = WorldMax.y;
            }
            else if (p.y < WorldMin.y)
            {
                status = WorldBoundStatus.CollideBottom;
                p.y = WorldMin.y;
            }

            if ((p.z < mWorldBound.min.z) || (p.z > mWorldBound.max.z))
            {
                status = WorldBoundStatus.Outside;
            }

            t.position = p;
            return status;
        }
        #endregion

        public void HeroHitByEnemy() { mHero?.HeroHit(); }
        private void EchoGameState()
        {
            mGameStateEcho.text = mWayPoints.GetWayPointState() + mHero?.GetHeroState() + mEnemySystem.GetEnemyState();
        }
    }
}