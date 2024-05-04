using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class DissolveController : NetworkBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;
    public float dissolveRate = 0.01f;
    public float refreshRate = 0.025f;

    private Material[] skinnedMaterials;

    
    void Start()
    {
        if (skinnedMesh != null)
            skinnedMaterials = skinnedMesh.materials;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TriggerDie()
    {
        TriggerDieClientRpc();
    }

    [ClientRpc]
    private void TriggerDieClientRpc()
    {
        StartCoroutine(DissolveCo());
    }

    IEnumerator DissolveCo ()
    {
        if(skinnedMaterials.Length > 0)
        {
            float counter = 0;

            while(skinnedMaterials[0].GetFloat("_DissolveAmount") < 1)
            {
                counter += dissolveRate;
                for(int i=0; i<skinnedMaterials.Length; i++)
                {
                    skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
                }
                yield return new WaitForSeconds(refreshRate);
            }
        }
    }

}
