using UnityEngine;

/// <summary>
/// Spawns a swarm of orbiting enemies inside the camera view at start.
/// Each enemy gets a slightly randomised orbit radius, speed and direction.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyShip enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private int enemyCount = 9;

    [Header("Randomisation")]
    [SerializeField] private Vector2 orbitRadiusRange = new Vector2(3.5f, 7f);
    [SerializeField] private Vector2 orbitSpeedRange  = new Vector2(2f, 4.5f);

    private void Start()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(c.x - halfW * 0.9f, c.x + halfW * 0.9f),
                Random.Range(c.y - halfH * 0.9f, c.y + halfH * 0.9f),
                0f);

            EnemyShip e = Instantiate(enemyPrefab, pos, Quaternion.identity);
            e.player = player;
            e.orbitRadius = Random.Range(orbitRadiusRange.x, orbitRadiusRange.y);
            e.orbitSpeed = Random.Range(orbitSpeedRange.x, orbitSpeedRange.y);
            e.orbitDirection = Random.value < 0.5f ? 1 : -1;
        }
    }
}
