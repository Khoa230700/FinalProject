using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        private PlayerBehaviorListener PlayerPosition;
        [HideInInspector] public float speedCur = 0f;
        private Health playerHealth;

        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;
            public float BackwardSpeed = 4.0f;
            public float StrafeSpeed = 4.0f;
            public float RunMultiplier = 2.0f;
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(
                new Keyframe(-90.0f, 1.0f),
                new Keyframe(0.0f, 1.0f),
                new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero)
                    return;

                if (input.x > 0 || input.x < 0)
                {
                    CurrentTargetSpeed = StrafeSpeed;
                }
                if (input.y < 0)
                {
                    CurrentTargetSpeed = BackwardSpeed;
                }
                if (input.y > 0)
                {
                    CurrentTargetSpeed = ForwardSpeed;
                }

#if !MOBILE_INPUT
                var runKey = KeyBindingManager.Instance.GetKeyBinding("Run");
                if (runKey != null && Input.GetKey(runKey.primary))
                {
                    CurrentTargetSpeed *= RunMultiplier;
                    m_Running = true;
                }
                else
                {
                    m_Running = false;
                }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }

        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f;
            public float stickToGroundHelperDistance = 0.5f;
            public float slowDownRate = 20f;
            public bool airControl;
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset;
        }

        public Camera cam;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();

        private Rigidbody m_RigidBody;
        [HideInInspector] public CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;

        public Vector3 Velocity
        {
            get { return m_RigidBody.linearVelocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
#if !MOBILE_INPUT
                return movementSettings.Running;
#else
                return false;
#endif
            }
        }

        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init(transform, cam.transform);
            playerHealth = GetComponent<Health>();
            PlayerPosition = GetComponent<PlayerBehaviorListener>();
        }

        private void Update()
        {
            if (playerHealth.health > 0)
            {
                RotateView();

                var jumpKey = KeyBindingManager.Instance.GetKeyBinding("Jump");
                if (jumpKey != null && Input.GetKeyDown(jumpKey.primary) && !m_Jump &&
                    PlayerPosition.PlayerPosition != BehaviorList.PlayerBehavior.Sit)
                {
                    m_Jump = true;
                }
            }
        }

        private void FixedUpdate()
        {
            if (playerHealth.health > 0)
            {
                GroundCheck();
                Vector2 input = GetInput();
                speedCur = m_RigidBody.linearVelocity.sqrMagnitude;

                if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) &&
                    (advancedSettings.airControl || m_IsGrounded))
                {
                    Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                    desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                    desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                    desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                    desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;

                    if (m_RigidBody.linearVelocity.sqrMagnitude <
                        (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
                    {
                        m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
                    }
                }

                if (m_IsGrounded)
                {
                    m_RigidBody.linearDamping = 5f;

                    if (m_Jump)
                    {
                        m_RigidBody.linearDamping = 0f;
                        m_RigidBody.linearVelocity = new Vector3(
                            m_RigidBody.linearVelocity.x, 0f, m_RigidBody.linearVelocity.z);
                        m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                        m_Jumping = true;
                    }

                    if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon &&
                        m_RigidBody.linearVelocity.magnitude < 1f)
                    {
                        m_RigidBody.Sleep();
                    }
                }
                else
                {
                    m_RigidBody.linearDamping = 0f;
                    if (m_PreviouslyGrounded && !m_Jumping)
                    {
                        StickToGroundHelper();
                    }
                }
                m_Jump = false;
            }
        }

        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }

        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position,
                    m_Capsule.radius * (1.0f - advancedSettings.shellOffset),
                    Vector3.down, out hitInfo,
                    ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.stickToGroundHelperDistance,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.linearVelocity = Vector3.ProjectOnPlane(m_RigidBody.linearVelocity, hitInfo.normal);
                }
            }
        }

        private Vector2 GetInput()
        {
            float horizontal = 0f;
            float vertical = 0f;

            var left = KeyBindingManager.Instance.GetKeyBinding("MoveLeft");
            var right = KeyBindingManager.Instance.GetKeyBinding("MoveRight");
            var forward = KeyBindingManager.Instance.GetKeyBinding("MoveForward");
            var backward = KeyBindingManager.Instance.GetKeyBinding("MoveBackward");

            if (left != null && Input.GetKey(left.primary)) horizontal -= 1f;
            if (right != null && Input.GetKey(right.primary)) horizontal += 1f;
            if (forward != null && Input.GetKey(forward.primary)) vertical += 1f;
            if (backward != null && Input.GetKey(backward.primary)) vertical -= 1f;

            Vector2 input = new Vector2(horizontal, vertical);
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }

        private void RotateView()
        {
            if (Mathf.Abs(Time.timeScale) < float.Epsilon)
                return;

            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation(transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                Quaternion velRotation =
                    Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.linearVelocity = velRotation * m_RigidBody.linearVelocity;
            }
        }

        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position,
                    m_Capsule.radius * (1.0f - advancedSettings.shellOffset),
                    Vector3.down, out hitInfo,
                    ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }

            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }
    }
}
