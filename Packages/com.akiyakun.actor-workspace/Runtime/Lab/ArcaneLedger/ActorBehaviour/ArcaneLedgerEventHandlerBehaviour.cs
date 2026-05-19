#nullable enable
using UnityEngine;
using afl;
using afl.Service.Input;
// using ActorWorkspace;

namespace afl.ArcaneLedger.ActorBehaviour
{
    public class ArcaneLedgerEventHandlerBehaviour// : ActorBehaviourBase
    {
    }
#if false
        public override UpdateFlags UpdateFlags { get; set; } = UpdateFlags.Update | UpdateFlags.FixedUpdate;

        PlayerInputProvider playerInputProvider = null!;
        PlayerInputObserver inputObserver = null!;
        Rigidbody rigidbody = null!;

        TopDownMoveBehaviour moveBehaviour = null!;
        InteractBehaviour interactBehaviour = null!;

        // InputAction moveAction = null!;

        bool enableInteract = false;
        GameObject? interactableObject = null;

        public override void Restore()
        {
        }

        public override void DoAwake()
        {
            // var actionMap = InputService.Instance.FindActionMap("Player");
            // if (actionMap == null) throw new System.Exception("Player action map not found");
            // moveAction = actionMap.FindAction("Move");

            playerInputProvider = new PlayerInputProvider();
            InputService.Instance.AddInputProvider(playerInputProvider);

            inputObserver = new PlayerInputObserver();
            inputObserver.Setup(playerInputProvider);

            rigidbody = Actor.GameObject.GetComponent<Rigidbody>();
            Debug.Assert(rigidbody != null);
            // var rootMotion = Actor.GameObject.GetComponentInChildren<SkeletonRootMotionBase>();

            // ActorBehaviour
            {
                moveBehaviour = Actor.ActorBehaviourController.Get<TopDownMoveBehaviour>()!;
                Debug.Assert(moveBehaviour != null);

                interactBehaviour = Actor.ActorBehaviourController.Get<InteractBehaviour>()!;
                Debug.Assert(interactBehaviour != null);
            }

            // イベントの購読
            {
                eventBag.In(EventBus,
                    (entity) => entity.Subscribe(ActorEvents.SetEnableInput, SetEnableInput),
                    (entity) => entity.Unsubscribe(ActorEvents.SetEnableInput, SetEnableInput));
            }

            var detector = Actor.GameObject.GetComponent<Collision2DDetector>();
            detector.TriggerEvent.OnEnter.AddListener((other) =>
            {
                if (other.gameObject.CompareTag("Interactable"))
                {
                    enableInteract = true;
                    interactableObject = other.gameObject;
                    // Debug.Log($"PlayerInputBehaviour OnTriggerEnter2D: {other.gameObject.name}");
                }
            });
            detector.TriggerEvent.OnExit.AddListener((other) =>
            {
                enableInteract = false;
                interactableObject = null;
            });

            WorldSceneBehaviour.playerObjet = Actor.GameObject;
        }

        public override void DoDestroy()
        {
            if (playerInputProvider != null)
            {
                InputService.Instance.RemoveInputProvider(playerInputProvider);
                playerInputProvider = null!;
            }
        }

        // MEMO:
        // キー入力処理はUpdate()で行う必要がある、DoFixedUpdate()だと毎フレーム必ず呼ばれるわけではないのでIsPushがすり抜けることが多々ある。
        public override void DoUpdate(float deltaTime)
        {
        }

        public override void DoFixedUpdate()
        {
            Vector3 moveAmount = Vector3.zero;

            var key = Keyboard.current;
            float speed = 1.0f;
            if (key.shiftKey.isPressed)
            {
                speed = 10.0f;
            }

            if (inputObserver.MoveLeft.IsDown)
            {
                moveAmount.x = -1.0f;
            }
            else if (inputObserver.MoveRight.IsDown)
            {
                moveAmount.x = +1.0f;
            }

            if (inputObserver.MoveDown.IsDown)
            {
                moveAmount.y = -1.0f;
            }
            else if (inputObserver.MoveUp.IsDown)
            {
                moveAmount.y = +1.0f;
            }

            if (moveAmount != Vector3.zero)
            {
                moveBehaviour.MoveDistance(moveAmount.normalized * speed);
            }

            if (inputObserver.Interact.IsPush)
            {
                EventBus.Publish(ActorEvents.RequestInteract);
                // UnityEditor.EditorApplication.isPaused = true;
            }
        }

        void SetEnableInput(bool enable)
        {
            inputObserver.SetEnable(enable);
        }

        // void OnDown(float progressTime)
        // {
        // }
    }
#endif
}
#nullable restore