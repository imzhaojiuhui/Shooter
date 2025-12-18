using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DrawLine : MonoBehaviour
{
    public Rigidbody2D bullet;
    
    // Start is called before the first frame update
    private GameObject _dot;
    void Start()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        _dot = transform.GetChild(0).gameObject;

        var oriPos = bullet.transform.position;
        var oriVec = bullet.linearVelocity;
        var physicsScene = SceneManager.GetActiveScene().GetPhysicsScene2D();
        Physics2D.simulationMode = SimulationMode2D.Script;

        for (int i = 0; i < 300; i++)
        {
            physicsScene.Simulate(Time.fixedDeltaTime);
            if (i % 3 != 0)
            {
                continue;
            }
            Physics2D.SyncTransforms();
            var dot = GameObject.Instantiate(_dot, bullet.transform.position, Quaternion.identity, transform);
            dot.gameObject.SetActive(true);
        }
        
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        bullet.transform.position = oriPos;
        bullet.linearVelocity = oriVec;
        bullet.angularVelocity = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
