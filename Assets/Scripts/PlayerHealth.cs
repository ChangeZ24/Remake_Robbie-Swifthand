using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public GameObject deathVFXPrefab;
    int trapsLayer;//获取layer前的数值

    // Start is called before the first frame update
    void Start()
    {
        trapsLayer = LayerMask.NameToLayer("traps");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == trapsLayer)
        {
            //使用该方法将特效替换到角色的位置
            Instantiate(deathVFXPrefab, transform.position, transform.rotation);

            gameObject.SetActive(false);
        }
    }
}
