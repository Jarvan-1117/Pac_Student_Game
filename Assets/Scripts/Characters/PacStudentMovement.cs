using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PacStudentMovement : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 4f;
    [SerializeField] private AudioSource movementAudio;

    private readonly Vector2[] waypoints =
    {
        new(-12.5f, 13f),
        new(-7.5f, 13f),
        new(-7.5f, 9f),
        new(-12.5f, 9f)
    };

    private Animator animator;
    private int segmentIndex;
    private float segmentElapsed;
    private float segmentDuration;
    private Vector3 segmentStart;
    private Vector3 segmentEnd;

    private void Start()
    {
        animator = GetComponent<Animator>();
        transform.position = WithCurrentZ(waypoints[0]);
        BeginSegment(0);

        if (movementAudio != null && movementAudio.clip != null)
        {
            movementAudio.loop = true;
            movementAudio.Play();
        }
    }

    private void Update()
    {
        segmentElapsed += Time.deltaTime;

        while (segmentElapsed >= segmentDuration)
        {
            transform.position = segmentEnd;
            segmentElapsed -= segmentDuration;
            BeginSegment((segmentIndex + 1) % waypoints.Length);
        }

        float progress = segmentElapsed / segmentDuration;
        transform.position = Vector3.Lerp(segmentStart, segmentEnd, progress);
    }

    private void BeginSegment(int nextSegmentIndex)
    {
        segmentIndex = nextSegmentIndex;
        segmentStart = WithCurrentZ(waypoints[segmentIndex]);
        segmentEnd = WithCurrentZ(waypoints[(segmentIndex + 1) % waypoints.Length]);
        segmentDuration = Vector3.Distance(segmentStart, segmentEnd) / moveSpeed;

        Vector3 direction = segmentEnd - segmentStart;
        animator.Play(GetAnimationState(direction));
    }

    private Vector3 WithCurrentZ(Vector2 point)
    {
        return new Vector3(point.x, point.y, transform.position.z);
    }

    private static string GetAnimationState(Vector3 direction)
    {
        if (direction.x > 0f)
        {
            return "PacStudent_Right";
        }

        if (direction.y < 0f)
        {
            return "PacStudent_Down";
        }

        if (direction.x < 0f)
        {
            return "PacStudent_Left";
        }

        return "PacStudent_Up";
    }
}
