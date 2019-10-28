using System.Collections;
using UnityEngine;

public class MoveAlongPath : MonoBehaviour
{
    //array of path to move along
    private Vector3[] path;
    //iterator to path array 
    private int itPath = 0;
    //speed steep by value of speed velocity or time according to MoveType 
    private float speedStep;
    //move is auto set when all necessay params have been set
    private bool move = false;
    //check if it is still necessary
    private bool inSight = false;
    //if finisehd path ... reset all values and stop all Coroutines
    private bool finishedPath = false;
    //while moving its true
    private bool moving = false;
    //force stop all 
    private bool forceStop = false;
    //execute on finish last move 
    private AudioSource finishedPathSource;
    private AudioClip finishedPathSound;
    public enum MoveType { Time, Speed }

    private MoveType moveType = MoveType.Speed;
    /// <summary>
    /// Set necessary params to move object 
    /// </summary>
    /// <param name="pathToFollow"> Array of ways to obj follow </param>
    /// <param name="speed"> Speed by step in time or velocity accordind to moveType param </param>
    /// <param name="_moveType"> Speed or time step to move </param>
    public void SetMoveParams(Vector3[] pathToFollow, float speed, MoveType _moveType, AudioClip _finishedPathSound, AudioSource _finishedPathSource)
    {
        itPath = 0;
        path = pathToFollow;
        speedStep = speed;
        moveType = _moveType;
        move = true;
        finishedPathSound = _finishedPathSound;
        finishedPathSource = _finishedPathSource;
    }
    public void SetSight(bool _inSight)
    {
        inSight = _inSight;
    }
    public bool GetSight()
    {
        return inSight;
    }
    /// <summary>
    /// Set all variables to default value
    /// </summary>
    private void Restart()
    {
        move = false;
        moving = false;
        speedStep = 0;
        itPath = 0;
        finishedPath = false;
        forceStop = false;
        finishedPathSound = null;
        finishedPathSource = null;
    }
    /// <summary>
    /// Stop All in next frame 
    /// </summary>
    public void Stop()
    {
        moving = false;
        forceStop = true;
    }
    /// <summary>
    /// Get is moving variable
    /// </summary>
    /// <returns>Return moving boolean </returns>
    public bool GetIsMoving()
    {
        return moving;
    }

    void Start()
    {
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        //stop and restart all after arriving at the destination
        if ((finishedPath && itPath > 0) || forceStop)
        {
            if (!forceStop && finishedPathSound != null && finishedPathSource != null)
            {

                //finishedPathSource.transform.SetParent(gameObject.transform);
                //finishedPathSource.transform.localPosition = Vector3.zero;

                PlayAudioClip(finishedPathSource, finishedPathSound);
            }

            StopAllCoroutines();
            Restart();
        }

        if (!inSight || !move)
            return;

        if (move)
        {
            speedStep = speedStep == 0 ? 0.5f : speedStep;
            StartCoroutine(Translation(gameObject.transform, path[itPath], speedStep, moveType));
            move = false;
        }
    }
    /// <summary>
    /// Play audio
    /// </summary>
    /// <param name="source"> Audio source </param>
    /// <param name="clip">Audio Clip</param>
    private void PlayAudioClip(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }

    public IEnumerator TranslateTo(Transform objTransform, Vector3 endPos, float value, MoveType moveType)
    {
        yield return Translation(objTransform, objTransform.position, endPos, value, moveType);
    }

    public IEnumerator Translation(Transform objTransform, Vector3 endPos, float value, MoveType moveType)
    {
        yield return Translation(objTransform, objTransform.position, endPos, value, moveType);
    }

    public IEnumerator Rotation(Transform objTransform, Vector3 degrees, float time)
    {
        Quaternion startRotation = objTransform.rotation;
        Quaternion endRotation = objTransform.rotation * Quaternion.Euler(degrees);
        float rate = 1.0f / time;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            objTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
    }

    public IEnumerator Translation(Transform objTransform, Vector3 startPos, Vector3 endPos, float value, MoveType moveType)
    {
        float rate = (moveType == MoveType.Time) ? 1.0f / value : 1.0f / Vector3.Distance(startPos, endPos) * value;
        float t = 0.0f;

        while (t < 1.0f)
        {

            if (value != speedStep)
            {
                rate = (moveType == MoveType.Time) ? 1.0f / speedStep : 1.0f / Vector3.Distance(startPos, endPos) * speedStep;
            }

            t += Time.deltaTime * rate;

            objTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));

            moving = true;

            if (Vector3.Distance(objTransform.position, endPos) < 0.001f)
            {
                if (itPath < path.Length - 1)
                {
                    itPath++;
                    StartCoroutine(Translation(gameObject.transform, path[itPath], speedStep, moveType));
                }
                else
                {
                    finishedPath = true;
                    moving = false;
                }

                t = 1.0f;
            }
            yield return null;
        }
    }
}
