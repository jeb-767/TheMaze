using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class BrokableWall : NetworkBehaviour
{
    public float life;
    public NetworkVariable<float> currentLife = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<bool> isBroken = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );  
    public Animator anim;
    public ParticleSystem effect;
    public ParticleSystem effect2;
    private bool isDead = false;
    private bool inmortal = false;
    public AdaptNormalMap texture;
    public AudioSource audioSource;
    void Start()
    {
        Debug.Log("BrokableWall instanted");
        currentLife.Value = life;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            currentLife.Value = life; // Aseguramos que el valor inicial se sincronice
            isBroken.Value = false; 
        }
        currentLife.OnValueChanged += OnCurrentLifeChanged; // Suscribimos al cambio de vida
        isBroken.OnValueChanged += OnIsBrokenChanged; // Suscribimos al cambio de broken status
    }
    public override void OnNetworkDespawn()
    {
        currentLife.OnValueChanged -= OnCurrentLifeChanged; // Desuscribimos al despawn
        isBroken.OnValueChanged -= OnIsBrokenChanged;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Attack") && !inmortal && !isDead)
        {
            Debug.Log("Hited");
            TakeDamageServerRpc(10f); // Llamamos al RPC para aplicar daño desde el servidor
        }
        else
        {

            return;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float damage)
    {
        if (isDead) return; // Si ya está roto, no hacemos nada
        currentLife.Value -= damage; // Reducimos la vida en el servidor, lo que sincronizará a los clientes
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestroyWallServerRpc()
    {
        if(isBroken.Value) return; // Si ya está roto, no hacemos nada
        isBroken.Value = true; // Cambiamos el estado a roto, lo que sincronizará a los clientes
    }
    private void OnIsBrokenChanged(bool previousValue, bool newValue)
    {
        if(newValue) // Si la pared se ha roto
        { // Marcamos la pared como rota para evitar múltiples llamadas
            anim.SetTrigger("Break");
            effect.Play();
        }
    }
    private void OnCurrentLifeChanged(float previousValue, float newValue)
    {   
        if (currentLife.Value <= 0f && !isDead)
        {
            DestroyWallServerRpc(); // Llamamos al RPC para destruir la pared desde el servidor
            
        }
        else
        {
            texture.GrietasChange();
            inmortal = true;
            StartCoroutine("resetInmortality");
            anim.SetTrigger("Damage");
            effect2.Play();
            audioSource.Play(); 
        }
        
    }
    public IEnumerator resetInmortality()
    {
        yield return new WaitForSeconds(0.7f);
        inmortal = false;
    }
}
