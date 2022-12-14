using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ThemeColor bulletColor;
    public float lifeTime = 3f;
    public int attack = 100;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            Enemy enemyScript = other.gameObject.GetComponent<Enemy>();

            if (enemyScript.enemyColor == bulletColor)
            {
                enemyScript.Damage(attack);
            }
            else
            {
                enemyScript.Heal(attack);
            }

            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
    }

    public void Shoot(float force)
    {
        gameObject.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0f, 0f, force));
    }
}
