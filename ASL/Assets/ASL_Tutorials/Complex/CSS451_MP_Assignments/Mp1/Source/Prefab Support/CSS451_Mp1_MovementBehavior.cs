using UnityEngine;

namespace Mps
{
    public class CSS451_Mp1_MovementBehavior : MonoBehaviour
    {

        public float mMovementLimit = 5f;               // the movement limit
        public Vector3 mMovementDirection = Vector3.up; // the movement direction
        public Color mToggleColor = Color.white;        // color change
        public float mRotationSpeed = 90f;              // per second
        public float mLinearSpeed = 1f;                 // per second

        private int mDirection = 1;                     // moving in positive or negative direction

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            float dt = Time.deltaTime;
            // Update rotation
            transform.Rotate(Vector3.up, mRotationSpeed * dt);

            Vector3 delta = mMovementDirection * (mDirection * mLinearSpeed * dt);
            transform.localPosition = transform.localPosition + delta;
            float test = Vector3.Dot(transform.localPosition, mMovementDirection);

            int moveDirection = 1;
            if (test > mMovementLimit)
                moveDirection = -1;
            else if (test < 0f)
                moveDirection = 1;
            else moveDirection = mDirection;

            if (moveDirection != mDirection)
            {
                mDirection = moveDirection;

                // show how to access components which may or may not be there
                Renderer r = GetComponent<Renderer>();
                if (r != null)
                {
                    Material m = r.material;
                    m.color = m.color + (mDirection * mToggleColor);
                }
            }
        }
    }
}