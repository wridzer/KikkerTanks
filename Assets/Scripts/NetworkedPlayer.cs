using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChatClientExample
{

	public struct InputUpdate : INetworkSerializable
	{
		public float horizontal, vertical, mouseX, mouseY;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref horizontal);
            serializer.SerializeValue(ref vertical);
            serializer.SerializeValue(ref mouseX);
            serializer.SerializeValue(ref mouseY);
        }
    }

    public class NetworkedPlayer : NetworkBehaviour, IDamageble
    {
        [Header("Player Settings")]
        [SerializeField] private float health;
        [SerializeField] private float minPower;
        [SerializeField] private float maxPower;
        [SerializeField] private float lineAlphaModifier;
        [SerializeField] private float moveSpeed;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject launchPoint;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private SpawnPointsInfo spawnPoints;

        private LineRenderer lineRenderer;
        private Vector2 aimPos;
        private float lineAlphaBegin = 0;
        private float lineAlphaEnd = 0;
        private float power;
        private bool alphaGoingUp = true;

        public float Health { get; set; }

        // Start is called before the first frame update
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Health = health;
            HealthClientRpc();
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 10f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, launchPoint.transform.position);

            transform.position = spawnPoints.spawnPointList[(int)OwnerClientId];
            if((int)OwnerClientId > (spawnPoints.spawnPointList.Count * 0.5f) - 1f)
            {
                transform.GetComponent<SpriteRenderer>().flipX= true;
            }
        }

        

        // Update is called once per frame
        void Update()
        {
            HealthClientRpc();

            // Check if client is object owner
            if (!IsOwner) return;

            aimPos = Input.mousePosition;

            Vector3 moveDir= Vector3.zero;

            if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
            if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

            transform.position += moveDir * moveSpeed * Time.deltaTime;

            lineRenderer.SetPosition(0, launchPoint.transform.position);

            if (Input.GetMouseButtonDown(0))
            {
                lineRenderer.enabled = true;
            }
            if (Input.GetMouseButton(0))
            {
                lineRenderer.SetPosition(1, aimPos);
                power = GetPower();
            }
            if (Input.GetMouseButtonUp(0))
            {
                Shoot(power);
            }

            NetworkVariable<InputUpdate> inputUpdate = new NetworkVariable<InputUpdate>(
                new InputUpdate
                {
                    horizontal = moveDir.x,
                    vertical = moveDir.y,
                    mouseX = aimPos.x,
                    mouseY = aimPos.y,
                }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        }

        private float GetPower()
        {
            if (alphaGoingUp)
            {
                if (lineAlphaBegin < 1)
                {
                    lineAlphaBegin += lineAlphaModifier;
                }
                else
                {
                    lineAlphaEnd += lineAlphaModifier;
                    if (lineAlphaEnd > 1) { alphaGoingUp = false; }
                }
            }
            if (!alphaGoingUp)
            {
                if (lineAlphaEnd > 0)
                {
                    lineAlphaEnd -= lineAlphaModifier;
                }
                else
                {
                    lineAlphaBegin -= lineAlphaModifier;
                    if (lineAlphaBegin < 0) { alphaGoingUp = true; }
                }
            }
            lineRenderer.startColor = new Color(255, 255, 0, lineAlphaBegin);
            lineRenderer.endColor = new Color(255, 0, 0, lineAlphaEnd);

            return (lineAlphaBegin + lineAlphaEnd) / 2 * (maxPower - minPower);
        }

        void Shoot(float _power)
        {
            lineRenderer.enabled = false;
            lineAlphaBegin = 0;
            lineAlphaEnd = 0;
            Vector2 newRot = (Input.mousePosition - launchPoint.transform.position).normalized;
            SpawnServerRpc(newRot.x, newRot.y, _power);
        }

        [ClientRpc]
        public void TakeDamageClientRpc(float _Damage)
        {
            Health -= _Damage;
            HealthClientRpc();
            if (Health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Destroy(gameObject);
        }

        [ServerRpc]
        private void SpawnServerRpc(float rotX, float rotY, float _power)
        {
            Vector2 newRot = new Vector2(rotX, rotY);
            GameObject rocket = Instantiate(
                bulletPrefab,
                launchPoint.transform.position,
                Quaternion.identity,
                this.transform
                );
            rocket.GetComponent<Rigidbody2D>().AddForce(newRot * _power);
            rocket.GetComponent<NetworkObject>().Spawn(true);
        }

        [ClientRpc]
        private void HealthClientRpc()
        {
            healthText.text = Health.ToString();
        }
    }

    /*public class NetworkedPlayer : NetworkedBehaviour, IDamageble
    {
        [Header("Player Settings")]
        [SerializeField] private float health;
        [SerializeField] private float minPower;
        [SerializeField] private float maxPower;
        [SerializeField] private float lineAlphaModifier;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject launchPoint;

        private LineRenderer lineRenderer;

        private Vector2 aimPos;
        private float lineAlphaBegin = 0;
        private float lineAlphaEnd = 0;
        private bool alphaGoingUp = true;
        private float power;

        public float Health { get; set; }

        public bool isLocal = false;
		public bool isServer = false;

		bool canFire = true;
		InputUpdate input;

		Client client;
		Server server;

		private void Start() {
			if (isLocal) {
				GetComponentInChildren<Camera>().enabled = true;
				if ( Camera.main ) {
					Camera.main.enabled = false;
				}

				client = FindObjectOfType<Client>();
			}
			if ( isServer ) {
				server = FindObjectOfType<Server>();
			}

            Health = health;
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startWidth = 1;
            lineRenderer.endWidth = 100f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, launchPoint.transform.position);
        }

		private void Update() {
			if (isLocal) {
				InputUpdate update = new InputUpdate(Input.mousePosition.x, Input.mousePosition.y, Input.GetMouseButtonUp(0));

				// Send input update to server
				InputUpdateMessage inputMsg = new InputUpdateMessage {
					input = update,
					networkId = this.networkId
				};
				client.SendPackedMessage(inputMsg);

                if (Input.GetMouseButtonDown(0))
                {
                    lineRenderer.enabled = true;
                }
                if (Input.GetMouseButton(0))
                {
                    lineRenderer.SetPosition(1, aimPos);
                    power = GetPower();
                }
                if (Input.GetMouseButtonUp(0))
                {
                    Shoot(power);
                }
            }

			if (isServer) {
				if(canFire && input.fire)
				{
                    GetPower();
                    Shoot(power);
				}

                aimPos = new Vector2(input.horizontal, input.vertical);

				// TODO: Send position update to all clients (maybe not every frame!)
				if (Time.frameCount % 3 == 0) { // assuming 60fps, so 20fps motion updates
												// We could consider sending this over a non-reliable pipeline
					UpdatePositionMessage posMsg = new UpdatePositionMessage {
						networkId = this.networkId,
						position = transform.position,
						rotation = transform.eulerAngles
					};

					server.SendBroadcast(posMsg);
				}
			}
		}

        void Shoot(float _power)
        {
            lineRenderer.enabled = false;
            lineAlphaBegin = 0;
            lineAlphaEnd = 0;
            Vector2 newRot = (Input.mousePosition - launchPoint.transform.position).normalized;
            GameObject rocket = Instantiate(
                bulletPrefab,
                launchPoint.transform.position,
                Quaternion.identity,
                this.transform
                );
            rocket.GetComponent<Rigidbody2D>().AddForce(newRot * _power);
        }

        public void TakeDamage(float _Damage)
        {
            Health -= _Damage;
            Debug.Log(Health);
            if (Health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Destroy(gameObject);
        }

        private float GetPower()
        {
            if (alphaGoingUp)
            {
                if (lineAlphaBegin < 1)
                {
                    lineAlphaBegin += lineAlphaModifier;
                }
                else
                {
                    lineAlphaEnd += lineAlphaModifier;
                    if (lineAlphaEnd > 1) { alphaGoingUp = false; }
                }
            }
            if (!alphaGoingUp)
            {
                if (lineAlphaEnd > 0)
                {
                    lineAlphaEnd -= lineAlphaModifier;
                }
                else
                {
                    lineAlphaBegin -= lineAlphaModifier;
                    if (lineAlphaBegin < 0) { alphaGoingUp = true; }
                }
            }
            lineRenderer.startColor = new Color(255, 255, 0, lineAlphaBegin);
            lineRenderer.endColor = new Color(255, 0, 0, lineAlphaEnd);

            return (lineAlphaBegin + lineAlphaEnd) / 2 * (maxPower - minPower);
        }

        public void UpdateInput(InputUpdate received) {
			input.horizontal = received.horizontal;
			input.vertical = received.vertical;
			input.fire = received.fire;
		}
	}*/
}