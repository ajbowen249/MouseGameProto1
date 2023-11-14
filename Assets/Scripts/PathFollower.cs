using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public delegate void PathCompleteCallback();

    public GameObject Path;
    public GameObject FollowingObject;
    public PathCompleteCallback OnPathComplete;

    // Segments per second
    public float Speed = 1;

    private PathNode[] _nodes;
    private int _index;
    private float _t;

    public static PathFollower SendObjectAlongPath(
        GameObject obj,
        GameObject path,
        PathCompleteCallback onComplete = null
    )
    {
        var followerObject = new GameObject();
        followerObject.AddComponent<PathFollower>();
        var follower = followerObject.GetComponent<PathFollower>();
        follower.Path = path;
        follower.FollowingObject = obj;
        follower.OnPathComplete = onComplete;

        return follower;
    }

    void Start()
    {
        _index = 0;
        _t = 0;
        _nodes = Path.transform.GetComponentsInChildren<PathNode>();
    }

    void Update()
    {
        if (_t < 1)
        {
            _t += Speed * Time.deltaTime;
            if (_t > 1)
            {
                _t = 1;
            }

            transform.position = Vector3.Lerp(
                _nodes[_index].transform.position,
                _nodes[_index + 1].transform.position,
                _t
            );

            FollowingObject.transform.position = transform.position;
            FollowingObject.transform.LookAt(_nodes[_index + 1].transform);
        }

        if (_t >= 1)
        {
            // -2 because we don't visit the last node, only lerp toward it from the second-to-last
            if (_index < _nodes.Length - 2)
            {
                _index++;
                _t = 0;
            }
            else
            {
                if (OnPathComplete != null)
                {
                    OnPathComplete();
                }

                Destroy(gameObject);
            }
        }
    }
}
