using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerNetwork : NetworkBehaviour
{
    

    Rigidbody2D m_Rigidbody;

    // Prefabs de balas
    [SerializeField] Transform bulletObjectRedPrefab;
    [SerializeField] Transform bulletObjectBluePrefab;

    // Punto desde el cual se dispara el proyectil
    [SerializeField] Transform playerShootPoint;

    // Variable utilizada para no estar enviando todo el rato input si no se esta presionando
    bool waitForNewInput = false;

    // Variable utilizada para cooldown
    bool inCooldown = false;
    float timeBetweenShoots = 0.5f;

    NetworkVariable<float> m_Speed = new NetworkVariable<float>(2.0810f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<TeamColor> playerTeam = new NetworkVariable<TeamColor>(TeamColor.red, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<float> playerRotation = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<int> vida = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> CanShoot = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

    [QFSW.QC.Command]
    void say(string message)
    {
       ChatServerRpc(message,new ServerRpcParams());
    }
    [QFSW.QC.Command]
    void frase1()
    {
        ChatServerRpc("¿Por qué los soldados siempre llevan paracaídas? Porque así pueden 'caer' bien en cualquier situación. :<", new ServerRpcParams());
    }
    [QFSW.QC.Command]
    void chat()
    {
        ChatServerRpc("¿Qué hace un mudo bailando? Una mudanza -.-", new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    void ChatServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        ChatClientRpc("Client["+ serverRpcParams.Receive.SenderClientId+ "]: "+message);
    }

    [ClientRpc]
    void ChatClientRpc(string message)
    {
        Debug.Log(message);
    }



    [ServerRpc (RequireOwnership = false)]
    public void ReceiveDamageServerRpc()
    {
        vida.Value -= 5;
    }

    public enum TeamColor { red, blue };

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        Debug.Log("HOLA ENTRA AL START");
        //quiero que se actualice en todos
        playerRotation.OnValueChanged += CallbackModificacioRotacio;
        vida.OnValueChanged += CallbackModificacioVida;
        playerTeam.OnValueChanged += CallbackCanviTeam;

        if (playerTeam.Value == TeamColor.red)
            GetComponentInChildren<Image>().color = Color.red;
        else
            GetComponentInChildren<Image>().color = Color.blue;
    }
  

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
            return;
        
    }

    private void CallbackCanviTeam(TeamColor oldValue, TeamColor newValue)
    {
        if(newValue==TeamColor.red)
            GetComponentInChildren<Image>().color = Color.red;
        else
            GetComponentInChildren<Image>().color = Color.blue;
    }

    private void CallbackModificacioRotacio(float oldValue, float newValue)
    {
        transform.rotation = Quaternion.AngleAxis(playerRotation.Value, Vector3.forward);
    }
    private void CallbackModificacioVida(int oldValue, int newValue)
    {
        Debug.Log("VidaAnterior " + oldValue + " - Vida Actual " + newValue);
        GetComponentInChildren<Image>().fillAmount = ((float)vida.Value / 50f);
        if(newValue==0)
            Destroy(gameObject);
    }
    public void ChangePlayerTeam(TeamColor newTeam)
    {
        if (IsOwner)
        {
            playerTeam.Value = newTeam;
        }
    }

    

    void Update()
    {
        if (!IsOwner) return;

        Vector3 movement = Vector3.zero;

        //moviment a física
        movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            movement += Vector3.up;
        if (Input.GetKey(KeyCode.S))
            movement -= Vector3.up;
        if (Input.GetKey(KeyCode.A))
            movement -= Vector3.right;
        if (Input.GetKey(KeyCode.D))
            movement += Vector3.right;
        if (movement != Vector3.zero)
            waitForNewInput = false;

        if(!waitForNewInput)
            MoveCharacterPhysicsServerRpc(movement.normalized * m_Speed.Value);

        if (Input.GetKeyDown(KeyCode.Space) && !inCooldown && CanShoot.Value)
        {
            inCooldown = true;
            Invoke("waitCooldown", timeBetweenShoots);
            SpawnBulletServerRpc(new ServerRpcParams());
        }
            


        // Rotación del jugador hacia el cursor
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z; // Mantener la misma altura que el jugador

        Vector3 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        playerRotation.Value = angle; // Actualizar la variable de rotación en el servidor
    }
   
    void waitCooldown()
    {
        inCooldown = false;
    }

    [ServerRpc]
    private void MoveCharacterPhysicsServerRpc(Vector2 velocity, ServerRpcParams serverRpcParams = default)
    {
        m_Rigidbody.velocity = velocity;
        if (m_Rigidbody.velocity == Vector2.zero)
            waitForNewInput = true;
    }

    [ServerRpc]
    private void SpawnBulletServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Transform spawnedBulletTransform;
        if (TeamColor.red == playerTeam.Value)
        {
            spawnedBulletTransform = Instantiate(bulletObjectRedPrefab, playerShootPoint.position, playerShootPoint.rotation);
        }
        else
        {
            spawnedBulletTransform = Instantiate(bulletObjectBluePrefab, playerShootPoint.position, playerShootPoint.rotation);
        }

        spawnedBulletTransform.GetComponent<NetworkObject>().Spawn(true);
        // Establecer la rotación en el eje Z del objeto actual utilizando los ángulos de Euler del targetTransform
        //spawnedBulletTransform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, playerShootPoint.eulerAngles.z);
        spawnedBulletTransform.GetComponent<Rigidbody2D>().velocity = spawnedBulletTransform.transform.up*2.5f;

    }



}
